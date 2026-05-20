using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerCollisionHandler : MonoBehaviour
    {
        [SerializeField] private PlayerDamageHandler damageHandler;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle"))
                damageHandler.ApplyHeadOnHit();
            else if (other.CompareTag("ScrapeObstacle"))
                damageHandler.ApplySideScrape();
        }
    }
}
