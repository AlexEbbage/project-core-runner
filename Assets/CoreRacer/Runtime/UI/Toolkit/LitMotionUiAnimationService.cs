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
        public bool ReducedMotion { get; set; }

        public void ShowScreen(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion) return;
            element.style.opacity = 0f;
            element.style.translate = new Translate(0f, 18f);
            Track(element, LMotion.Create(0f, 1f, 0.2f).WithEase(Ease.OutCubic).BindToStyleOpacity(element));
            Track(element, LMotion.Create(new Vector2(0f, 18f), Vector2.zero, 0.24f).WithEase(Ease.OutCubic).BindToStyleTranslate(element));
        }

        public void ShowPopup(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion) return;
            element.style.opacity = 0f;
            element.style.scale = new Scale(new Vector2(0.96f, 0.96f));
            Track(element, LMotion.Create(0f, 1f, 0.18f).WithEase(Ease.OutQuad).BindToStyleOpacity(element));
            Track(element, LMotion.Create(new Vector3(0.96f, 0.96f, 1f), Vector3.one, 0.22f).WithEase(Ease.OutBack).BindToStyleScale(element));
        }

        public void HidePopup(VisualElement element)
        {
            Stop(element);
            element.style.opacity = 1f;
            element.style.scale = new Scale(Vector2.one);
            element.AddToClassList("is-hidden");
            element.pickingMode = PickingMode.Ignore;
        }

        public void PlayInvalidAction(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion) return;
            Track(element, LMotion.Punch.Create(Vector2.zero, new Vector2(12f, 0f), 0.28f).BindToStyleTranslate(element));
        }

        public void PlaySuccess(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion) return;
            Track(element, LMotion.Punch.Create(Vector3.one, new Vector3(0.08f, 0.08f, 0f), 0.32f).BindToStyleScale(element));
        }

        public void ShowToast(VisualElement element)
        {
            Prepare(element);
            if (ReducedMotion) return;
            element.style.opacity = 0f;
            Track(element, LMotion.Create(0f, 1f, 0.16f).WithEase(Ease.OutQuad).BindToStyleOpacity(element));
        }

        public void Stop(VisualElement element)
        {
            if (element == null || !_handles.TryGetValue(element, out var handles)) return;
            for (var i = 0; i < handles.Count; i++) if (handles[i].IsActive()) handles[i].Cancel();
            handles.Clear();
        }

        public void StopAll()
        {
            foreach (var pair in _handles)
                for (var i = 0; i < pair.Value.Count; i++) if (pair.Value[i].IsActive()) pair.Value[i].Cancel();
            _handles.Clear();
        }

        private void Prepare(VisualElement element)
        {
            Stop(element);
            element.RemoveFromClassList("is-hidden");
            element.pickingMode = PickingMode.Position;
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
