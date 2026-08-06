using System;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class UiToastService : IDisposable
    {
        private readonly Label _toast;
        private readonly IUiAnimationService _animations;
        private IVisualElementScheduledItem _hideTask;

        public UiToastService(Label toast, IUiAnimationService animations)
        {
            _toast = toast;
            _animations = animations;
        }

        public void Show(string message, bool error = false)
        {
            if (_toast == null)
                return;

            _hideTask?.Pause();
            _toast.text = message ?? string.Empty;
            _toast.EnableInClassList(UiClassNames.Error, error);
            _toast.EnableInClassList(UiClassNames.Success, !error);
            UiVisibility.SetVisible(_toast, true);
            _animations.ShowToast(_toast);
            _hideTask = _toast.schedule.Execute(Hide).StartingIn(2600);
        }

        public void Dispose()
        {
            Hide();
            _animations.Stop(_toast);
        }

        public void Hide()
        {
            _hideTask?.Pause();
            _hideTask = null;
            UiVisibility.SetVisible(_toast, false);
        }
    }
}
