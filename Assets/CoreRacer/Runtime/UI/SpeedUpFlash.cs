using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI
{
    public sealed class SpeedUpFlash : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text label;
        [SerializeField] private string message = "SPEED UP";
        [SerializeField] private float displaySeconds = 0.75f;
        private Coroutine _routine;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        public void Show(string overrideMessage = null)
        {
            if (label != null) label.text = string.IsNullOrWhiteSpace(overrideMessage) ? message : overrideMessage;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Routine());
        }

        private IEnumerator Routine()
        {
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(displaySeconds);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            _routine = null;
        }
    }
}
