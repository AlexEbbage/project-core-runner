using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float countdownSeconds = 3f;

    public bool IsCountingDown { get; private set; }

    public IEnumerator PlayCountdown(System.Action onComplete)
    {
        if (root != null) root.SetActive(true);

        IsCountingDown = true;

        float remaining = countdownSeconds;
        while (remaining > 0f)
        {
            int display = Mathf.CeilToInt(remaining);
            if (label != null)
                label.text = display.ToString();

            remaining -= Time.deltaTime;
            yield return null;
        }

        if (label != null) label.text = "GO!";

        yield return new WaitForSeconds(1f);

        if (root != null) root.SetActive(false);
        IsCountingDown = false;

        onComplete?.Invoke();
    }
}
