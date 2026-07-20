using System.Collections.Generic;
using System.Text;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class PowerupStripView : MonoBehaviour
    {
        [SerializeField] private Text activeText;
        private readonly Dictionary<PowerupType, float> _active = new Dictionary<PowerupType, float>();
        private readonly List<PowerupType> _keys = new List<PowerupType>(8);
        private readonly StringBuilder _displayBuilder = new StringBuilder(128);
        private int _renderHash;

        public int ActiveCount => _active.Count;
        public string DisplayText => activeText != null ? activeText.text : string.Empty;

        public void SetActive(PowerupType type, float seconds)
        {
            _active[type] = seconds;
            Refresh();
        }

        public void Remove(PowerupType type)
        {
            _active.Remove(type);
            Refresh();
        }

        public void Clear()
        {
            _active.Clear();
            Refresh();
        }

        private void Update()
        {
            if (_active.Count == 0) return;
            _keys.Clear();
            _keys.AddRange(_active.Keys);
            for (int i = 0; i < _keys.Count; i++)
                _active[_keys[i]] = Mathf.Max(0f, _active[_keys[i]] - UnityEngine.Time.deltaTime);
            var renderHash = CalculateRenderHash();
            if (renderHash != _renderHash)
                Refresh();
        }

        private void Refresh()
        {
            if (activeText == null) return;
            if (_active.Count == 0)
            {
                activeText.text = string.Empty;
                _renderHash = 0;
                return;
            }

            _displayBuilder.Clear();
            foreach (var pair in _active)
            {
                if (_displayBuilder.Length > 0)
                    _displayBuilder.Append("   |   ");
                _displayBuilder.Append(GetDisplayName(pair.Key));
                _displayBuilder.Append("  ");
                _displayBuilder.Append(Mathf.CeilToInt(pair.Value));
                _displayBuilder.Append('s');
            }
            activeText.text = _displayBuilder.ToString();
            _renderHash = CalculateRenderHash();
        }

        private int CalculateRenderHash()
        {
            unchecked
            {
                var hash = 17;
                foreach (var pair in _active)
                {
                    hash = hash * 31 + (int)pair.Key;
                    hash = hash * 31 + Mathf.CeilToInt(pair.Value);
                }
                return hash;
            }
        }

        private static string GetDisplayName(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.ScoreMultiplier: return "2x SCORE";
                case PowerupType.CoinMultiplier: return "2x COINS";
                case PowerupType.AutoPilot: return "AUTO PILOT";
                case PowerupType.SlowMo: return "SLOW MO";
                default: return type.ToString().ToUpperInvariant();
            }
        }
    }
}
