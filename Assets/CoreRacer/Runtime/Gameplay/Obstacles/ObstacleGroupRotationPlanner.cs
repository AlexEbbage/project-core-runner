namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleGroupRotationPlanner
    {
        private int _lastSide = -1;

        public int NextSide(int sideCount, int proposedSide)
        {
            if (sideCount <= 1)
                return 0;

            var side = ((proposedSide % sideCount) + sideCount) % sideCount;
            if (side == _lastSide)
                side = (side + 1) % sideCount;

            _lastSide = side;
            return side;
        }

        public void Reset()
        {
            _lastSide = -1;
        }
    }
}
