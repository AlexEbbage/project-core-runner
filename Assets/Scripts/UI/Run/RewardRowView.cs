using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardRowView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;

    public Image IconImage => iconImage;
    public TMP_Text LabelText => labelText;
    public TMP_Text ValueText => valueText;
}
