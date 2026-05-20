using System;
using System.Collections.Generic;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.Services.LiveOps
{
    [CreateAssetMenu(menuName = "Core Racer/LiveOps/Live Event")]
    public sealed class LiveEventDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public string StartUtcIso;
        public string EndUtcIso;
        public float SoftCurrencyMultiplier = 1f;
        public float ExperienceMultiplier = 1f;
        public List<RewardGrant> LoginBonusRewards = new List<RewardGrant>();

        public bool IsActive(DateTimeOffset utcNow)
        {
            if (!DateTimeOffset.TryParse(StartUtcIso, out var start))
                return false;
            if (!DateTimeOffset.TryParse(EndUtcIso, out var end))
                return false;
            return utcNow >= start && utcNow < end;
        }
    }
}
