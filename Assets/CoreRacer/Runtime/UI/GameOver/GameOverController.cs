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
        [SerializeField] private GameObject finalResultRoot;
        [SerializeField] private GameObject continuePromptRoot;

        public void Show(RunResult result)
        {
            base.Show();
            SetMode(finalResult: true);
            UiTextBinder.SetText(scoreText, result.Score.ToString("N0"));
            UiTextBinder.SetText(coinsText, result.Coins.ToString("N0"));
            UiTextBinder.SetText(xpText, result.Experience.ToString("N0"));
            UiTextBinder.SetText(premiumText, result.PremiumCurrency.ToString("N0"));
            UiTextBinder.SetText(messageText, "Run complete");
        }

        public void ShowContinueOffer()
        {
            base.Show();
            SetMode(finalResult: false);
            UiTextBinder.SetText(scoreText, string.Empty);
            UiTextBinder.SetText(coinsText, string.Empty);
            UiTextBinder.SetText(xpText, string.Empty);
            UiTextBinder.SetText(premiumText, string.Empty);
            UiTextBinder.SetText(messageText, "Continue?");
        }

        public void ShowDoubleRewardGranted(RunResult extra)
        {
            UiTextBinder.SetText(messageText, $"Double reward claimed: +{extra.Coins} coins, +{extra.Experience} XP");
        }

        private void SetMode(bool finalResult)
        {
            if (finalResultRoot != null) finalResultRoot.SetActive(finalResult);
            if (continuePromptRoot != null) continuePromptRoot.SetActive(!finalResult);
        }
    }
}
