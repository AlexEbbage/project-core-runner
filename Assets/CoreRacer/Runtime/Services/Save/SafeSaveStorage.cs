using System;

namespace CoreRacer.Services.Save
{
    /// <summary>
    /// Backwards-compatible save wrapper with checksum validation and a previous-known-good backup.
    /// A logical save writes all related keys before flushing the backend once.
    /// </summary>
    public sealed class SafeSaveStorage : ISaveStorage, IFlushableSaveStorage
    {
        private readonly ISaveStorage _inner;

        public SafeSaveStorage(ISaveStorage inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool Exists(string key) => _inner.Exists(key);

        public string Load(string key)
        {
            var primary = _inner.Load(key);
            if (IsValid(key, primary))
                return primary;

            var backup = _inner.Load(GetBackupKey(key));
            if (IsValidBackup(key, backup))
                return backup;

            // Legacy saves may not have checksums yet. Preserve compatibility rather than deleting data.
            return primary;
        }

        public void Save(string key, string value)
        {
            value = value ?? string.Empty;

            var existingPrimary = _inner.Load(key);
            if (_inner.Exists(key) && IsValid(key, existingPrimary))
            {
                _inner.Save(GetBackupKey(key), existingPrimary);
                _inner.Save(GetBackupChecksumKey(key), ComputeChecksum(existingPrimary));
            }
            else if (!_inner.Exists(GetBackupKey(key)))
            {
                // First save: a valid copy is still preferable to no recovery path.
                _inner.Save(GetBackupKey(key), value);
                _inner.Save(GetBackupChecksumKey(key), ComputeChecksum(value));
            }

            _inner.Save(key, value);
            _inner.Save(GetChecksumKey(key), ComputeChecksum(value));
            Flush();
        }

        public void Delete(string key)
        {
            _inner.Delete(key);
            _inner.Delete(GetChecksumKey(key));
            _inner.Delete(GetBackupKey(key));
            _inner.Delete(GetBackupChecksumKey(key));
            Flush();
        }

        public void Flush()
        {
            if (_inner is IFlushableSaveStorage flushable)
                flushable.Flush();
        }

        private bool IsValid(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return !_inner.Exists(GetChecksumKey(key));
            if (!_inner.Exists(GetChecksumKey(key)))
                return true;
            return string.Equals(_inner.Load(GetChecksumKey(key)), ComputeChecksum(value), StringComparison.Ordinal);
        }

        private bool IsValidBackup(string key, string value)
        {
            return !string.IsNullOrEmpty(value)
                && _inner.Exists(GetBackupChecksumKey(key))
                && string.Equals(_inner.Load(GetBackupChecksumKey(key)), ComputeChecksum(value), StringComparison.Ordinal);
        }

        private static string GetChecksumKey(string key) => key + ".checksum";
        private static string GetBackupKey(string key) => key + ".backup";
        private static string GetBackupChecksumKey(string key) => key + ".backup.checksum";

        private static string ComputeChecksum(string value)
        {
            value = value ?? string.Empty;
            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                uint hash = offset;
                for (var i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }
                return hash.ToString("X8");
            }
        }
    }
}
