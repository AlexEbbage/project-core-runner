using UnityEngine;

public partial class ObstacleRingGenerator
{
    #region Color / random / utility

    private void ApplyColor(RingInstance ring, int index, float time)
    {
        if (ring == null || ring.obstacleInstance == null || obstacleColorGradient == null)
            return;

        float t = 0.5f + 0.5f * Mathf.Sin(time);
        Color baseColor = obstacleColorGradient.Evaluate(t);
        Color darkColor = Color.Lerp(baseColor, Color.black, darkenFactor);
        bool isEven = (index % 2) == 0;
        Color c = isEven ? baseColor : darkColor;

        if (_colorPropertyBlock == null)
        {
            _colorPropertyBlock = new MaterialPropertyBlock();
        }

        var renderers = ring.renderers;
        if (renderers == null || renderers.Length == 0)
        {
            renderers = ring.obstacleInstance.GetComponentsInChildren<Renderer>();
            ring.renderers = renderers;
        }
        foreach (var rend in renderers)
        {
            if (rend != null)
            {
                rend.GetPropertyBlock(_colorPropertyBlock);
                _colorPropertyBlock.SetColor(ColorProperty, c);
                rend.SetPropertyBlock(_colorPropertyBlock);
            }
        }
    }

    #endregion
}
