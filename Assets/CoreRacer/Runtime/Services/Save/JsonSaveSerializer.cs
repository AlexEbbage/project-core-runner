using UnityEngine;

namespace CoreRacer.Services.Save
{
    public sealed class JsonSaveSerializer
    {
        public string Serialize<T>(T value)
        {
            return JsonUtility.ToJson(value, true);
        }

        public T Deserialize<T>(string json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json))
                return new T();

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                return new T();
            }
        }
    }
}
