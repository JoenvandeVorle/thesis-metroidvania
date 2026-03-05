using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using Metroidvania.InputSystem;
using System;

namespace Tests.AplibTests
{
    [RequireComponent(typeof(Rigidbody2D))]
    // A basic agent that moves itself towards the goal position
    public class AgentAsTactic : Agent
    {
        [SerializeField] private GameObject goal;
        [SerializeField] private int arenaSize = 14;
        [SerializeField] private int minDistanceFromSpawn = 5;
        [SerializeField] private int fallHeight = -5;

        private Rigidbody2D rb;

        private int currentMovement = 0; // 0: no movement, 1: left, 2: right
        private bool isJumping;
        private float prevDistanceToGoal;
        private InputGenerator inputInstance = InputGenerator.instance;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            inputInstance.SetKeyboardDisable(true);
        }

        private float getRandomPos()
        {
            float pos = Random.Range(-arenaSize + minDistanceFromSpawn, arenaSize - minDistanceFromSpawn);
            pos = pos < 0 ? pos - minDistanceFromSpawn : pos + minDistanceFromSpawn;
            return pos;
        }

        public override void OnEpisodeBegin()
        {
            // Set the goals x position randomly within the Arena
            goal.transform.localPosition = new Vector2(getRandomPos(), 1.5f);

            // Set the Agent position and speed if fallen
            if (this.transform.localPosition.y <= fallHeight)
            {
                this.transform.localPosition = new Vector2(getRandomPos(),0);
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            prevDistanceToGoal = Vector2.Distance(transform.localPosition, goal.transform.localPosition);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(this.transform.localPosition);
            sensor.AddObservation(goal.transform.localPosition);
            // sensor.AddObservation(rb.linearVelocity);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            int movement = actions.DiscreteActions[0];
            int jump = actions.DiscreteActions[1];

            if (currentMovement != movement)
            {
                switch (movement)
                {
                    case 1:
                        inputInstance.HoldLeft();
                        currentMovement = 1;
                        break;
                    case 2:
                        inputInstance.HoldRight();
                        currentMovement = 2;
                        break;
                    default:
                        inputInstance.ReleaseMovement();
                        currentMovement = 0;
                        break;
                }
            }

            if (jump == 1 && !isJumping)
            {
                inputInstance.HoldJump();
                isJumping = true;
            }
            else if (jump == 0 && isJumping)
            {
                inputInstance.ReleaseJump();
                isJumping = false;
            }

            float distanceToGoal = Vector2.Distance(transform.localPosition, goal.transform.localPosition);
            if (distanceToGoal <= 1) // reached goal
            {
                SetReward(1.0f);
                EndEpisode();
            }
            if (transform.localPosition.y <= fallHeight) // fell down
            {
                SetReward(-1.0f);
                EndEpisode();
            }

            if (distanceToGoal < prevDistanceToGoal)
                AddReward(0.01f);
            else
                AddReward(-0.01f);
            prevDistanceToGoal = distanceToGoal;
        }

        // Heuristic method for testing the agent using keyboard controls
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // var continuousActionsOut = actionsOut.ContinuousActions;
            var discreteActionsOut = actionsOut.DiscreteActions;
            var movement = InputReader.instance.inputActions.Gameplay.Move.ReadValue<float>(); // this is -1, 0 ,1 instead of 0,1,2
            discreteActionsOut[0] = movement == 0 ? 0 : movement == -1 ? 1 : 2; // convert to 0,1,2
            discreteActionsOut[1] = InputReader.instance.inputActions.Gameplay.Jump.IsPressed() ? 1 : 0;
        }
    }
}
