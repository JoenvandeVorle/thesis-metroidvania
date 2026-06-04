using System;
using System.Collections.Generic;
using Metroidvania;
using Metroidvania.Characters.Knight;
using Metroidvania.Pathfinding;
using UnityEngine;

public class TargetPathFinder : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float pathUpdateRate = 3f;

    public GameObject target;
    private Pathfinder pathfinderInstance;
    private Path currentPath;
    private KnightCharacterController character;
    private float pathUpdateTimer;

    void Start()
    {
        if (player == null || target == null)
        {
            Debug.LogError("Player or Target GameObject is not assigned!");
            return;
        }

        pathfinderInstance = Pathfinder.instance;
        character = player.GetComponent<KnightCharacterController>();

        pathUpdateTimer = pathUpdateRate;
    }

    void Update()
    {
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateRate)
        {
            if (!(character.elapsedGroundTime >= 0.2f))
                return; // Do not update in the air

            UpdatePath();
            pathUpdateTimer = 0f;
        }
    }

    // TODO:: when following path, can differentiate between hor jump and ver jump by looking at
    // next 4 nodes and checking the max y difference.
    public List<Vector2> GetCurrentPath() => currentPath?.vectorPath;

    public Tuple<int, Vector2> GetNextPathNode()
    {
        if (currentPath == null)
        {
            Debug.LogWarning("No current path available for GetNextPathNode.");
            return Tuple.Create(-1, Vector2.zero);
        }
        return GetNextPathNode(player.transform.position, currentPath.vectorPath);
    }

    public static Tuple<int, Vector2> GetNextPathNode(Vector2 pos, List<Vector2> path)
    {
        int closestNodeIdx = -1;
        float closestNodeDistance = float.MaxValue;
        for (int i = 0; i < path.Count - 1; i++)
        {
            float distance = Vector2.Distance(path[i], pos);
            if (distance < 0.5f)
                return Tuple.Create(i + 1, path[i + 1]);

            if (distance < closestNodeDistance)
            {
                closestNodeDistance = distance;
                closestNodeIdx = i;
            }
        }
        if (closestNodeIdx == path.Count - 1)
            return Tuple.Create(-1, path[^1]);
        return Tuple.Create(closestNodeIdx + 1, path[closestNodeIdx + 1]);
    }

    public Tuple<int, Vector2> FindEndOfJumpStartingAtIndex(int index)
    {
        if (currentPath == null)
        {
            Debug.LogWarning("No current path available in FindEndOfJumpAtIndex.");
            return Tuple.Create(-1, Vector2.zero);
        }
        return FindEndOfJumpStartingAtIndex(currentPath.vectorPath, index);
    }

    public static Tuple<int, Vector2> FindEndOfJumpStartingAtIndex(List<Vector2> vectorPath, int startIndex)
    {
        if (startIndex < 0 || startIndex >= vectorPath.Count)
        {
            Debug.LogWarning("Invalid path or index for FindEndOfJumpAtIndex.");
            return Tuple.Create(-1, Vector2.zero);
        }

        for (int i = startIndex + 1; i < vectorPath.Count - 1; i++)
        {
            float yDiff = vectorPath[i].y - vectorPath[startIndex].y;
            if (yDiff <= 0) // end of jump when next node is at same or lower height
                return Tuple.Create(i, vectorPath[i]);
        }

        int jumpEndIndex = Mathf.Min(startIndex + Vars.MAX_JUMP_HEIGHT + 1, vectorPath.Count - 1);
        return Tuple.Create(jumpEndIndex, vectorPath[jumpEndIndex]);
    }

    public void UpdatePath()
    {
        Vector2 playerPos = player.transform.position;
        Vector2 targetPos = target.transform.position;

        if (currentPath != null)
            pathfinderInstance.ReleasePath(ref currentPath);

        currentPath = pathfinderInstance.FindPathForPlayer(playerPos, targetPos);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (currentPath != null)
            new GizmosDrawer().DrawPathNodes(currentPath);
    }
#endif
}
