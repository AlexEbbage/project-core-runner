using System;
using CoreRacer.Services.Accessibility;
using CoreRacer.Services.Support;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class SettingsScreenPresenter : UiScreenPresenterBase
    {
        private readonly SettingsScreenView _view;
        private readonly CoreRacerUiContext _context;
        private readonly UiModalService _modal;
        private readonly UiToastService _toast;
        private readonly IUiAnimationService _animations;
        private readonly Action _openGallery;

        public SettingsScreenPresenter(
            SettingsScreenView view,
            CoreRacerUiContext context,
            IUiAnimationService animations,
            UiModalService modal,
            UiToastService toast,
            Action openGallery)
            : base(CoreRacerScreenId.Settings, view.Root, animations)
        {
            _view = view;
            _context = context;
            _animations = animations;
            _modal = modal;
            _toast = toast;
            _openGallery = openGallery;
        }

        protected override void OnInitialize()
        {
            _view.Music.RegisterValueChangedCallback(OnMusicChanged);
            _view.Sfx.RegisterValueChangedCallback(OnSfxChanged);
            _view.Haptics.RegisterValueChangedCallback(OnHapticsChanged);
            _view.DragControls.RegisterValueChangedCallback(OnDragChanged);
            _view.ReducedMotion.RegisterValueChangedCallback(OnReducedMotionChanged);
            _view.HighContrast.RegisterValueChangedCallback(OnContrastChanged);
            _view.Privacy.clicked += OpenPrivacy;
            _view.Support.clicked += OpenSupport;
            _view.ResetTutorial.clicked += ResetTutorial;
            _view.Gallery.clicked += OpenGallery;
            if (_context.Settings != null)
                _context.Settings.Changed += Refresh;
            if (_context.Accessibility != null)
                _context.Accessibility.Changed += OnAccessibilityChanged;
        }

        protected override void OnDispose()
        {
            _view.Music.UnregisterValueChangedCallback(OnMusicChanged);
            _view.Sfx.UnregisterValueChangedCallback(OnSfxChanged);
            _view.Haptics.UnregisterValueChangedCallback(OnHapticsChanged);
            _view.DragControls.UnregisterValueChangedCallback(OnDragChanged);
            _view.ReducedMotion.UnregisterValueChangedCallback(OnReducedMotionChanged);
            _view.HighContrast.UnregisterValueChangedCallback(OnContrastChanged);
            _view.Privacy.clicked -= OpenPrivacy;
            _view.Support.clicked -= OpenSupport;
            _view.ResetTutorial.clicked -= ResetTutorial;
            _view.Gallery.clicked -= OpenGallery;
            if (_context.Settings != null)
                _context.Settings.Changed -= Refresh;
            if (_context.Accessibility != null)
                _context.Accessibility.Changed -= OnAccessibilityChanged;
        }

        public override void Refresh()
        {
            if (_context.Settings != null)
            {
                _view.Music.SetValueWithoutNotify(_context.Settings.State.MusicVolume);
                _view.Sfx.SetValueWithoutNotify(_context.Settings.State.SfxVolume);
                _view.Haptics.SetValueWithoutNotify(_context.Settings.State.HapticsEnabled);
            }

            if (_context.Accessibility != null)
            {
                var state = _context.Accessibility.State;
                _view.DragControls.SetValueWithoutNotify(state.DragControlsEnabled);
                _view.ReducedMotion.SetValueWithoutNotify(state.ReducedVfxMode);
                _view.HighContrast.SetValueWithoutNotify(state.HighContrastMode);
                _view.Root.panel?.visualTree.EnableInClassList(UiClassNames.HighContrast, state.HighContrastMode);
                _animations.ReducedMotion = state.ReducedVfxMode;
            }
        }

        private void OnMusicChanged(ChangeEvent<float> evt) => _context.Settings?.SetMusicVolume(evt.newValue);
        private void OnSfxChanged(ChangeEvent<float> evt) => _context.Settings?.SetSfxVolume(evt.newValue);
        private void OnHapticsChanged(ChangeEvent<bool> evt)
        {
            _context.Settings?.SetHaptics(evt.newValue);
            _context.Accessibility?.Update(state => state.HapticsEnabled = evt.newValue);
        }
        private void OnDragChanged(ChangeEvent<bool> evt) => _context.Accessibility?.Update(state => state.DragControlsEnabled = evt.newValue);
        private void OnReducedMotionChanged(ChangeEvent<bool> evt) => _context.Accessibility?.Update(state => state.ReducedVfxMode = evt.newValue);
        private void OnContrastChanged(ChangeEvent<bool> evt) => _context.Accessibility?.Update(state => state.HighContrastMode = evt.newValue);

        private void OnAccessibilityChanged(ComfortSettingsState state)
        {
            _animations.ReducedMotion = state.ReducedVfxMode;
            Refresh();
        }

        private void OpenPrivacy()
        {
            _modal.Open(
                "PRIVACY CONTROLS",
                "Privacy preferences are managed by Core Racer's consent service. Production policy and deletion links are supplied by the release configuration.",
                "CLOSE",
                _modal.Close);
        }

        private void OpenSupport()
        {
            var summary = _context.Support != null
                ? _context.Support.BuildTextBundle(PlayerSupportInfo.Create())
                : "Support exporter is not registered.";
            _modal.Open("SUPPORT SUMMARY", summary, "CLOSE", _modal.Close);
        }

        private void ResetTutorial()
        {
            _context.Tutorial?.ResetForTesting();
            _view.Status.text = _context.Tutorial != null ? "Tutorial progress reset." : "Tutorial service is unavailable.";
            _toast.Show(_view.Status.text, _context.Tutorial == null);
        }

        private void OpenGallery()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _openGallery?.Invoke();
#else
            _toast.Show("Component gallery is available in development builds only.", true);
#endif
        }
    }
}
