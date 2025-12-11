using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fancy countdown UI:
/// - Shows 3..2..1..GO! in the center of the screen
/// - Each tick scales / fades / changes color
/// - Optional screen shake on each tick
/// - Optional slow-motion during the countdown
/// 
/// Hook:
///   countdownUI.BeginCountdown(3, OnCountdownFinished);
/// </summary>
public class CountdownUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject root;

    [Header("Timing")]
    [Tooltip("Seconds each number stays on screen.")]
    [SerializeField] private float stepDuration = 1.0f;

    [Tooltip("Countdown string for GO step.")]
    [SerializeField] private string goText = "GO!";

    [Header("Animation Curves")]
    [Tooltip("Scale over [0..1] of each step.")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.2f, 1, 1.2f);

    [Tooltip("Alpha over [0..1] of each step.")]
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0f, 0.2f, 1f);

    //[Header("Colors")]
    //[SerializeField] private Color startColor = Color.yellow;
    //[SerializeField] private Color endColor = Color.red;
    //[SerializeField] private Color goColor = Color.green;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    //[Header("Screen Shake")]
    //[Tooltip("Optional screen shake helper for impact on each tick.")]
    //[SerializeField] private ScreenShakeHelper screenShake;
    //[SerializeField] private float tickShakeIntensity = 0.2f;
    //[SerializeField] private float goShakeIntensity = 0.6f;

    //[Header("Slow Motion")]
    //[SerializeField] private bool useSlowMotion = true;
    //[SerializeField] private float countdownTimeScale = 0.3f;

    private Coroutine _routine;
    //private float _originalTimeScale = 1f;
    private Action _onComplete;

    private void Awake()
    {
        //if (canvasGroup == null)
        //{
        //    canvasGroup = GetComponentInChildren<CanvasGroup>();
        //}

        if (countdownText == null)
        {
            countdownText = GetComponentInChildren<TMP_Text>();
        }

        //if (canvasGroup != null)
        //{
        //    canvasGroup.alpha = 0f;
        //}

        if (countdownText != null)
        {
            countdownText.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Starts a countdown from startNumber down to 1, then GO.
    /// Example: BeginCountdown(3, OnCountdownFinished);
    /// </summary>
    public void BeginCountdown(int startNumber, Action onComplete)
    {
        if (startNumber <= 0) startNumber = 3;

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        root.SetActive(true);

        _onComplete = onComplete;
        _routine = StartCoroutine(CountdownRoutine(startNumber));
    }

    private IEnumerator CountdownRoutine(int startNumber)
    {
        //if (canvasGroup != null) canvasGroup.alpha = 1f;

        //// Slow motion (using unscaled time for the animation)
        //if (useSlowMotion)
        //{
        //    _originalTimeScale = Time.timeScale;
        //    Time.timeScale = countdownTimeScale;
        //}

        // 3..2..1..
        for (int n = startNumber; n >= 1; n--)
        {
            yield return PlayOneStep(n.ToString(), isGoStep: false);
        }

        // GO!
        yield return PlayOneStep(goText, isGoStep: true);

        //// Restore time scale
        //if (useSlowMotion)
        //{
        //    Time.timeScale = _originalTimeScale;
        //}

        // Hide UI
        root.SetActive(false);

        _routine = null;

        _onComplete?.Invoke();
    }

    private IEnumerator PlayOneStep(string text, bool isGoStep)
    {
        if (countdownText == null)
            yield break;

        countdownText.text = text;

        //// Color
        //if (!isGoStep)
        //{
        //    // Tick colors shift between startColor and endColor based on the numeric value (rough).
        //    float tColor = 0.5f;
        //    if (int.TryParse(text, out int n))
        //    {
        //        // e.g. if we typically use 3..2..1, map 3->0, 2->0.5, 1->1
        //        tColor = Mathf.InverseLerp(3f, 1f, n);
        //    }
        //    countdownText.color = Color.Lerp(startColor, endColor, tColor);
        //}
        //else
        //{
        //    countdownText.color = goColor;
        //}

        // Sound
        if (audioManager != null)
        {
            if (isGoStep)
            {
                audioManager.PlayCountdownGo();
            }
            else
            {
                audioManager.PlayCountdownTimer();
            }
        }

        //// Screen shake
        //if (screenShake != null)
        //{
        //    float intensity = isGoStep ? goShakeIntensity : tickShakeIntensity;
        //    if (intensity > 0f)
        //    {
        //        screenShake.Shake(intensity);
        //    }
        //}

        // Animate scale & alpha over unscaled time so slow-motion doesn't affect the UI
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, stepDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scaleEval = scaleCurve.Evaluate(t);
            float alphaEval = alphaCurve.Evaluate(t);

            countdownText.transform.localScale = Vector3.one * scaleEval;

            countdownText.alpha = alphaEval;

            yield return null;
        }

        // ensure visible at end of step (except alpha for next fade)
        countdownText.transform.localScale = Vector3.one * scaleCurve.Evaluate(1f);
    }
}
