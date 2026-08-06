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
        void RefreshCurrent();
    }

    public sealed class CoreRacerScreenRouter : ICoreRacerScreenRouter, IDisposable
    {
        private readonly Dictionary<CoreRacerScreenId, IUiScreenPresenter> _screens;
        private readonly Dictionary<CoreRacerScreenId, Button> _navigation;
        private IUiScreenPresenter _currentPresenter;
        private bool _disposed;

        public CoreRacerScreenRouter(
            IDictionary<CoreRacerScreenId, IUiScreenPresenter> screens,
            IDictionary<CoreRacerScreenId, Button> navigation)
        {
            if (screens == null)
                throw new ArgumentNullException(nameof(screens));
            if (navigation == null)
                throw new ArgumentNullException(nameof(navigation));

            _screens = new Dictionary<CoreRacerScreenId, IUiScreenPresenter>(screens);
            _navigation = new Dictionary<CoreRacerScreenId, Button>(navigation);
            if (_screens.Count == 0)
                throw new ArgumentException("At least one screen must be registered.", nameof(screens));

            foreach (var pair in _screens)
                pair.Value.Initialize();
        }

        public CoreRacerScreenId Current { get; private set; }
        public event Action<CoreRacerScreenId> Changed;

        public void Show(CoreRacerScreenId screen)
        {
            ThrowIfDisposed();
            if (!_screens.TryGetValue(screen, out var target))
                throw new InvalidOperationException($"Screen '{screen}' is not registered.");

            if (!ReferenceEquals(_currentPresenter, target))
            {
                _currentPresenter?.Hide();
                foreach (var pair in _screens)
                {
                    if (!ReferenceEquals(pair.Value, target))
                        pair.Value.Hide();
                }
                _currentPresenter = target;
            }

            Current = screen;
            target.Show();
            UpdateNavigation(screen);
            Changed?.Invoke(screen);
        }

        public void RefreshCurrent()
        {
            ThrowIfDisposed();
            _currentPresenter?.Refresh();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var pair in _screens)
                pair.Value.Dispose();
            _screens.Clear();
            _navigation.Clear();
            _currentPresenter = null;
            Changed = null;
        }

        private void UpdateNavigation(CoreRacerScreenId selected)
        {
            foreach (var pair in _navigation)
            {
                pair.Value.EnableInClassList(UiClassNames.Selected, pair.Key == selected);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CoreRacerScreenRouter));
        }
    }
}
