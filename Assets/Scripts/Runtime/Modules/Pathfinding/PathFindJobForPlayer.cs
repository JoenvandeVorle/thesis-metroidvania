using Unity.Collections;

namespace Metroidvania.Pathfinding
{
    // State space: (x, y, jumpPhase) where:
    //   jumpPhase = 0            → grounded (solid floor below)
    //   jumpPhase = 1..maxJump   → ascending (cells risen from launch point)
    //   jumpPhase = maxJump+1    → falling  (gravity, single phase – grid bounds limit depth)
    [Unity.Burst.BurstCompile]
    public struct PathFindJobForPlayer : Unity.Jobs.IJob
    {
        private const int k_MoveStraightCost = 10;
        private const int k_MoveDiagonalCost = 16;
        private const int k_MoveToFloorCost = 5;

        public CellPosition start;
        public CellPosition end;
        public CellPosition gridSize;

        // Maximum cells the player can jump straight up.
        public int maxJumpHeight;

        // Read-only walkability map (standard width*height nativeNodes array).
        [ReadOnly]
        public NativeArray<PathNodeReference> pathNodes;

        [WriteOnly]
        public NativeList<int> generatedPath;

        // Per-state A* bookkeeping (allocated internally as Allocator.Temp).
        private struct StateNode
        {
            public int g;
            public int h;
            public readonly int F => g + h;
            public int cameFromIndex; // index into the state array, -1 = none
        }

        public void Execute()
        {
            int w = gridSize.x;
            int cells = w * gridSize.y;
            int fallingPhase = maxJumpHeight + 1;
            int totalStates = cells * (fallingPhase + 1);

            NativeArray<StateNode> states = new NativeArray<StateNode>(totalStates, Allocator.Temp);
            for (int i = 0; i < totalStates; i++)
                states[i] = new StateNode { g = int.MaxValue, h = int.MaxValue, cameFromIndex = -1 };

            int endCell = end.x + end.y * w;

            // Assumes the player starts grounded (phase 0).
            int startIdx = start.x + start.y * w;
            var s0 = states[startIdx];
            s0.g = 0;
            s0.h = CalculateDistanceCost(start, end);
            states[startIdx] = s0;

            NativeList<int> openList = new(Allocator.Temp);
            NativeList<int> closedList = new(Allocator.Temp);
            openList.Add(startIdx);

            while (openList.Length > 0)
            {
                int curIdx = GetLowestFState(openList, states);
                int curPhase = curIdx / cells;
                int curCell = curIdx % cells;
                int cx = curCell % w;
                int cy = curCell / w;

                if (curCell == endCell)
                    break;

                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == curIdx) { openList.RemoveAtSwapBack(i); break; }
                }
                closedList.Add(curIdx);

                if (curPhase == 0) // grounded
                {
                    // Walk left/right; fall off edges automatically.
                    ExploreGround(cx - 1, cy, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveToFloorCost);
                    ExploreGround(cx + 1, cy, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveToFloorCost);

                    if (maxJumpHeight >= 1)
                    {
                        // Jump straight up.
                        ExploreAscend(cx, cy + 1, 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveStraightCost);
                        // Jump diagonally (clears wall to the side first).
                        if (IsWalkable(cx - 1, cy)) ExploreAscend(cx - 1, cy + 1, 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                        if (IsWalkable(cx + 1, cy)) ExploreAscend(cx + 1, cy + 1, 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                    }
                }
                else if (curPhase < maxJumpHeight) // ascending
                {
                    if (curPhase < maxJumpHeight)
                    {
                        ExploreAscend(cx, cy + 1, curPhase + 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveStraightCost);
                        if (IsWalkable(cx - 1, cy)) ExploreAscend(cx - 1, cy + 1, curPhase + 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                        if (IsWalkable(cx + 1, cy)) ExploreAscend(cx + 1, cy + 1, curPhase + 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                    }

                    // Drift horizontally
                    ExploreAir(cx - 1, cy, curPhase + 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveStraightCost);
                    ExploreAir(cx + 1, cy, curPhase + 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveStraightCost);

                    ExploreFall(cx, cy - 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveStraightCost);
                    if (IsWalkable(cx - 1, cy - 1)) ExploreFall(cx - 1, cy - 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                    if (IsWalkable(cx + 1, cy - 1)) ExploreFall(cx + 1, cy - 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                }
                else // falling
                {
                    // Fall straight down or diagonally.
                    ExploreFall(cx, cy - 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveStraightCost);
                    if (IsWalkable(cx - 1, cy - 1)) ExploreFall(cx - 1, cy - 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                    if (IsWalkable(cx + 1, cy - 1)) ExploreFall(cx + 1, cy - 1, curIdx, cells, fallingPhase, states, openList, closedList, k_MoveDiagonalCost);
                }
            }

            // Accept the end cell at any phase; prefer lowest g.
            int bestEndIdx = -1;
            int bestG = int.MaxValue;
            for (int p = 0; p <= fallingPhase; p++)
            {
                int si = endCell + p * cells;
                if (states[si].cameFromIndex != -1 && states[si].g < bestG)
                {
                    bestG = states[si].g;
                    bestEndIdx = si;
                }
            }

            // Trace path end→start (Path.Setup reverses it).
            // TODO:: add jump state to returned path?
            if (bestEndIdx != -1)
            {
                generatedPath.Add(bestEndIdx % cells);
                int cur = bestEndIdx;
                while (states[cur].cameFromIndex != -1)
                {
                    cur = states[cur].cameFromIndex;
                    generatedPath.Add(cur % cells);
                }
            }

            states.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        // Walk on ground; automatically enter falling phase if destination has no floor.
        private void ExploreGround(int nx, int ny, int fromIdx, int cells, int fallingPhase,
            NativeArray<StateNode> states, NativeList<int> openList, NativeList<int> closedList, int cost)
        {
            if (!IsWalkable(nx, ny)) return;
            int destPhase = HasFloor(nx, ny) ? 0 : fallingPhase;
            AddNeighbor(nx + ny * gridSize.x + destPhase * cells, fromIdx, new CellPosition(nx, ny), cost, states, openList, closedList);
        }

        // Move to an ascending cell; land (phase 0) if a floor is present at the destination.
        private void ExploreAscend(int nx, int ny, int newPhase, int fromIdx, int cells, int fallingPhase,
            NativeArray<StateNode> states, NativeList<int> openList, NativeList<int> closedList, int cost)
        {
            if (!IsWalkable(nx, ny)) return;
            int destPhase = HasFloor(nx, ny) ? 0 : newPhase;
            AddNeighbor(nx + ny * gridSize.x + destPhase * cells, fromIdx, new CellPosition(nx, ny), cost, states, openList, closedList);
        }

        // Horizontal movement in air; keeps the same phase (height budget unchanged) or lands.
        private void ExploreAir(int nx, int ny, int keepPhase, int fromIdx, int cells, int fallingPhase,
            NativeArray<StateNode> states, NativeList<int> openList, NativeList<int> closedList, int cost)
        {
            if (!IsWalkable(nx, ny)) return;
            int destPhase = HasFloor(nx, ny) ? 0 : keepPhase;
            AddNeighbor(nx + ny * gridSize.x + destPhase * cells, fromIdx, new CellPosition(nx, ny), cost, states, openList, closedList);
        }

        // Move downward under gravity; land (phase 0) when a floor is present at the destination.
        private void ExploreFall(int nx, int ny, int fromIdx, int cells, int fallingPhase,
            NativeArray<StateNode> states, NativeList<int> openList, NativeList<int> closedList, int cost)
        {
            if (!IsWalkable(nx, ny)) return;
            int destPhase = fallingPhase;
            if (HasFloor(nx, ny))
            {
                destPhase = 0;
                cost = k_MoveToFloorCost;
            }
            AddNeighbor(nx + ny * gridSize.x + destPhase * cells, fromIdx, new CellPosition(nx, ny), cost, states, openList, closedList);
        }

        private readonly void AddNeighbor(int neighborIdx, int fromIdx, CellPosition neighborPos, int cost,
            NativeArray<StateNode> states, NativeList<int> openList, NativeList<int> closedList)
        {
            if (closedList.Contains(neighborIdx)) return;
            int tentativeG = states[fromIdx].g + cost;
            StateNode neighbor = states[neighborIdx];
            if (tentativeG < neighbor.g)
            {
                neighbor.g = tentativeG;
                neighbor.h = CalculateDistanceCost(neighborPos, end);
                neighbor.cameFromIndex = fromIdx;
                states[neighborIdx] = neighbor;
                if (!openList.Contains(neighborIdx))
                    openList.Add(neighborIdx);
            }
        }

        private bool IsWalkable(int x, int y)
        {
            if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y) return false;
            return pathNodes[x + y * gridSize.x].walkable;
        }

        // A cell has a floor if the cell directly below is solid (not walkable) or it sits on the grid bottom.
        private bool HasFloor(int x, int y)
        {
            if (y == 0) return false;
            return !pathNodes[x + (y - 1) * gridSize.x].walkable;
        }

        private readonly int GetLowestFState(NativeList<int> openList, NativeArray<StateNode> states)
        {
            int lowestIdx = openList[0];
            int lowestF = states[lowestIdx].F;
            for (int i = 1; i < openList.Length; i++)
            {
                int f = states[openList[i]].F;
                if (f < lowestF) { lowestF = f; lowestIdx = openList[i]; }
            }
            return lowestIdx;
        }

        private readonly int CalculateDistanceCost(CellPosition a, CellPosition b)
        {
            int xDist = System.Math.Abs(a.x - b.x);
            int yDist = System.Math.Abs(a.y - b.y);
            int remaining = System.Math.Abs(xDist - yDist);
            return k_MoveDiagonalCost * System.Math.Min(xDist, yDist) + k_MoveStraightCost * remaining;
        }
    }
}
