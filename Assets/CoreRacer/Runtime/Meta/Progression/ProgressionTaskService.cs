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
        public IReadOnlyList<ProgressionTaskDefinition> Definitions => _definitions;

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

        public bool IsClaimed(string id)
        {
            for (int i = 0; i < _profile.State.ClaimedTasks.Count; i++)
                if (_profile.State.ClaimedTasks[i].Id == id)
                    return _profile.State.ClaimedTasks[i].Value;
            return false;
        }

        public ProgressionTaskSummary GetSummary()
        {
            int ready = 0;
            int claimed = 0;
            for (int i = 0; i < _definitions.Count; i++)
            {
                var task = _definitions[i];
                if (task == null)
                    continue;

                if (IsClaimed(task.Id))
                    claimed++;
                else if (IsComplete(task))
                    ready++;
            }

            return new ProgressionTaskSummary(_definitions.Count, ready, claimed);
        }

        public bool TryClaim(string id)
        {
            var task = _definitions.Find(x => x != null && x.Id == id);
            if (task == null || !IsComplete(task))
                return false;

            if (IsClaimed(id))
                return false;

            return _profile.TryMutate(state =>
            {
                for (var i = 0; i < state.ClaimedTasks.Count; i++)
                {
                    if (state.ClaimedTasks[i].Id == id && state.ClaimedTasks[i].Value)
                        return false;
                }

                _rewards.ApplyManyToState(state, task.Rewards);
                state.ClaimedTasks.Add(new SerializableBoolById(id, true));
                return true;
            });
        }
    }
}
