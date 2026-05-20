using System;

namespace CoreRacer.Meta.Progression
{
    [Serializable]
    public struct SerializableIntById
    {
        public string Id;
        public int Value;

        public SerializableIntById(string id, int value)
        {
            Id = id;
            Value = value;
        }
    }

    [Serializable]
    public struct SerializableBoolById
    {
        public string Id;
        public bool Value;

        public SerializableBoolById(string id, bool value)
        {
            Id = id;
            Value = value;
        }
    }
}
