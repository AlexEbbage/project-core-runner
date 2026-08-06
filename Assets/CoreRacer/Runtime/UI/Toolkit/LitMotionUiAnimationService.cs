using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class LitMotionUiAnimationService : IUiAnimationService
    {
        private readonly Dictionary<VisualElement, List<MotionHandle>> _handles = new Dictionary<VisualElement, List<MotionHandle>>();
        private readonly UiAnimationSettings _settings;

        public LitMotionUiAnimationService(UiAnimationSettings settings = null)
        {
            _settings = settings != null ? settings : UiAnimationSettings.CreateRuntimeDefaults();
        }

        public bool ReducedMotion { get; set; }

        public void ShowScreen(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion)
                return;

            element.style.opacity = 0f;
            element.style.translate = new Translate(0f, _settings.ScreenTravel);
            Track(element, LMotion.Create(0f, 1f, _settings.ScreenDuration).WithEase(_settings.ScreenEase).BindToStyleOpacity(element));
            Track(element, LMotion.Create(new Vector2(0f, _settings.ScreenTravel), Vector2.zero, _settings.ScreenDuration).WithEase(_settings.ScreenEase).BindToStyleTranslate(element));
        }

        public void ShowPopup(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion)
                return;

            element.style.opacity = 0f;
            element.style.scale = new Scale(new Vector2(0.96f, 0.96f));
            Track(element, LMotion.Create(0f, 1f, _settings.PopupDuration).WithEase(Ease.OutQuad).BindToStyleOpacity(element));
            Track(element, LMotion.Create(new Vector3(0.96f, 0.96f, 1f), Vector3.one, _settings.PopupDuration).WithEase(_settings.PopupEase).BindToStyleScale(element));
        }

        public void HidePopup(VisualElement element)
        {
            ResetAndHide(element);
        }

        public void ShowBottomSheet(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion)
                return;

            element.style.opacity = 0f;
            element.style.translate = new Translate(0f, _settings.BottomSheetTravel);
            Track(element, LMotion.Create(0f, 1f, _settings.PopupDuration).WithEase(Ease.OutQuad).BindToStyleOpacity(element));
            Track(element, LMotion.Create(new Vector2(0f, _settings.BottomSheetTravel), Vector2.zero, _settings.PopupDuration).WithEase(_settings.ScreenEase).BindToStyleTranslate(element));
        }

        public void HideBottomSheet(VisualElement element)
        {
            ResetAndHide(element);
        }

        public void PlayInvalidAction(VisualElement element)
        {
            PrepareVisibleState(element);
            if (ReducedMotion)
                return;

            Track(element, LMotion.Punch.Create(Vector2.zero, new Vector2(_settings.InvalidShakeDistance, 0f), _settings.FeedbackDuration).BindToStyleTranslate(element));
        }

        public void PlaySuccess(VisualElement element)
        {
            PrepareVisibleState(element);
            if (ReducedMotion)
                return;

            var punch = new Vector3(_settings.SuccessPunchScale, _settings.SuccessPunchScale, 0f);
            Track(element, LMotion.Punch.Create(Vector3.one, punch, _settings.FeedbackDuration).BindToStyleScale(element));
        }

        public void PlayAttention(VisualElement element)
        {
            PrepareVisibleState(element);
            if (ReducedMotion)
                return;

            Track(element, LMotion.Punch.Create(Vector3.one, new Vector3(0.04f, 0.04f, 0f), _settings.FeedbackDuration).BindToStyleScale(element));
        }

        public void ShowToast(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion)
                return;

            element.style.opacity = 0f;
            element.style.translate = new Translate(0f, 18f);
            Track(element, LMotion.Create(0f, 1f, _settings.ToastDuration).WithEase(Ease.OutQuad).BindToStyleOpacity(element));
            Track(element, LMotion.Create(new Vector2(0f, 18f), Vector2.zero, _settings.ToastDuration).WithEase(Ease.OutQuad).BindToStyleTranslate(element));
        }

        public void Stop(VisualElement element)
        {
            if (element == null || !_handles.TryGetValue(element, out var handles))
                return;

            for (var i = 0; i < handles.Count; i++)
            {
                if (handles[i].IsActive())
                    handles[i].Cancel();
            }
            handles.Clear();
        }

        public void StopAll()
        {
            foreach (var pair in _handles)
            {
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    if (pair.Value[i].IsActive())
                        pair.Value[i].Cancel();
                }
            }
            _handles.Clear();
        }

        private void Prepare(VisualElement element)
        {
            if (element == null)
                return;

            Stop(element);
            UiVisibility.SetVisible(element, true);
            ResetStyles(element);
        }

        private void PrepareVisibleState(VisualElement element)
        {
            if (element == null)
                return;

            Stop(element);
            ResetStyles(element);
        }

        private void ResetAndHide(VisualElement element)
        {
            if (element == null)
                return;

            Stop(element);
            ResetStyles(element);
            UiVisibility.SetVisible(element, false);
        }

        private static void ResetStyles(VisualElement element)
        {
            element.style.opacity = 1f;
            element.style.translate = new Translate(0f, 0f);
            element.style.scale = new Scale(Vector2.one);
        }

        private void Track(VisualElement element, MotionHandle handle)
        {
            if (!_handles.TryGetValue(element, out var handles))
            {
                handles = new List<MotionHandle>();
                _handles.Add(element, handles);
            }
            handles.Add(handle);
        }
    }
}
