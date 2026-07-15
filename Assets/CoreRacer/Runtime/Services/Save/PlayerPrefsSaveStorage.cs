using UnityEngine;

namespace CoreRacer.Services.Save
{
    public sealed class PlayerPrefsSaveStorage : ISaveStorage, IFlushableSaveStorage
    {
        public bool Exists(string key) => PlayerPrefs.HasKey(key);

        public string Load(string key) => PlayerPrefs.GetString(key, string.Empty);

        public void Save(string key, string value)
        {
            PlayerPrefs.SetString(key, value ?? string.Empty);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public void Flush()
        {
            PlayerPrefs.Save();
        }
    }
}
