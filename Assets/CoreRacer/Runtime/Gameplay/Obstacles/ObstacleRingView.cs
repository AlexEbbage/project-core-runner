using CoreRacer.Common.Pooling;
using UnityEngine;

namespace CoreRacer.Gameplay.Obstacles
{
    public sealed class ObstacleRingView : PoolableBehaviour
    {
        [SerializeField] private Transform segmentsRoot;
        [SerializeField] private GameObject segmentPrefab;
        private GameObject[] _segments;
        private GameObject _authoredObstacle;
        private GameObject _authoredObstaclePrefab;

        public float Z => transform.position.z;
        public string PatternId { get; private set; }
        public bool UsesAuthoredObstacle => _authoredObstacle != null && _authoredObstacle.activeSelf;

        public void Build(ObstaclePatternDefinition pattern, int sideCount, float z)
        {
            transform.position = new Vector3(0f, 0f, z);
            PatternId = pattern != null ? pattern.Id : string.Empty;
            EnsureSegments(sideCount);

            for (int i = 0; i < _segments.Length; i++)
                _segments[i].SetActive(false);

            if (pattern != null && pattern.ObstaclePrefab != null)
            {
                EnsureAuthoredObstacle(pattern.ObstaclePrefab);
                _authoredObstacle.SetActive(true);
                var controller = _authoredObstacle.GetComponent<ObstacleRingController>();
                if (controller != null)
                    controller.Configure(Random.Range(pattern.MinRotationSpeedDegrees, pattern.MaxRotationSpeedDegrees));
            }
            else if (_authoredObstacle != null)
            {
                _authoredObstacle.SetActive(false);
            }

            if ((pattern == null || pattern.ObstaclePrefab == null) && pattern != null && pattern.Segments != null)
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
            var sideAngle = 360f / Mathf.Max(1, sideCount);
            rotation = Mathf.Round(rotation / sideAngle) * sideAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private void EnsureAuthoredObstacle(GameObject prefab)
        {
            if (_authoredObstacle != null && _authoredObstaclePrefab == prefab)
                return;

            if (_authoredObstacle != null)
                Destroy(_authoredObstacle);

            _authoredObstaclePrefab = prefab;
            _authoredObstacle = Instantiate(prefab, segmentsRoot != null ? segmentsRoot : transform);
            _authoredObstacle.name = prefab.name;
            _authoredObstacle.transform.localPosition = Vector3.zero;
            _authoredObstacle.transform.localRotation = Quaternion.identity;
            _authoredObstacle.transform.localScale = Vector3.one;
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
