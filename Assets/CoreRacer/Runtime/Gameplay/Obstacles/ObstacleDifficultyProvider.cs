namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleDifficultyProvider
    {
        private readonly ObstacleGenerationConfig _config;
        private float _elapsed;
        private float _multiplier = 1f;

        public float CurrentDifficulty => (_config.BaseDifficulty + _elapsed * _config.DifficultyPerSecond) * _multiplier;

        public ObstacleDifficultyProvider(ObstacleGenerationConfig config)
        {
            _config = config;
        }

        public void Reset()
        {
            _elapsed = 0f;
        }

        public void SetMultiplier(float multiplier)
        {
            _multiplier = UnityEngine.Mathf.Max(0.05f, multiplier);
        }

        public void Tick(float deltaTime)
        {
            _elapsed += UnityEngine.Mathf.Max(0f, deltaTime);
        }
    }
}
