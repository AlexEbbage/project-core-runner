using CoreRacer.Config.Gameplay;
using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerDamageHandler : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private GameBalanceConfigV2 balance;
        private float _lastScrapeTime;

        public void ApplyHeadOnHit()
        {
            health.Damage(health.MaxHealth);
        }

        public void ApplySideScrape()
        {
            var cooldown = balance != null ? balance.SideScrapeCooldown : 0.25f;
            if (UnityEngine.Time.time < _lastScrapeTime + cooldown)
                return;

            _lastScrapeTime = UnityEngine.Time.time;
            var damage = balance != null ? balance.SideScrapeDamage : 1f;
            health.Damage(damage);
        }
    }
}
