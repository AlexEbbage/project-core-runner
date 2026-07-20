using System;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunStatsTrackerV2 : MonoBehaviour
    {
        [SerializeField] private Transform player;
        private bool _running;
        private float _startZ;
        private float _distance;
        private float _duration;
        private int _powerups;
        private int _reportedDistance;

        public float Distance => _distance;
        public float Duration => _duration;
        public int PowerupsCollected => _powerups;
        public event Action<int> DistanceChanged;

        public void BeginRun()
        {
            _running = true;
            _duration = 0;
            _distance = 0;
            _reportedDistance = 0;
            _powerups = 0;
            _startZ = player != null ? player.position.z : 0f;
            DistanceChanged?.Invoke(0);
        }

        public void EndRun() => _running = false;
        public void RecordPowerupCollected() => _powerups++;

        private void Update()
        {
            if (!_running)
                return;

            _duration += UnityEngine.Time.deltaTime;
            if (player != null)
            {
                _distance = Mathf.Max(0f, player.position.z - _startZ);
                var wholeDistance = Mathf.FloorToInt(_distance);
                if (wholeDistance != _reportedDistance)
                {
                    _reportedDistance = wholeDistance;
                    DistanceChanged?.Invoke(wholeDistance);
                }
            }
        }
    }
}
