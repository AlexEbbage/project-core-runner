using System;
using CoreRacer.Meta.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu.Progression
{
    public sealed class ProgressionTaskRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text progressText;
        [SerializeField] private Button claimButton;
        private string _taskId;
        private Action<string> _onClaim;

        public void Bind(ProgressionTaskDefinition task, int currentValue, bool claimed, Action<string> onClaim)
        {
            _taskId = task != null ? task.Id : string.Empty;
            _onClaim = onClaim;
            var target = task != null ? Mathf.Max(1, task.TargetValue) : 1;
            var progress = Mathf.Clamp01(currentValue / (float)target);
            if (titleText != null) titleText.text = task != null ? task.DisplayName : "Missing task";
            if (descriptionText != null) descriptionText.text = task != null ? task.Description : string.Empty;
            if (progressSlider != null) progressSlider.value = progress;
            if (progressText != null) progressText.text = $"{Mathf.Min(currentValue, target):N0}/{target:N0}";
            if (claimButton != null) claimButton.interactable = progress >= 1f && !claimed;
        }

        private void OnEnable()
        {
            if (claimButton != null) claimButton.onClick.AddListener(Claim);
        }

        private void OnDisable()
        {
            if (claimButton != null) claimButton.onClick.RemoveListener(Claim);
        }

        private void Claim()
        {
            if (!string.IsNullOrWhiteSpace(_taskId))
                _onClaim?.Invoke(_taskId);
        }
    }
}
