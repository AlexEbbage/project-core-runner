using CoreRacer.Meta.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu.Progression
{
    public sealed class ProgressionRewardNodeView : MonoBehaviour
    {
        [SerializeField] private Text labelText;
        [SerializeField] private GameObject claimedState;
        [SerializeField] private GameObject availableState;

        public void Bind(RewardGrant reward, bool claimed, bool available)
        {
            if (labelText != null) labelText.text = Format(reward);
            if (claimedState != null) claimedState.SetActive(claimed);
            if (availableState != null) availableState.SetActive(available && !claimed);
        }

        private string Format(RewardGrant reward)
        {
            if (reward == null) return "Reward";
            if (reward.Type == RewardGrantType.UnlockItem) return reward.ItemId;
            return $"{reward.Amount:N0} {reward.Type}";
        }
    }
}
