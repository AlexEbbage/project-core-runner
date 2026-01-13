using UnityEngine;
using UnityEngine.Events;

public class UiInteractionHandler : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip clickSfxOverride;
    [SerializeField] private UnityEvent onClick;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();
    }

    public void HandleClick()
    {
        if (audioManager != null)
        {
            if (clickSfxOverride != null)
                audioManager.PlaySfx(clickSfxOverride);
            else
                audioManager.PlayButtonClick();
        }

        onClick?.Invoke();
    }
}
