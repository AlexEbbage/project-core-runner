using System;
using CoreRacer.Meta.Boosters;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class BoosterOptionView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text actionText;
        [SerializeField] private Button selectButton;

        private string _boosterId;
        private Action<string> _onSelected;

        public string BoosterId => _boosterId;
        public Button SelectButton => selectButton;

        private void OnEnable()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(Select);
        }

        private void OnDisable()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(Select);
        }

        public void Bind(BoosterDefinition booster, bool equipped, Action<string> onSelected)
        {
            _boosterId = booster != null ? booster.Id : string.Empty;
            _onSelected = onSelected;
            if (nameText != null)
                nameText.text = booster != null ? $"{booster.Family}: {booster.DisplayName}" : "Missing booster";
            if (statusText != null)
                statusText.text = equipped ? "Equipped for next run" : BuildEffectText(booster);
            if (actionText != null)
                actionText.text = equipped ? "Unequip" : "Equip";
            if (selectButton != null)
                selectButton.interactable = booster != null;
        }

        private void Select()
        {
            if (!string.IsNullOrWhiteSpace(_boosterId))
                _onSelected?.Invoke(_boosterId);
        }

        private static string BuildEffectText(BoosterDefinition booster)
        {
            if (booster == null)
                return "Unavailable";

            switch (booster.EffectType)
            {
                case BoosterEffectType.StartShield:
                    return $"Start protected for {booster.Value:0.#}s";
                case BoosterEffectType.CoinMultiplier:
                    return $"x{booster.Value:0.#} coins this run";
                case BoosterEffectType.ScoreMultiplier:
                    return $"x{booster.Value:0.#} score this run";
                default:
                    return "Run booster";
            }
        }
    }
}
