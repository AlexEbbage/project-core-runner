using System;
using System.Collections.Generic;

namespace CoreRacer.Meta.Inventory
{
    [Serializable]
    public sealed class UnlockableItemState
    {
        public List<string> UnlockedIds = new List<string>();

        public bool IsUnlocked(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && UnlockedIds.Contains(id);
        }

        public bool Unlock(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || UnlockedIds.Contains(id))
                return false;

            UnlockedIds.Add(id);
            return true;
        }
    }
}
