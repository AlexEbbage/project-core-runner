using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu.Progression
{
    public sealed class ProgressionTasksHubView : MonoBehaviour
    {
        [SerializeField] private Text headerText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Slider overallProgress;

        public void Bind(int completed, int total)
        {
            if (headerText != null) headerText.text = "Challenges";
            if (summaryText != null) summaryText.text = $"{completed}/{Mathf.Max(0, total)} complete";
            if (overallProgress != null) overallProgress.value = total <= 0 ? 0f : Mathf.Clamp01(completed / (float)total);
        }
    }
}
