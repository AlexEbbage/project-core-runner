using System;
using CoreRacer.Common.Pooling;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PickupView : PoolableBehaviour
    {
        [SerializeField] private Transform radialBody;

        public PickupType Type;
        public PowerupType PowerupType;
        public int Amount = 1;
        public Transform RadialBody => radialBody;
        public Vector3 WorldPosition => radialBody != null ? radialBody.position : transform.position;
        public event Action<PickupView> Collected;

        private void OnTriggerEnter(Collider other)
        {
            TryCollect(other);
        }

        public void PlaceAtWorldPosition(Vector3 position)
        {
            if (radialBody == null)
            {
                transform.SetPositionAndRotation(position, Quaternion.identity);
                return;
            }

            var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
            var radius = new Vector2(position.x, position.y).magnitude;
            transform.SetPositionAndRotation(new Vector3(0f, 0f, position.z), Quaternion.Euler(0f, 0f, angle));
            radialBody.localPosition = new Vector3(radius, 0f, 0f);
        }

        public void TryCollect(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Collected?.Invoke(this);
        }
    }
}
