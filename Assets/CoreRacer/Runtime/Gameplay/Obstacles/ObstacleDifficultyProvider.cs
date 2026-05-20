namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleDifficultyProvider
    {
        private readonly ObstacleGenerationConfig _config;
        private float _elapsed;

        public float CurrentDifficulty => _config.BaseDifficulty + _elapsed * _config.DifficultyPerSecond;

        public ObstacleDifficultyProvider(ObstacleGenerationConfig config)
        {
            _config = config;
        }

        public void Reset()
        {
            _elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
        }
    }
}
