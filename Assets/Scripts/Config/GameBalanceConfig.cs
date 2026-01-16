using UnityEngine;

/// <summary>
/// Central balancing config for core gameplay values.
/// If assigned to systems, it overrides their local inspector values.
/// Create via Assets -> Create -> Game Config -> Game Balance Config.
/// </summary>
[CreateAssetMenu(
    fileName = "GameBalanceConfig",
    menuName = "Game Config/Game Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("Player Health")]
    public float maxHealth = 100f;
    public float sideScrapeDamage = 25f;
    public float sideScrapeCooldown = 0.2f;

    [Header("Scoring & Combo")]
    public float distanceScoreMultiplier = 1f;
    public int pickupBaseScore = 10;
    public float comboIncreasePerPickup = 1f;
    public float maxComboValue = 10f;
    public float comboDecayPerSecond = 1f;
    public float comboToMultiplierFactor = 0.1f;

    [Header("Obstacles")]
    public int obstacleLaneCount = 3;
    public float obstacleLaneSpacing = 3f;
    [Range(0f, 1f)] public float obstacleSpawnProbability = 0.7f;
    public int obstacleSpawnEveryNthSegment = 2;

    [Header("Pickups")]
    public int coinValue = 1;
    public int pickupLaneCount = 3;
    public float pickupLaneSpacing = 3f;
    [Range(0f, 1f)] public float pickupSegmentProbability = 0.7f;
    public int pickupMinPerSegment = 1;
    public int pickupMaxPerSegment = 2;
    public float pickupFloatHeight = 0.5f;

    [Header("Powerups")]
    [Range(0f, 1f)] public float powerupSpawnChance = 0.15f;
    public float autoPilotDuration = 4f;
    public float coinMultiplierValue = 2f;
    public float coinMultiplierDuration = 6f;
    public float scoreMultiplierValue = 2f;
    public float scoreMultiplierDuration = 6f;
    public float shieldDuration = 5f;
    public float coinBonanzaSpawnMultiplier = 2.5f;
    public float coinBonanzaDuration = 6f;

    [Header("Continues")]
    public int maxContinuesPerRun = 3;
    public float continueRespawnBackDistance = 8f;
    public float continueRespawnHeightOffset = 0.5f;
}
