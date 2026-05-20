namespace CoreRacer.Services.Network
{
    public sealed class OfflineModeService
    {
        private readonly NetworkStatusService _network;
        public OfflineModeService(NetworkStatusService network) { _network = network; }
        public bool CanRunGameplay => true;
        public bool CanUseAds => _network == null || _network.IsOnline;
        public bool CanUseIap => _network == null || _network.IsOnline;
        public bool ShouldQueueAnalytics => _network != null && !_network.IsOnline;
    }
}
