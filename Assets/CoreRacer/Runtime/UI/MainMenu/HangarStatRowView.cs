using CoreRacer.Meta.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class HangarStatRowView : MonoBehaviour
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Text valueText;
        [SerializeField] private Slider slider;

        public void Bind(ShipStatType stat, float value, float maxValue = 10f)
        {
            if (labelText != null) labelText.text = stat.ToString();
            if (valueText != null) valueText.text = value.ToString("0.0");
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = Mathf.Max(1f, maxValue);
                slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
            }
        }
    }
}
