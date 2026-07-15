using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerOrbitalMotor motor;
        [SerializeField] private bool running;
        [SerializeField] private bool autoPilotActive;
        [SerializeField] private float autoPilotInput;

        private TutorialService _tutorial;
        private bool _tutorialInputNotified;
        private TrailRenderer[] _trails;

        public PlayerOrbitalMotor Motor => motor;

        private void Awake()
        {
            _trails = GetComponentsInChildren<TrailRenderer>(true);
        }

        public void BeginRun()
        {
            running = true;
            _tutorialInputNotified = false;
            if (motor != null)
                motor.ResetMotor(transform.position.z);

            for (var i = 0; i < _trails.Length; i++)
                _trails[i].Clear();
        }

        public void EndRun()
        {
            running = false;
            autoPilotActive = false;
            autoPilotInput = 0f;
            motor?.SetSpeedMultiplier(1f);
        }

        public void SetAutoPilot(bool active, float input = 0f)
        {
            autoPilotActive = active;
            autoPilotInput = Mathf.Clamp(input, -1f, 1f);
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            motor?.SetSpeedMultiplier(multiplier);
        }

        private void Update()
        {
            if (!running || motor == null)
                return;

            var input = autoPilotActive ? autoPilotInput : (inputReader != null ? inputReader.Read().Horizontal : ReadFallbackHorizontal());
            if (!_tutorialInputNotified && Mathf.Abs(input) > 0.01f)
            {
                _tutorialInputNotified = true;
                if (_tutorial == null) GameServices.TryGet(out _tutorial);
                _tutorial?.Notify(TutorialStepKind.WaitForInput, "player");
            }
            motor.Move(input, UnityEngine.Time.deltaTime);
        }

        private float ReadFallbackHorizontal()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetAxisRaw("Horizontal");
#else
            return 0f;
#endif
        }
    }
}
