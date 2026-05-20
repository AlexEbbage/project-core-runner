using UnityEngine;

namespace CoreRacer.Common
{
    public static class IdUtility
    {
        public static bool IsValidId(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && id.Trim() == id && !id.Contains(" ");
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return value.Trim().ToLowerInvariant().Replace(" ", "_");
        }

        public static string RequiredObjectName(Object obj)
        {
            return obj == null ? "<null>" : obj.name;
        }
    }
}
