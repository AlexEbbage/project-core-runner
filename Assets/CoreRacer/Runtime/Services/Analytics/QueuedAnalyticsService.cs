using System.Collections.Generic;
using CoreRacer.Services.Network;

namespace CoreRacer.Services.Analytics
{
    public sealed class QueuedAnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsService _inner;
        private readonly NetworkStatusService _network;
        private readonly Queue<QueuedEvent> _queue = new Queue<QueuedEvent>();
        private readonly int _maxQueueSize;

        private struct QueuedEvent
        {
            public string Name;
            public IReadOnlyDictionary<string, object> Parameters;
        }

        public QueuedAnalyticsService(IAnalyticsService inner, NetworkStatusService network, int maxQueueSize = 200)
        {
            _inner = inner;
            _network = network;
            _maxQueueSize = maxQueueSize;
            if (_network != null) _network.Changed += OnNetworkChanged;
        }

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            if (_network == null || _network.IsOnline)
            {
                Flush();
                _inner?.Track(eventName, parameters);
                return;
            }

            if (_queue.Count >= _maxQueueSize)
                _queue.Dequeue();
            _queue.Enqueue(new QueuedEvent { Name = eventName, Parameters = parameters });
        }

        public void Flush()
        {
            while (_queue.Count > 0 && (_network == null || _network.IsOnline))
            {
                var entry = _queue.Dequeue();
                _inner?.Track(entry.Name, entry.Parameters);
            }
        }

        private void OnNetworkChanged(NetworkStatus status)
        {
            if (status == NetworkStatus.Online || status == NetworkStatus.Poor)
                Flush();
        }
    }
}
