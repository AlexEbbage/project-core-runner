using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    public sealed class TunnelAtmosphereController : MonoBehaviour
    {
        [SerializeField] private Light keyLight;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private Gradient zoneFogGradient;
        [SerializeField] private float fogDensity = 0.02f;
        [SerializeField] private float colourCycleSeconds = 30f;
        [SerializeField] private bool controlRenderSettings = true;

        private void Update()
        {
            var colour = zoneFogGradient != null
                ? zoneFogGradient.Evaluate(Mathf.PingPong(UnityEngine.Time.time / Mathf.Max(1f, colourCycleSeconds), 1f))
                : Color.black;

            if (keyLight != null)
                keyLight.color = colour;

            if (targetCamera != null)
                targetCamera.backgroundColor = Color.Lerp(Color.black, colour, 0.2f);

            if (controlRenderSettings)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = colour;
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }
}
