using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Player;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.UI.GameOver;
using CoreRacer.UI.Hud;
using CoreRacer.UI.MainMenu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerSceneBuilder
    {
        [MenuItem("Tools/Core Racer/Create Clean Replacement Scene")]
        public static void CreateCleanScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("CoreRacer_Bootstrapper");
            bootstrap.AddComponent<GameBootstrapper>();

            var runRoot = new GameObject("RunRoot");
            var refs = runRoot.AddComponent<RunSceneReferences>();
            var run = runRoot.AddComponent<RunController>();

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "PlayerShip_Prototype";
            player.tag = "Player";
            player.transform.position = new Vector3(3f, 0f, 0f);
            var motor = player.AddComponent<PlayerOrbitalMotor>();
            var input = player.AddComponent<PlayerInputReader>();
            var controller = player.AddComponent<PlayerController>();
            AssignSerializedObject(controller, "inputReader", input);
            AssignSerializedObject(controller, "motor", motor);
            var health = player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerDamageHandler>();
            player.AddComponent<PlayerCollisionHandler>();

            var score = runRoot.AddComponent<RunScoreTracker>();
            var coins = runRoot.AddComponent<RunCurrencyTracker>();
            var stats = runRoot.AddComponent<RunStatsTrackerV2>();
            var obstacles = runRoot.AddComponent<ObstacleWorldController>();
            var pickups = runRoot.AddComponent<PickupWorldController>();
            var powerups = player.AddComponent<PowerupRuntimeController>();

            var canvas = CreateCanvas();
            var hud = CreateHud(canvas.transform, score, coins, health);
            var gameOver = CreateGameOver(canvas.transform);
            var menu = CreateMenu(canvas.transform, run);

            refs.Player = controller;
            refs.PlayerHealth = health;
            refs.ScoreTracker = score;
            refs.CurrencyTracker = coins;
            refs.StatsTracker = stats;
            refs.ObstacleWorld = obstacles;
            refs.PickupWorld = pickups;
            refs.Powerups = powerups;
            refs.Hud = hud;
            refs.GameOver = gameOver;

            AssignSerializedObject(obstacles, "player", player.transform);
            AssignSerializedObject(pickups, "player", player.transform);
            AssignSerializedObject(pickups, "scoreTracker", score);
            AssignSerializedObject(pickups, "currencyTracker", coins);
            AssignSerializedObject(pickups, "powerups", powerups);

            AssignSerializedObject(run, "references", refs);

            if (!AssetDatabase.IsValidFolder("Assets/CoreRacer/Generated")) AssetDatabase.CreateFolder("Assets/CoreRacer", "Generated");
            if (!AssetDatabase.IsValidFolder("Assets/CoreRacer/Generated/Scenes")) AssetDatabase.CreateFolder("Assets/CoreRacer/Generated", "Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/CoreRacer/Generated/Scenes/CoreRacer_CleanReplacement.unity");
            AssetDatabase.Refresh();
            Debug.Log("Created Assets/CoreRacer/Generated/Scenes/CoreRacer_CleanReplacement.unity. Assign generated config assets before production playtesting.");
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            return canvas;
        }

        private static HudController CreateHud(Transform parent, RunScoreTracker score, RunCurrencyTracker coins, PlayerHealth health)
        {
            var go = new GameObject("HUD");
            go.transform.SetParent(parent, false);
            var hud = go.AddComponent<HudController>();
            AssignSerializedObject(hud, "scoreTracker", score);
            AssignSerializedObject(hud, "currencyTracker", coins);
            AssignSerializedObject(hud, "playerHealth", health);
            AssignSerializedObject(hud, "scoreText", CreateText(go.transform, "ScoreText", "0", new Vector2(0, -40)));
            AssignSerializedObject(hud, "coinsText", CreateText(go.transform, "CoinsText", "0", new Vector2(0, -90)));
            AssignSerializedObject(hud, "healthText", CreateText(go.transform, "HealthText", "1/1", new Vector2(0, -140)));
            return hud;
        }

        private static GameOverController CreateGameOver(Transform parent)
        {
            var go = new GameObject("GameOver");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<GameOverController>();
            AssignSerializedObject(view, "scoreText", CreateText(go.transform, "Score", "Score", Vector2.zero));
            AssignSerializedObject(view, "coinsText", CreateText(go.transform, "Coins", "Coins", new Vector2(0, -50)));
            AssignSerializedObject(view, "xpText", CreateText(go.transform, "XP", "XP", new Vector2(0, -100)));
            AssignSerializedObject(view, "premiumText", CreateText(go.transform, "Premium", "Gems", new Vector2(0, -150)));
            AssignSerializedObject(view, "messageText", CreateText(go.transform, "Message", "Run complete", new Vector2(0, -220)));
            go.SetActive(false);
            return view;
        }

        private static MainMenuShell CreateMenu(Transform parent, RunController run)
        {
            var go = new GameObject("MainMenu");
            go.transform.SetParent(parent, false);
            var shell = go.AddComponent<MainMenuShell>();
            var play = go.AddComponent<PlayPageController>();
            AssignSerializedObject(play, "runController", run);
            return shell;
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700, 60);
            rect.anchoredPosition = anchoredPosition;
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 32;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            return t;
        }

        private static void AssignSerializedObject(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogWarning($"Property {propertyName} not found on {target.name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
