using PlayerGrid;
using Utilities.CompassDirections;

namespace ShipAttackers.Torpedo
{
    public class TorpedoHandler : ShipAttackerHandler<TorpedoBehaviour>
    {
        private ECompassDirection _direction;

        private void Awake()
        {
            GridHandler.instance.OnTorpedoFire.AddListener(AttackLine);
        }

        private void AttackLine(GridCell _cell, ECompassDirection _direction)
        {
            this._direction = _direction;
            AttackCell(_cell);
        }

        protected override void AttackCell(GridCell _cell)
        {
            TorpedoBehaviour attacker = SpawnAttacker();
            attacker.Initialize(_cell);

            attacker.Direction = _direction;
        }
    }
}
