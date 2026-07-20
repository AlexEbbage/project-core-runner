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
        private readonly List<PowerupType> _expired = new List<PowerupType>(8);
        private PowerupEffectRegistry _registry;
        private PowerupContext _context;
        private PlayerProfileService _profile;

        public event Action<PowerupType, float> PowerupActivated;
        public event Action<PowerupType> PowerupExpired;

        public bool TryGetRemainingSeconds(PowerupType type, out float remainingSeconds)
        {
            if (_active.TryGetValue(type, out var active))
            {
                remainingSeconds = active.RemainingSeconds;
                return true;
            }

            remainingSeconds = 0f;
            return false;
        }

        private void Awake()
        {
            _registry = new PowerupEffectRegistry();
            _context = contextBuilder != null ? contextBuilder.Build() : new PowerupContext();
            CoreRacer.Bootstrap.GameServices.TryGet(out _profile);
        }

        private void OnDisable()
        {
            ClearAll();
        }

        public void Activate(PowerupType type)
        {
            IPowerupEffect effect;
            if (!_registry.TryGet(type, out effect))
                return;

            if (_profile == null)
                CoreRacer.Bootstrap.GameServices.TryGet(out _profile);

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

        public void ClearAll()
        {
            if (_active.Count == 0)
                return;

            _expired.Clear();
            foreach (var pair in _active)
                _expired.Add(pair.Key);

            for (var i = 0; i < _expired.Count; i++)
            {
                var type = _expired[i];
                _active[type].Effect.Deactivate(_context);
                PowerupExpired?.Invoke(type);
            }

            _active.Clear();
            _expired.Clear();
        }

        private void Update()
        {
            if (_active.Count == 0)
                return;

            var delta = UnityEngine.Time.deltaTime;
            _expired.Clear();
            foreach (var pair in _active)
            {
                pair.Value.RemainingSeconds -= delta;
                pair.Value.Effect.Tick(_context, delta);
                if (pair.Value.RemainingSeconds <= 0f)
                    _expired.Add(pair.Key);
            }

            for (var i = 0; i < _expired.Count; i++)
            {
                var type = _expired[i];
                var active = _active[type];
                active.Effect.Deactivate(_context);
                _active.Remove(type);
                PowerupExpired?.Invoke(type);
            }
        }
    }
}
