using UnityEngine;

namespace Utilities
{
    public struct CellUnpacker
    {
        public static Vector2Int CellPosition(byte cell)
        {
            float d = cell / 10f;
            int x = Mathf.FloorToInt(d);
            float y = (d - x) * 10;

            return new Vector2Int(x, (int)y);
        }
    }
}
