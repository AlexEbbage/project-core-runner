using System;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class ComponentGalleryPresenter : IDisposable
    {
        private readonly VisualElement _root;
        private readonly Button _close;
        private readonly Button _motion;
        private readonly IUiAnimationService _animations;

        public ComponentGalleryPresenter(VisualElement root, IUiAnimationService animations)
        {
            _root = root;
            _animations = animations;
            _close = root.Require<Button>("GalleryCloseButton");
            _motion = root.Require<Button>("GalleryMotionButton");
            _close.clicked += Close;
            _motion.clicked += PlayMotion;
            UiModalInputBlocker.Attach(_root);
        }

        public void Open()
        {
            UiVisibility.SetVisible(_root, true);
            _animations.ShowScreen(_root);
        }

        public void Close()
        {
            _animations.HidePopup(_root);
        }

        public void Dispose()
        {
            UiModalInputBlocker.Detach(_root);
            _close.clicked -= Close;
            _motion.clicked -= PlayMotion;
        }

        private void PlayMotion()
        {
            _animations.PlaySuccess(_root.Require<VisualElement>("GalleryMotionTarget"));
        }
    }
}
