using System;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class UiModalService : IDisposable
    {
        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly Label _body;
        private readonly Button _primary;
        private readonly Button _close;
        private readonly IUiAnimationService _animations;
        private Action _primaryAction;

        public UiModalService(VisualElement modalRoot, IUiAnimationService animations)
        {
            _root = modalRoot ?? throw new ArgumentNullException(nameof(modalRoot));
            _animations = animations ?? throw new ArgumentNullException(nameof(animations));
            _title = _root.Require<Label>("ModalTitle");
            _body = _root.Require<Label>("ModalBody");
            _primary = _root.Require<Button>("ModalPrimaryButton");
            _close = _root.Require<Button>("ModalCloseButton");
            _primary.clicked += InvokePrimary;
            _close.clicked += Close;
            UiModalInputBlocker.Attach(_root);
        }

        public bool IsOpen => !_root.ClassListContains(UiClassNames.Hidden);

        public void Open(string title, string body, string primaryText, Action primaryAction, bool destructive = false)
        {
            _title.text = title ?? string.Empty;
            _body.text = body ?? string.Empty;
            _primary.text = string.IsNullOrWhiteSpace(primaryText) ? "OK" : primaryText;
            _primaryAction = primaryAction;
            _primary.EnableInClassList("button--danger", destructive);
            UiVisibility.SetVisible(_root, true);
            _animations.ShowPopup(_root);
            _primary.Focus();
        }

        public void Close()
        {
            _primaryAction = null;
            _animations.HidePopup(_root);
        }

        public void Dispose()
        {
            UiModalInputBlocker.Detach(_root);
            _primary.clicked -= InvokePrimary;
            _close.clicked -= Close;
            _primaryAction = null;
        }

        private void InvokePrimary()
        {
            var action = _primaryAction;
            if (action == null)
            {
                Close();
                return;
            }

            action.Invoke();
        }
    }
}
