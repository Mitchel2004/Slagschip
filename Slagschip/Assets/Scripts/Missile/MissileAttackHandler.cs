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
            GridHandler.instance.onAttacked.AddListener(Test);
        }

        private void Test(Vector3 pos)
        {
            MissileBehaviour m = Instantiate(missileObject, missileData.StartPos, Quaternion.identity);
            m.MissileData = new MissileData(missileData, pos);
        }
    }
}
