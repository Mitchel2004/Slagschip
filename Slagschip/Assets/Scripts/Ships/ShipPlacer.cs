using UnityEngine;
using UnityEngine.Events;

namespace Ships
{
    public class ShipPlacer : MonoBehaviour
    {
        [SerializeField] private ShipBehaviour[] ships;

        public bool IsPlacing { get; private set; } = false;
        public UnityEvent DonePlacing { get; private set; } = new UnityEvent();


        public void PlaceShip(EShip ship)
        {
            IsPlacing = true;

            ships[(byte)ship].gameObject.SetActive(true);
            ships[(byte)ship].onPlace.AddListener(StopPlacing);
        }

        private void StopPlacing()
        {
            IsPlacing = false;
            DonePlacing.Invoke();
        }
    }
}
