using PlayerGrid;
using ShipAttackers.Data;
using UnityEngine;

namespace ShipAttackers
{
    public abstract class ShipAttackerHandler<T> : MonoBehaviour where T : ShipAttacker
    {
        [SerializeField] protected T attackerPrefab;

        protected T SpawnAttacker()
        {
            return Instantiate(attackerPrefab);
        }

        protected virtual void AttackCell(GridCell _cell)
        {
            T attacker = SpawnAttacker();
            attacker.Initialize(_cell);
        }
    }
}
