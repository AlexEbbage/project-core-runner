using System;

namespace CoreRacer.Services.Save
{
    /// <summary>
    /// Backwards-compatible save wrapper: stores the normal raw value, plus backup and checksum keys.
    /// Existing un-checksummed saves still load normally.
    /// </summary>
    public sealed class SafeSaveStorage : ISaveStorage
    {
        private readonly ISaveStorage _inner;

        public SafeSaveStorage(ISaveStorage inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool Exists(string key)
        {
            return _inner.Exists(key);
        }

        public string Load(string key)
        {
            var primary = _inner.Load(key);
            var checksumKey = GetChecksumKey(key);

            if (string.IsNullOrEmpty(primary) || !_inner.Exists(checksumKey))
                return primary;

            var expectedChecksum = _inner.Load(checksumKey);
            var actualChecksum = ComputeChecksum(primary);
            if (string.Equals(expectedChecksum, actualChecksum, StringComparison.Ordinal))
                return primary;

            var backup = _inner.Load(GetBackupKey(key));
            if (!string.IsNullOrEmpty(backup) && string.Equals(_inner.Load(GetBackupChecksumKey(key)), ComputeChecksum(backup), StringComparison.Ordinal))
                return backup;

            // Fall back to primary instead of hard-failing; repository migrations can still decide what to do.
            return primary;
        }

        public void Save(string key, string value)
        {
            value = value ?? string.Empty;
            var checksum = ComputeChecksum(value);
            _inner.Save(key, value);
            _inner.Save(GetChecksumKey(key), checksum);
            _inner.Save(GetBackupKey(key), value);
            _inner.Save(GetBackupChecksumKey(key), checksum);
        }

        public void Delete(string key)
        {
            _inner.Delete(key);
            _inner.Delete(GetChecksumKey(key));
            _inner.Delete(GetBackupKey(key));
            _inner.Delete(GetBackupChecksumKey(key));
        }

        private static string GetChecksumKey(string key) => key + ".checksum";
        private static string GetBackupKey(string key) => key + ".backup";
        private static string GetBackupChecksumKey(string key) => key + ".backup.checksum";

        private static string ComputeChecksum(string value)
        {
            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                uint hash = offset;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }
                return hash.ToString("X8");
            }
        }
    }
}
