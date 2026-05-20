using CoreRacer.Config.Run;
using CoreRacer.Gameplay.Player;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunContinueService
    {
        private readonly ContinueConfig _config;
        private readonly PlayerController _player;
        private readonly PlayerHealth _health;

        public RunContinueService(PlayerController player, PlayerHealth health, ContinueConfig config)
        {
            _player = player;
            _health = health;
            _config = config ?? new ContinueConfig();
        }

        public bool CanContinue(RunSession session)
        {
            return session != null && session.ContinuesUsed < _config.MaxContinuesPerRun;
        }

        public void ContinueRun(RunSession session)
        {
            if (session == null)
                return;

            session.ContinuesUsed++;
            if (_player != null)
            {
                var position = _player.transform.position;
                position.z -= _config.RespawnBackDistance;
                position.y += _config.RespawnHeightOffset;
                _player.transform.position = position;
            }

            if (_health != null)
                _health.Revive(_config.InvulnerabilitySeconds);
        }
    }
}
