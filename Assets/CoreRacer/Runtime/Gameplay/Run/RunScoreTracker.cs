using System;
using CoreRacer.Config.Gameplay;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunScoreTracker : MonoBehaviour
    {
        [SerializeField] private GameBalanceConfigV2 balance;
        [SerializeField] private Transform player;

        private float _lastZ;
        private bool _running;
        private float _score;
        private float _combo;
        private float _scorePowerupMultiplier = 1f;

        public int CurrentScore => Mathf.FloorToInt(_score);
        public float Combo => _combo;
        public float CurrentMultiplier => 1f + _combo * ComboToMultiplierFactor;
        public float ComboToMultiplierFactor => balance != null ? balance.ComboToMultiplierFactor : 0.1f;
        public event Action<int> ScoreChanged;
        public event Action<float> ComboChanged;

        public void BeginRun()
        {
            _running = true;
            _score = 0;
            _combo = 0;
            _lastZ = player != null ? player.position.z : 0f;
            ScoreChanged?.Invoke(CurrentScore);
            ComboChanged?.Invoke(_combo);
        }

        public void EndRun() => _running = false;

        public void SetScorePowerupMultiplier(float multiplier)
        {
            _scorePowerupMultiplier = Mathf.Max(1f, multiplier);
        }

        public void AddPickupScore(int baseScore)
        {
            var add = baseScore * CurrentMultiplier * _scorePowerupMultiplier;
            _score += add;
            _combo = Mathf.Min(MaxCombo, _combo + ComboPerPickup);
            ScoreChanged?.Invoke(CurrentScore);
            ComboChanged?.Invoke(_combo);
        }

        private void Update()
        {
            if (!_running || player == null)
                return;

            var z = player.position.z;
            var distance = Mathf.Max(0f, z - _lastZ);
            _lastZ = z;
            _score += distance * DistanceMultiplier * _scorePowerupMultiplier;
            _combo = Mathf.Max(0f, _combo - ComboDecay * UnityEngine.Time.deltaTime);
            ScoreChanged?.Invoke(CurrentScore);
            ComboChanged?.Invoke(_combo);
        }

        private float DistanceMultiplier => balance != null ? balance.DistanceScoreMultiplier : 1f;
        private float ComboPerPickup => balance != null ? balance.ComboIncreasePerPickup : 1f;
        private float MaxCombo => balance != null ? balance.MaxComboValue : 10f;
        private float ComboDecay => balance != null ? balance.ComboDecayPerSecond : 1f;
    }
}
