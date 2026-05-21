using CoreRacer.UI.MainMenu;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Settings
{
    public sealed class SettingsHubController : MonoBehaviour
    {
        [SerializeField] private Button generalButton;
        [SerializeField] private Button comfortButton;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button supportButton;
        [SerializeField] private GameObject generalPanel;
        [SerializeField] private GameObject comfortPanel;
        [SerializeField] private GameObject privacyPanel;
        [SerializeField] private GameObject supportPanel;
        [SerializeField] private SettingsMenuController settingsMenu;
        [SerializeField] private ComfortSettingsController comfortSettings;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            if (generalButton != null) generalButton.onClick.AddListener(ShowGeneral);
            if (comfortButton != null) comfortButton.onClick.AddListener(ShowComfort);
            if (privacyButton != null) privacyButton.onClick.AddListener(ShowPrivacy);
            if (supportButton != null) supportButton.onClick.AddListener(ShowSupport);
            ShowGeneral();
        }

        private void OnDisable()
        {
            if (generalButton != null) generalButton.onClick.RemoveListener(ShowGeneral);
            if (comfortButton != null) comfortButton.onClick.RemoveListener(ShowComfort);
            if (privacyButton != null) privacyButton.onClick.RemoveListener(ShowPrivacy);
            if (supportButton != null) supportButton.onClick.RemoveListener(ShowSupport);
        }

        public void ShowGeneral()
        {
            SetPanel(generalPanel, true);
            SetPanel(comfortPanel, false);
            SetPanel(privacyPanel, false);
            SetPanel(supportPanel, false);
            settingsMenu?.Refresh();
        }

        public void ShowComfort()
        {
            SetPanel(generalPanel, false);
            SetPanel(comfortPanel, true);
            SetPanel(privacyPanel, false);
            SetPanel(supportPanel, false);
            comfortSettings?.Refresh();
        }

        public void ShowPrivacy()
        {
            SetPanel(generalPanel, false);
            SetPanel(comfortPanel, false);
            SetPanel(privacyPanel, true);
            SetPanel(supportPanel, false);
        }

        public void ShowSupport()
        {
            SetPanel(generalPanel, false);
            SetPanel(comfortPanel, false);
            SetPanel(privacyPanel, false);
            SetPanel(supportPanel, true);
        }

        private static void SetPanel(GameObject panel, bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }
    }
}
