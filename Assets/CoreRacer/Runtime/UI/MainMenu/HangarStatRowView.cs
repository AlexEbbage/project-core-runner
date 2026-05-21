using CoreRacer.Meta.Ships;
using CoreRacer.Localization;
using CoreRacer.Bootstrap;
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
            if (labelText != null) labelText.text = ResolveLabel(stat);
            if (valueText != null) valueText.text = value.ToString("0.0");
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = Mathf.Max(1f, maxValue);
                slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
            }
        }

        private static string ResolveLabel(ShipStatType stat)
        {
            if (!GameServices.TryGet<LocalizationServiceV2>(out var localization))
                return stat.ToString();

            switch (stat)
            {
                case ShipStatType.Speed: return localization.Get("ui.hangar_stat_speed");
                case ShipStatType.Handling: return localization.Get("ui.hangar_stat_handling");
                case ShipStatType.Boost: return localization.Get("ui.hangar_stat_boost");
                default: return stat.ToString();
            }
        }
    }
}
