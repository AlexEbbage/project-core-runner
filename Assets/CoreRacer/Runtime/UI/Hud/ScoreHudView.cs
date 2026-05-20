using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class ScoreHudView : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;

        public void Render(int score, float combo)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (comboText != null) comboText.text = combo > 0f ? $"Combo {combo:0.0}" : string.Empty;
        }
    }
}
