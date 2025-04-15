using Missile.Data;
using PlayerGrid;
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

        private void AttackCell(Vector3 pos, bool hit)
        {
            MissileBehaviour missile = Instantiate(missileObject, missileData.StartPos, Quaternion.identity);
            missile.MissileData = new MissileData(missileData, pos, hit);
        }
    }
}
