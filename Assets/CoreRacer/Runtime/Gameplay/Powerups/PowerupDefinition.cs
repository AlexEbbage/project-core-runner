using UnityEngine;

namespace CoreRacer.Gameplay.Powerups
{
    [CreateAssetMenu(menuName = "Core Racer/Powerups/Powerup Definition")]
    public sealed class PowerupDefinition : ScriptableObject
    {
        public PowerupType Type;
        public string DisplayName;
        public Sprite Icon;
        public PowerupTuning BaseTuning = new PowerupTuning(5f, 1f);
    }
}
