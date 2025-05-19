using PlayerGrid;
using UnityEngine;
using Utilities.CompassDirections;
using Utilities.Timer;

namespace ShipAttackers.Mine
{
    using Data;

    public class NavalMineBehaviour : ShipAttacker
    {
        [SerializeField] private float startHeight = -2f;
        [SerializeField] private ECompassDirection startDirection = ECompassDirection.West;
        [SerializeField] private float floatCooldown = 2;

        private static bool _dismantled = false;

        private CountdownTimer _timer;

        public override void Initialize(GridCell _cell)
        {
            data = new AttackerData(_cell.position, _cell.worldPosition + (Vector3.up * startHeight));
            base.Initialize(_cell);

            _timer = new CountdownTimer(floatCooldown);
            _timer.onTimerStop += DismantleOrExplode;
            _timer.Start();
        }

        public void Update()
        {
            transform.position = Vector3.Lerp(data.EndPosition, data.StartPosition, _timer.Progress);
        }

        private void DismantleOrExplode()
        {
            AttackPosition = data.GridPosition;
            if (Ship != null && Ship.dismantleMines && !_dismantled)
            {
                _dismantled = true;
                Destroy(this);
                return;
            }

            for (byte i = 0; i < CompassDirections.Directions; i++)
            {
                AttackPosition = data.GridPosition + CompassDirections.DirectionToVector(startDirection);
                if (Ship != null && Ship.dismantleMines && !_dismantled)
                {
                    _dismantled = true;
                    return;
                }
                startDirection = CompassDirections.RotateClockwise(startDirection);
            }

            AttackPosition = data.GridPosition;
            if (Attack())
                return;

            for (byte i = 0; i < CompassDirections.Directions; i++)
            {
                AttackPosition = data.GridPosition + CompassDirections.DirectionToVector(startDirection);
                if (Attack())
                    return;
                startDirection = CompassDirections.RotateClockwise(startDirection);
            }
        }

        protected override bool Attack()
        {
            if (Ship != null && !Ship.IsHitAtPoint(AttackPosition))
            {
                GridHandler.instance.MineCallback(AttackPosition);
                Ship.Hit(AttackPosition);

                //TODO: SplashEffect
                if (AttackPosition != data.GridPosition)
                {
                    GridHandler.instance.MineCallback(data.GridPosition, false);
                }

                Destroy(gameObject);
                return true;
            }
            return false;
        }
    }
}
