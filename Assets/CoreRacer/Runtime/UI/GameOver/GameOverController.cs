using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.GameOver
{
    public sealed class GameOverController : UiView
    {
        [SerializeField] private RunController runController;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text xpText;
        [SerializeField] private Text premiumText;
        [SerializeField] private Text messageText;
        [SerializeField] private GameObject finalResultRoot;
        [SerializeField] private GameObject continuePromptRoot;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button hubButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button declineContinueButton;
        [SerializeField] private Button doubleRewardsButton;

        private bool _doubleRewardsClaimed;
        private bool _doubleRewardPending;
        private bool _continuePending;

        private void Awake()
        {
            if (runController == null)
                runController = FindObjectOfType<RunController>();
            ResolveButtons();
        }

        private void OnEnable()
        {
            if (retryButton != null) retryButton.onClick.AddListener(Retry);
            if (hubButton != null) hubButton.onClick.AddListener(ReturnToHub);
            if (continueButton != null) continueButton.onClick.AddListener(ContinueRun);
            if (declineContinueButton != null) declineContinueButton.onClick.AddListener(DeclineContinue);
            if (doubleRewardsButton != null) doubleRewardsButton.onClick.AddListener(DoubleRewards);
        }

        private void OnDisable()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(Retry);
            if (hubButton != null) hubButton.onClick.RemoveListener(ReturnToHub);
            if (continueButton != null) continueButton.onClick.RemoveListener(ContinueRun);
            if (declineContinueButton != null) declineContinueButton.onClick.RemoveListener(DeclineContinue);
            if (doubleRewardsButton != null) doubleRewardsButton.onClick.RemoveListener(DoubleRewards);
        }

        public void Show(RunResult result)
        {
            base.Show();
            _doubleRewardsClaimed = false;
            _doubleRewardPending = false;
            _continuePending = false;
            SetMode(finalResult: true);
            UiTextBinder.SetText(scoreText, result.Score.ToString("N0"));
            UiTextBinder.SetText(coinsText, result.Coins.ToString("N0"));
            UiTextBinder.SetText(xpText, result.Experience.ToString("N0"));
            UiTextBinder.SetText(premiumText, result.PremiumCurrency.ToString("N0"));
            UiTextBinder.SetText(messageText, "Run complete");
            SetButton(doubleRewardsButton, result.Coins > 0 || result.Experience > 0 || result.PremiumCurrency > 0);
            SetButton(retryButton, true);
            SetButton(hubButton, true);
        }

        public void ShowContinueOffer()
        {
            base.Show();
            _continuePending = false;
            SetMode(finalResult: false);
            UiTextBinder.SetText(scoreText, string.Empty);
            UiTextBinder.SetText(coinsText, string.Empty);
            UiTextBinder.SetText(xpText, string.Empty);
            UiTextBinder.SetText(premiumText, string.Empty);
            UiTextBinder.SetText(messageText, "Continue?");
            SetButton(continueButton, true);
            SetButton(declineContinueButton, true);
        }

        public void ShowDoubleRewardGranted(RunResult extra)
        {
            _doubleRewardsClaimed = true;
            _doubleRewardPending = false;
            SetButton(doubleRewardsButton, false);
            UiTextBinder.SetText(messageText, $"Double reward claimed: +{extra.Coins} coins, +{extra.Experience} XP");
        }

        public void ShowDoubleRewardUnavailable()
        {
            _doubleRewardPending = false;
            SetButton(doubleRewardsButton, !_doubleRewardsClaimed);
            UiTextBinder.SetText(messageText, "Double reward is currently unavailable.");
        }

        public void ShowContinueUnavailable()
        {
            _continuePending = false;
            SetButton(continueButton, false);
            UiTextBinder.SetText(messageText, "Continue is currently unavailable.");
        }

        public void SetDoubleRewardPending(bool pending)
        {
            _doubleRewardPending = pending;
            SetButton(doubleRewardsButton, !pending && !_doubleRewardsClaimed);
            if (pending)
                UiTextBinder.SetText(messageText, "Loading reward...");
        }

        public void SetContinuePending(bool pending)
        {
            _continuePending = pending;
            SetButton(continueButton, !pending);
            SetButton(declineContinueButton, !pending);
            if (pending)
                UiTextBinder.SetText(messageText, "Loading continue...");
        }

        public void Retry()
        {
            if (_doubleRewardPending || _continuePending)
                return;
            runController?.RetryRun();
        }

        public void ReturnToHub()
        {
            if (_doubleRewardPending || _continuePending)
                return;
            runController?.ReturnToMenu();
        }

        public void ContinueRun()
        {
            if (_continuePending)
                return;
            runController?.ContinueRun();
        }

        public void DeclineContinue()
        {
            if (_continuePending)
                return;
            runController?.DeclineContinue();
        }

        public void DoubleRewards()
        {
            if (_doubleRewardsClaimed || _doubleRewardPending)
                return;

            runController?.DoubleRunRewards();
        }


        private void ResolveButtons()
        {
            if (retryButton == null) retryButton = FindButton("RetryButton");
            if (hubButton == null) hubButton = FindButton("MenuButton");
            if (continueButton == null) continueButton = FindButton("ContinueButton");
            if (declineContinueButton == null) declineContinueButton = FindButton("EndRunButton");
            if (doubleRewardsButton == null) doubleRewardsButton = FindButton("DoubleRewardsButton");
        }

        private Button FindButton(string objectName)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
                if (buttons[i] != null && buttons[i].name == objectName)
                    return buttons[i];
            return null;
        }

        private void SetMode(bool finalResult)
        {
            if (finalResultRoot != null) finalResultRoot.SetActive(finalResult);
            if (continuePromptRoot != null) continuePromptRoot.SetActive(!finalResult);
        }

        private static void SetButton(Button button, bool interactable)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }
}
