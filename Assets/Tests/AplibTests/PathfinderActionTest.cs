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
    /// <summary>
    /// Simple test to verify that the TransformPathfinderAction2D is working. 
    /// </summary>
    public class PathfinderActionTest
    {
        [UnityTest]
        public IEnumerator PerformPathfinderActionTest()
        {
            // Arrange
            SimpleTestBeliefSet beliefSet = new();

            // This action cannot move upwards! It is for walking, not jumping.
            TransformPathfinderAction2D<SimpleTestBeliefSet> move = new(
                beliefSet =>
                {
                    GameObject player = beliefSet.Player;
                    return player.GetComponent<Rigidbody2D>();
                },
                beliefSet => beliefSet.TargetPosition,
                1f
            );
    
            PrimitiveTactic<SimpleTestBeliefSet> moveTowardsTargetTactic = new(move);

            // Create a desire for the agent to reach the target position.
            Goal<SimpleTestBeliefSet> reachTargetGoal = new(
                moveTowardsTargetTactic,
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
            Debug.Log("Starting test PathfinderActionTest");
            SceneManager.LoadScene("SimpleTestScene");
        }
    }
}
