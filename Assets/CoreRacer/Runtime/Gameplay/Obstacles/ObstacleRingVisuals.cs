using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleRingVisuals : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material safeMaterial;
        [SerializeField] private Material dangerMaterial;
        [SerializeField] private AnimationCurve pulse = AnimationCurve.EaseInOut(0f, 0.8f, 1f, 1.2f);
        [SerializeField] private float pulseSpeed = 2f;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void SetDanger(bool danger)
        {
            var material = danger ? dangerMaterial : safeMaterial;
            if (material == null || renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null)
                    renderers[i].sharedMaterial = material;
        }

        private void Update()
        {
            var value = pulse.Evaluate(Mathf.PingPong(UnityEngine.Time.time * pulseSpeed, 1f));
            transform.localScale = Vector3.one * value;
        }
    }
}
