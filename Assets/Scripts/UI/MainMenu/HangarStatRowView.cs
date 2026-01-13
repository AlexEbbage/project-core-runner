using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangarStatRowView : MonoBehaviour
{
    [SerializeField] private ShipStatType statType;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text valueText;

    public ShipStatType StatType => statType;

    public void SetValue(float normalized, float rawValue)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(normalized);
        if (valueText != null)
            valueText.text = rawValue.ToString("0.0");
    }
}
