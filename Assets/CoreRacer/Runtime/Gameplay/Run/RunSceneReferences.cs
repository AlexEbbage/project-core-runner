using CoreRacer.Common.Validation;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using UnityEngine;

namespace CoreRacer.Gameplay.Run
{
    public sealed class RunSceneReferences : MonoBehaviour
    {
        public PlayerController Player;
        public PlayerHealth PlayerHealth;
        public PlayerCosmeticsController PlayerCosmetics;
        public RunScoreTracker ScoreTracker;
        public RunCurrencyTracker CurrencyTracker;
        public RunStatsTrackerV2 StatsTracker;
        public ObstacleWorldController ObstacleWorld;
        public PickupWorldController PickupWorld;
        public PowerupRuntimeController Powerups;
        [Tooltip("Must implement IRunUiPresenter. Kept as MonoBehaviour so Unity can serialize the scene reference.")]
        public MonoBehaviour RunUiBehaviour;

        public IRunUiPresenter RunUi => RunUiBehaviour as IRunUiPresenter;

        public bool HasRequiredReferences()
        {
            return ValidateReferences().IsValid;
        }

        public ValidationResult ValidateReferences()
        {
            var result = new ValidationResult();
            if (Player == null) result.Error("RunSceneReferences.Player is missing.");
            if (PlayerHealth == null) result.Error("RunSceneReferences.PlayerHealth is missing.");
            if (PlayerCosmetics == null) result.Warning("RunSceneReferences.PlayerCosmetics is missing. Selected ships/cosmetics will not be applied.");
            if (ScoreTracker == null) result.Error("RunSceneReferences.ScoreTracker is missing.");
            if (CurrencyTracker == null) result.Error("RunSceneReferences.CurrencyTracker is missing.");
            if (StatsTracker == null) result.Error("RunSceneReferences.StatsTracker is missing.");
            if (ObstacleWorld == null) result.Warning("RunSceneReferences.ObstacleWorld is missing. Runs can start, but no obstacles will spawn.");
            if (PickupWorld == null) result.Warning("RunSceneReferences.PickupWorld is missing. Runs can start, but no pickups will spawn.");
            if (Powerups == null) result.Warning("RunSceneReferences.Powerups is missing. Powerup pickups will not apply effects.");
            if (RunUi == null) result.Error("RunSceneReferences.RunUiBehaviour is missing or does not implement IRunUiPresenter.");
            return result;
        }
    }
}
