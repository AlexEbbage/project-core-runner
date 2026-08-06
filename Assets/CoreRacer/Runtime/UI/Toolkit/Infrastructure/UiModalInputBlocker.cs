using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public static class UiModalInputBlocker
    {
        public static void Attach(VisualElement element)
        {
            if (element == null)
                return;

            element.RegisterCallback<PointerDownEvent>(Stop);
            element.RegisterCallback<PointerMoveEvent>(Stop);
            element.RegisterCallback<PointerUpEvent>(Stop);
            element.RegisterCallback<ClickEvent>(Stop);
            element.RegisterCallback<KeyDownEvent>(Stop);
            element.RegisterCallback<NavigationMoveEvent>(Stop);
            element.RegisterCallback<NavigationSubmitEvent>(Stop);
            element.RegisterCallback<NavigationCancelEvent>(Stop);
        }

        public static void Detach(VisualElement element)
        {
            if (element == null)
                return;

            element.UnregisterCallback<PointerDownEvent>(Stop);
            element.UnregisterCallback<PointerMoveEvent>(Stop);
            element.UnregisterCallback<PointerUpEvent>(Stop);
            element.UnregisterCallback<ClickEvent>(Stop);
            element.UnregisterCallback<KeyDownEvent>(Stop);
            element.UnregisterCallback<NavigationMoveEvent>(Stop);
            element.UnregisterCallback<NavigationSubmitEvent>(Stop);
            element.UnregisterCallback<NavigationCancelEvent>(Stop);
        }

        private static void Stop(EventBase evt)
        {
            evt.StopImmediatePropagation();
        }
    }
}
