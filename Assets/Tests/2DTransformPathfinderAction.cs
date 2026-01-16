using Aplib.Core;
using Aplib.Core.Belief.BeliefSets;
using UnityEngine;

namespace Tests.AplibTests
{
    /// <summary>
    /// A special action which uses a NavMesh to move and rotate a transform towards a target location.
    /// Same as the UnityTransformPathfinderAction, but for 2D rigidbodies.
    /// </summary>
    /// <typeparam name="TBeliefSet">The type of belief set this has to work with.</typeparam>
    public class TransformPathfinderAction2D<TBeliefSet> : UnityPathfinderAction2D<TBeliefSet>
        where TBeliefSet : IBeliefSet
    {
        /// <summary>
        /// Creates a special action which uses a NavMesh to move and rotate a transform towards a target location.
        /// </summary>
        /// <param name="metadata">Helpful metadata to specify what this action is used for</param>
        /// <param name="objectQuery">The function which determines the object to move around.</param>
        /// <param name="location">The function which determines the target destination of the path-finding.</param>
        /// <param name="heightOffset">An optional offset to correct for an objects height during pathfinding.</param>
        public TransformPathfinderAction2D(
            Metadata metadata,
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            System.Func<TBeliefSet, Vector2> location,
            float heightOffset = 0f)
            : base(metadata, objectQuery, location, effect: PathfindingAction(objectQuery), heightOffset)
        { }

        public TransformPathfinderAction2D(
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            System.Func<TBeliefSet, Vector2> location,
            float heightOffset = 0f)
            : this(new Metadata(), objectQuery, location, heightOffset)
        { }

        public TransformPathfinderAction2D(
            Metadata metadata,
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            Vector2 location,
            float heightOffset = 0f)
            : this(metadata, objectQuery, location: ConstantLocation(location), heightOffset)
        { }
    
        public TransformPathfinderAction2D(
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            Vector2 location,
            float heightOffset = 0f)
            : this(new Metadata(), objectQuery, location: ConstantLocation(location), heightOffset)
        { }

        private static System.Action<TBeliefSet, Vector2> PathfindingAction(System.Func<TBeliefSet, Rigidbody2D> objectQuery)
            => (beliefSet, destination) =>
            {
                Rigidbody2D rigidbody = objectQuery(beliefSet);

                // Calculate the new direction
                Vector2 direction = destination - rigidbody.position;
                direction.Normalize();

                rigidbody.position = destination;

                // Flip the sprite based on the direction
                if (direction.x > 0)
                {
                    rigidbody.transform.localScale = new Vector3(1, 1, 1);
                }
                else if (direction.x < 0)
                {
                    rigidbody.transform.localScale = new Vector3(-1, 1, 1);
                }

                // Then rotate to face the direction (currently wrong)
                // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // rigidbody.rotation = angle;
            };
    }
}