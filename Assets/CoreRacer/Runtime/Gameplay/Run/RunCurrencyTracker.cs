using System;
using CoreRacer.Config.Gameplay;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunCurrencyTracker : MonoBehaviour
    {
        [SerializeField] private GameBalanceConfigV2 balance;
        private int _coins;
        private float _coinMultiplier = 1f;
        private float _runCoinMultiplier = 1f;

        public int Coins => _coins;
        public float RunCoinMultiplier => _runCoinMultiplier;
        public float EffectiveCoinMultiplier => _coinMultiplier * _runCoinMultiplier;
        public event Action<int, int> CoinsChanged;

        public void BeginRun()
        {
            _coins = 0;
            _coinMultiplier = 1f;
            _runCoinMultiplier = 1f;
            CoinsChanged?.Invoke(0, _coins);
        }

        public void SetCoinMultiplier(float multiplier)
        {
            _coinMultiplier = Mathf.Max(1f, multiplier);
        }

        public void SetRunCoinMultiplier(float multiplier)
        {
            _runCoinMultiplier = Mathf.Max(1f, multiplier);
        }

        public void AddCoinPickup(int pickupCount = 1)
        {
            var baseValue = balance != null ? balance.CoinValue : 1;
            var added = Mathf.Max(1, Mathf.RoundToInt(baseValue * pickupCount * EffectiveCoinMultiplier));
            _coins += added;
            CoinsChanged?.Invoke(added, _coins);
        }
    }
}
