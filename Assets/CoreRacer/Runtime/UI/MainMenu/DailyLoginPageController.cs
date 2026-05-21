using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Localization;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Economy;
using CoreRacer.Monetisation.Ads;
using CoreRacer.UI.MainMenu.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class DailyLoginPageController : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private DailyLoginRewardPreviewView rowPrefab;
        [SerializeField] private Text statusText;
        [SerializeField] private Text claimButtonLabelText;
        [SerializeField] private Text claimX2ButtonLabelText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button claimX2Button;

        private readonly List<DailyLoginRewardPreviewView> _rows = new List<DailyLoginRewardPreviewView>();
        private DailyRewardCalendarService _service;
        private RewardedAdController _rewardedAds;
        private LocalizationServiceV2 _localization;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            if (claimButton != null) claimButton.onClick.AddListener(Claim);
            if (claimX2Button != null) claimX2Button.onClick.AddListener(ClaimX2);
            GameServices.TryGet(out _service);
            GameServices.TryGet(out _rewardedAds);
            GameServices.TryGet(out _localization);
            Refresh();
        }

        private void OnDisable()
        {
            if (claimButton != null) claimButton.onClick.RemoveListener(Claim);
            if (claimX2Button != null) claimX2Button.onClick.RemoveListener(ClaimX2);
        }

        public void Refresh()
        {
            var preview = _service != null ? _service.GetCalendarPreview() : null;
            if (preview == null || rowPrefab == null || contentRoot == null)
            {
                if (statusText != null)
                    statusText.text = "Daily reward content is not wired.";
                return;
            }

            EnsureRows(preview.Count);
            var currentIndex = _service.GetCurrentCalendarIndex();
            var canClaim = _service.CanClaimToday();
            for (int i = 0; i < _rows.Count; i++)
            {
                var active = i < preview.Count;
                _rows[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                var day = preview[i];
                var claimed = !canClaim && i == currentIndex;
                var current = i == currentIndex;
                _rows[i].Bind(i + 1, day.Rewards.Count > 0 ? day.Rewards[0] : null, claimed, current);
            }

            if (claimButton != null) claimButton.interactable = canClaim;
            if (claimX2Button != null) claimX2Button.interactable = canClaim && (_rewardedAds == null || _rewardedAds.CanShow(AdPlacement.DailyLoginDoubleReward));
            if (claimButtonLabelText != null) claimButtonLabelText.text = canClaim ? Localize("ui.daily_login_claim_button") : Localize("ui.daily_login_claimed_button");
            if (claimX2ButtonLabelText != null) claimX2ButtonLabelText.text = canClaim ? "Claim x2" : Localize("ui.daily_login_claimed_button");
            if (statusText != null)
            {
                statusText.text = canClaim
                    ? "Today's reward is ready."
                    : "Today's reward is already claimed.";

                if (canClaim && claimX2Button != null && !claimX2Button.interactable)
                    statusText.text = "Rewarded ad is not ready for Claim x2.";
            }
        }

        private void EnsureRows(int count)
        {
            while (_rows.Count < count)
            {
                var row = Instantiate(rowPrefab, contentRoot);
                row.gameObject.SetActive(false);
                _rows.Add(row);
            }
        }

        private void Claim()
        {
            if (_service != null && _service.TryClaim(false))
                Refresh();
        }

        private void ClaimX2()
        {
            if (_service == null)
                return;

            if (_rewardedAds == null)
            {
                if (_service.TryClaim(true))
                    Refresh();
                return;
            }

            _rewardedAds.ShowOrBypass(AdPlacement.DailyLoginDoubleReward, result =>
            {
                if (result == RewardedAdResult.Rewarded || result == RewardedAdResult.BypassedByPremium)
                    _service.TryClaim(true);
                Refresh();
            });
        }

        private string Localize(string key)
        {
            return _localization != null ? _localization.Get(key) : key;
        }
    }
}
