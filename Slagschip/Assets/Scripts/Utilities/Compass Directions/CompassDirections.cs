using UnityEngine;
using UnityEngine.UIElements;

public struct CompassDirections
{
    public static Vector2Int DirectionToVector(ECompassDirection direction)
    {
        int index = (byte)direction;
        float angleDegrees = index * 45f;
        float y = Mathf.Cos(angleDegrees * Mathf.Deg2Rad);
        float x = Mathf.Sin(angleDegrees * Mathf.Deg2Rad);

        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    public static ECompassDirection VectorToDirection(Vector2Int direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int index = Mathf.RoundToInt((angle + 360) % 360 / 45);

        return (ECompassDirection)index;
    }

    public static  RotateClockwise()
    {
    }
}
