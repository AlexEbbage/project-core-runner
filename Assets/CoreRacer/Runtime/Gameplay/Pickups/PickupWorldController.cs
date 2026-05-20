using System.Collections.Generic;
using CoreRacer.Common.Pooling;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using UnityEngine;

namespace CoreRacer.Gameplay.Pickups
{
    public sealed class PickupWorldController : MonoBehaviour
    {
        [SerializeField] private PickupGenerationConfig config;
        [SerializeField] private Transform player;
        [SerializeField] private Transform pickupParent;
        [SerializeField] private RunScoreTracker scoreTracker;
        [SerializeField] private RunCurrencyTracker currencyTracker;
        [SerializeField] private PowerupRuntimeController powerups;

        private ComponentPool<PickupView> _coinPool;
        private ComponentPool<PickupView> _powerupPool;
        private readonly List<PickupView> _active = new List<PickupView>();
        private PickupPatternGenerator _patterns;
        private PowerupLootTable _lootTable;
        private float _nextSpawnZ;
        private bool _running;

        private void Awake()
        {
            if (config == null) return;
            var parent = pickupParent != null ? pickupParent : transform;
            if (config.CoinPrefab != null) _coinPool = new ComponentPool<PickupView>(config.CoinPrefab, parent, config.PrewarmCoins);
            if (config.PowerupPrefab != null) _powerupPool = new ComponentPool<PickupView>(config.PowerupPrefab, parent, config.PrewarmPowerups);
            _patterns = new PickupPatternGenerator(config);
            _lootTable = new PowerupLootTable(config);
        }

        public void BeginRun()
        {
            if (config == null)
            {
                _running = false;
                return;
            }

            _running = true;
            ClearActive();
            _nextSpawnZ = (player != null ? player.position.z : 0f) + 24f;
        }

        public void EndRun()
        {
            _running = false;
        }

        private void Update()
        {
            if (!_running || player == null)
                return;

            while (_nextSpawnZ < player.position.z + config.SpawnAheadDistance)
            {
                SpawnPattern(_nextSpawnZ);
                _nextSpawnZ += Mathf.Max(2f, config.RingSpacing);
            }

            RecycleBehind(player.position.z);
        }

        private void SpawnPattern(float z)
        {
            if (Random.value < config.PowerupChance && _powerupPool != null)
            {
                SpawnPowerup(z);
                return;
            }

            if (_coinPool == null)
                return;

            var points = _patterns.GenerateArc(z);
            for (int i = 0; i < points.Count; i++)
                SpawnCoin(points[i]);
        }

        private void SpawnCoin(Vector3 position)
        {
            var pickup = _coinPool.Take();
            pickup.Type = PickupType.Coin;
            pickup.Amount = 1;
            pickup.transform.position = position;
            pickup.Collected += HandleCollected;
            _active.Add(pickup);
        }

        private void SpawnPowerup(float z)
        {
            var pickup = _powerupPool.Take();
            pickup.Type = PickupType.Powerup;
            pickup.PowerupType = _lootTable.Roll();
            pickup.transform.position = new Vector3(0f, config.RingRadius, z);
            pickup.Collected += HandleCollected;
            _active.Add(pickup);
        }

        private void HandleCollected(PickupView pickup)
        {
            pickup.Collected -= HandleCollected;
            _active.Remove(pickup);

            if (pickup.Type == PickupType.Coin)
            {
                currencyTracker?.AddCoinPickup(pickup.Amount);
                scoreTracker?.AddPickupScore(10);
                _coinPool.Return(pickup);
            }
            else
            {
                powerups?.Activate(pickup.PowerupType);
                _powerupPool.Return(pickup);
            }
        }

        private void RecycleBehind(float playerZ)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var pickup = _active[i];
                if (pickup.transform.position.z >= playerZ - config.RecycleBehindDistance)
                    continue;

                pickup.Collected -= HandleCollected;
                _active.RemoveAt(i);
                if (pickup.Type == PickupType.Coin) _coinPool.Return(pickup);
                else _powerupPool.Return(pickup);
            }
        }

        private void ClearActive()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var pickup = _active[i];
                pickup.Collected -= HandleCollected;
                if (pickup.Type == PickupType.Coin) _coinPool?.Return(pickup);
                else _powerupPool?.Return(pickup);
            }
            _active.Clear();
        }
    }
}
