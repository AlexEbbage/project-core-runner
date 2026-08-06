using System;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public static class UiElementQuery
    {
        public static T Require<T>(this VisualElement root, string name) where T : VisualElement
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            var element = root.Q<T>(name);
            if (element != null)
                return element;

            var contract = string.IsNullOrWhiteSpace(root.name) ? root.GetType().Name : root.name;
            throw new InvalidOperationException(
                $"UI contract '{contract}' is missing required element '{name}' of type {typeof(T).Name}. " +
                "Check the matching UXML template and keep source-referenced element names stable.");
        }

        public static T Optional<T>(this VisualElement root, string name) where T : VisualElement
        {
            return root?.Q<T>(name);
        }
    }
}
