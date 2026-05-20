#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Builders
{
    public sealed class CoreRacerManualWiringChecklistWindow : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("Tools/Core Racer/Manual Wiring Checklist")]
        public static void Open()
        {
            GetWindow<CoreRacerManualWiringChecklistWindow>("Core Racer Wiring");
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Core Racer Manual Wiring Checklist", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this after importing the final replacement package. It mirrors docs/rewrite/08-final-manual-wiring-guide.md.", MessageType.Info);

            DrawSection("1. Bootstrap", "Create a GameBootstrapper object, assign ShopCatalog/StringTable, and choose Dummy or SDK ad services.");
            DrawSection("2. Run Scene", "Create/verify RunController, RunSceneReferences, player, obstacle world, pickup world, HUD and game-over controllers.");
            DrawSection("3. Player", "Assign PlayerInputReader, PlayerOrbitalMotor, PlayerHealth, PlayerDamageHandler, PlayerCollisionHandler, PlayerVisual and cosmetics.");
            DrawSection("4. Obstacles/Pickups", "Assign ring prefab, segment prefab, pickup prefab, pattern config, pickup config and layer masks.");
            DrawSection("5. UI", "Wire main menu pages, top bar, HUD texts/sliders, buttons, modals and progression rows.");
            DrawSection("6. SDKs", "Replace dummy adapters with LevelPlay, Firebase, Unity IAP and Mobile Notifications implementations after packages are installed.");
            DrawSection("7. Validation", "Run Generate Default Config Assets, Create Clean Replacement Scene, Validate Project, Validate Open Scene Wiring.");
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSection(string title, string body)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(body, EditorStyles.wordWrappedLabel);
        }
    }
}
#endif
