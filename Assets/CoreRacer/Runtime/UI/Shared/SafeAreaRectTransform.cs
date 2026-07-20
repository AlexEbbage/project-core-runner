using UnityEngine;

namespace CoreRacer.UI.Shared
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaRectTransform : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        public Rect LastAppliedSafeArea => _lastSafeArea;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (_lastSafeArea != Screen.safeArea || _lastScreenSize != screenSize)
                Apply();
        }

        public void Apply()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize.x <= 0 || screenSize.y <= 0)
                return;

            var anchors = Normalize(Screen.safeArea, screenSize);
            _rectTransform.anchorMin = anchors.min;
            _rectTransform.anchorMax = anchors.max;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = screenSize;
        }

        public static Rect Normalize(Rect safeArea, Vector2 screenSize)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
                return new Rect(0f, 0f, 1f, 1f);

            return Rect.MinMaxRect(
                safeArea.xMin / screenSize.x,
                safeArea.yMin / screenSize.y,
                safeArea.xMax / screenSize.x,
                safeArea.yMax / screenSize.y);
        }
    }
}
