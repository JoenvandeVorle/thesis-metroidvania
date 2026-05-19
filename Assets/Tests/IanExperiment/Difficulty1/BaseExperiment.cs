using System.Collections;
using System.Collections.Generic;
using Aplib.Core;
using Aplib.Core.Agents;
using Aplib.Core.Belief.Beliefs;
using Aplib.Core.Belief.BeliefSets;
using Aplib.Core.Desire.DesireSets;
using Aplib.Core.Desire.Goals;
using Aplib.Core.Desire.GoalStructures;
using Aplib.Core.Intent.Actions;
using Aplib.Core.Intent.Tactics;
using Aplib.Integrations.Unity;
using NUnit.Framework;
using Tests.AplibTests;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class ExperimentBeliefSet : BeliefSet
    {
        public readonly Belief<GameObject, GameObject> Player =
            new(reference: GameObject.Find("Player"), x => x);

        public readonly Belief<GameObject, GameObject> Target = new(
            GameObject.Find("Target"), x => x);

        public readonly Belief<TargetPathFinder, List<Vector2>> currentPath = new(
            GameObject.Find("Player").GetComponent<TargetPathFinder>(), x => x.GetCurrentPath());
    }

    public class BaseExperiment
    {
        protected InputGenerator inputGenerator;
        protected ExperimentBeliefSet beliefSet;
        protected GameObject player;
        protected JumpTacticForTest jumpTacticAgent;
        protected TargetPathFinder pathfinder;
        protected System.Tuple<int, Vector2> nextPathNode;

        protected void Arrange()
        {
            inputGenerator = InputGenerator.instance;
            beliefSet = new ExperimentBeliefSet();
            player = beliefSet.Player;

            jumpTacticAgent = player.GetComponent<JumpTacticForTest>();
            if (jumpTacticAgent == null)
                Assert.Fail("JumpTacticForTest component not found on Player GameObject");
            jumpTacticAgent.enabled = false;

            pathfinder = player.GetComponent<TargetPathFinder>();
            if (pathfinder == null)
                Assert.Fail("TargetPathFinder component not found on Player GameObject");

            if (pathfinder.GetCurrentPath() == null)
                Debug.Log("Remember to wait for the pathfinder first when running the test");
        }

        #region movement tactics

        protected Action<ExperimentBeliefSet> moveAction;
        protected Action<ExperimentBeliefSet> stopMovementAction;
        protected PrimitiveTactic<ExperimentBeliefSet> startMovementTactic;
        protected PrimitiveTactic<ExperimentBeliefSet> stopMovementTactic;
        protected FirstOfTactic<ExperimentBeliefSet> moveTactic;

        bool hasReachedJumpPoint(ExperimentBeliefSet beliefSet)
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
            startMovementTactic = new(moveAction);

            stopMovementAction = new(beliefSet => inputGenerator.ReleaseMovement());
            stopMovementTactic = new(stopMovementAction, hasReachedJumpPoint);
            moveTactic = new FirstOfTactic<ExperimentBeliefSet>(stopMovementTactic, startMovementTactic);
        }

        #endregion

        #region  jump tactic agent

        protected Action<ExperimentBeliefSet> jumpAction;
        protected Action<ExperimentBeliefSet> stopJumpAction;
        protected PrimitiveTactic<ExperimentBeliefSet> startJumpTactic;
        protected PrimitiveTactic<ExperimentBeliefSet> stopJumpTactic;
        protected FirstOfTactic<ExperimentBeliefSet> jumpTactic;

        bool hasReachedJumpTarget(ExperimentBeliefSet beliefSet)
        {
            if (jumpTacticAgent.ReachedGoal)
                Debug.Log($"Reached jump target: {jumpTacticAgent.goalPosition}");
            return jumpTacticAgent.ReachedGoal;
        }

        protected void InitializeJumpTactics()
        {
            jumpAction = new(
                beliefSet =>
                {
                    if (jumpTacticAgent.enabled)
                        return;
                    Vector2 jumpEndPoint = pathfinder.FindEndOfJumpStartingAtIndex(nextPathNode.Item1 - 1);
                    Debug.Log($"Starting jump to: " + jumpEndPoint + $" from index {nextPathNode.Item1 - 1}");
                    jumpTacticAgent.StartAgent(jumpEndPoint);
                }
            );
            startJumpTactic = new(jumpAction, hasReachedJumpPoint);

            stopJumpAction = new(
                beliefSet =>
                {
                    jumpTacticAgent.StopAgent();
                    nextPathNode = pathfinder.GetNextPathNode();
                }
            );
            stopJumpTactic = new(stopJumpAction, hasReachedJumpTarget);
            jumpTactic = new FirstOfTactic<ExperimentBeliefSet>(stopJumpTactic, startJumpTactic);
        }

        #endregion

        protected FirstOfTactic<ExperimentBeliefSet> moveAndJumpTactic;
        protected Goal<ExperimentBeliefSet> reachedTargetGoal;
        protected PrimitiveGoalStructure<ExperimentBeliefSet> experimentGoalStructure;
        protected DesireSet<ExperimentBeliefSet> desireSet;
        protected BdiAgent<ExperimentBeliefSet> agent;
        protected AplibRunner runner;

        public void SetupExperiment()
        {
            Arrange();
            InitializeMoveTactics();
            InitializeJumpTactics();

            moveAndJumpTactic = new FirstOfTactic<ExperimentBeliefSet>(jumpTactic, moveTactic);

            bool didNotFall()
            {
                GameObject player = beliefSet.Player;
                return player.transform.position.y > -10;
            }

            reachedTargetGoal = new Goal<ExperimentBeliefSet>(
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
            experimentGoalStructure = new PrimitiveGoalStructure<ExperimentBeliefSet>(reachedTargetGoal);
            desireSet = new DesireSet<ExperimentBeliefSet>(experimentGoalStructure);
            agent = new BdiAgent<ExperimentBeliefSet>(beliefSet, desireSet);
            runner = new AplibRunner(agent);
        }

        public IEnumerator PerformExperiment()
        {
            SetupExperiment();
            while (pathfinder.GetCurrentPath() == null)
            {
                Debug.Log("Waiting for path to be calculated...");
                yield return TestWait.ForSeconds(1f);
            }
            nextPathNode = pathfinder.GetNextPathNode();

            // Act
            Assert.GreaterOrEqual(player.transform.position.y, -10, "Player should start above y = -10");
            yield return runner.Test();

            // Assert
            Assert.AreEqual(CompletionStatus.Success, agent.Status);
        }
    }
}
