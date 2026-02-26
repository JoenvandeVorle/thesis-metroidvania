using Aplib.Core;
using Aplib.Core.Belief.Beliefs;
using Aplib.Core.Belief.BeliefSets;
using Aplib.Core.Desire.Goals;
using Aplib.Core.Intent.Actions;
using Aplib.Core.Intent.Tactics;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Aplib.Core.Desire.GoalStructures;
using Aplib.Core.Desire.DesireSets;
using Aplib.Core.Agents;
using Aplib.Integrations.Unity;
using JetBrains.Annotations;

namespace Tests.AplibTests
{
    public class MovementTest
    {
        [UnityTest]
        public IEnumerator PerformMovementTest()
        {
            // Arrange
            InputGenerator inputGenerator = InputGenerator.instance;
            SimpleTestBeliefSet beliefSet = new();

            // Create an intent for the agent that moves the agent towards the target position.
            Action<SimpleTestBeliefSet> move = new(
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    Vector2 playerPosition = player.transform.position;
                    Vector2 targetPosition = beliefSet.TargetPosition;
                    Vector2 direction = (targetPosition - playerPosition).normalized;
                    inputGenerator.MoveTowards(direction);
                }
            );

            Action<SimpleTestBeliefSet> stopMovement = new(
                beliefSet => inputGenerator.ReleaseMovement()
            );
    
            PrimitiveTactic<SimpleTestBeliefSet> startMovementTactic = new(move);

            bool hasReachedTargetGuard(SimpleTestBeliefSet beliefSet)
            {
                GameObject player = beliefSet.Player;
                Vector2 playerPosition = player.transform.position;
                Vector2 targetPosition = beliefSet.TargetPosition;
                return Vector2.Distance(playerPosition, targetPosition) < 1f;
            }

            PrimitiveTactic<SimpleTestBeliefSet> stopMovementTactic = new(stopMovement, hasReachedTargetGuard);
            FirstOfTactic<SimpleTestBeliefSet> moveTactic = new(startMovementTactic, stopMovementTactic);

            // Create a desire for the agent to reach the target position.
            Goal<SimpleTestBeliefSet> reachTargetGoal = new(
                moveTactic,
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    Vector2 playerPosition = player.transform.position;
                    Vector2 targetPosition = beliefSet.TargetPosition;
                    return Vector2.Distance(playerPosition, targetPosition) < 1f;
                }
            );
            PrimitiveGoalStructure<SimpleTestBeliefSet> reachTargetGoalStructure = new(reachTargetGoal);
            RepeatGoalStructure<SimpleTestBeliefSet> repeat = new(reachTargetGoalStructure);
            DesireSet<SimpleTestBeliefSet> desireSet = new(repeat);

            // Setup the agent with the belief set and desire set and initialize the test runner.
            BdiAgent<SimpleTestBeliefSet> agent = new(beliefSet, desireSet);
            AplibRunner testRunner = new(agent);

            // Act
            yield return testRunner.Test();

            // Assert
            Assert.AreEqual(CompletionStatus.Success, agent.Status);
        }

        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test MovementTest");
            SceneManager.LoadScene("SimpleTestScene");
        }
    }
}
