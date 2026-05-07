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
    // Based on IanAgentRays
    public class JumpBehaviorLong : Agent
    {
        [SerializeField] private GameObject goals; // parent object containing possible goal positions
        [SerializeField] private GameObject starts; // parent object containing possible start positions
        [SerializeField] private GameObject optionalPlatforms; // contains optional platforms
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private bool useGridObservations = true;
        [SerializeField] private int observationGridSize = 7;
        [SerializeField] private float observationGridCellRadius = 0.3f;
        [SerializeField] private bool drawObservationGrid = false;
        [SerializeField] private bool isTraining = true;
        [SerializeField] private int fallHeight = -5; // y coord below which the agent is considered to have fallen
        [SerializeField] private float playerHeight = 0.25f;

        public GameObject goal;
        private Rigidbody2D rb;
        private GameObject start;
        private int currentMovement = 0; // 0: no movement, 1: left, 2: right
        private bool isHoldingJump = false;
        private float startDistanceToGoal;
        private float[,] observationGrid;
        private InputGenerator inputInstance = InputGenerator.instance;
        private KnightCharacterController character;
        private bool startLeft = true;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            inputInstance.SetKeyboardDisable(true);
            observationGrid = new float[observationGridSize, observationGridSize];
            character = GetComponent<KnightCharacterController>();

            if (isTraining)
            {
                foreach (Transform child in starts.transform)
                    child.gameObject.SetActive(false);

                foreach (Transform child in goals.transform)
                    child.gameObject.SetActive(false);
            }
        }

        public override void OnEpisodeBegin()
        {
            if (isTraining)
            {
                ResetStartAndGoal();

                // Select 1 optional platform and enable it
                int platformIndex = Random.Range(0, optionalPlatforms.transform.childCount);
                for (int i = 0; i < optionalPlatforms.transform.childCount; i++)
                {
                    optionalPlatforms.transform.GetChild(i).gameObject.SetActive(i == platformIndex);
                }

                // Set the Agent position and speed if fallen
                if (this.transform.localPosition.y <= fallHeight)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }

            if (goal == null)
                return;
            startDistanceToGoal = Vector2.Distance(transform.localPosition, goal.transform.localPosition);
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
            Vector2 toGoal = goal.transform.localPosition - transform.localPosition;
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

            switch (jump)
            {
                case 0 when !isHoldingJump:
                    inputInstance.ReleaseJump();
                    break;
                case 1 when !isHoldingJump:
                    inputInstance.HoldJump();
                    break;
                case 2 when !isHoldingJump: // Long Jump
                    StartCoroutine(ReleaseJumpAfterDelay(0.5f));
                    break;
            }

            float distanceToGoal = Vector2.Distance(transform.localPosition, goal.transform.localPosition);
            if (distanceToGoal <= 0.5) // reached goal
            {
                AddReward(8.0f);
                EndEpisode();
            }
            if (transform.localPosition.y <= fallHeight) // fell down
            {
                AddReward(-4.0f);
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

        private void ResetStartAndGoal()
        {
            if (start != null)
                start.SetActive(false);

            if (goal != null)
                goal.SetActive(false);

            bool pickLeft = startLeft;
            start = pickLeft ? starts.transform.GetChild(0).gameObject : starts.transform.GetChild(1).gameObject;
            start.SetActive(true);
            this.transform.localPosition = start.transform.localPosition;

            goal = pickLeft ? goals.transform.GetChild(1).gameObject : goals.transform.GetChild(0).gameObject;
            goal.SetActive(true);

            startLeft = !startLeft;
        }

        private IEnumerator ReleaseJumpAfterDelay(float delay)
        {
            inputInstance.HoldJump();
            isHoldingJump = true;
            yield return new WaitForSeconds(delay);
            inputInstance.ReleaseJump();
            isHoldingJump = false;
        }
    }
}
