using Missile.Data;
using PlayerGrid;
using Ships;
using UnityEngine;

namespace Missile
{
    public class MissileAttackHandler : MonoBehaviour
    {
        [SerializeField] private MissileData missileData;
        [SerializeField] private MissileBehaviour missileObject;

        private void Start()
        {
            GridHandler.instance.onAttacked.AddListener(AttackCell);
        }

        private void AttackCell(GridCell _cell, bool _hit)
        {
            MissileBehaviour missile = Instantiate(missileObject, missileData.StartPos, Quaternion.identity);
            missile.MissileData = new MissileData(missileData, _cell.worldPosition, _hit);

            ShipBehaviour ship = GridHandler.instance.ShipFromCell(_cell);
            if (ship != null)
            {
                missile.hit.AddListener(ship.FindEffectOnOffset(_cell.position - ship.position).Play);
            }
        }
    }
}
