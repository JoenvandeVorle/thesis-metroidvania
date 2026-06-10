using System.Collections;
using System.Collections.Generic;
using Aplib.Core.Agents;
using Aplib.Core.Desire.DesireSets;
using Aplib.Core.Desire.Goals;
using Aplib.Core.Desire.GoalStructures;
using Aplib.Core.Intent.Actions;
using Aplib.Core.Intent.Tactics;
using Aplib.Integrations.Unity;
using Metroidvania.Characters.Knight;
using NUnit.Framework;
using Tests.AplibTests;
using UnityEngine;

namespace Tests.Experiments
{
    public abstract class BaseAplibExperiment : BaseExperiment
    {
        public static int PATHFINDER_WAIT_TIMEOUT = 5;
        protected JumpTacticForTest jumpTacticAgent;
        protected TargetPathFinder pathfinder;
        protected System.Tuple<int, Vector2> nextPathNode;

        #region movement tactics

        protected Action<ExperimentBeliefSet> moveAction;
        protected Action<ExperimentBeliefSet> stopMovementAction;
        protected PrimitiveTactic<ExperimentBeliefSet> startMovementTactic;
        protected PrimitiveTactic<ExperimentBeliefSet> stopMovementTactic;
        protected FirstOfTactic<ExperimentBeliefSet> moveTactic;

        protected bool hasReachedJumpPoint(ExperimentBeliefSet beliefSet)
        {
            if (IsJumping)
                return true;

            GameObject player = beliefSet.Player;
            if (nextPathNode.Item1 == -1)
            {
                Debug.LogWarning("No next path index available in hasReachedJumpPoint guard.");
                return false;
            }
            if (nextPathNode.Item2.y > player.transform.position.y + 0.5f)
            {
                Debug.Log($"Reached jump point at i:{nextPathNode.Item1} - {nextPathNode.Item2}");
                return true;
            }
            return false;
        }

        bool hasPath(ExperimentBeliefSet beliefSet)
        {
            List<Vector2> path = beliefSet.currentPath;
            return path != null && path.Count > 0;
        }

        protected void InitializeMoveTactics()
        {
            moveAction = new(
                beliefSet =>
                {
                    nextPathNode = pathfinder.GetNextPathNode();
                    Vector3 direction = ((Vector3)nextPathNode.Item2 - player.transform.position).normalized;
                    inputGenerator.MoveTowards(direction);
                }
            );
            startMovementTactic = new(moveAction, hasPath);

            stopMovementAction = new(beliefSet => inputGenerator.ReleaseMovement());
            stopMovementTactic = new(stopMovementAction, hasReachedJumpPoint);
            moveTactic = new FirstOfTactic<ExperimentBeliefSet>(stopMovementTactic, startMovementTactic);
        }

        #endregion

        #region jump tactics

        protected Action<ExperimentBeliefSet> jumpAction;
        protected Action<ExperimentBeliefSet> stopJumpAction;
        protected PrimitiveTactic<ExperimentBeliefSet> startJumpTactic;
        protected PrimitiveTactic<ExperimentBeliefSet> stopJumpTactic;
        protected FirstOfTactic<ExperimentBeliefSet> jumpTactic;
        protected int jumpGoalIndex = -1;
        protected abstract bool IsJumping { get; }

        protected abstract bool hasReachedJumpTarget(ExperimentBeliefSet beliefSet);

        protected bool checkNextFiveNodes(ExperimentBeliefSet beliefSet)
        {
            GameObject player = beliefSet.Player;
            List<Vector2> path = beliefSet.currentPath;
            if (path == null) return false;

            for (int i = jumpGoalIndex; i < Mathf.Min(jumpGoalIndex + 5, path.Count); i++)
            {
                if (Vector2.Distance(player.transform.position, path[i]) < 0.5f)
                {
                    Debug.Log($"Considered reached jump target by proximity to node at index {i}: {path[i]}");
                    return true;
                }
            }
            return false;
        }

        bool mayJump(ExperimentBeliefSet beliefSet) => hasReachedJumpPoint(beliefSet) && hasPath(beliefSet);

        protected void InitializeJumpTactics()
        {
            jumpAction = new(
                beliefSet =>
                {
                    if (IsJumping)
                        return;
                    System.Tuple<int, Vector2> jumpEndPoint = pathfinder.FindEndOfJumpStartingAtIndex(nextPathNode.Item1 - 1);
                    Debug.Log($"Starting jump to: " + jumpEndPoint.Item2 + $" from index {nextPathNode.Item1 - 1}");
                    StartJump(jumpEndPoint);
                    jumpGoalIndex = jumpEndPoint.Item1;
                }
            );
            startJumpTactic = new(jumpAction, mayJump);

            stopJumpAction = new(
                beliefSet =>
                {
                    StopJump();
                    nextPathNode = pathfinder.GetNextPathNode();
                }
            );
            stopJumpTactic = new(stopJumpAction, hasReachedJumpTarget);
            jumpTactic = new FirstOfTactic<ExperimentBeliefSet>(stopJumpTactic, startJumpTactic);
        }

        protected abstract void StartJump(System.Tuple<int, Vector2> jumpEndPoint);

        protected abstract void StopJump();

        #endregion

        #region path correction

        protected Action<ExperimentBeliefSet> updatePathAction;
        protected PrimitiveTactic<ExperimentBeliefSet> updatePathTactic;
        bool isOffPath(ExperimentBeliefSet beliefSet)
        {
            GameObject player = beliefSet.Player;
            if (!hasPath(beliefSet))
                return true;
            System.Tuple<int, Vector2> closestNode = pathfinder.GetNextPathNode();
            if (Vector2.Distance(player.transform.position, closestNode.Item2) > 4.0f)
                return true;
            return false;
        }

        protected void InitializePathCorrection()
        {
            updatePathAction = new(
                beliefSet =>
                {
                    Debug.Log("Updating path due to deviation...");
                    pathfinder.UpdatePath();
                    nextPathNode = pathfinder.GetNextPathNode();
                    StopJump();
                }
            );
            updatePathTactic = new(updatePathAction, isOffPath);
        }

        #endregion

        #region experiment goal and agent

        protected FirstOfTactic<ExperimentBeliefSet> moveAndJumpTactic;
        protected Goal<ExperimentBeliefSet> reachedTargetGoal;
        protected PrimitiveGoalStructure<ExperimentBeliefSet> experimentGoalStructure;
        protected DesireSet<ExperimentBeliefSet> desireSet;
        protected BdiAgent<ExperimentBeliefSet> agent;

        public void SetupExperiment()
        {
            InitializeMoveTactics();
            InitializeJumpTactics();
            InitializePathCorrection();

            moveAndJumpTactic = new FirstOfTactic<ExperimentBeliefSet>(updatePathTactic, jumpTactic, moveTactic);

            bool didNotFall()
            {
                GameObject player = beliefSet.Player;
                return player.transform.position.y > -10;
            }

            bool isAlive()
            {
                KnightCharacterController controller = beliefSet.PlayerController;
                return controller != null && controller.lifeAttribute.currentValue > 0;
            }

            reachedTargetGoal = new Goal<ExperimentBeliefSet>(
                moveAndJumpTactic,
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    GameObject target = beliefSet.Target;

                    if (!didNotFall() || !isAlive())
                    {
                        runner.Abort("Fell or died");
                        return false;
                    }

                    return Vector2.Distance(player.transform.position, target.transform.position) < 0.5f;
                }
            );
            experimentGoalStructure = new PrimitiveGoalStructure<ExperimentBeliefSet>(reachedTargetGoal);
            desireSet = new DesireSet<ExperimentBeliefSet>(experimentGoalStructure);
            agent = new BdiAgent<ExperimentBeliefSet>(beliefSet, desireSet);
            runner = new AbortableAplibRunner(agent);
        }

        #endregion

        protected override void Arrange()
        {
            base.Arrange();

            jumpTacticAgent = player.GetComponent<JumpTacticForTest>();
            if (jumpTacticAgent == null)
                Assert.Fail("JumpTacticForTest component not found on Player GameObject");
            jumpTacticAgent.enabled = false;

            pathfinder = player.GetComponent<TargetPathFinder>();
            if (pathfinder == null)
                Assert.Fail("TargetPathFinder component not found on Player GameObject");

            if (pathfinder.GetCurrentPath() == null)
                Debug.Log("Remember to wait for the pathfinder first when running the test");

            SetupExperiment();
        }

        public override IEnumerator Act(int run)
        {
            int pathfinderWaitTime = 0;
            while (pathfinder.GetCurrentPath() == null && pathfinderWaitTime < PATHFINDER_WAIT_TIMEOUT)
            {
                pathfinderWaitTime++;
                yield return TestWait.ForSeconds(0.5f);
            }
            if (pathfinder.GetCurrentPath() == null)
            {
                Debug.LogError("Pathfinder failed to calculate a path within the timeout period.");
                runner.Abort("Pathfinder timeout");
                yield break;
            }
            Debug.Log("Found Path!");
            nextPathNode = pathfinder.GetNextPathNode();

            yield return runner.Test();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            StopJump();
        }
    }
}
