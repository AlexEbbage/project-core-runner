using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public interface IUiAnimationService
    {
        bool ReducedMotion { get; set; }
        void ShowScreen(VisualElement element);
        void ShowPopup(VisualElement element);
        void HidePopup(VisualElement element);
        void PlayInvalidAction(VisualElement element);
        void PlaySuccess(VisualElement element);
        void ShowToast(VisualElement element);
        void Stop(VisualElement element);
        void StopAll();
    }
}
