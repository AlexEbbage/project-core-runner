using System.Collections.Generic;

namespace CoreRacer.Services.Analytics
{
    public interface IAnalyticsService
    {
        void Track(string eventName, IReadOnlyDictionary<string, object> parameters = null);
    }
}
