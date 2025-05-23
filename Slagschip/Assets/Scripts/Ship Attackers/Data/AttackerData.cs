using PlayerGrid;
using UnityEngine;

namespace ShipAttackers.Data
{
    public struct AttackerData
    {
        public Vector2Int GridPosition { get; private set; }
        public Vector3 EndPosition { get; set; }
        public Vector3 StartPosition { get; private set; }

        public AttackerData(Vector2Int _gridPosition, Vector3 _startPosition)
        {
            GridPosition = _gridPosition;
            EndPosition = GridHandler.instance.CellWorldPosition(GridPosition);
            StartPosition = _startPosition;
        }
    }
}
