using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Services.Save;
using NUnit.Framework;
using UnityEngine;

namespace CoreRacer.Tests.PlayMode
{
    public abstract class IsolatedPlayerProfilePlayModeTest
    {
        private static readonly string[] ProfileKeys =
        {
            SaveKeys.PlayerProfile,
            SaveKeys.PlayerProfile + ".checksum",
            SaveKeys.PlayerProfile + ".backup",
            SaveKeys.PlayerProfile + ".backup.checksum"
        };

        private readonly Dictionary<string, string> _savedValues = new Dictionary<string, string>();

        [SetUp]
        public void IsolatePlayerProfile()
        {
            foreach (var bootstrap in Object.FindObjectsOfType<GameBootstrapper>(true))
                Object.DestroyImmediate(bootstrap.gameObject);
            GameServices.ClearRegistry();
            _savedValues.Clear();
            foreach (var key in ProfileKeys)
            {
                if (PlayerPrefs.HasKey(key)) _savedValues[key] = PlayerPrefs.GetString(key);
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

        [TearDown]
        public void RestorePlayerProfile()
        {
            foreach (var key in ProfileKeys) PlayerPrefs.DeleteKey(key);
            foreach (var pair in _savedValues) PlayerPrefs.SetString(pair.Key, pair.Value);
            PlayerPrefs.Save();
        }
    }
}
