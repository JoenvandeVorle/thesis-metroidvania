using Metroidvania.Characters.Knight;
using Metroidvania.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Tests.AplibTests
{
    [RequireComponent(typeof(KnightCharacterController))]
    [RequireComponent(typeof(DecisionRequester))]
    [RequireComponent(typeof(BehaviorParameters))]
    [RequireComponent(typeof(Rigidbody2D))]

    public abstract class HighJumpTacticBase : Agent
    {
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] protected bool useGridObservations = false;
        [SerializeField] protected int observationGridSize = 14;
        [SerializeField] protected float observationGridCellRadius = 0.3f;
        [SerializeField] protected bool drawObservationGrid = false;
        [SerializeField] protected int fallHeight = -5; // y coord below which the agent is considered to have fallen
        [SerializeField] protected float playerHeight = 0.2f;

        protected Vector3 goal;
        protected Rigidbody2D rb;
        protected GameObject start;
        protected int currentMovement = 0; // 0: no movement, 1: left, 2: right
        protected bool isHoldingJump = false;
        protected float startDistanceToGoal;
        protected float[,] observationGrid;
        protected InputGenerator inputInstance = InputGenerator.instance;
        protected KnightCharacterController character;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            inputInstance.SetKeyboardDisable(true);
            observationGrid = new float[observationGridSize, observationGridSize];
            character = GetComponent<KnightCharacterController>();
        }

        public override void OnEpisodeBegin()
        {
            DoOnEpisodeBegin();

            startDistanceToGoal = Vector2.Distance(transform.position, goal);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
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
            Vector2 toGoal = goal - transform.position;
            sensor.AddObservation(toGoal);
            Debug.DrawLine(transform.position, goal, Color.yellow, 0.1f);
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

            float distanceToGoal = Vector2.Distance(transform.position, goal);
            if (distanceToGoal <= 0.5) // reached goal
            {
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

        protected abstract void DoOnEpisodeBegin();
    }
}
