using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Run;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.Debugging
{
    /// <summary>
    /// Small runtime overlay for sanity-checking replacement scene wiring.
    /// </summary>
    public sealed class RuntimeDiagnosticsOverlay : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private RunController runController;
        [SerializeField] private RunScoreTracker scoreTracker;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ObstacleWorldController obstacleWorld;
        [SerializeField] private bool editorOnly = true;

        private void Update()
        {
            if (label == null)
                return;

            if (editorOnly && !Application.isEditor)
            {
                label.enabled = false;
                return;
            }

            label.enabled = true;
            var services = GameServices.IsReady ? "ready" : "missing";
            var state = runController != null ? runController.State.ToString() : "n/a";
            var score = scoreTracker != null ? scoreTracker.CurrentScore.ToString("N0") : "n/a";
            var health = playerHealth != null ? $"{playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0}" : "n/a";
            var difficulty = obstacleWorld != null ? obstacleWorld.CurrentDifficulty.ToString("0.00") : "n/a";
            label.text = $"Services: {services}\nRun: {state}\nScore: {score}\nHealth: {health}\nDifficulty: {difficulty}\nFPS: {(1f / Mathf.Max(UnityEngine.Time.unscaledDeltaTime, 0.0001f)):0}";
        }
    }
}
