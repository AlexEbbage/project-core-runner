using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;

namespace CoreRacer.Meta.Progression
{
    public sealed class ProgressionTaskService
    {
        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;
        private readonly List<ProgressionTaskDefinition> _definitions;

        public ProgressionTaskService(PlayerProfileService profile, RewardGrantService rewards, List<ProgressionTaskDefinition> definitions)
        {
            _profile = profile;
            _rewards = rewards;
            _definitions = definitions ?? new List<ProgressionTaskDefinition>();
        }

        public int GetProgress(ProgressionTaskDefinition task)
        {
            var s = _profile.State;
            switch (task.Metric)
            {
                case ProgressionTaskMetric.RunsCompleted: return s.TotalRuns;
                case ProgressionTaskMetric.CoinsCollected: return s.TotalCoinsCollected;
                case ProgressionTaskMetric.PowerupsCollected: return s.TotalPowerupsCollected;
                case ProgressionTaskMetric.BestScore: return s.BestScore;
                default: return 0;
            }
        }

        public bool IsComplete(ProgressionTaskDefinition task) => GetProgress(task) >= task.TargetValue;

        public bool TryClaim(string id)
        {
            var task = _definitions.Find(x => x != null && x.Id == id);
            if (task == null || !IsComplete(task))
                return false;

            for (int i = 0; i < _profile.State.ClaimedTasks.Count; i++)
                if (_profile.State.ClaimedTasks[i].Id == id && _profile.State.ClaimedTasks[i].Value)
                    return false;

            for (int i = 0; i < task.Rewards.Count; i++)
                _rewards.Grant(task.Rewards[i]);

            _profile.State.ClaimedTasks.Add(new SerializableBoolById(id, true));
            _profile.Save();
            return true;
        }
    }
}
