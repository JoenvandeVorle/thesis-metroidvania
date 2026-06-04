using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;
using Metroidvania.InputSystem;
using Metroidvania.Characters.Knight;
using UnityEngine.InputSystem;
using System.Collections;

namespace Tests.AplibTests
{
    [RequireComponent(typeof(KnightCharacterController))]
    [RequireComponent(typeof(DecisionRequester))]
    [RequireComponent(typeof(BehaviorParameters))]
    // Behavior is the same as JumpBehaviorLong, but with training code removed
    public class JumpTacticForTest : Agent
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private bool useGridObservations = true;
        [SerializeField] private int observationGridSize = 7;
        [SerializeField] private float observationGridCellRadius = 0.3f;
        [SerializeField] private bool drawObservationGrid = false;
        [SerializeField] private float playerHeight = 0.25f;

        public bool ReachedGoal { get; private set; }
        public Vector3 goalPosition { get; private set; }
        public int goalIndex { get; private set; }
        private int currentMovement = 0; // 0: no movement, 1: left, 2: right
        private bool isHoldingJump = false;
        private float startDistanceToGoal;
        private float[,] observationGrid;
        private InputGenerator inputGenerator = InputGenerator.instance;
        private KnightCharacterController character;

        private void Start()
        {
            inputGenerator.SetKeyboardDisable(true);
            observationGrid = new float[observationGridSize, observationGridSize];
            character = GetComponent<KnightCharacterController>();
        }

        public void StartAgent(System.Tuple<int, Vector2> jumpTarget)
        {
            goalPosition = jumpTarget.Item2;
            goalIndex = jumpTarget.Item1;
            ReachedGoal = false;
            this.enabled = true;
        }

        public void StopAgent()
        {
            enabled = false;
        }

        protected override void OnDisable()
        {
            inputGenerator.ReleaseMovement();
            inputGenerator.ReleaseJump();
            ReachedGoal = false;
            Debug.Log("Disabling JumpTacticForTest, stopping all inputs");
            base.OnDisable();
        }

        public override void OnEpisodeBegin()
        {
            if (goalPosition == null)
                return;
            startDistanceToGoal = Vector2.Distance(transform.position, goalPosition);
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
            Vector2 toGoal = goalPosition - transform.position;
            sensor.AddObservation(toGoal);
            Debug.DrawLine(transform.position, goalPosition, Color.yellow, 0.1f);
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
                        inputGenerator.HoldLeft();
                        currentMovement = 1;
                        break;
                    case 2:
                        inputGenerator.HoldRight();
                        currentMovement = 2;
                        break;
                    default:
                        inputGenerator.ReleaseMovement();
                        currentMovement = 0;
                        break;
                }
            }

            switch (jump)
            {
                case 0 when !isHoldingJump:
                    inputGenerator.ReleaseJump();
                    break;
                case 1 when !isHoldingJump:
                    inputGenerator.HoldJump();
                    break;
                case 2 when !isHoldingJump: // Long Jump
                    StartCoroutine(ReleaseJumpAfterDelay(0.5f));
                    break;
            }

            float distanceToGoal = Vector2.Distance(transform.position, goalPosition);
            if (distanceToGoal <= 1 && character.elapsedGroundTime >= 0.01f) // reached goal
            {
                ReachedGoal = true;
                AddReward(8.0f);
                EndEpisode();
            }
            if (StepCount >= MaxStep - 1) // timeout
            {
                AddReward(-4.0f);
            }

            // Add reward based on distance to goal
            float distanceReward = (startDistanceToGoal - distanceToGoal) / startDistanceToGoal;
            // AddReward(distanceReward > 0 ? distanceReward * 0.1f : distanceReward * 0.01f); // scale it down
            AddReward(distanceReward * 0.1f); // scale it down

            AddReward(-0.0025f); // small step penalty to encourage faster solutions
        }

        // Heuristic method for testing the agent using keyboard controls
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            var movement = InputReader.instance.inputActions.Gameplay.Move.ReadValue<float>(); // this is -1, 0 ,1 instead of 0,1,2
            discreteActionsOut[0] = movement == 0 ? 0 : movement == -1 ? 1 : 2; // convert to 0,1,2
            InputAction jump = InputReader.instance.inputActions.Gameplay.Jump;
            discreteActionsOut[1] = InputReader.instance.inputActions.Gameplay.Attack.IsPressed() ? 2 : jump.IsPressed() ? 1 : 0;
        }

        private IEnumerator ReleaseJumpAfterDelay(float delay)
        {
            inputGenerator.HoldJump();
            isHoldingJump = true;
            yield return new WaitForSeconds(delay);
            inputGenerator.ReleaseJump();
            isHoldingJump = false;
        }
    }
}
