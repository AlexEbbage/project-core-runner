using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class HealthHudView : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Slider healthSlider;

        public void Render(float current, float max)
        {
            if (healthText != null) healthText.text = $"{current:0}/{max:0}";
            if (healthSlider != null)
            {
                healthSlider.maxValue = Mathf.Max(1f, max);
                healthSlider.value = Mathf.Clamp(current, 0f, healthSlider.maxValue);
            }
        }
    }
}
