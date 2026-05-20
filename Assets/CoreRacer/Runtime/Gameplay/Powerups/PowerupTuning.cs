namespace CoreRacer.Gameplay.Powerups
{
    [System.Serializable]
    public struct PowerupTuning
    {
        public float Duration;
        public float Strength;

        public PowerupTuning(float duration, float strength)
        {
            Duration = duration;
            Strength = strength;
        }
    }
}
