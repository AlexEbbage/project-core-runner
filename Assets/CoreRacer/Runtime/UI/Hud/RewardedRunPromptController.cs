using CoreRacer.Monetisation.Ads;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class RewardedRunPromptController : UiView
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        private System.Action _accepted;
        private System.Action _declined;

        public void ShowPrompt(string title, string description, System.Action accepted, System.Action declined)
        {
            _accepted = accepted;
            _declined = declined;
            UiTextBinder.SetText(titleText, title);
            UiTextBinder.SetText(descriptionText, description);
            Show();
        }

        public void Accept()
        {
            Hide();
            _accepted?.Invoke();
        }

        public void Decline()
        {
            Hide();
            _declined?.Invoke();
        }
    }
}
