# EditMode/SafeSaveStorageTests.cs

```csharp
using CoreRacer.Services.Save;
using NUnit.Framework;
using System.Collections.Generic;

namespace CoreRacer.Tests.EditMode
{
    public sealed class SafeSaveStorageTests
    {
        private sealed class MemoryStorage : ISaveStorage
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
            public bool Exists(string key) => _values.ContainsKey(key);
            public string Load(string key) => _values.TryGetValue(key, out var value) ? value : string.Empty;
            public void Save(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }

        [Test]
        public void SafeSaveStorage_Loads_Raw_Legacy_Value_When_No_Checksum_Exists()
        {
            var memory = new MemoryStorage();
            memory.Save("profile", "legacy-json");
            var safe = new SafeSaveStorage(memory);

            Assert.AreEqual("legacy-json", safe.Load("profile"));
        }

        [Test]
        public void SafeSaveStorage_Writes_Backup_And_Returns_Primary_Value()
        {
            var safe = new SafeSaveStorage(new MemoryStorage());
            safe.Save("profile", "new-json");

            Assert.AreEqual("new-json", safe.Load("profile"));
        }
    }
}

```
