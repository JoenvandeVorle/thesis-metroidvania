using UnityEngine;

namespace Metroidvania.Pathfinding.Blocks
{
    public abstract class GraphBlockBase : MonoBehaviour
    {
        private Pathfinder _Pathfinder;
        public Pathfinder pathfinder => _Pathfinder;

        private void Awake()
        {
            _Pathfinder = GetComponentInParent<Pathfinder>();
        }

        public abstract bool IsBlocked(PathNode node);

        public abstract BoundsInt GetBounds();

        public abstract Vector2 GetOffsetInWorldSpace();

        public override string ToString()
        {
            return $"{name} (offset: {GetBounds().min}, size: {GetBounds().size})";
        }
    }
}
