using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Localization;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.FTUE
{
    public sealed class TutorialOverlayController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button continueButton;

        private TutorialService _tutorial;
        private LocalizationServiceV2 _localization;

        private void Awake()
        {
            if (root == null) root = gameObject;
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnEnable()
        {
            if (GameServices.TryGet(out _tutorial))
            {
                _tutorial.StepChanged += Render;
                _tutorial.Completed += Hide;
            }
            GameServices.TryGet(out _localization);
        }

        private void OnDisable()
        {
            if (_tutorial != null)
            {
                _tutorial.StepChanged -= Render;
                _tutorial.Completed -= Hide;
            }
        }

        public void BeginIfNeeded()
        {
            if (_tutorial != null && _tutorial.ShouldRunForFreshInstall())
                _tutorial.Start();
        }

        private void Render(TutorialStepDefinition step)
        {
            if (step == null)
            {
                Hide();
                return;
            }

            root.SetActive(true);
            UiTextBinder.SetText(titleText, Localize(step.TitleKey));
            UiTextBinder.SetText(bodyText, Localize(step.BodyKey));
            if (continueButton != null) continueButton.gameObject.SetActive(step.RequiresExplicitContinue);
        }

        private string Localize(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return _localization != null ? _localization.Get(key) : key;
        }

        private void OnContinueClicked()
        {
            _tutorial?.Advance();
        }

        private void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
