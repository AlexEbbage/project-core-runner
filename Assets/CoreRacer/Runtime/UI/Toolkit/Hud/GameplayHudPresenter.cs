using System;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class GameplayHudPresenter : IDisposable
    {
        private readonly GameplayHudView _view;
        private readonly CoreRacerUiContext _context;
        private readonly IUiAnimationService _animations;
        private bool _initialized;

        public GameplayHudPresenter(GameplayHudView view, CoreRacerUiContext context, IUiAnimationService animations)
        {
            _view = view;
            _context = context;
            _animations = animations;
        }

        public void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;
            _view.Pause.clicked += Pause;
            var refs = _context.RunReferences;
            if (refs?.ScoreTracker != null) refs.ScoreTracker.ScoreChanged += SetScore;
            if (refs?.StatsTracker != null) refs.StatsTracker.DistanceChanged += SetDistance;
            if (refs?.CurrencyTracker != null) refs.CurrencyTracker.CoinsChanged += SetCoins;
            if (refs?.PlayerHealth != null) refs.PlayerHealth.HealthChanged += SetHealth;
            if (refs?.Powerups != null)
            {
                refs.Powerups.PowerupActivated += AddPowerup;
                refs.Powerups.PowerupExpired += RemovePowerup;
            }
            Reset();
        }

        public void Show()
        {
            Initialize();
            UiVisibility.SetVisible(_view.Root, true, false);
            _animations.ShowScreen(_view.Root);
        }

        public void Hide()
        {
            _animations.Stop(_view.Root);
            UiVisibility.SetVisible(_view.Root, false);
        }

        public void Reset()
        {
            _view.Distance.text = "0m";
            _view.Score.text = "0";
            _view.Coins.text = "0";
            _view.Health.text = string.Empty;
            _view.Zone.text = "ZONE 1";
            _view.Progress.value = 0f;
            _view.Powerups.Clear();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;
            _initialized = false;
            _view.Pause.clicked -= Pause;
            var refs = _context.RunReferences;
            if (refs?.ScoreTracker != null) refs.ScoreTracker.ScoreChanged -= SetScore;
            if (refs?.StatsTracker != null) refs.StatsTracker.DistanceChanged -= SetDistance;
            if (refs?.CurrencyTracker != null) refs.CurrencyTracker.CoinsChanged -= SetCoins;
            if (refs?.PlayerHealth != null) refs.PlayerHealth.HealthChanged -= SetHealth;
            if (refs?.Powerups != null)
            {
                refs.Powerups.PowerupActivated -= AddPowerup;
                refs.Powerups.PowerupExpired -= RemovePowerup;
            }
        }

        private void Pause()
        {
            if (_context.RunController != null && _context.RunController.State == CoreRacer.Gameplay.Run.RunState.Running)
                _context.RunController.PauseRun();
        }

        private void SetScore(int value) => _view.Score.text = value.ToString("N0");
        private void SetDistance(int value)
        {
            _view.Distance.text = $"{value:N0}m";
            _view.Progress.value = Mathf.Repeat(value, 1000) / 10f;
        }
        private void SetCoins(int value, int _) => _view.Coins.text = value.ToString("N0");
        private void SetHealth(float current, float max) => _view.Health.text = current >= max ? string.Empty : $"HULL {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

        private void AddPowerup(PowerupType type, float seconds)
        {
            RemovePowerup(type);
            var indicator = new VisualElement { name = "Powerup_" + type };
            indicator.AddToClassList("hud-powerup");
            var icon = new Label(PowerupGlyph(type));
            icon.AddToClassList("hud-powerup__icon");
            var label = new Label($"{type.ToString().ToUpperInvariant()}  {seconds:0}s");
            label.AddToClassList("hud-powerup__label");
            indicator.Add(icon);
            indicator.Add(label);
            _view.Powerups.Add(indicator);
            _animations.PlaySuccess(indicator);
        }

        private void RemovePowerup(PowerupType type)
        {
            _view.Powerups.Q<VisualElement>("Powerup_" + type)?.RemoveFromHierarchy();
        }

        private static string PowerupGlyph(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Shield: return "◆";
                case PowerupType.Magnet: return "U";
                case PowerupType.SlowMo: return "◷";
                case PowerupType.ScoreMultiplier: return "2X";
                case PowerupType.CoinMultiplier: return "C";
                default: return "⚡";
            }
        }
    }
}
