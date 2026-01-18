using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionTasksHubView : MonoBehaviour
{
    [SerializeField] private Button dailyTabButton;
    [SerializeField] private Button weeklyTabButton;
    [SerializeField] private Button monthlyTabButton;
    [SerializeField] private Image dailyTabImage;
    [SerializeField] private Image weeklyTabImage;
    [SerializeField] private Image monthlyTabImage;
    [SerializeField] private Color selectedTabColor = new(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color idleTabColor = new(1f, 1f, 1f, 0.15f);
    [SerializeField] private GameObject dailyContent;
    [SerializeField] private GameObject weeklyContent;
    [SerializeField] private GameObject monthlyContent;
    [SerializeField] private bool showDailyLoginPreview = true;

    private readonly Dictionary<ProgressionCadence, GameObject> _contentLookup = new();

    private void Awake()
    {
        _contentLookup[ProgressionCadence.Daily] = dailyContent;
        _contentLookup[ProgressionCadence.Weekly] = weeklyContent;
        _contentLookup[ProgressionCadence.Monthly] = monthlyContent;

        if (dailyTabButton != null)
            dailyTabButton.onClick.AddListener(() => Show(ProgressionCadence.Daily));
        if (weeklyTabButton != null)
            weeklyTabButton.onClick.AddListener(() => Show(ProgressionCadence.Weekly));
        if (monthlyTabButton != null)
            monthlyTabButton.onClick.AddListener(() => Show(ProgressionCadence.Monthly));

        EnsureDailyLoginPreview();
        Show(ProgressionCadence.Daily);
    }

    public void Show(ProgressionCadence cadence)
    {
        foreach (var pair in _contentLookup)
        {
            if (pair.Value != null)
                pair.Value.SetActive(pair.Key == cadence);
        }

        UpdateTabVisuals(cadence);
    }

    private void UpdateTabVisuals(ProgressionCadence cadence)
    {
        SetTabColor(dailyTabImage, cadence == ProgressionCadence.Daily);
        SetTabColor(weeklyTabImage, cadence == ProgressionCadence.Weekly);
        SetTabColor(monthlyTabImage, cadence == ProgressionCadence.Monthly);
    }

    private void SetTabColor(Image image, bool isSelected)
    {
        if (image == null)
            return;

        image.color = isSelected ? selectedTabColor : idleTabColor;
    }

    private void EnsureDailyLoginPreview()
    {
        if (!showDailyLoginPreview || dailyContent == null)
            return;

        if (dailyContent.GetComponentInChildren<DailyLoginRewardPreviewView>(true) != null)
            return;

        TMP_Text template = dailyContent.GetComponentInChildren<TMP_Text>(true);
        DailyLoginRewardPreviewView.Create(dailyContent.transform, template);
    }
}
