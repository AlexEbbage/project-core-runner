using System;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public static class UiElementQuery
    {
        public static T Require<T>(this VisualElement root, string name) where T : VisualElement
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            var element = root.Q<T>(name);
            if (element == null)
                throw new InvalidOperationException($"Required UI element '{name}' ({typeof(T).Name}) was not found.");
            return element;
        }
    }
}
