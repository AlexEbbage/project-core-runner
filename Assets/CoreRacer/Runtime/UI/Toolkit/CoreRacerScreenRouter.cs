using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public interface ICoreRacerScreenRouter
    {
        CoreRacerScreenId Current { get; }
        event Action<CoreRacerScreenId> Changed;
        void Show(CoreRacerScreenId screen);
    }

    public sealed class CoreRacerScreenRouter : ICoreRacerScreenRouter
    {
        private readonly Dictionary<CoreRacerScreenId, VisualElement> _screens;
        private readonly Dictionary<CoreRacerScreenId, Button> _navigation;
        private readonly IUiAnimationService _animations;
        public CoreRacerScreenId Current { get; private set; }
        public event Action<CoreRacerScreenId> Changed;

        public CoreRacerScreenRouter(Dictionary<CoreRacerScreenId, VisualElement> screens, Dictionary<CoreRacerScreenId, Button> navigation, IUiAnimationService animations)
        {
            _screens = screens ?? throw new ArgumentNullException(nameof(screens));
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _animations = animations ?? throw new ArgumentNullException(nameof(animations));
        }

        public void Show(CoreRacerScreenId screen)
        {
            if (!_screens.TryGetValue(screen, out var target))
                throw new InvalidOperationException($"Screen '{screen}' is not registered.");
            foreach (var pair in _screens) SetVisible(pair.Value, pair.Key == screen);
            foreach (var pair in _navigation) pair.Value.EnableInClassList("is-selected", pair.Key == screen);
            Current = screen;
            _animations.ShowScreen(target);
            Changed?.Invoke(screen);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            element.EnableInClassList("is-hidden", !visible);
            element.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
        }
    }
}
