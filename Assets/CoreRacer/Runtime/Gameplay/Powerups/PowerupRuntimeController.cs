using System;
using System.Collections.Generic;
using CoreRacer.Meta.Profile;
using UnityEngine;

namespace CoreRacer.Gameplay.Powerups
{
    public sealed class PowerupRuntimeController : MonoBehaviour
    {
        [SerializeField] private PowerupUpgradeConfigV2 upgradeConfig;
        [SerializeField] private PowerupContextBuilder contextBuilder;

        private readonly Dictionary<PowerupType, ActivePowerup> _active = new Dictionary<PowerupType, ActivePowerup>();
        private PowerupEffectRegistry _registry;
        private PowerupContext _context;
        private PlayerProfileService _profile;

        public event Action<PowerupType, float> PowerupActivated;
        public event Action<PowerupType> PowerupExpired;

        private void Awake()
        {
            _registry = new PowerupEffectRegistry();
            _context = contextBuilder != null ? contextBuilder.Build() : new PowerupContext();
            CoreRacer.Bootstrap.GameServices.TryGet(out _profile);
        }

        public void Activate(PowerupType type)
        {
            IPowerupEffect effect;
            if (!_registry.TryGet(type, out effect))
                return;

            var level = _profile != null ? _profile.GetUpgradeLevel(_profile.State.PowerupUpgradeLevels, type.ToString()) : 0;
            var tuning = upgradeConfig != null ? upgradeConfig.GetEntry(type).GetTuning(level) : new PowerupTuning(5f, 1f);

            ActivePowerup active;
            if (_active.TryGetValue(type, out active))
            {
                active.Refresh(tuning);
                PowerupActivated?.Invoke(type, tuning.Duration);
                return;
            }

            active = new ActivePowerup(type, effect, tuning);
            _active[type] = active;
            effect.Activate(_context, tuning);
            PowerupActivated?.Invoke(type, tuning.Duration);
        }

        private void Update()
        {
            if (_active.Count == 0)
                return;

            var expired = new List<PowerupType>();
            foreach (var pair in _active)
            {
                pair.Value.RemainingSeconds -= UnityEngine.Time.deltaTime;
                pair.Value.Effect.Tick(_context, UnityEngine.Time.deltaTime);
                if (pair.Value.RemainingSeconds <= 0f)
                    expired.Add(pair.Key);
            }

            for (int i = 0; i < expired.Count; i++)
            {
                var type = expired[i];
                var active = _active[type];
                active.Effect.Deactivate(_context);
                _active.Remove(type);
                PowerupExpired?.Invoke(type);
            }
        }
    }
}
