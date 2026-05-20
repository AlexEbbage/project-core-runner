using System;
using CoreRacer.Services.Logging;
using UnityEngine;

namespace CoreRacer.Services.Network
{
    public sealed class NetworkStatusService : MonoBehaviour
    {
        [SerializeField] private float pollIntervalSeconds = 5f;
        private float _nextPoll;
        private IGameLogger _logger;

        public NetworkStatus Status { get; private set; } = NetworkStatus.Unknown;
        public bool IsOnline => Status == NetworkStatus.Online || Status == NetworkStatus.Poor;
        public event Action<NetworkStatus> Changed;

        public void Initialize(IGameLogger logger = null)
        {
            _logger = logger;
            Refresh(true);
        }

        private void Update()
        {
            if (UnityEngine.Time.unscaledTime < _nextPoll) return;
            _nextPoll = UnityEngine.Time.unscaledTime + pollIntervalSeconds;
            Refresh(false);
        }

        public void Refresh(bool force)
        {
            var reachability = Application.internetReachability;
            var next = reachability == NetworkReachability.NotReachable ? NetworkStatus.Offline : reachability == NetworkReachability.ReachableViaCarrierDataNetwork ? NetworkStatus.Poor : NetworkStatus.Online;
            if (!force && next == Status) return;
            Status = next;
            _logger?.Info(LogCategory.System, "Network status: " + Status, this);
            Changed?.Invoke(Status);
        }
    }
}
