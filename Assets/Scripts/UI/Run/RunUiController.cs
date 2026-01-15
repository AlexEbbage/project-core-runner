using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RunUiController : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private HudView hudView;
    [SerializeField] private PauseMenuView pauseMenuView;
    [SerializeField] private SettingsMenuView settingsMenuView;
    [SerializeField] private DeathContinueView deathContinueView;
    [SerializeField] private ResultsView resultsView;

    [Header("Results")]
    [SerializeField] private float resultsTimerDuration = 5f;

    public event Action OnPauseRequested;
    public event Action OnResumeRequested;
    public event Action OnMainMenuRequested;
    public event Action<bool, bool, bool, bool> OnSettingsChanged;
    public event Action OnWatchAdContinueRequested;
    public event Action OnBackToBaseRequested;
    public event Action OnWatchAdDoubleRewardsRequested;
    public event Action OnBackToHubRequested;

    private Coroutine resultsTimerRoutine;

    private void Awake()
    {
        if (hudView != null && hudView.PauseButton != null)
            hudView.PauseButton.onClick.AddListener(HandlePausePressed);

        if (pauseMenuView != null)
        {
            pauseMenuView.ContinueButton?.onClick.AddListener(HandleResumePressed);
            pauseMenuView.SettingsButton?.onClick.AddListener(HandleSettingsPressed);
            pauseMenuView.MainMenuButton?.onClick.AddListener(HandleMainMenuPressed);
        }

        if (settingsMenuView != null)
        {
            settingsMenuView.BackButton?.onClick.AddListener(HandleSettingsBackPressed);
            settingsMenuView.MusicToggle?.onValueChanged.AddListener(_ => BroadcastSettingsChanged());
            settingsMenuView.SfxToggle?.onValueChanged.AddListener(_ => BroadcastSettingsChanged());
            settingsMenuView.VibrateToggle?.onValueChanged.AddListener(_ => BroadcastSettingsChanged());
            settingsMenuView.InputToggle?.onValueChanged.AddListener(_ => BroadcastSettingsChanged());
        }

        if (deathContinueView != null)
        {
            deathContinueView.WatchAdContinueButton?.onClick.AddListener(HandleWatchAdContinuePressed);
            deathContinueView.BackToBaseButton?.onClick.AddListener(HandleBackToBasePressed);
        }

        if (resultsView != null)
        {
            resultsView.DoubleRewardsButton?.onClick.AddListener(HandleDoubleRewardsPressed);
            resultsView.BackToHubButton?.onClick.AddListener(HandleBackToHubPressed);
        }
    }

    public void SetScore(int score)
    {
        if (hudView?.ScoreText != null)
            hudView.ScoreText.text = score.ToString("0");
    }

    public void SetBestScore(int bestScore)
    {
        if (hudView?.BestText != null)
            hudView.BestText.text = $"Best {bestScore:0}";
    }

    public void SetProgress(float normalized)
    {
        if (hudView?.ProgressBar != null)
            hudView.ProgressBar.value = Mathf.Clamp01(normalized);
    }

    public void SetCurrentScore(int score)
    {
        if (deathContinueView?.CurrentScoreText != null)
            deathContinueView.CurrentScoreText.text = $"Score: {score:0}";
    }

    public void ShowPause()
    {
        SetOverlayVisible(pauseMenuView?.CanvasGroup, true);
    }

    public void HidePause()
    {
        SetOverlayVisible(pauseMenuView?.CanvasGroup, false);
    }

    public void ShowSettings()
    {
        SetOverlayVisible(settingsMenuView?.CanvasGroup, true);
    }

    public void HideSettings()
    {
        SetOverlayVisible(settingsMenuView?.CanvasGroup, false);
    }

    public void ShowDeathContinue()
    {
        SetOverlayVisible(deathContinueView?.CanvasGroup, true);
    }

    public void HideDeathContinue()
    {
        SetOverlayVisible(deathContinueView?.CanvasGroup, false);
    }

    public void ShowResults(float durationSeconds)
    {
        SetOverlayVisible(resultsView?.CanvasGroup, true);
        InitializeResultsTimer(durationSeconds);
    }

    public void ShowResults()
    {
        ShowResults(resultsTimerDuration);
    }

    public void HideResults()
    {
        StopResultsTimer();
        SetOverlayVisible(resultsView?.CanvasGroup, false);
    }

    public void ClearRewards()
    {
        if (resultsView?.RewardsContentRoot == null)
            return;

        for (int i = resultsView.RewardsContentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(resultsView.RewardsContentRoot.GetChild(i).gameObject);
        }
    }

    public RewardRowView AddRewardRow(Sprite icon, string label, string value)
    {
        if (resultsView?.RewardsContentRoot == null || resultsView.RewardRowPrefab == null)
            return null;

        var instance = Instantiate(resultsView.RewardRowPrefab, resultsView.RewardsContentRoot);
        if (instance.IconImage != null)
            instance.IconImage.sprite = icon;
        if (instance.LabelText != null)
            instance.LabelText.text = label;
        if (instance.ValueText != null)
            instance.ValueText.text = value;
        return instance;
    }

    public void HandleDoubleRewardsAdSuccess()
    {
        HideResults();
        OnBackToHubRequested?.Invoke();
    }

    private void InitializeResultsTimer(float durationSeconds)
    {
        StopResultsTimer();

        if (resultsView?.TimerBar != null)
        {
            resultsView.TimerBar.minValue = 0f;
            resultsView.TimerBar.maxValue = 1f;
            resultsView.TimerBar.value = 1f;
        }

        if (resultsView?.BackToHubButton != null)
            resultsView.BackToHubButton.gameObject.SetActive(false);

        if (resultsView?.DoubleRewardsButton != null)
            resultsView.DoubleRewardsButton.interactable = true;

        if (durationSeconds > 0f)
            resultsTimerRoutine = StartCoroutine(ResultsTimerRoutine(durationSeconds));
    }

    private IEnumerator ResultsTimerRoutine(float durationSeconds)
    {
        float elapsed = 0f;
        while (elapsed < durationSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / durationSeconds);
            if (resultsView?.TimerBar != null)
                resultsView.TimerBar.value = 1f - t;
            yield return null;
        }

        if (resultsView?.BackToHubButton != null)
            resultsView.BackToHubButton.gameObject.SetActive(true);

        if (resultsView?.DoubleRewardsButton != null)
            resultsView.DoubleRewardsButton.interactable = false;

        resultsTimerRoutine = null;
    }

    private void StopResultsTimer()
    {
        if (resultsTimerRoutine != null)
            StopCoroutine(resultsTimerRoutine);
        resultsTimerRoutine = null;
    }

    private void HandlePausePressed()
    {
        ShowPause();
        OnPauseRequested?.Invoke();
    }

    private void HandleResumePressed()
    {
        HidePause();
        OnResumeRequested?.Invoke();
    }

    private void HandleSettingsPressed()
    {
        HidePause();
        ShowSettings();
    }

    private void HandleSettingsBackPressed()
    {
        HideSettings();
        ShowPause();
    }

    private void HandleMainMenuPressed()
    {
        OnMainMenuRequested?.Invoke();
    }

    private void HandleWatchAdContinuePressed()
    {
        OnWatchAdContinueRequested?.Invoke();
    }

    private void HandleBackToBasePressed()
    {
        HideDeathContinue();
        ShowResults();
        OnBackToBaseRequested?.Invoke();
    }

    private void HandleDoubleRewardsPressed()
    {
        resultsView?.DoubleRewardsButton?.interactable = false;
        OnWatchAdDoubleRewardsRequested?.Invoke();
    }

    private void HandleBackToHubPressed()
    {
        HideResults();
        OnBackToHubRequested?.Invoke();
    }

    private void BroadcastSettingsChanged()
    {
        OnSettingsChanged?.Invoke(
            settingsMenuView?.MusicToggle?.isOn ?? false,
            settingsMenuView?.SfxToggle?.isOn ?? false,
            settingsMenuView?.VibrateToggle?.isOn ?? false,
            settingsMenuView?.InputToggle?.isOn ?? false);
    }

    private static void SetOverlayVisible(CanvasGroup canvasGroup, bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
        canvasGroup.gameObject.SetActive(visible);
    }
}
