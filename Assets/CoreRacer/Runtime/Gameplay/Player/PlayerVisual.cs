using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float bankDegrees = 18f;
        [SerializeField] private float bankSharpness = 10f;
        private float _lastAngle;
        private Coroutine _dissolveRoutine;

        private void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform;

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void SetVisible(bool visible)
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
        }

        public void PlayDissolve(float duration = 0.35f)
        {
            if (_dissolveRoutine != null)
                StopCoroutine(_dissolveRoutine);
            _dissolveRoutine = StartCoroutine(DissolveRoutine(Mathf.Max(0.01f, duration)));
        }

        public void RestoreVisible()
        {
            if (_dissolveRoutine != null)
                StopCoroutine(_dissolveRoutine);
            _dissolveRoutine = null;
            SetVisible(true);
            SetDissolveAmount(0f);
        }

        public void SetBankFromInput(float horizontalInput)
        {
            if (visualRoot == null)
                return;

            var target = Quaternion.Euler(0f, 0f, -Mathf.Clamp(horizontalInput, -1f, 1f) * bankDegrees);
            var t = 1f - Mathf.Exp(-bankSharpness * UnityEngine.Time.deltaTime);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, target, t);
        }

        public void SetBankFromMotor(PlayerOrbitalMotor motor)
        {
            if (motor == null)
                return;

            var delta = Mathf.DeltaAngle(_lastAngle, motor.AngleDegrees) / Mathf.Max(UnityEngine.Time.deltaTime, 0.001f);
            _lastAngle = motor.AngleDegrees;
            SetBankFromInput(Mathf.Clamp(delta / 180f, -1f, 1f));
        }

        private System.Collections.IEnumerator DissolveRoutine(float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                SetDissolveAmount(elapsed / duration);
                yield return null;
            }

            SetDissolveAmount(1f);
            SetVisible(false);
            _dissolveRoutine = null;
        }

        private void SetDissolveAmount(float amount)
        {
            if (renderers == null)
                return;

            var block = new MaterialPropertyBlock();
            block.SetFloat("_DissolveAmount", Mathf.Clamp01(amount));
            for (var i = 0; i < renderers.Length; i++)
                if (renderers[i] != null)
                    renderers[i].SetPropertyBlock(block);
        }
    }
}
