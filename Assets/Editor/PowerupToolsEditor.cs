using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PowerupToolsEditor
{
    private const string PowerupMenuRoot = "Tools/Powerups/";

    [MenuItem(PowerupMenuRoot + "Validate Current Scene")]
    public static void ValidateCurrentScene()
    {
        StringBuilder report = new StringBuilder();
        int errorCount = 0;
        int warningCount = 0;

        HudController hudController = Object.FindFirstObjectByType<HudController>(FindObjectsInactive.Include);
        if (hudController == null)
        {
            errorCount++;
            report.AppendLine("- Missing HudController in scene.");
        }
        else
        {
            SerializedObject hudObject = new SerializedObject(hudController);
            SerializedProperty indicators = hudObject.FindProperty("powerupIndicators");
            int indicatorCount = indicators != null ? indicators.arraySize : 0;
            if (indicatorCount == 5)
            {
                report.AppendLine("- HUD powerup indicator array is configured for five slots.");
            }
            else if (indicatorCount == 0)
            {
                warningCount++;
                report.AppendLine("- HUD powerup indicators are not serialized in scene; runtime fallback strip will be used.");
            }
            else
            {
                errorCount++;
                report.AppendLine($"- HUD powerup indicator count is {indicatorCount}; expected 5.");
            }
        }

        ObstacleRingGenerator generator = Object.FindFirstObjectByType<ObstacleRingGenerator>(FindObjectsInactive.Include);
        if (generator == null)
        {
            errorCount++;
            report.AppendLine("- Missing ObstacleRingGenerator in scene.");
        }
        else
        {
            SerializedObject generatorObject = new SerializedObject(generator);
            SerializedProperty spawnChance = generatorObject.FindProperty("powerupSpawnChance");
            SerializedProperty entries = generatorObject.FindProperty("powerupEntries");

            if (spawnChance != null && spawnChance.floatValue <= 0f)
            {
                warningCount++;
                report.AppendLine("- Generator powerup spawn chance is 0; runtime fallback chance will be used.");
            }

            if (entries == null || entries.arraySize == 0)
            {
                warningCount++;
                report.AppendLine("- Generator powerup entries are empty; runtime default roster will be used.");
            }
            else
            {
                bool hasUnsupportedEntry = false;
                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                    SerializedProperty typeProperty = entry.FindPropertyRelative("type");
                    PowerupType type = (PowerupType)typeProperty.enumValueIndex;
                    if (!PowerupUpgradeConfig.IsTargetGameplayPowerup(type))
                    {
                        hasUnsupportedEntry = true;
                        break;
                    }
                }

                if (hasUnsupportedEntry)
                {
                    errorCount++;
                    report.AppendLine("- Generator contains unsupported legacy powerup entries.");
                }
                else
                {
                    report.AppendLine("- Generator entries only reference supported gameplay powerups.");
                }
            }
        }

        PlayerPowerupController controller = Object.FindFirstObjectByType<PlayerPowerupController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            errorCount++;
            report.AppendLine("- Missing PlayerPowerupController in scene.");
        }
        else if (!controller.enabled)
        {
            warningCount++;
            report.AppendLine("- PlayerPowerupController is disabled in scene; GameManager now enables it at runtime.");
        }
        else
        {
            report.AppendLine("- PlayerPowerupController is present and enabled.");
        }

        PowerupUpgradeConfig config = FindConfigAsset();
        if (config == null)
        {
            errorCount++;
            report.AppendLine("- Missing PowerupUpgradeConfig asset.");
        }
        else
        {
            PowerupUpgradeConfig.PowerupUpgradeEntry[] upgrades = config.GetAvailableUpgrades();
            if (upgrades.Length != 5)
            {
                errorCount++;
                report.AppendLine($"- PowerupUpgradeConfig resolved {upgrades.Length} upgrade rows; expected 5.");
            }
            else
            {
                report.AppendLine("- PowerupUpgradeConfig resolves all five supported powerups.");
            }
        }

        MainMenuUI mainMenu = Object.FindFirstObjectByType<MainMenuUI>(FindObjectsInactive.Include);
        if (mainMenu == null)
        {
            errorCount++;
            report.AppendLine("- Missing MainMenuUI in scene.");
        }
        else
        {
            report.AppendLine("- MainMenuUI present; Lab entry is available through the menu button repurpose.");
        }

        string title = errorCount > 0 ? "Powerup Validation Failed" : warningCount > 0 ? "Powerup Validation Warnings" : "Powerup Validation Passed";
        EditorUtility.DisplayDialog(title, report.ToString(), "OK");
    }

    [MenuItem(PowerupMenuRoot + "Populate Upgrade Config Defaults")]
    public static void PopulateUpgradeConfigDefaults()
    {
        PowerupUpgradeConfig config = FindConfigAsset();
        if (config == null)
        {
            EditorUtility.DisplayDialog("Powerups", "PowerupUpgradeConfig asset was not found.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Populate Upgrade Config", "Replace the current powerup upgrade asset contents with the supported five-powerup default roster?", "Populate", "Cancel"))
            return;

        Undo.RecordObject(config, "Populate Powerup Upgrade Defaults");
        config.MaterializeDefaultTargetRoster();
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Powerups", "Powerup upgrade defaults populated.", "OK");
    }

    [MenuItem(PowerupMenuRoot + "Populate Generator Entries")]
    public static void PopulateGeneratorEntries()
    {
        ObstacleRingGenerator generator = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<ObstacleRingGenerator>()
            : null;

        if (generator == null)
            generator = Object.FindFirstObjectByType<ObstacleRingGenerator>(FindObjectsInactive.Include);

        if (generator == null)
        {
            EditorUtility.DisplayDialog("Powerups", "No ObstacleRingGenerator found in the current scene.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Populate Generator Entries", $"Populate supported powerup entries on '{generator.name}' and raise spawn chance if needed?", "Populate", "Cancel"))
            return;

        Undo.RecordObject(generator, "Populate Powerup Generator Entries");
        generator.ApplyDefaultPowerupEntries();
        EditorUtility.SetDirty(generator);
        EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
        EditorUtility.DisplayDialog("Powerups", "Generator defaults populated.", "OK");
    }

    [MenuItem(PowerupMenuRoot + "Activate/x2 Score", true)]
    [MenuItem(PowerupMenuRoot + "Activate/x2 Coin Spawn", true)]
    [MenuItem(PowerupMenuRoot + "Activate/Magnet", true)]
    [MenuItem(PowerupMenuRoot + "Activate/Autopilot", true)]
    [MenuItem(PowerupMenuRoot + "Activate/Shield", true)]
    private static bool ValidateActivatePowerup()
    {
        return EditorApplication.isPlaying && Object.FindFirstObjectByType<PlayerPowerupController>() != null;
    }

    [MenuItem(PowerupMenuRoot + "Activate/x2 Score")]
    private static void ActivateScoreMultiplier() => ActivatePowerup(PowerupType.ScoreMultiplier);

    [MenuItem(PowerupMenuRoot + "Activate/x2 Coin Spawn")]
    private static void ActivateCoinMultiplier() => ActivatePowerup(PowerupType.CoinMultiplier);

    [MenuItem(PowerupMenuRoot + "Activate/Magnet")]
    private static void ActivateMagnet() => ActivatePowerup(PowerupType.Magnet);

    [MenuItem(PowerupMenuRoot + "Activate/Autopilot")]
    private static void ActivateAutopilot() => ActivatePowerup(PowerupType.AutoPilot);

    [MenuItem(PowerupMenuRoot + "Activate/Shield")]
    private static void ActivateShield() => ActivatePowerup(PowerupType.Shield);

    private static void ActivatePowerup(PowerupType powerupType)
    {
        PlayerPowerupController controller = Object.FindFirstObjectByType<PlayerPowerupController>();
        if (controller == null)
            return;

        controller.ActivatePowerup(powerupType);
    }

    private static PowerupUpgradeConfig FindConfigAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:PowerupUpgradeConfig");
        if (guids == null || guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<PowerupUpgradeConfig>(path);
    }
}
