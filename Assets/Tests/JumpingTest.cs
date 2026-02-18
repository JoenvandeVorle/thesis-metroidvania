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
using UnityEngine.InputSystem;

namespace Tests.AplibTests
{
    /// <summary>
    /// Simple test to verify that we can access the player's jumping ability.
    /// </summary>
    public class InputSystemTestJump : InputTestFixture
    {
        // We cannot use the NUnit setup methods because the InputTestFixture already uses them.
        // Instead, we override the SetUp method.
        public override void Setup()
        {
            base.Setup();
            Debug.Log("Starting test InputSystemTestJump");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        public IEnumerator PerformJumpingTest()
        {
            // CURERNTLY NOT WORKING

            // Arrange
            SimpleTestBeliefSet beliefSet = new();
            var keyboard = InputSystem.AddDevice<Keyboard>();

            Action<SimpleTestBeliefSet> jump = new(
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    Debug.Log("Simulating jump input");
                    Press(keyboard.zKey);
                }
            );

            // Press again to test
            Debug.Log("Pressing attack key");
            Press(keyboard.xKey);
    
            PrimitiveTactic<SimpleTestBeliefSet> jumpTactic = new(jump);

            // Create a desire for the agent to reach the target position using jumping.
            Goal<SimpleTestBeliefSet> jumpGoal = new(
                jumpTactic,
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    Vector2 playerPosition = player.transform.position;
                    Vector2 targetPosition = beliefSet.TargetPosition;
                    return Vector2.Distance(playerPosition, targetPosition) < 5f;
                }
            );
            PrimitiveGoalStructure<SimpleTestBeliefSet> reachTargetGoalStructure = new(jumpGoal);
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

    }
}
