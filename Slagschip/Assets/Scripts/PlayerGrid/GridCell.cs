using UnityEngine;

namespace PlayerGrid
{
    public class GridCell
    {
        public Vector2Int position;
        public bool isTaken;

        public GridCell(Vector2Int position)
        {
            this.position = position;
        }
    }
}
