using CoreRacer.Bootstrap;
using CoreRacer.Services.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI
{
    public sealed class GraphicsToggleButton : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private GraphicsSettingsManager graphicsSettings;

        private void Awake()
        {
            if (toggle == null) toggle = GetComponent<Toggle>();
            if (graphicsSettings == null) graphicsSettings = FindObjectOfType<GraphicsSettingsManager>();
        }

        private void OnEnable()
        {
            if (toggle != null) toggle.onValueChanged.AddListener(SetHighQuality);
        }

        private void OnDisable()
        {
            if (toggle != null) toggle.onValueChanged.RemoveListener(SetHighQuality);
        }

        private void SetHighQuality(bool enabled)
        {
            if (graphicsSettings != null)
                graphicsSettings.SetHighQuality(enabled);
        }
    }
}
