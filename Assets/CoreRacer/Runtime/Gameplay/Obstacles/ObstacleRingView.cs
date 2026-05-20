using CoreRacer.Common.Pooling;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleRingView : PoolableBehaviour
    {
        [SerializeField] private Transform segmentsRoot;
        [SerializeField] private GameObject segmentPrefab;
        private GameObject[] _segments;

        public float Z => transform.position.z;

        public void Build(ObstaclePatternDefinition pattern, int sideCount, float z)
        {
            transform.position = new Vector3(0f, 0f, z);
            EnsureSegments(sideCount);

            for (int i = 0; i < _segments.Length; i++)
                _segments[i].SetActive(false);

            if (pattern != null && pattern.Segments != null)
            {
                for (int i = 0; i < pattern.Segments.Count; i++)
                {
                    var rule = pattern.Segments[i];
                    if (!rule.Blocked) continue;
                    var index = ((rule.SideIndex % sideCount) + sideCount) % sideCount;
                    _segments[index].SetActive(true);
                }
            }

            var rotation = pattern != null ? Random.Range(pattern.MinRotationDegrees, pattern.MaxRotationDegrees) : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private void EnsureSegments(int sideCount)
        {
            if (_segments != null && _segments.Length == sideCount)
                return;

            if (_segments != null)
                for (int i = 0; i < _segments.Length; i++)
                    if (_segments[i] != null) Destroy(_segments[i]);

            _segments = new GameObject[sideCount];
            for (int i = 0; i < sideCount; i++)
            {
                var segment = Instantiate(segmentPrefab, segmentsRoot != null ? segmentsRoot : transform);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, 360f * i / sideCount);
                _segments[i] = segment;
            }
        }
    }
}
