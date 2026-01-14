using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionRewardNodeView : MonoBehaviour
{
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardLabel;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private GameObject lockedState;
    [SerializeField] private GameObject claimableState;
    [SerializeField] private GameObject claimedState;
    [SerializeField] private Button claimButton;

    public Image RewardIcon => rewardIcon;
    public TMP_Text RewardLabel => rewardLabel;
    public TMP_Text PointsText => pointsText;
    public GameObject LockedState => lockedState;
    public GameObject ClaimableState => claimableState;
    public GameObject ClaimedState => claimedState;
    public Button ClaimButton => claimButton;

    public void SetState(ProgressionRewardState state)
    {
        if (lockedState != null)
            lockedState.SetActive(state == ProgressionRewardState.Locked);
        if (claimableState != null)
            claimableState.SetActive(state == ProgressionRewardState.Claimable);
        if (claimedState != null)
            claimedState.SetActive(state == ProgressionRewardState.Claimed);
    }
}
