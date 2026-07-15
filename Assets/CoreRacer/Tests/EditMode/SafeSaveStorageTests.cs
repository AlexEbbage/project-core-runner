using System.Collections.Generic;
using CoreRacer.Services.Save;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class SafeSaveStorageTests
    {
        [Test]
        public void LoadReturnsPrimaryWhenChecksumMatches()
        {
            var inner = new MemorySaveStorage();
            var safe = new SafeSaveStorage(inner);

            safe.Save("profile", "clean-json");

            Assert.AreEqual("clean-json", safe.Load("profile"));
        }

        [Test]
        public void LoadFallsBackToBackupWhenPrimaryChecksumFails()
        {
            var inner = new MemorySaveStorage();
            var safe = new SafeSaveStorage(inner);

            safe.Save("profile", "clean-json");
            inner.Save("profile", "tampered-json");

            Assert.AreEqual("clean-json", safe.Load("profile"));
        }

        private sealed class MemorySaveStorage : ISaveStorage
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
            public bool Exists(string key) => _values.ContainsKey(key);
            public string Load(string key) => _values.TryGetValue(key, out var value) ? value : null;
            public void Save(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }
    }
}
