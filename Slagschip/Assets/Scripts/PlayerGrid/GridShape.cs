using UnityEngine;

namespace PlayerGrid
{
    [System.Serializable]
    public class GridShape
    {
        public Vector2Int[] offsets;

        public void RotateClockwise()
        {
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = new Vector2Int(offsets[i].y, -offsets[i].x);
            }
        }

        public void RotateCounterClockwise()
        {
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = new Vector2Int(-offsets[i].y, offsets[i].x);
            }
        }
    }
}