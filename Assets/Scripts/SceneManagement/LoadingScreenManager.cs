using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup loadingGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressLabel;
    [SerializeField] private Image fadeOverlay;

    [Header("Behavior")]
    [SerializeField] private float minimumDisplayTime = 0.35f;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool blackFadeOnly = false;

    private Coroutine loadingRoutine;
    private Coroutine fadeRoutine;
    private Coroutine transitionRoutine;
    private float currentProgress;

    private void Awake()
    {
        if (loadingGroup != null)
        {
            loadingGroup.alpha = 0f;
            loadingGroup.blocksRaycasts = false;
            loadingGroup.interactable = false;
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }

        if (progressLabel != null)
        {
            progressLabel.text = "0%";
        }

        if (fadeOverlay != null)
        {
            SetFadeAlpha(0f);
            fadeOverlay.raycastTarget = false;
            fadeOverlay.gameObject.SetActive(false);
        }

        currentProgress = 0f;
    }

    private void OnDisable()
    {
        if (loadingRoutine != null)
        {
            StopCoroutine(loadingRoutine);
            loadingRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        ShowLoadingUI(false);
        ResetFadeState();
    }

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("LoadingScreenManager: Scene name was empty.");
            return;
        }

        StartSceneLoad(() => SceneManager.LoadSceneAsync(sceneName));
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        StartSceneLoad(() => SceneManager.LoadSceneAsync(sceneIndex));
    }

    public void ShowLoadingScreen()
    {
        ShowLoadingUI(!blackFadeOnly);
        SetProgress(0f);
        StartFade(true);
    }

    public void HideLoadingScreen()
    {
        StartFade(false);
    }

    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        UpdateProgress(currentProgress);
    }

    public float GetProgress()
    {
        return currentProgress;
    }

    public float GetFadeDuration()
    {
        return fadeDuration;
    }

    public bool UsesUnscaledTime()
    {
        return useUnscaledTime;
    }

    public void PlayBlackFadeTransition(System.Action midAction, System.Action onComplete = null)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(BlackFadeTransitionRoutine(midAction, onComplete));
    }

    private void StartSceneLoad(System.Func<AsyncOperation> loadOperation)
    {
        if (loadingRoutine != null)
        {
            StopCoroutine(loadingRoutine);
        }

        loadingRoutine = StartCoroutine(LoadSceneRoutine(loadOperation));
    }

    private IEnumerator LoadSceneRoutine(System.Func<AsyncOperation> loadOperation)
    {
        ShowLoadingScreen();

        float timer = 0f;
        AsyncOperation asyncOperation = loadOperation();
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            SetProgress(progress);

            bool finishedLoading = asyncOperation.progress >= 0.9f;
            bool readyToActivate = finishedLoading && timer >= minimumDisplayTime;

            if (readyToActivate)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        SetProgress(1f);
        HideLoadingScreen();
        loadingRoutine = null;
    }

    private IEnumerator BlackFadeTransitionRoutine(System.Action midAction, System.Action onComplete)
    {
        ShowLoadingUI(false);

        if (fadeOverlay == null || fadeDuration <= 0f)
        {
            midAction?.Invoke();
            onComplete?.Invoke();
            transitionRoutine = null;
            yield break;
        }

        StartFade(true);
        yield return WaitForFadeDuration();

        midAction?.Invoke();

        StartFade(false);
        yield return WaitForFadeDuration();

        onComplete?.Invoke();
        transitionRoutine = null;
    }

    private IEnumerator WaitForFadeDuration()
    {
        if (useUnscaledTime)
        {
            yield return new WaitForSecondsRealtime(fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }
    }

    private void ShowLoadingUI(bool visible)
    {
        if (loadingGroup == null)
        {
            return;
        }

        loadingGroup.alpha = visible ? 1f : 0f;
        loadingGroup.blocksRaycasts = visible;
        loadingGroup.interactable = visible;
    }

    private void StartFade(bool fadeIn)
    {
        if (fadeOverlay == null || fadeDuration <= 0f)
        {
            if (fadeOverlay != null)
            {
                SetFadeAlpha(fadeIn ? 1f : 0f);
                fadeOverlay.gameObject.SetActive(fadeIn);
                fadeOverlay.raycastTarget = fadeIn;
            }

            if (!fadeIn)
            {
                ShowLoadingUI(false);
            }

            fadeRoutine = null;
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeOverlayRoutine(fadeIn));
    }

    private IEnumerator FadeOverlayRoutine(bool fadeIn)
    {
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.raycastTarget = true;

        float startAlpha = GetFadeAlpha();
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float curved = fadeCurve != null ? fadeCurve.Evaluate(t) : t;
            SetFadeAlpha(Mathf.Lerp(startAlpha, endAlpha, curved));
            yield return null;
        }

        SetFadeAlpha(endAlpha);
        fadeOverlay.raycastTarget = fadeIn;

        if (!fadeIn)
        {
            fadeOverlay.gameObject.SetActive(false);
            ShowLoadingUI(false);
        }

        fadeRoutine = null;
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null)
        {
            return;
        }

        Color color = fadeOverlay.color;
        color.a = alpha;
        fadeOverlay.color = color;
    }

    private float GetFadeAlpha()
    {
        return fadeOverlay != null ? fadeOverlay.color.a : 0f;
    }

    private void UpdateProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }

        if (progressLabel != null)
        {
            int percent = Mathf.RoundToInt(progress * 100f);
            progressLabel.text = $"{percent}%";
        }
    }

    private void ResetFadeState()
    {
        if (fadeOverlay == null)
        {
            return;
        }

        SetFadeAlpha(0f);
        fadeOverlay.raycastTarget = false;
        fadeOverlay.gameObject.SetActive(false);
    }
}
