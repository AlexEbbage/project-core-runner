using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private string key;
    [SerializeField] private string fallback;
    [SerializeField] private bool applyOnEnable = true;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LocalizationService.LanguageChanged += UpdateText;

        if (applyOnEnable)
            UpdateText();
    }

    private void OnDisable()
    {
        LocalizationService.LanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (targetText == null)
            return;

        var fallbackText = string.IsNullOrEmpty(fallback) ? targetText.text : fallback;
        targetText.text = LocalizationService.Get(key, fallbackText);
    }
}
