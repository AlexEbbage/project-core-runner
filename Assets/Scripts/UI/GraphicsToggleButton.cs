using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GraphicsToggleButton : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private GraphicsSettingsManager graphicsSettings;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (graphicsSettings == null)
        {
            graphicsSettings = FindFirstObjectByType<GraphicsSettingsManager>();
        }

        _button.onClick.AddListener(OnClicked);
        UpdateLabel();
    }

    private void OnClicked()
    {
        if (graphicsSettings == null) return;

        if (GraphicsSettingsManager.CurrentQuality == GraphicsQuality.High)
        {
            graphicsSettings.SetLowQuality();
        }
        else
        {
            graphicsSettings.SetHighQuality();
        }

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (label == null) return;

        string text = (GraphicsSettingsManager.CurrentQuality == GraphicsQuality.High)
            ? "Graphics: High"
            : "Graphics: Low";

        label.text = text;
    }
}
