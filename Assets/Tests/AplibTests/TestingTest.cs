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

namespace Tests.AplibTests
{
    public class SimpleTestBeliefSet : BeliefSet
    {
        /// <summary>
        /// The player object in the scene.
        /// </summary>
        public readonly Belief<GameObject, GameObject> Player = 
            new(reference: GameObject.Find("Player"), x => x);

        /// <summary>
        /// The target position that the player needs to move towards.
        /// </summary>
        public readonly Belief<Transform, Vector2> TargetPosition =
            new(GameObject.Find("Target").transform, x => x.position);
    }

    /// <summary>
    /// Simple test to verify that the agent and aplib runner are working.
    /// </summary>
    public class TestingTest
    {
        /// <summary>
        /// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use `yield return null;` to skip a frame.
        /// </summary>
        [UnityTest]
        public IEnumerator PerformTestingTest()
        {
            // Arrange
            // Create a belief set for the agent.
            SimpleTestBeliefSet beliefSet = new();

            // Create an intent for the agent that moves the agent towards the target position.
            Action<SimpleTestBeliefSet> moveTowardsTargetAction = new(
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    const float playerSpeed = 5f;
                    Vector2 playerPosition = player.transform.position;
                    Vector2 targetPosition = beliefSet.TargetPosition;
                    player.transform.position = Vector2.MoveTowards(playerPosition,
                        targetPosition,
                        maxDistanceDelta: playerSpeed * Time.deltaTime
                    );
                }
            );
            PrimitiveTactic<SimpleTestBeliefSet> moveTowardsTargetTactic = new(moveTowardsTargetAction);

            // Create a desire for the agent to reach the target position.
            Goal<SimpleTestBeliefSet> reachTargetGoal = new(
                moveTowardsTargetTactic,
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    Vector2 playerPosition = player.transform.position;
                    Vector2 targetPosition = beliefSet.TargetPosition;
                    return Vector2.Distance(playerPosition, targetPosition) < 0.1f;
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
            Debug.Log("Starting test TestingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [TearDown]
        public void TearDown()
        {
            Debug.Log("Finished test TestingTest");
        }
    }
}
