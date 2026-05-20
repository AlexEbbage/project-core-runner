using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerRespawnController : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerController controller;
        [SerializeField] private float invulnerabilitySeconds = 2f;
        [SerializeField] private float backwardsOffset = 4f;

        private void Awake()
        {
            if (health == null) health = GetComponent<PlayerHealth>();
            if (controller == null) controller = GetComponent<PlayerController>();
        }

        public void RespawnAt(float z)
        {
            var position = transform.position;
            position.z = z - backwardsOffset;
            transform.position = position;
            if (health != null) health.Revive(invulnerabilitySeconds);
            if (controller != null) controller.BeginRun();
        }
    }
}
