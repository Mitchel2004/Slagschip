using UnityEngine;
using Utilities.Timer;

public class NavalMineBehaviour : MonoBehaviour
{
    [SerializeField] private float startHeight = -2;
    public Vector2Int position;
    private const float _startTime = 2;
    private Vector3 initialPosition;
    private CountdownTimer _timer;

    private void Awake()
    {
        transform.position = transform.position + (Vector3.up * startHeight);
    }

    public void Initialize(MineData _data)
    {
        initialPosition = _data.worldPosition;
        position = _data.gridPosition;
        _timer = new CountdownTimer(_startTime);
        _timer.onTimerStop += Check;
    }

    public void Update()
    {
        transform.position = Vector3.Lerp(transform.position, initialPosition, _timer.Progress);
    }

    private void Check()
    {

    }
}
