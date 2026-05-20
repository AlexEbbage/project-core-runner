using CoreRacer.Gameplay.Player;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PickupMagnetController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float radius = 3f;
        [SerializeField] private float pullSpeed = 12f;
        [SerializeField] private LayerMask pickupMask = ~0;
        private readonly Collider[] _hits = new Collider[64];
        private float _radiusMultiplier = 1f;

        public void SetRadiusMultiplier(float multiplier)
        {
            _radiusMultiplier = Mathf.Max(1f, multiplier);
        }

        private void Awake()
        {
            if (target == null)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null) target = player.transform;
            }
        }

        private void Update()
        {
            if (target == null)
                return;

            var count = Physics.OverlapSphereNonAlloc(target.position, radius * _radiusMultiplier, _hits, pickupMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                var pickup = _hits[i] != null ? _hits[i].GetComponentInParent<PickupView>() : null;
                if (pickup == null) continue;
                pickup.transform.position = Vector3.MoveTowards(pickup.transform.position, target.position, pullSpeed * UnityEngine.Time.deltaTime);
            }
        }
    }
}
