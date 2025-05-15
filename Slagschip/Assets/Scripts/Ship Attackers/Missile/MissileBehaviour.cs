using PlayerGrid;
using UnityEngine;

namespace ShipAttackers.Missile
{
    using Data;

    public class MissileBehaviour : ShipAttacker
    {
        [SerializeField] private Vector3 startPosition;
        [SerializeField][Range(0f, 1f)] private float hitDistance = 0.9f;

        [SerializeField] private AnimationCurve heightCurve;
        [SerializeField] private float heightMultiplier;
        [SerializeField] private float speed;

        private Vector3 _normal;
        private float _length;
        private float _time;

        private void Start()
        {
            Calculate2DSpace();
        }

        public override void Initialize(GridCell _cell)
        {
            data = new AttackerData(_cell.position, startPosition);
            AttackPosition = data.GridPosition;
            base.Initialize(_cell);
        }

        private void Calculate2DSpace()
        {
            _normal = (data.EndPosition - data.StartPosition).normalized;
            _length = Mathf.Sqrt(Mathf.Pow(data.EndPosition.x - data.StartPosition.x, 2) + Mathf.Pow(data.EndPosition.z - data.StartPosition.z, 2));
        }

        private void Update()
        {
            Move();

            if (_time >= 1 || (GridHandler.instance.Hit(data.GridPosition) && _time >= hitDistance))
            {
                Attack();
                Destroy(gameObject);
            }

            Rotate();
        }

        private void Move()
        {
            float x = Mathf.Lerp(0, _length, _time);
            float y = heightCurve.Evaluate(_time);
            Vector3 rotated = new Vector3(x * _normal.x, y * heightMultiplier, x * _normal.z);
            transform.position = rotated + data.StartPosition;

            _time += Time.deltaTime * speed;
            _time = Mathf.Clamp01(_time);
        }

        private void Rotate()
        {
            float x1 = Mathf.Lerp(0, _length, _time);
            float y1 = heightCurve.Evaluate(_time);
            Vector3 rotated1 = new Vector3(x1 * _normal.x, y1 * heightMultiplier, x1 * _normal.z);

            Vector3 toTarget = (rotated1 + data.StartPosition - transform.position).normalized;
            Vector3 forward = Vector3.Cross(toTarget, transform.right);
            if (forward == Vector3.zero)
            {
                forward = Vector3.Cross(toTarget, transform.forward);
            }

            Quaternion targetRotation = Quaternion.LookRotation(forward, toTarget);
            transform.rotation = targetRotation;
        }
    }
}
