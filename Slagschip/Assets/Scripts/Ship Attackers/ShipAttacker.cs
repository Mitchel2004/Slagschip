using FX;
using PlayerGrid;
using Ships;
using UnityEngine;

namespace ShipAttackers
{
    using Data;

    public abstract class ShipAttacker : MonoBehaviour
    {
        [SerializeField] private bool useMissFX;
        [SerializeField] protected FXSystem missFxPrefab;

        protected AttackerData data;

        private FXSystem _missFX;

        public Vector2Int AttackPosition { get; set; }

        public ShipBehaviour Ship 
        {  
            get
            {
                return GridHandler.instance.ShipFromPosition(AttackPosition);
            }
        }

        public virtual void Initialize(GridCell _cell)
        {
            transform.position = data.StartPosition;

            if (useMissFX)
                _missFX = Instantiate(missFxPrefab, data.EndPosition, Quaternion.identity);
        }

        protected void Miss()
        {
            _missFX.Play();
        }

        protected virtual bool Attack()
        {
            if (Ship != null)
            {
                Ship.Hit(AttackPosition);
                return true;
            }
            else
            {
                Miss();
                return false;
            }
        }
    }
}
