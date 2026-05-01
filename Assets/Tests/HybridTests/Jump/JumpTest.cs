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
using Unity.MLAgents.Policies;
using Tests.AplibTests;

namespace Tests.HybridTests
{
    public class HybridJumpBeliefSet : BeliefSet
    {
        public readonly Belief<GameObject, GameObject> Player =
            new(reference: GameObject.Find("Player"), x => x);

        /// <summary>
        /// The list of targets
        /// </summary>
        public readonly Belief<Transform, List<Transform>> Targets =
            new(GameObject.Find("Targets").transform, x => {
                List<Transform> targets = new();
                foreach (Transform child in x)
                    targets.Add(child);
                return targets;
            });

        public readonly Belief<GameObject, GameObject> ClosestTarget = new(
            GameObject.Find("Player"),
            player => {
                Transform targetsParent = GameObject.Find("Targets").transform;
                GameObject closestTarget = null;
                float closestDistance = float.MaxValue;
                foreach (Transform target in targetsParent)
                {
                    if (!target.gameObject.activeSelf)
                        continue;

                    float distance = Vector2.Distance(player.transform.position, target.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = target.gameObject;
                    }
                }
                return closestTarget;
            }
        );

        public readonly Belief<GameObject, JumpBehaviorLong> JumpBehavior =
            new(GameObject.Find("Player"), x => x.GetComponent<JumpBehaviorLong>());
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
            // BehaviorParameters RLBehavior = GameObject.Find("Player").GetComponent<BehaviorParameters>();
            InputGenerator inputGenerator = InputGenerator.instance;
            HybridJumpBeliefSet beliefSet = new();

            // Create an intent for the agent that moves the agent towards the target position.
            Action<HybridJumpBeliefSet> jumpBehavior = new(
                beliefSet =>
                {
                    GameObject closestTarget = beliefSet.ClosestTarget;
                    JumpBehaviorLong jump = beliefSet.JumpBehavior;
                    jump.enabled = true;
                    jump.goal = closestTarget;
                }
            );

            Action<HybridJumpBeliefSet> stopMovement = new(
                beliefSet =>
                {
                    JumpBehaviorLong jump = beliefSet.JumpBehavior;
                    jump.enabled = false;
                    GameObject closestTarget = beliefSet.ClosestTarget;
                    closestTarget.SetActive(false);
                    Debug.Log("Disabled target: " + closestTarget.name);
                }
            );

            PrimitiveTactic<HybridJumpBeliefSet> startMovementTactic = new(jumpBehavior);

            bool hasReachedTargetGuard(HybridJumpBeliefSet beliefSet)
            {
                GameObject player = beliefSet.Player;
                Vector2 playerPosition = player.transform.position;
                GameObject closestTarget = beliefSet.ClosestTarget;
                Vector2 targetPosition = closestTarget.transform.position;
                return Vector2.Distance(playerPosition, targetPosition) < 1f;
            }

            PrimitiveTactic<HybridJumpBeliefSet> stopMovementTactic = new(stopMovement, hasReachedTargetGuard);
            FirstOfTactic<HybridJumpBeliefSet> moveTactic = new(stopMovementTactic, startMovementTactic);

            // Once all targets are disabled, the agent has succeeded.
            Goal<HybridJumpBeliefSet> reachAllTargetsGoal = new(
                moveTactic,
                beliefSet =>
                {
                    List<Transform> targets = beliefSet.Targets;
                    foreach (Transform target in targets)
                    {
                        if (target.gameObject.activeSelf)
                            return false;
                    }
                    return true;
                }
            );
            PrimitiveGoalStructure<HybridJumpBeliefSet> reachTargetGoalStructure = new(reachAllTargetsGoal);
            DesireSet<HybridJumpBeliefSet> desireSet = new(reachTargetGoalStructure);

            // Setup the agent with the belief set and desire set and initialize the test runner.
            BdiAgent<HybridJumpBeliefSet> agent = new(beliefSet, desireSet);
            AplibRunner testRunner = new(agent);

            // Act
            yield return testRunner.Test();

            // Assert
            Assert.AreEqual(CompletionStatus.Success, agent.Status);
        }

    }
}
