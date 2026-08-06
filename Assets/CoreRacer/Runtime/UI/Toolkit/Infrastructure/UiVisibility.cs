using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public static class UiVisibility
    {
        public static void SetVisible(VisualElement element, bool visible, bool pickableWhenVisible = true)
        {
            if (element == null)
                return;

            element.EnableInClassList(UiClassNames.Hidden, !visible);
            element.pickingMode = visible && pickableWhenVisible ? PickingMode.Position : PickingMode.Ignore;
        }

        public static void SetAvailable(Button button, bool available, bool preserveLayout = false)
        {
            if (button == null)
                return;

            button.EnableInClassList("is-invisible", !available && preserveLayout);
            button.EnableInClassList(UiClassNames.Hidden, !available && !preserveLayout);
            button.pickingMode = available ? PickingMode.Position : PickingMode.Ignore;
            button.SetEnabled(available);
        }
    }
}
