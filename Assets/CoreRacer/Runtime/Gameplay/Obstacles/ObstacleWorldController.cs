using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleWorldController : MonoBehaviour
    {
        [SerializeField] private ObstacleGenerationConfig config;
        [SerializeField] private Transform player;
        [SerializeField] private Transform ringParent;

        private ObstacleDifficultyProvider _difficulty;
        private ObstacleRingSpawner _spawner;
        private bool _running;

        public float CurrentDifficulty => _difficulty != null ? _difficulty.CurrentDifficulty : 0f;

        private void Awake()
        {
            if (config != null && config.RingPrefab != null)
            {
                _difficulty = new ObstacleDifficultyProvider(config);
                _spawner = new ObstacleRingSpawner(config, ringParent != null ? ringParent : transform);
            }
        }

        public void BeginRun()
        {
            _running = true;
            _difficulty?.Reset();
            _spawner?.Reset(player != null ? player.position.z : 0f);
        }

        public void EndRun()
        {
            _running = false;
        }

        private void Update()
        {
            if (!_running || player == null || _spawner == null || _difficulty == null)
                return;

            _difficulty.Tick(UnityEngine.Time.deltaTime);
            var z = player.position.z;
            _spawner.EnsureAhead(z, _difficulty.CurrentDifficulty);
            _spawner.RecycleBehind(z);
        }
    }
}
