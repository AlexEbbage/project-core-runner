using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.FTUE
{
    [CreateAssetMenu(menuName = "Core Racer/FTUE/Tutorial Config")]
    public sealed class TutorialConfig : ScriptableObject
    {
        public string TutorialId = "core_racer_ftue_v1";
        public bool RunOnFreshInstall = true;
        public List<TutorialStepDefinition> Steps = new List<TutorialStepDefinition>();
    }
}
