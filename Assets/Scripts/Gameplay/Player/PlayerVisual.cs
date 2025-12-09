using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisible(bool visible)
    {
        if (_renderers == null) return;

        foreach (var r in _renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    public void ResetVisual()
    {
        SetVisible(true);
        // If you scale / rotate / detach cubes on explosion, reset here too.
        transform.localScale = Vector3.one;
    }
}
