using System.Collections.Generic;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.Hud
{
    public sealed class PowerupStripView : MonoBehaviour
    {
        [SerializeField] private Text activeText;
        private readonly Dictionary<PowerupType, float> _active = new Dictionary<PowerupType, float>();

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

        private void Update()
        {
            if (_active.Count == 0) return;
            var keys = new List<PowerupType>(_active.Keys);
            for (int i = 0; i < keys.Count; i++)
                _active[keys[i]] = Mathf.Max(0f, _active[keys[i]] - UnityEngine.Time.deltaTime);
            Refresh();
        }

        private void Refresh()
        {
            if (activeText == null) return;
            if (_active.Count == 0)
            {
                activeText.text = string.Empty;
                return;
            }

            var parts = new List<string>();
            foreach (var pair in _active)
                parts.Add($"{pair.Key} {pair.Value:0}s");
            activeText.text = string.Join("  ", parts.ToArray());
        }
    }
}
