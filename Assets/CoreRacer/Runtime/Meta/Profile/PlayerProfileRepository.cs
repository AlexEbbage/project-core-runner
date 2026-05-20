using CoreRacer.Services.Save;

namespace CoreRacer.Meta.Profile
{
    public sealed class PlayerProfileRepository
    {
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;
        private readonly ProfileMigrationService _migration;

        public PlayerProfileRepository(ISaveStorage storage, JsonSaveSerializer serializer, ProfileMigrationService migration)
        {
            _storage = storage;
            _serializer = serializer;
            _migration = migration;
        }

        public PlayerProfileState Load()
        {
            var json = _storage.Load(SaveKeys.PlayerProfile);
            if (string.IsNullOrWhiteSpace(json))
                return PlayerProfileDefaults.CreateNew();

            var state = _serializer.Deserialize<PlayerProfileState>(json);
            return _migration.Migrate(state);
        }

        public void Save(PlayerProfileState state)
        {
            _storage.Save(SaveKeys.PlayerProfile, _serializer.Serialize(state));
        }
    }
}
