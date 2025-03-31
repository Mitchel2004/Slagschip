using UnityEngine;

namespace PlayerGrid
{
    public class GridCell
    {
        public Vector2Int position;

        public GridCell(Vector2Int position)
        {
            this.position = position;
        }
    }
}
