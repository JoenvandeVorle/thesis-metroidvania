using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using Metroidvania.InputSystem;
using System;

namespace Metroidvania
{
    [RequireComponent(typeof(Rigidbody2D))]
    // A basic agent that moves itself towards the goal position
    public class BasicAgent : Agent
    {
        [SerializeField] private GameObject goal;
        [SerializeField] private int arenaSize = 6;
        [SerializeField] private int fallHeight = -2;
        [SerializeField] private float forceMultiplier = 10; 

        private Rigidbody2D rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public override void OnEpisodeBegin()
        {
            // Set the goals x position randomly within the Arena
            goal.transform.localPosition = new Vector2(Random.Range(-arenaSize,arenaSize),0);

            // Set the Agent position and speed if fallen
            if (this.transform.localPosition.y <= fallHeight)
            {
                this.transform.localPosition = new Vector2(Random.Range(-arenaSize,arenaSize),0);
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(this.transform.localPosition);
            sensor.AddObservation(goal.transform.localPosition);
            sensor.AddObservation(rb.linearVelocityX);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // one action (move x direction)
            float xMovementSignal = actions.ContinuousActions[0];
            rb.AddForce(new Vector2(xMovementSignal, 0) * forceMultiplier);

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
        }

        // Heuristic method for testing the agent using keyboard controls
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = InputReader.instance.MoveValue();
        }
    }
}
