using System.Collections;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class CountdownController : UiView
    {
        [SerializeField] private Text countdownText;
        [SerializeField] private float stepSeconds = 0.5f;

        public void Play(System.Action completed)
        {
            StopAllCoroutines();
            StartCoroutine(CountdownRoutine(completed));
        }

        private IEnumerator CountdownRoutine(System.Action completed)
        {
            Show();
            for (int i = 3; i >= 1; i--)
            {
                UiTextBinder.SetText(countdownText, i.ToString());
                yield return new WaitForSecondsRealtime(stepSeconds);
            }
            UiTextBinder.SetText(countdownText, "GO!");
            yield return new WaitForSecondsRealtime(stepSeconds);
            Hide();
            completed?.Invoke();
        }
    }
}
