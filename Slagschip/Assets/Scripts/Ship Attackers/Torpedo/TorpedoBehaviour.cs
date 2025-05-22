using PlayerGrid;
using UnityEngine;

namespace ShipAttackers.Torpedo
{
    using Data;
    using UnityEngine.UIElements;
    using Utilities.CompassDirections;

    public class TorpedoBehaviour : ShipAttacker
    {
        public ECompassDirection Direction { private get; set; }
        public float GridOffset;

        [SerializeField] private float speed;

        private Vector3 _direction;
        private float _time;
        private Vector2Int _position;
        private bool _canAttack = true;

        public override void Initialize(GridCell _cell)
        {
            data = new AttackerData(_cell.position, _cell.worldPosition);

            base.Initialize(_cell);
        }
        
        private void Start()
        {
            _direction = Direction switch
            {
                ECompassDirection.North => Vector3.back,
                ECompassDirection.East => Vector3.left,
                ECompassDirection.South => Vector3.forward,
                ECompassDirection.West => Vector3.right,
                _ => Vector3.zero
            };

            transform.position = data.StartPosition + _direction * GridOffset;
        }

        private void Update()
        {
            if (_time < 1 && !GridHandler.instance.Hit(_position))
            {
                Move();
            }
            else if (_canAttack)
            {
                _canAttack = false;
                AttackPosition = _position;
                Attack();
            }
        }

        private void Move()
        {
            float _progress = Mathf.Lerp(0, GridOffset * 2 + 10, _time);

            transform.position = data.StartPosition - _direction * _progress;

            Vector3 _worldPosition = transform.position + _direction * GridOffset;
            _position = new Vector2Int(Mathf.FloorToInt(_worldPosition.x), Mathf.FloorToInt(_worldPosition.z));

            _time += Time.deltaTime * speed;
            _time = Mathf.Clamp01(_time);
        }

        protected override bool Attack()
        {
            if (Ship != null)
            {
                GridHandler.instance.TorpedoCallback(AttackPosition);
                Ship.Hit(AttackPosition);
                Destroy(gameObject);

                return true;
            }

            GridHandler.instance.TorpedoCallback(AttackPosition, false);

            Destroy(gameObject);

            return false;
        }
    }
}
