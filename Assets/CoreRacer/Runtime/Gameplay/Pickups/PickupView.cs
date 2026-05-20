using System;
using CoreRacer.Common.Pooling;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PickupView : PoolableBehaviour
    {
        public PickupType Type;
        public PowerupType PowerupType;
        public int Amount = 1;
        public event Action<PickupView> Collected;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Collected?.Invoke(this);
        }
    }
}
