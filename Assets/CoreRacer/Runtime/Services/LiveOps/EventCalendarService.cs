using System.Collections.Generic;
using CoreRacer.Common.Time;

namespace CoreRacer.Services.LiveOps
{
    public sealed class EventCalendarService
    {
        private readonly IGameClock _clock;
        private readonly List<LiveEventDefinition> _events;

        public EventCalendarService(IGameClock clock, List<LiveEventDefinition> events)
        {
            _clock = clock;
            _events = events ?? new List<LiveEventDefinition>();
        }

        public List<LiveEventDefinition> GetActiveEvents()
        {
            var active = new List<LiveEventDefinition>();
            for (int i = 0; i < _events.Count; i++)
            {
                var evt = _events[i];
                if (evt != null && evt.IsActive(_clock.UtcNow))
                    active.Add(evt);
            }
            return active;
        }

        public bool IsEventActive(string eventId)
        {
            var active = GetActiveEvents();
            for (int i = 0; i < active.Count; i++)
                if (active[i].Id == eventId)
                    return true;
            return false;
        }
    }
}
