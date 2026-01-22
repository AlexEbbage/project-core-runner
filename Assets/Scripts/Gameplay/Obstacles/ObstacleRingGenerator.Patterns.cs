using System.Collections.Generic;
using UnityEngine;

public partial class ObstacleRingGenerator
{
    #region Type-run state

    private void EnsurePatternState()
    {
        if (_patternRingsRemaining > 0)
            return;

        int difficulty = GetDifficultyLevel();
        _currentPatternPrefab = ChooseRandomObstaclePrefab(difficulty);
        if (_currentPatternPrefab == null)
        {
            _patternRingsRemaining = 0;
            return;
        }

        _currentWedgePatternType = GetWedgePatternType(_currentPatternPrefab);

        int minRun = Mathf.Max(1, _currentPatternPrefab.minRunLength + difficulty * _currentPatternPrefab.runLengthDifficultyBonus);
        int maxRun = Mathf.Max(minRun, _currentPatternPrefab.maxRunLength + difficulty * _currentPatternPrefab.runLengthDifficultyBonus);
        _patternRingsRemaining = RandomRange(minRun, maxRun + 1);
        _patternIndex = 0;

        _currentRotationConfig = ChooseRotationConfig(_currentPatternPrefab, difficulty);
        _currentRotationStep = RandomRange(0, Mathf.Max(1, sideCount));
        _rotationDirectionSign = _currentRotationConfig != null && !_currentRotationConfig.allowBothDirections
            ? (RandomValue() > 0.5f ? 1 : -1)
            : 0;
    }

    private ObstacleRingPrefab ChooseRandomObstaclePrefab(int difficulty)
    {
        var available = new List<ObstacleRingPrefab>();
        if (obstaclePrefabs != null)
        {
            foreach (var entry in obstaclePrefabs)
            {
                if (entry == null || entry.prefab == null)
                    continue;

                if (difficulty < entry.minDifficulty || difficulty > entry.maxDifficulty)
                    continue;

                available.Add(entry);
            }
        }

        if (available.Count == 0)
        {
            if (obstaclePrefabs != null)
            {
                foreach (var entry in obstaclePrefabs)
                {
                    if (entry != null && entry.prefab != null)
                        return entry;
                }
            }
            return null;
        }

        int totalWeight = 0;
        foreach (var entry in available)
        {
            totalWeight += Mathf.Max(0, entry.weight);
        }

        if (totalWeight <= 0)
            return available[0];

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in available)
        {
            int w = Mathf.Max(0, entry.weight);
            cumulative += w;
            if (roll < cumulative)
                return entry;
        }

        return available[0];
    }

    private static RotationDifficultyConfig ChooseRotationConfig(ObstacleRingPrefab prefab, int difficulty)
    {
        if (prefab == null || prefab.rotationByDifficulty == null || prefab.rotationByDifficulty.Length == 0)
            return null;

        RotationDifficultyConfig bestMatch = null;
        foreach (var config in prefab.rotationByDifficulty)
        {
            if (config == null)
                continue;

            if (config.difficultyLevel == difficulty)
                return config;

            if (config.difficultyLevel <= difficulty)
            {
                if (bestMatch == null || config.difficultyLevel > bestMatch.difficultyLevel)
                    bestMatch = config;
            }
        }

        return bestMatch ?? prefab.rotationByDifficulty[0];
    }

    #endregion

    #region Obstacle configuration

    private void ConfigureObstacleRing(RingInstance ring)
    {
        if (_currentPatternPrefab == null)
            return;

        GameObject prefab = _currentPatternPrefab.prefab;
        if (ring.obstacleInstance != null && ring.obstacleConfig != _currentPatternPrefab)
        {
            Destroy(ring.obstacleInstance);
            SetObstacleInstance(ring, null);
        }

        if (prefab != null && ring.obstacleInstance == null)
        {
            var instance = Instantiate(prefab, ring.root);
            SetObstacleInstance(ring, instance);
            EnsureObstacleDissolvers(instance);
        }

        ring.obstacleConfig = _currentPatternPrefab;

        if (ring.obstacleInstance != null)
        {
            ReactivateObstacleInstance(ring.obstacleInstance);

            float angleStep = 360f / Mathf.Max(3, sideCount);
            float zAngle = _currentRotationStep * angleStep;

            var t = ring.obstacleInstance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, 0f, zAngle);

            ConfigureObstacleInstance(ring.obstacleInstance);
        }

        AdvancePatternRotation();
        _patternIndex++;
    }

    private void AdvancePatternRotation()
    {
        if (_currentRotationConfig == null || _patternRingsRemaining <= 1)
            return;

        int minStep = Mathf.Max(0, _currentRotationConfig.minRotationSteps);
        int maxStep = Mathf.Max(minStep, _currentRotationConfig.maxRotationSteps);
        int steps = RandomRange(minStep, maxStep + 1);
        if (steps == 0)
            return;

        int direction;
        if (_currentRotationConfig.allowBothDirections)
        {
            direction = RandomValue() > 0.5f ? 1 : -1;
        }
        else
        {
            direction = _rotationDirectionSign == 0 ? 1 : _rotationDirectionSign;
        }

        _currentRotationStep = Mod(_currentRotationStep + steps * direction, sideCount);
    }

    private void ConfigureObstacleInstance(GameObject instance)
    {
        if (instance == null)
            return;

        var fan = instance.GetComponent<FanObstacle>();
        if (fan != null)
        {
            fan.SetSideCount(sideCount);
            fan.ApplyDifficulty(GetDifficulty01());
        }

        var laser = instance.GetComponent<LaserObstacle>();
        if (laser != null)
        {
            laser.SetSideCount(sideCount);
            ConfigureLaserSettings(laser);
        }

        var door = instance.GetComponent<DoorObstacle>();
        if (door != null)
        {
            door.ResetCycle();
        }

        var wedge = instance.GetComponent<WedgeObstacle>();
        if (wedge != null)
        {
            wedge.SetSideCount(sideCount);
            wedge.RegeneratePattern();
            _currentWedgePatternType = wedge.CurrentPatternType;
        }
    }

    private void ConfigureLaserSettings(LaserObstacle laser)
    {
        if (laser == null)
            return;

        LaserBehaviorSettings settings = laserRandomOrientationSettings;

        float difficulty = GetDifficulty01();
        float rotationSpeed = settings.rotationSpeed;
        float pulseDuration = settings.pulseDuration;
        float dutyCycle = settings.dutyCycle;

        if (scaleLaserWithDifficulty)
        {
            rotationSpeed = Mathf.Lerp(settings.rotationSpeed, laserRotationSpeedAtMaxDifficulty, difficulty);
            pulseDuration = Mathf.Lerp(settings.pulseDuration, laserPulseDurationAtMaxDifficulty, difficulty);
            dutyCycle = Mathf.Lerp(settings.dutyCycle, laserDutyCycleAtMaxDifficulty, difficulty);
        }

        laser.ConfigureRotation(settings.enableRotation, rotationSpeed);
        laser.ConfigureBeamCycle(
            settings.enableBeamCycling,
            pulseDuration,
            dutyCycle,
            settings.startBeamsOn,
            settings.randomizeBeamCyclePhase);
    }

    #endregion
}
