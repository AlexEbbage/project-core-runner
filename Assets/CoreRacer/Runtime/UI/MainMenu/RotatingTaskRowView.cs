using CoreRacer.Meta.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class RotatingTaskRowView : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text expiryText;
        [SerializeField] private Button claimButton;

        private string _taskId;
        private System.Action<string> _claimRequested;

        private void Awake()
        {
            if (claimButton != null) claimButton.onClick.AddListener(() => _claimRequested?.Invoke(_taskId));
        }

        public void Render(RotatingTaskViewModel model, System.Action<string> claimRequested)
        {
            _taskId = model.Id;
            _claimRequested = claimRequested;
            if (titleText != null) titleText.text = model.DisplayName;
            if (descriptionText != null) descriptionText.text = model.Description;
            if (progressText != null) progressText.text = $"{model.Progress}/{model.Target}";
            if (expiryText != null) expiryText.text = model.ExpiresAtUtc == default ? string.Empty : $"Expires {model.ExpiresAtUtc:MMM d, HH:mm} UTC";
            if (claimButton != null)
            {
                claimButton.interactable = model.Status == RotatingTaskStatus.Completed;
                var label = claimButton.GetComponentInChildren<Text>();
                if (label != null) label.text = model.Status == RotatingTaskStatus.Claimed ? "Claimed" : model.Status == RotatingTaskStatus.Completed ? "Claim" : "In Progress";
            }
        }
    }
}
