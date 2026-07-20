using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PickupTriggerRelay : MonoBehaviour
    {
        [SerializeField] private PickupView owner;

        public void Bind(PickupView pickup)
        {
            owner = pickup;
        }

        private void Awake()
        {
            if (owner == null)
                owner = GetComponentInParent<PickupView>();
        }

        private void OnTriggerEnter(Collider other)
        {
            owner?.TryCollect(other);
        }
    }
}
