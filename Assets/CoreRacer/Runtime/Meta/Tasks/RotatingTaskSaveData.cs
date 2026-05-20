using System;
using System.Collections.Generic;

namespace CoreRacer.Meta.Tasks
{
    [Serializable]
    public sealed class RotatingTaskSaveData
    {
        public int Version = 1;
        public List<RotatingTaskSlot> ActiveSlots = new List<RotatingTaskSlot>();
    }
}
