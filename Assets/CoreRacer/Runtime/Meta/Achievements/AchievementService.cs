using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;

namespace CoreRacer.Meta.Achievements
{
    public sealed class AchievementService
    {
        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;
        private readonly List<AchievementDefinition> _definitions;

        public AchievementService(PlayerProfileService profile, RewardGrantService rewards, List<AchievementDefinition> definitions)
        {
            _profile = profile;
            _rewards = rewards;
            _definitions = definitions ?? new List<AchievementDefinition>();
        }

        public int GetProgress(AchievementDefinition definition)
        {
            var s = _profile.State;
            switch (definition.Metric)
            {
                case AchievementMetricType.ProfileLevel: return s.Level;
                case AchievementMetricType.TotalRuns: return s.TotalRuns;
                case AchievementMetricType.BestScore: return s.BestScore;
                case AchievementMetricType.TotalCoinsCollected: return s.TotalCoinsCollected;
                case AchievementMetricType.TotalPowerupsCollected: return s.TotalPowerupsCollected;
                case AchievementMetricType.UnlockedItems: return s.Inventory.UnlockedIds.Count;
                default: return 0;
            }
        }

        public bool IsComplete(AchievementDefinition definition)
        {
            return GetProgress(definition) >= definition.RequiredValue;
        }

        public bool IsClaimed(string id)
        {
            for (int i = 0; i < _profile.State.ClaimedAchievements.Count; i++)
                if (_profile.State.ClaimedAchievements[i].Id == id)
                    return _profile.State.ClaimedAchievements[i].Value;
            return false;
        }

        public bool TryClaim(string id)
        {
            var definition = _definitions.Find(x => x != null && x.Id == id);
            if (definition == null || !IsComplete(definition) || IsClaimed(id))
                return false;

            for (int i = 0; i < definition.Rewards.Count; i++)
                _rewards.Grant(definition.Rewards[i]);

            _profile.State.ClaimedAchievements.Add(new SerializableBoolById(id, true));
            _profile.Save();
            return true;
        }
    }
}
