using FX;
using PlayerGrid;
using Ships;
using UnityEngine;

namespace ShipAttackers
{
    using Data;

    public abstract class ShipAttacker : MonoBehaviour
    {
        [SerializeField] protected FXSystem missFX;
        protected AttackerData data;
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
                //missFX.Play();
                return false;
            }
        }
    }
}
