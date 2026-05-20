using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreRacer.SceneManagement
{
    public sealed class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;
        [SerializeField] private float fadeDuration = 0.25f;

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadRoutine(sceneName));
        }

        public void FadeToBlack(Action onBlack)
        {
            StartCoroutine(FadeRoutine(1f, onBlack));
        }

        public void FadeFromBlack()
        {
            StartCoroutine(FadeRoutine(0f, null));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            yield return FadeRoutine(1f, null);
            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
            {
                if (progressBar != null) progressBar.value = Mathf.Clamp01(operation.progress / 0.9f);
                yield return null;
            }
            yield return FadeRoutine(0f, null);
        }

        private IEnumerator FadeRoutine(float target, Action onComplete)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            var start = canvasGroup.alpha;
            var elapsed = 0f;
            canvasGroup.blocksRaycasts = target > 0f;
            while (elapsed < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                yield return null;
            }
            canvasGroup.alpha = target;
            canvasGroup.blocksRaycasts = target > 0f;
            onComplete?.Invoke();
        }
    }
}
