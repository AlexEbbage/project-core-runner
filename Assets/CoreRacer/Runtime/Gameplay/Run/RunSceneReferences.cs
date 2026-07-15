using CoreRacer.Common.Validation;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.UI.GameOver;
using CoreRacer.UI.Hud;
using CoreRacer.UI.MainMenu;
using CoreRacer.UI.Pause;
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
        public HudController Hud;
        public GameOverController GameOver;
        public MainMenuShell MainMenu;
        public PauseMenuController PauseMenu;

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
            if (Hud == null) result.Warning("RunSceneReferences.Hud is missing. Gameplay HUD will not update.");
            if (GameOver == null) result.Warning("RunSceneReferences.GameOver is missing. Game-over UI must be triggered manually.");
            if (MainMenu == null) result.Warning("RunSceneReferences.MainMenu is missing. Return-to-menu UI will not be shown automatically.");
            if (PauseMenu == null) result.Warning("RunSceneReferences.PauseMenu is missing. Pause UI will not be hidden automatically.");
            return result;
        }
    }
}
