using Aplib.Core;
using Aplib.Core.Belief.Beliefs;
using Aplib.Core.Belief.BeliefSets;
using Aplib.Core.Desire.Goals;
using Aplib.Core.Intent.Actions;
using Aplib.Core.Intent.Tactics;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Aplib.Core.Desire.GoalStructures;
using Aplib.Core.Desire.DesireSets;
using Aplib.Core.Agents;
using Aplib.Integrations.Unity;
using Tests.AplibTests;
using Metroidvania.Pathfinding;
using Tuple = System.Tuple;
using System.Runtime.CompilerServices;

namespace Tests.HybridTests
{
    public class HybridJumpBeliefSet : BeliefSet
    {
        public readonly Belief<GameObject, GameObject> Player =
            new(reference: GameObject.Find("Player"), x => x);

        public readonly Belief<GameObject, GameObject> Target = new(
            GameObject.Find("Target_R_3"), x => x);

        public readonly Belief<TargetPathFinder, List<Vector2>> currentPath = new(
            GameObject.Find("Player").GetComponent<TargetPathFinder>(), x => x.GetCurrentPath());
    }

    public class HybridJumpTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test HybridJumpTest");
            SceneManager.LoadScene("HybridJumpTest");
        }

        [UnityTest]
        public IEnumerator PerformHybridJumpTest()
        {
            // Arrange
            InputGenerator inputGenerator = InputGenerator.instance;
            HybridJumpBeliefSet beliefSet = new();
            GameObject player = beliefSet.Player;
            JumpTacticForTest jumpTacticAgent = player.GetComponent<JumpTacticForTest>();
            if (jumpTacticAgent == null)
                Assert.Fail("JumpTacticForTest component not found on Player GameObject");
            jumpTacticAgent.enabled = false;
            TargetPathFinder pathfinder = player.GetComponent<TargetPathFinder>();
            if (pathfinder == null)
                Assert.Fail("TargetPathFinder component not found on Player GameObject");

            while (pathfinder.GetCurrentPath() == null)
            {
                Debug.Log("Waiting for path to be calculated...");
                yield return TestWait.ForSeconds(1f);
            }
            System.Tuple<int, Vector2> nextPathNode = pathfinder.GetNextPathNode();

            #region movement tactics

            Action<HybridJumpBeliefSet> move = new(
                beliefSet =>
                {
                    nextPathNode = pathfinder.GetNextPathNode();
                    // Debug.Log("Doing movement action to: " + nextPathInfo.Item2);
                    Vector3 direction = ((Vector3)nextPathNode.Item2 - player.transform.position).normalized;
                    inputGenerator.MoveTowards(direction);
                }
            );

            Action<HybridJumpBeliefSet> stopMovement = new(
                beliefSet =>
                {
                    Debug.Log("Stopping movement");
                    inputGenerator.ReleaseMovement();
                }
            );

            PrimitiveTactic<HybridJumpBeliefSet> startMovementTactic = new(move);

            bool hasReachedJumpPoint(HybridJumpBeliefSet beliefSet)
            {
                if (jumpTacticAgent.enabled)
                    return true;

                GameObject player = beliefSet.Player;
                if (nextPathNode.Item1 == -1)
                {
                    Debug.LogWarning("No next path index available in hasReachedJumpPoint guard.");
                    return false;
                }
                if (nextPathNode.Item2.y > player.transform.position.y)
                {
                    Debug.Log($"Reached jump point at i:{nextPathNode.Item1} - {nextPathNode.Item2}");
                }
                return nextPathNode.Item2.y > player.transform.position.y;
            }

            PrimitiveTactic<HybridJumpBeliefSet> stopMovementTactic = new(stopMovement, hasReachedJumpPoint);
            FirstOfTactic<HybridJumpBeliefSet> moveTactic = new(stopMovementTactic, startMovementTactic);

            #endregion

            #region jump tactic agent
            Action<HybridJumpBeliefSet> jumpAction = new(
                beliefSet =>
                {
                    if (jumpTacticAgent.enabled)
                        return;
                    Vector2 jumpEndPoint = pathfinder.FindEndOfJumpStartingAtIndex(nextPathNode.Item1 - 1);
                    Debug.Log($"Starting jump to: " + jumpEndPoint + $" from index {nextPathNode.Item1 - 1}");
                    jumpTacticAgent.StartAgent(jumpEndPoint);
                }
            );

            Action<HybridJumpBeliefSet> stopJump = new(
                beliefSet =>
                {
                    Debug.Log("Stopping jump");
                    jumpTacticAgent.StopAgent();
                    nextPathNode = pathfinder.GetNextPathNode();
                }
            );

            PrimitiveTactic<HybridJumpBeliefSet> startJumpTactic = new(jumpAction, hasReachedJumpPoint);

            bool hasReachedJumpTarget(HybridJumpBeliefSet beliefSet)
            {
                if (jumpTacticAgent.ReachedGoal)
                    Debug.Log($"Reached jump target: {jumpTacticAgent.goalPosition}");
                return jumpTacticAgent.ReachedGoal;
            }

            PrimitiveTactic<HybridJumpBeliefSet> stopJumpTactic = new(stopJump, hasReachedJumpTarget);
            FirstOfTactic<HybridJumpBeliefSet> jumpTactic = new(stopJumpTactic, startJumpTactic);
            #endregion

            FirstOfTactic<HybridJumpBeliefSet> moveAndJumpTactic = new(jumpTactic, moveTactic);

            Goal<HybridJumpBeliefSet> reachedTargetGoal = new(
                moveAndJumpTactic,
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    GameObject target = beliefSet.Target;

                    if (!didNotFall())
                    {
                        Assert.Fail("Player fell during the test");
                        return false;
                    }

                    return Vector2.Distance(player.transform.position, target.transform.position) < 0.5f;
                }
            );
            PrimitiveGoalStructure<HybridJumpBeliefSet> reachTargetGoalStructure = new(reachedTargetGoal);
            DesireSet<HybridJumpBeliefSet> desireSet = new(reachTargetGoalStructure);

            // Setup the agent with the belief set and desire set and initialize the test runner.
            BdiAgent<HybridJumpBeliefSet> agent = new(beliefSet, desireSet);
            AplibRunner testRunner = new(agent);

            // Act
            Assert.GreaterOrEqual(player.transform.position.y, -10);
            yield return testRunner.Test();

            // Assert
            Assert.AreEqual(CompletionStatus.Success, agent.Status);

            bool didNotFall()
            {
                GameObject player = beliefSet.Player;
                return player.transform.position.y > -10;
            }
        }

    }
}
