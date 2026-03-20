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

    private void Awake()
    {
        EnsureScaffold();
    }

    public void SetState(ProgressionRewardState state)
    {
        EnsureScaffold();

        if (lockedState != null)
            lockedState.SetActive(state == ProgressionRewardState.Locked);
        if (claimableState != null)
            claimableState.SetActive(state == ProgressionRewardState.Claimable);
        if (claimedState != null)
            claimedState.SetActive(state == ProgressionRewardState.Claimed);

        if (claimButton != null)
            claimButton.interactable = state == ProgressionRewardState.Claimable;
    }

    public void SetClaimAction(System.Action action)
    {
        EnsureScaffold();

        if (claimButton == null)
            return;

        claimButton.onClick.RemoveAllListeners();
        if (action != null)
            claimButton.onClick.AddListener(() => action());
    }

    private void EnsureScaffold()
    {
        if (rewardIcon == null)
            rewardIcon = GetComponentInChildren<Image>(true);

        if (rewardLabel == null)
            rewardLabel = GetComponentInChildren<TMP_Text>(true);

        if (claimButton == null)
            claimButton = GetComponentInChildren<Button>(true);
    }
}
