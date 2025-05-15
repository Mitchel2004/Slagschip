using PlayerGrid;

namespace ShipAttackers.Missile
{
    public class MissileAttackHandler : ShipAttackerHandler<MissileBehaviour>
    {

        private void Start()
        {
            GridHandler.instance.OnAttacked.AddListener(AttackCell);
        }
    }
}
