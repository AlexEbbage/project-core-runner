using CoreRacer.Meta.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu.Progression
{
    public sealed class DailyLoginRewardPreviewView : MonoBehaviour
    {
        [SerializeField] private Text dayText;
        [SerializeField] private Text rewardText;
        [SerializeField] private GameObject claimedBadge;
        [SerializeField] private GameObject currentBadge;

        public void Bind(int day, RewardGrant reward, bool claimed, bool current)
        {
            if (dayText != null) dayText.text = $"Day {day}";
            if (rewardText != null) rewardText.text = FormatReward(reward);
            if (claimedBadge != null) claimedBadge.SetActive(claimed);
            if (currentBadge != null) currentBadge.SetActive(current);
        }

        private string FormatReward(RewardGrant reward)
        {
            if (reward == null) return "Reward";
            if (reward.Type == RewardGrantType.UnlockItem) return reward.ItemId;
            return $"{reward.Amount:N0} {reward.Type}";
        }
    }
}
