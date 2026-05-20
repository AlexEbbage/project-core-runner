using UnityEngine.UI;

namespace CoreRacer.UI.Shared
{
    public static class UiTextBinder
    {
        public static void SetText(Text text, string value)
        {
            if (text != null)
                text.text = value ?? string.Empty;
        }
    }
}
