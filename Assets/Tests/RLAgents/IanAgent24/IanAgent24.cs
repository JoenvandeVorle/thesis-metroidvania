using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;
using Metroidvania.InputSystem;
using Metroidvania.Characters.Knight;
using System;

namespace Tests.AplibTests
{
    [RequireComponent(typeof(KnightCharacterController))]
    [RequireComponent(typeof(DecisionRequester))]
    [RequireComponent(typeof(BehaviorParameters))]
    // Agent based on Ian's "Train24" agent
    // Don't forget to create a IanAgent24.yaml config file
    public class IanAgent24 : Agent
    {
        [SerializeField] private GameObject goals; // parent object containing possible goal positions
        [SerializeField] private GameObject starts; // parent object containing possible start positions
        [SerializeField] private GameObject optionalPlatforms; // contains optional platforms
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private int observationGridSize = 14;
        [SerializeField] private float observationGridCellRadius= 0.3f;
        [SerializeField] private bool drawObservationGrid = false;
        [SerializeField] private int fallHeight = -5; // y coord below which the agent is considered to have fallen
        [SerializeField] private float playerHeight = 0.2f;

        private Rigidbody2D rb;
        private GameObject start;
        private GameObject goal;
        private int currentMovement = 0; // 0: no movement, 1: left, 2: right
        private bool isHoldingJump = false;
        private float prevDistanceToGoal;
        private float[,] observationGrid;
        private InputGenerator inputInstance = InputGenerator.instance;
        private KnightCharacterController character;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            inputInstance.SetKeyboardDisable(true);
            observationGrid = new float[observationGridSize, observationGridSize];
            character = GetComponent<KnightCharacterController>();
        }

        public override void OnEpisodeBegin()
        {
            ResetStartAndGoal();

            // Enable/disable optional platforms
            foreach (Transform platform in optionalPlatforms.transform)
            {
                platform.gameObject.SetActive(Random.value > 0.5f);
            }

            // Set the Agent position and speed if fallen
            if (this.transform.localPosition.y <= fallHeight)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            prevDistanceToGoal = Vector2.Distance(transform.localPosition, goal.transform.localPosition);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Observe the 14x14=196 grid around the agent, with 0 for empty, 1 for platform
            for (int i = 0; i < observationGridSize; i++)
            {
                for (int j = 0; j < observationGridSize; j++)
                {
                    Vector2 checkPos = new Vector2(
                        transform.position.x + (i - observationGridSize / 2),
                        transform.position.y + (j - observationGridSize / 2) - playerHeight
                    );
                    Collider2D hit = Physics2D.OverlapCircle(checkPos, observationGridCellRadius, groundLayer);
                    sensor.AddObservation(observationGrid[i, j]);
                    observationGrid[i, j] = hit != null ? 1f : 0f;
                    if (drawObservationGrid)
                        Debug.DrawRay(checkPos, Vector2.up * 0.2f, observationGrid[i, j] == 1f ? Color.green : Color.red, 0.1f);
                }
            }

            sensor.AddObservation(character.elapsedAirtime);
            sensor.AddObservation(isHoldingJump ? 1f : 0f);
            Vector2 toGoal = goal.transform.localPosition - transform.localPosition;
            sensor.AddObservation(toGoal);
            // Debug.Log($"Distance to goal: {toGoal}");
            Debug.DrawLine(transform.position, goal.transform.position, Color.yellow, 0.1f);
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

            if (jump == 1 && !isHoldingJump)
            {
                inputInstance.HoldJump();
                isHoldingJump = true;
            }
            else if (jump == 0 && isHoldingJump)
            {
                inputInstance.ReleaseJump();
                isHoldingJump = false;
            }

            float distanceToGoal = Vector2.Distance(transform.localPosition, goal.transform.localPosition);
            if (distanceToGoal <= 1) // reached goal
            {
                SetReward(5.0f);
                EndEpisode();
            }
            if (transform.localPosition.y <= fallHeight) // fell down
            {
                SetReward(-10.0f);
                EndEpisode();
            }

            AddReward(-0.0025f); // small step penalty to encourage faster solutions
        }

        // Heuristic method for testing the agent using keyboard controls
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            var movement = InputReader.instance.inputActions.Gameplay.Move.ReadValue<float>(); // this is -1, 0 ,1 instead of 0,1,2
            discreteActionsOut[0] = movement == 0 ? 0 : movement == -1 ? 1 : 2; // convert to 0,1,2
            discreteActionsOut[1] = InputReader.instance.inputActions.Gameplay.Jump.IsPressed() ? 1 : 0;
        }

        private GameObject GetRandomChild(GameObject parent)
        {
            int childCount = parent.transform.childCount;
            if (childCount == 0) return null;
            int randomIndex = Random.Range(0, childCount);
            return parent.transform.GetChild(randomIndex).gameObject;
        }

        private void ResetStartAndGoal()
        {
            if (start != null)
                start.SetActive(false);

            if (goal != null)
                goal.SetActive(false);

            start = GetRandomChild(starts);
            start.SetActive(true);
            this.transform.localPosition = start.transform.localPosition;

            goal = GetRandomChild(goals);
            goal.SetActive(true);
        }
    }
}
