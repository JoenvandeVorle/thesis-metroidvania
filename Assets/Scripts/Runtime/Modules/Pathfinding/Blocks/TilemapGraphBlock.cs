using UnityEngine;
using UnityEngine.Tilemaps;

namespace Metroidvania.Pathfinding.Blocks
{
    public class TilemapGraphBlock : GraphBlockBase
    {
        [SerializeField] private Tilemap m_Tilemap;

        public override bool IsBlocked(PathNode node)
        {
            return m_Tilemap.GetTile(m_Tilemap.WorldToCell(node.worldPosition));
        }

        public override BoundsInt GetBounds()
        {
            m_Tilemap.RefreshAllTiles();
            m_Tilemap.CompressBounds();
            return m_Tilemap.cellBounds;
        }

        public override Vector2 GetOffsetInWorldSpace()
        {
            return m_Tilemap.CellToWorld(m_Tilemap.cellBounds.min);
        }
    }
}
