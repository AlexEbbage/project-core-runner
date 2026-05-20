using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.GameOver
{
    public sealed class GameOverController : UiView
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text xpText;
        [SerializeField] private Text premiumText;
        [SerializeField] private Text messageText;

        public void Show(RunResult result)
        {
            base.Show();
            UiTextBinder.SetText(scoreText, result.Score.ToString("N0"));
            UiTextBinder.SetText(coinsText, result.Coins.ToString("N0"));
            UiTextBinder.SetText(xpText, result.Experience.ToString("N0"));
            UiTextBinder.SetText(premiumText, result.PremiumCurrency.ToString("N0"));
            UiTextBinder.SetText(messageText, "Run complete");
        }

        public void ShowDoubleRewardGranted(RunResult extra)
        {
            UiTextBinder.SetText(messageText, $"Double reward claimed: +{extra.Coins} coins, +{extra.Experience} XP");
        }
    }
}
