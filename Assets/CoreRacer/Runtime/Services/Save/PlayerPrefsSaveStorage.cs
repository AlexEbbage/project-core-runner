using UnityEngine;

namespace CoreRacer.Services.Save
{
    public sealed class PlayerPrefsSaveStorage : ISaveStorage
    {
        public bool Exists(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public string Load(string key)
        {
            return PlayerPrefs.GetString(key, string.Empty);
        }

        public void Save(string key, string value)
        {
            PlayerPrefs.SetString(key, value ?? string.Empty);
            PlayerPrefs.Save();
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
