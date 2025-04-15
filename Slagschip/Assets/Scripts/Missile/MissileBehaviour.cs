using Missile.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Missile
{
    public class MissileBehaviour : MonoBehaviour
    {
        [SerializeField][Range(0f, 1f)] private float hitDistance = 0.9f;

        public UnityEvent hit;

        private Vector3 _normal;
        private float _length;
        private float _time;

        private MissileData _data;

        public MissileData MissileData
        { 
            private get => _data; 
            set => _data = value; 
        }

        private void Start()
        {
            Calculate2DSpace();
        }

        private void Calculate2DSpace()
        {
            _normal = (_data.EndPos - _data.StartPos).normalized;
            _length = Mathf.Sqrt(Mathf.Pow(_data.EndPos.x - _data.StartPos.x, 2) + Mathf.Pow(_data.EndPos.z - _data.StartPos.z, 2));
        }

        private void Update()
        {
            Move();

            if (_time >= 1 || (_data.hit && _time >= hitDistance))
            {
                hit.Invoke();
                Destroy(gameObject);
            }

            Rotate();
        }

        private void Move()
        {
            float x = Mathf.Lerp(0, _length, _time);
            float y = _data.heightCurve.Evaluate(_time);
            Vector3 rotated = new Vector3(x * _normal.x, y * _data.heightMultiplier, x * _normal.z);
            transform.position = rotated + _data.StartPos;

            _time += Time.deltaTime * _data.speed;
            _time = Mathf.Clamp01(_time);
        }

        private void Rotate()
        {
            float x1 = Mathf.Lerp(0, _length, _time);
            float y1 = _data.heightCurve.Evaluate(_time);
            Vector3 rotated1 = new Vector3(x1 * _normal.x, y1 * _data.heightMultiplier, x1 * _normal.z);

            Vector3 toTarget = (rotated1 + _data.StartPos - transform.position).normalized;
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
