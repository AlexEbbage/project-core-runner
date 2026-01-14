using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionTaskRowView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private GameObject completeOverlay;
    [SerializeField] private TMP_Text completeIndicator;

    public Image IconImage => iconImage;
    public TMP_Text DescriptionText => descriptionText;
    public TMP_Text ProgressText => progressText;
    public Slider ProgressSlider => progressSlider;
    public Image RewardIcon => rewardIcon;
    public TMP_Text RewardText => rewardText;
    public GameObject CompleteOverlay => completeOverlay;
    public TMP_Text CompleteIndicator => completeIndicator;
}
