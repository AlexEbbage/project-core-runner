using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class UiSafeAreaController
    {
        private readonly VisualElement _safeArea;
        private Rect _lastSafeArea;
        private Vector2Int _lastResolution;

        public UiSafeAreaController(VisualElement safeArea)
        {
            _safeArea = safeArea;
        }

        public void Refresh()
        {
            if (_safeArea == null || _safeArea.panel == null || Screen.width <= 0 || Screen.height <= 0)
                return;

            var current = Screen.safeArea;
            var resolution = new Vector2Int(Screen.width, Screen.height);
            if (current == _lastSafeArea && resolution == _lastResolution)
                return;

            _lastSafeArea = current;
            _lastResolution = resolution;

            var panel = _safeArea.panel;
            var topLeft = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(current.xMin, Screen.height - current.yMax));
            var bottomRight = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(current.xMax, Screen.height - current.yMin));
            var panelWidth = panel.visualTree.resolvedStyle.width;
            var panelHeight = panel.visualTree.resolvedStyle.height;

            _safeArea.style.paddingLeft = Mathf.Max(0f, topLeft.x);
            _safeArea.style.paddingTop = Mathf.Max(0f, topLeft.y);
            _safeArea.style.paddingRight = Mathf.Max(0f, panelWidth - bottomRight.x);
            _safeArea.style.paddingBottom = Mathf.Max(0f, panelHeight - bottomRight.y);
        }
    }
}
