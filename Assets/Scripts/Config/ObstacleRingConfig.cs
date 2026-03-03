using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Obstacle Ring Config", fileName = "ObstacleRingConfig")]
public class ObstacleRingConfig : ScriptableObject
{
    [Header("Prefab")]
    [Tooltip("Prefab that contains an ObstacleRingController.")]
    [SerializeField] private GameObject ringPrefab;

    [Header("Obstacle Type")]
    [SerializeField] private ObstacleType obstacleType = ObstacleType.Walls;

    [Header("Difficulty Bands")]
    [SerializeField] private List<ObstacleRingDifficultyConfig> difficultyConfigs = new List<ObstacleRingDifficultyConfig>();

    public GameObject RingPrefab => ringPrefab;
    public ObstacleType ObstacleType => obstacleType;
    public IReadOnlyList<ObstacleRingDifficultyConfig> DifficultyConfigs => difficultyConfigs;

    /// <summary>
    /// Returns all difficulty configs that can be used for the given difficulty level.
    /// </summary>
    public List<ObstacleRingDifficultyConfig> GetValidDifficultyConfigs(float difficultyLevel, List<ObstacleRingDifficultyConfig> buffer = null)
    {
        if (buffer == null)
            buffer = new List<ObstacleRingDifficultyConfig>();
        else
            buffer.Clear();

        for (int i = 0; i < difficultyConfigs.Count; i++)
        {
            var cfg = difficultyConfigs[i];
            if (cfg == null) continue;

            if (difficultyLevel >= cfg.minLevel && difficultyLevel <= cfg.maxLevel)
            {
                buffer.Add(cfg);
            }
        }

        return buffer;
    }
}

[Serializable]
public class ObstacleRingDifficultyConfig
{
    [Header("Difficulty Range")]
    [Tooltip("Minimum difficulty level (inclusive) where this config can be used.")]
    public float minLevel = 0;

    [Tooltip("Maximum difficulty level (inclusive) where this config can be used.")]
    public float maxLevel = 999;

    [Header("Pattern")]
    [Tooltip("Minimum rotation in degrees applied per iteration of this pattern.")]
    public float minRotationsPerIteration = 0;

    [Tooltip("Maximum rotation in degrees applied per iteration of this pattern.")]
    public float maxRotationsPerIteration = 0;

    [Tooltip("Minimum number of times this pattern will repeat consecutively.")]
    public int minIterationCount = 1;

    [Tooltip("Maximum number of times this pattern will repeat consecutively.")]
    public int maxIterationCount = 1;

    [Tooltip("If true, the same rotation direction is used for all iterations in this pattern. If false, direction can flip each iteration.")]
    public bool rotateInOneDirectionPerIteration = true;

    [Header("Speed")]
    [Tooltip("Minimum speed for this obstacle when this config is active.")]
    public float minSpeed = 0;

    [Tooltip("Maximum speed for this obstacle when this config is active.")]
    public float maxSpeed = 0;
}
