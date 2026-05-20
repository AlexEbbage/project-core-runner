using System;
using CoreRacer.Services.Save;

namespace CoreRacer.Services.Compliance
{
    public sealed class DataControlsService
    {
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;
        public DataControlsService(ISaveStorage storage, JsonSaveSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public string ExportLocalDataSummary()
        {
            return "Core Racer local data export created at " + DateTimeOffset.UtcNow.ToString("o") + "\n" +
                   "This build stores gameplay profile, settings, consent, tutorial, task and economy state locally. Use support bundle export for full debug details.";
        }

        public void DeleteLocalProgress()
        {
            _storage?.Delete(SaveKeys.PlayerProfile);
        }

        public void DeleteConsentState()
        {
            _storage?.Delete(SaveKeys.Consent);
        }
    }
}
