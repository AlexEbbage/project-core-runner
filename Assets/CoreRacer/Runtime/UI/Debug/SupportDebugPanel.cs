using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Services.Support;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Debugging
{
    public sealed class SupportDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text outputText;
        [SerializeField] private Button generateButton;
        [SerializeField] private Button resetTutorialButton;

        private SupportBundleExporter _exporter;
        private TutorialService _tutorial;

        private void Awake()
        {
            if (generateButton != null) generateButton.onClick.AddListener(Generate);
            if (resetTutorialButton != null) resetTutorialButton.onClick.AddListener(ResetTutorial);
        }

        private void OnEnable()
        {
            GameServices.TryGet(out _exporter);
            GameServices.TryGet(out _tutorial);
        }

        public void Generate()
        {
            if (outputText != null)
                outputText.text = _exporter != null ? _exporter.BuildTextBundle(PlayerSupportInfo.Create()) : "Support exporter not registered.";
        }

        public void ResetTutorial()
        {
            if (_tutorial == null) GameServices.TryGet(out _tutorial);
            _tutorial?.ResetForTesting();
            if (outputText != null)
                outputText.text = _tutorial != null ? "Tutorial progress reset." : "Tutorial service not registered.";
        }
    }
}
