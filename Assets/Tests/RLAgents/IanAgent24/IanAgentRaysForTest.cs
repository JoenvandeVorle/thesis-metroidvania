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
    public class IanAgentRaysForTest : Agent
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private bool useGridObservations = false;
        [SerializeField] private int observationGridSize = 14;
        [SerializeField] private float observationGridCellRadius = 0.3f;
        [SerializeField] private bool drawObservationGrid = false;
        [SerializeField] private int fallHeight = -5; // y coord below which the agent is considered to have fallen
        [SerializeField] private float playerHeight = 0.2f;

        public bool ReachedGoal { get; private set; } = false;
        private GameObject goal;
        private Rigidbody2D rb;
        private int currentMovement = 0; // 0: no movement, 1: left, 2: right
        private bool isHoldingJump = false;
        private float startDistanceToGoal;
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

        protected override void OnDisable()
        {
            inputInstance.ReleaseMovement();
            inputInstance.ReleaseJump();
            base.OnDisable();
        }

        public void SetGoal(GameObject newGoal)
        {
            goal = newGoal;
        }

        public override void OnEpisodeBegin()
        {
            // Set the Agent position and speed if fallen
            if (this.transform.position.y <= fallHeight)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            startDistanceToGoal = Vector2.Distance(transform.position, goal.transform.position);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Observe the 14x14=196 grid around the agent, with 0 for empty, 1 for platform
            if (useGridObservations)
            {
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
                            Debug.DrawRay(checkPos, Vector2.up * 0.3f, observationGrid[i, j] == 1f ? Color.green : Color.red, 0.1f);
                    }
                }
            }

            sensor.AddObservation(character.elapsedAirtime);
            sensor.AddObservation(isHoldingJump ? 1f : 0f);
            Vector2 toGoal = goal.transform.position - transform.position;
            sensor.AddObservation(toGoal);
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

            float distanceToGoal = Vector2.Distance(transform.position, goal.transform.position);
            if (distanceToGoal <= 1) // reached goal
            {
                ReachedGoal = true;
                AddReward(8.0f);
                EndEpisode();
            }
            if (transform.position.y <= fallHeight) // fell down
            {
                AddReward(-3.0f);
                EndEpisode();
            }
            if (StepCount >= MaxStep - 1) // timeout
            {
                AddReward(-2.0f);
            }

            // Add reward based on distance to goal
            float distanceReward = (startDistanceToGoal - distanceToGoal) / startDistanceToGoal;
            AddReward(distanceReward * 0.1f); // scale it down

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
    }
}
