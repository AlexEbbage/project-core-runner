namespace CoreRacer.Localization
{
    public sealed class LocalizationServiceV2
    {
        private readonly StringTable _table;

        public LocalizationServiceV2(StringTable table)
        {
            _table = table;
        }

        public string Get(string key)
        {
            return _table != null ? _table.Get(key) : key;
        }
    }
}
