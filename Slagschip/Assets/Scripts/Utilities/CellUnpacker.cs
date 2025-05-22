using Unity.Networking.Transport.Error;
using UnityEngine;

namespace Utilities
{
    public struct CellUnpacker
    {
        public static Vector2Int CellPosition(byte cell)
        {
            if (cell >= 100)
                cell -= 100;

            float d = cell / 10f;
            int x = Mathf.FloorToInt(d);
            float y = (d - x) * 10;

            return new Vector2Int(x, (int)y);
        }

        public static byte PackCell(Vector2Int position)
        {
            int x = position.x * 10;

            return (byte)(x + position.y);
        }
    }
}
