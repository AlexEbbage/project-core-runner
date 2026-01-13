using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup loadingGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressLabel;

    [Header("Behavior")]
    [SerializeField] private float minimumDisplayTime = 0.35f;

    private Coroutine loadingRoutine;

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
        ShowLoadingUI(true);

        float timer = 0f;
        AsyncOperation asyncOperation = loadOperation();
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            UpdateProgress(progress);

            bool finishedLoading = asyncOperation.progress >= 0.9f;
            bool readyToActivate = finishedLoading && timer >= minimumDisplayTime;

            if (readyToActivate)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        UpdateProgress(1f);
        ShowLoadingUI(false);
        loadingRoutine = null;
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
}
