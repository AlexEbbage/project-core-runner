using UnityEngine;
using UnityEngine.UI;

public class SpeedUpFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float maxAlpha = 0.4f;
    [SerializeField] private float fadeDuration = 0.15f;

    private void Awake()
    {
        if (flashImage == null)
            flashImage = GetComponent<Image>();

        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    public void PlayFlash()
    {
        if (flashImage == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        Color c = flashImage.color;
        float t = 0f;

        // up
        while (t < fadeDuration * 0.5f)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / (fadeDuration * 0.5f);
            c.a = Mathf.Lerp(0f, maxAlpha, lerp);
            flashImage.color = c;
            yield return null;
        }

        // down
        t = 0f;
        while (t < fadeDuration * 0.5f)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / (fadeDuration * 0.5f);
            c.a = Mathf.Lerp(maxAlpha, 0f, lerp);
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
    }
}
