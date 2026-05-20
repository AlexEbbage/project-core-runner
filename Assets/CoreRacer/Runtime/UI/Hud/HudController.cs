using CoreRacer.Gameplay.Player;
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
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text healthText;

        private void OnEnable()
        {
            if (scoreTracker != null) scoreTracker.ScoreChanged += HandleScoreChanged;
            if (currencyTracker != null) currencyTracker.CoinsChanged += HandleCoinsChanged;
            if (playerHealth != null) playerHealth.HealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            if (scoreTracker != null) scoreTracker.ScoreChanged -= HandleScoreChanged;
            if (currencyTracker != null) currencyTracker.CoinsChanged -= HandleCoinsChanged;
            if (playerHealth != null) playerHealth.HealthChanged -= HandleHealthChanged;
        }

        private void HandleScoreChanged(int score) => UiTextBinder.SetText(scoreText, score.ToString("N0"));
        private void HandleCoinsChanged(int added, int total) => UiTextBinder.SetText(coinsText, total.ToString("N0"));
        private void HandleHealthChanged(float current, float max) => UiTextBinder.SetText(healthText, $"{current:0}/{max:0}");
    }
}
