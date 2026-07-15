using System;
using CoreRacer.Config.Gameplay;
using UnityEngine;

namespace CoreRacer.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private GameBalanceConfigV2 balance;
        private float _health;
        private float _invulnerableUntil;
        private float _maxHealthBonus;

        public float CurrentHealth => _health;
        public float MaxHealth => (balance != null ? balance.MaxHealth : 2f) + _maxHealthBonus;
        public bool IsAlive => _health > 0f;
        public bool IsInvulnerable => UnityEngine.Time.time < _invulnerableUntil;
        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake() => ResetHealth();

        public void SetMaxHealthBonus(float bonus)
        {
            _maxHealthBonus = Mathf.Max(0f, bonus);
        }

        public void ResetHealth()
        {
            _health = MaxHealth;
            _invulnerableUntil = 0f;
            HealthChanged?.Invoke(_health, MaxHealth);
        }

        public void Revive(float invulnerabilitySeconds)
        {
            _health = MaxHealth;
            _invulnerableUntil = UnityEngine.Time.time + Mathf.Max(0f, invulnerabilitySeconds);
            HealthChanged?.Invoke(_health, MaxHealth);
        }

        public void Damage(float amount)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0f)
                return;

            _health = Mathf.Max(0f, _health - amount);
            HealthChanged?.Invoke(_health, MaxHealth);
            if (_health <= 0f)
                Died?.Invoke();
        }
    }
}
