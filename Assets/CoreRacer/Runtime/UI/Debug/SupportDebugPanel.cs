using CoreRacer.Bootstrap;
using CoreRacer.Services.Support;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Debugging
{
    public sealed class SupportDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text outputText;
        [SerializeField] private Button generateButton;

        private SupportBundleExporter _exporter;

        private void Awake()
        {
            if (generateButton != null) generateButton.onClick.AddListener(Generate);
        }

        private void OnEnable()
        {
            GameServices.TryGet(out _exporter);
        }

        public void Generate()
        {
            if (outputText != null)
                outputText.text = _exporter != null ? _exporter.BuildTextBundle(PlayerSupportInfo.Create()) : "Support exporter not registered.";
        }
    }
}
