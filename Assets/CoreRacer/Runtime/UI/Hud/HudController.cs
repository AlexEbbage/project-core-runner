using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class HudController : UiView
    {
        [SerializeField] private RunScoreTracker scoreTracker;
        [SerializeField] private RunCurrencyTracker currencyTracker;
        [SerializeField] private RunStatsTrackerV2 statsTracker;
        [SerializeField] private PowerupRuntimeController powerups;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text healthText;
        [SerializeField] private PowerupStripView powerupStrip;

        public Text ScoreText => scoreText;
        public Text DistanceText => distanceText;
        public Text CoinsText => coinsText;
        public Text HealthText => healthText;
        public PowerupStripView PowerupStrip => powerupStrip;

        private void OnEnable()
        {
            if (scoreTracker != null) scoreTracker.ScoreChanged += HandleScoreChanged;
            if (currencyTracker != null) currencyTracker.CoinsChanged += HandleCoinsChanged;
            if (statsTracker != null) statsTracker.DistanceChanged += HandleDistanceChanged;
            if (playerHealth != null) playerHealth.HealthChanged += HandleHealthChanged;
            if (powerups != null)
            {
                powerups.PowerupActivated += HandlePowerupActivated;
                powerups.PowerupExpired += HandlePowerupExpired;
            }

            HandleScoreChanged(scoreTracker != null ? scoreTracker.CurrentScore : 0);
            HandleCoinsChanged(0, currencyTracker != null ? currencyTracker.Coins : 0);
            HandleDistanceChanged(statsTracker != null ? Mathf.FloorToInt(statsTracker.Distance) : 0);
            if (playerHealth != null) HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void OnDisable()
        {
            if (scoreTracker != null) scoreTracker.ScoreChanged -= HandleScoreChanged;
            if (currencyTracker != null) currencyTracker.CoinsChanged -= HandleCoinsChanged;
            if (statsTracker != null) statsTracker.DistanceChanged -= HandleDistanceChanged;
            if (playerHealth != null) playerHealth.HealthChanged -= HandleHealthChanged;
            if (powerups != null)
            {
                powerups.PowerupActivated -= HandlePowerupActivated;
                powerups.PowerupExpired -= HandlePowerupExpired;
            }
            powerupStrip?.Clear();
        }

        private void HandleScoreChanged(int score) => UiTextBinder.SetText(scoreText, $"SCORE  {score:N0}");
        private void HandleDistanceChanged(int distance) => UiTextBinder.SetText(distanceText, $"DIST  {distance:N0} m");
        private void HandleCoinsChanged(int added, int total) => UiTextBinder.SetText(coinsText, $"COINS  {total:N0}");
        private void HandleHealthChanged(float current, float max) => UiTextBinder.SetText(healthText, $"HULL  {current:0}/{max:0}");
        private void HandlePowerupActivated(PowerupType type, float seconds) => powerupStrip?.SetActive(type, seconds);
        private void HandlePowerupExpired(PowerupType type) => powerupStrip?.Remove(type);
    }
}
