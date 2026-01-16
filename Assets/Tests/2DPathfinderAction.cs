using Aplib.Core;
using Aplib.Core.Belief.BeliefSets;
using UnityEngine;
using UnityEngine.AI;

namespace Tests.AplibTests
{
    /// <summary>
    /// A special action, which will use a NavMesh to determine where a given object needs to move towards, and
    /// executes a given action with that information.
    /// Same as the UnityPathfinderAction, but for 2D rigidbodies.
    /// </summary>
    /// <typeparam name="TBeliefSet">The type of belief set this has to work with.</typeparam>
    /// <remarks>This does not move anything, it just calculates required information.</remarks>
    public class UnityPathfinderAction2D<TBeliefSet> : Aplib.Core.Intent.Actions.Action<TBeliefSet>
        where TBeliefSet : IBeliefSet
    {
        /// <summary>
        /// Creates a special action, which will use a NavMesh to determine where a given object needs to move towards, and
        /// executes a given action with that information.
        /// </summary>
        /// <param name="metadata">Helpful metadata to specify what this action is used for</param>
        /// <param name="objectQuery">The function which determines the object to move around.</param>
        /// <param name="location">The function which determines the target destination of the path-finding.</param>
        /// <param name="effect">
        /// Given the belief set and the next position determined by the NavMesh agent, this arbitraty effect will
        /// be aplied with said information.
        /// </param>
        /// <param name="heightOffset">An optional offset to correct for an objects height during pathfinding.</param>
        public UnityPathfinderAction2D(
            Metadata metadata,
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            System.Func<TBeliefSet, Vector2> location,
            System.Action<TBeliefSet, Vector2> effect,
            float heightOffset = 0f)
            : base(metadata, PathfindingAction(objectQuery, location, effect, heightOffset: heightOffset))
        { }

        public UnityPathfinderAction2D(
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            System.Func<TBeliefSet, Vector2> location,
            System.Action<TBeliefSet, Vector2> effect,
            float heightOffset = 0f)
            : this(new Metadata(), objectQuery, location, effect, heightOffset)
        { }

        public UnityPathfinderAction2D(
            Metadata metadata,
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            Vector2 location,
            System.Action<TBeliefSet, Vector2> effect,
            float heightOffset = 0f)
            : this(metadata, objectQuery, location: ConstantLocation(location), effect, heightOffset)
        { }

        public UnityPathfinderAction2D(
            System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            Vector2 location,
            System.Action<TBeliefSet, Vector2> effect,
            float heightOffset = 0f)
            : this(new Metadata(), objectQuery, location: ConstantLocation(location), effect, heightOffset)
        { }


        /// <summary>
        /// Simply wraps a constant location in a function.
        /// </summary>
        /// <param name="location">The location to always return.</param>
        /// <returns>The constant location.</returns>
        protected static System.Func<TBeliefSet, Vector2> ConstantLocation(Vector2 location) => _ => location;

        private static System.Action<TBeliefSet> PathfindingAction(System.Func<TBeliefSet, Rigidbody2D> objectQuery,
            System.Func<TBeliefSet, Vector2> locationQuery,
            System.Action<TBeliefSet, Vector2> effect,
            float speed = 7f,
            float heightOffset = 0f) => beliefSet =>
        {
            Rigidbody2D rigidbody = objectQuery(beliefSet);
            Vector2 target = locationQuery(beliefSet);

            // Navmesh does not work in 2D unity games, so we have to simply move the object directly towards the target.
            Vector2 direction = target - rigidbody.position;
            direction.Normalize();

            Vector2 newPosition = Vector2.MoveTowards(
                rigidbody.position, target, maxDistanceDelta: Time.deltaTime * speed);

            Debug.DrawLine(newPosition, rigidbody.position, Color.blue);

            // Call the effect with the new position and direction
            effect(beliefSet, newPosition);
        };
    }
}