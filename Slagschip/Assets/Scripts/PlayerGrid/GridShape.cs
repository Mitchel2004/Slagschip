using System.Linq;
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

        public bool ContainsOffset(Vector2Int _offset)
        {
            return offsets.Contains(_offset);
        }

        public int this[Vector2Int offset]
        {
            get 
            {
                for (int i = 0; i < offsets.Length; ++i)
                {
                    if (offsets[i] == offset)
                        return i;
                }
                throw new System.NullReferenceException();
            }
        }
    }
}