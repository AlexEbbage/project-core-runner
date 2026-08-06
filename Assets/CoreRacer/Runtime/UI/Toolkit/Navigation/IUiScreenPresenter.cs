using System;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public interface IUiScreenPresenter : IDisposable
    {
        CoreRacerScreenId Id { get; }
        VisualElement Root { get; }
        bool IsVisible { get; }
        void Initialize();
        void Show();
        void Hide();
        void Refresh();
    }

    public abstract class UiScreenPresenterBase : IUiScreenPresenter
    {
        private readonly IUiAnimationService _animations;
        private bool _initialized;

        protected UiScreenPresenterBase(CoreRacerScreenId id, VisualElement root, IUiAnimationService animations)
        {
            Id = id;
            Root = root ?? throw new ArgumentNullException(nameof(root));
            _animations = animations ?? throw new ArgumentNullException(nameof(animations));
        }

        public CoreRacerScreenId Id { get; }
        public VisualElement Root { get; }
        public bool IsVisible => !Root.ClassListContains(UiClassNames.Hidden);

        public void Initialize()
        {
            if (_initialized)
                return;

            OnInitialize();
            _initialized = true;
        }

        public void Show()
        {
            Initialize();
            Refresh();
            UiVisibility.SetVisible(Root, true);
            _animations.ShowScreen(Root);
            OnShown();
        }

        public void Hide()
        {
            if (!_initialized)
            {
                UiVisibility.SetVisible(Root, false);
                return;
            }

            _animations.Stop(Root);
            UiVisibility.SetVisible(Root, false);
            OnHidden();
        }

        public abstract void Refresh();

        public void Dispose()
        {
            if (!_initialized)
                return;

            OnDispose();
            _initialized = false;
        }

        protected abstract void OnInitialize();
        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
        protected abstract void OnDispose();
    }
}
