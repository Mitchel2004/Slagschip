using UnityEngine;

namespace PlayerGrid
{
    public class GridCell
    {
        public Vector2Int position;
        public Vector3 worldPosition;
        public bool isTaken;

        public GridCell(Vector2Int _position, Vector3 _worldPosition)
        {
            position = _position;
            worldPosition = _worldPosition;
        }
    }
}
