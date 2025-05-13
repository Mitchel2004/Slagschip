using UnityEngine;

public struct MineData
{
    public Vector2Int gridPosition;
    public Vector3 worldPosition;

    public MineData(Vector2Int _gridPosition, Vector3 _worldPosition)
    {
        gridPosition = _gridPosition;
        worldPosition = _worldPosition;
    }
}
