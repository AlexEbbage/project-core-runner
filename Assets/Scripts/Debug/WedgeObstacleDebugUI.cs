using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple on-screen debug for wedge patterns:
/// - Shows whether we're in a wedge run
/// - Shows current local pattern type
/// - Shows rings remaining in this run
/// - Shows rotation step & step limit & direction mode
/// 
/// Attach this to a Canvas GameObject, assign a Text component in the inspector, 
/// and assign your ObstacleRingGenerator.
/// </summary>
public class WedgeObstacleDebugUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObstacleRingGenerator ringGenerator;
    [SerializeField] private TMP_Text debugText;

    [Header("Options")]
    [SerializeField] private bool onlyShowWhenWedgeRun = false;
    [SerializeField] private float updateInterval = 0.1f;

    private float _timer;

    private void Reset()
    {
        debugText = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        if (ringGenerator == null || debugText == null)
            return;

        _timer += Time.deltaTime;
        if (_timer < updateInterval)
            return;

        _timer = 0f;
        Refresh();
    }

    private void Refresh()
    {
        bool inRun = ringGenerator.InWedgeRunDebug;

        if (onlyShowWhenWedgeRun && !inRun)
        {
            debugText.text = "";
            return;
        }

        string pattern = ringGenerator.WedgeCurrentLocalPatternDebug.ToString();
        int remaining = ringGenerator.WedgeRunRingsRemainingDebug;
        int step = ringGenerator.WedgeCurrentRotationStepDebug;
        int maxStep = ringGenerator.MaxWedgeRotationStepsPerRingDebug;
        int dirSign = ringGenerator.WedgeRotationDirectionSignDebug;

        string dirDesc;
        if (dirSign > 0) dirDesc = "+ (clockwise)";
        else if (dirSign < 0) dirDesc = "- (counter-clockwise)";
        else dirDesc = "both";

        debugText.text =
            $"[Wedge Run]\n" +
            $"Active: {inRun}\n" +
            $"Pattern: {pattern}\n" +
            $"Rings left: {remaining}\n" +
            $"Current rot step: {step}\n" +
            $"Max step / ring: {maxStep}\n" +
            $"Direction: {dirDesc}";
    }
}
