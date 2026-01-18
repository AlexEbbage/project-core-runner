using System.Collections.Generic;
using UnityEngine;

public partial class ObstacleRingGenerator
{
    #region Type-run state

    private void EnsurePatternState()
    {
        if (_patternRingsRemaining > 0 || _wedgeRunRingsRemaining > 0)
            return;

        float difficulty = GetDifficulty01();
        int minRun = Mathf.RoundToInt(Mathf.Lerp(minPatternRunLength, minPatternRunLengthAtMaxDifficulty, difficulty));
        int maxRun = Mathf.RoundToInt(Mathf.Lerp(maxPatternRunLength, maxPatternRunLengthAtMaxDifficulty, difficulty));
        maxRun = Mathf.Max(maxRun, minRun);
        float shiftedChance = Mathf.Lerp(shiftedPatternChance, shiftedPatternChanceAtMaxDifficulty, difficulty);

        _currentPatternType = ChooseRandomObstacleType(difficulty);

        _currentPatternMode = (RandomValue() < shiftedChance)
            ? PatternMode.ShiftedOrientation
            : PatternMode.RandomOrientationEachRing;

        _patternRingsRemaining = RandomRange(minRun, maxRun + 1);

        _currentRotationStep = RandomRange(0, Mathf.Max(1, sideCount));
        _rotationStepDelta = RandomRange(-2, 3);
        if (_rotationStepDelta == 0)
            _rotationStepDelta = 1;
    }

    private ObstacleRingType ChooseRandomObstacleType(float difficulty)
    {
        var available = new List<ObstacleRingPrefab>();
        if (obstaclePrefabs != null)
        {
            foreach (var e in obstaclePrefabs)
            {
                if (e != null && e.prefab != null)
                    available.Add(e);
            }
        }

        if (available.Count == 0)
            return ObstacleRingType.Wedge;

        int totalWeight = 0;
        foreach (var entry in available)
        {
            int baseWeight = Mathf.Max(0, entry.weight);
            int bonus = Mathf.RoundToInt(Mathf.Max(0f, entry.weightBonusAtMaxDifficulty) * difficulty);
            totalWeight += Mathf.Max(0, baseWeight + bonus);
        }

        if (totalWeight <= 0)
            return available[0].type;

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in available)
        {
            int baseWeight = Mathf.Max(0, entry.weight);
            int bonus = Mathf.RoundToInt(Mathf.Max(0f, entry.weightBonusAtMaxDifficulty) * difficulty);
            int w = Mathf.Max(0, baseWeight + bonus);
            cumulative += w;
            if (roll < cumulative)
                return entry.type;
        }

        return available[0].type;
    }

    private GameObject GetPrefabForType(ObstacleRingType type)
    {
        if (obstaclePrefabs == null) return null;

        foreach (var e in obstaclePrefabs)
        {
            if (e != null && e.prefab != null && e.type == type)
                return e.prefab;
        }
        return null;
    }

    #endregion

    #region Non-wedge configuration

    private void ConfigureNonWedgeRing(RingInstance ring, ObstacleRingType type)
    {
        int rotationStep;
        if (_currentPatternMode == PatternMode.RandomOrientationEachRing)
        {
            rotationStep = RandomRange(0, sideCount);
        }
        else
        {
            rotationStep = _currentRotationStep;
            int stepChange = RandomRange(-2, 3);
            if (stepChange == 0)
                stepChange = 1;
            _rotationStepDelta = stepChange;
            _currentRotationStep = Mod(_currentRotationStep + _rotationStepDelta, sideCount);
        }

        float angleStep = 360f / Mathf.Max(3, sideCount);
        float zAngle = rotationStep * angleStep;

        GameObject prefab = GetPrefabForType(type);

        if (ring.obstacleInstance != null && ring.type != type)
        {
            Destroy(ring.obstacleInstance);
            SetObstacleInstance(ring, null);
        }

        if (prefab != null && ring.obstacleInstance == null)
        {
            SetObstacleInstance(ring, Instantiate(prefab, ring.root));
        }

        ring.type = type;

        if (ring.obstacleInstance != null)
        {
            if (!ring.obstacleInstance.activeSelf)
                ring.obstacleInstance.SetActive(true);

            var t = ring.obstacleInstance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, 0f, zAngle);

            ConfigureObstacleInstanceNonWedge(ring.obstacleInstance, type);
        }
    }

    private void ConfigureObstacleInstanceNonWedge(GameObject instance, ObstacleRingType type)
    {
        if (instance == null) return;

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
    }

    private void ConfigureLaserSettings(LaserObstacle laser)
    {
        if (laser == null) return;

        LaserBehaviorSettings settings = _currentPatternMode == PatternMode.ShiftedOrientation
            ? laserShiftedOrientationSettings
            : laserRandomOrientationSettings;

        float difficulty = enableDifficultyScaling ? GetDifficulty01() : 0f;
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

    #region Wedge configuration using WedgePatternSet

    private void ConfigureWedgeRing(RingInstance ring, ObstacleRingType type)
    {
        bool isWedgeType = type == ObstacleRingType.Wedge;

        if (!isWedgeType)
        {
            _inWedgeRun = false;
            _currentWedgeSet = null;
            return;
        }

        if (ring.obstacleInstance != null && ring.type != type)
        {
            Destroy(ring.obstacleInstance);
            SetObstacleInstance(ring, null);
        }

        if (!_inWedgeRun || _wedgeRunRingsRemaining <= 0 || _currentWedgeSet == null)
        {
            _currentWedgeSet = ChooseWedgePatternSet();
            if (_currentWedgeSet == null)
            {
                ConfigureNonWedgeRing(ring, type);
                return;
            }

            _inWedgeRun = true;
            int minRings = _currentWedgeSet.minRings;
            int maxRings = _currentWedgeSet.maxRings;
            if (enableDifficultyScaling)
            {
                float difficulty = GetDifficulty01();
                int minTarget = _currentWedgeSet.minRingsAtMaxDifficulty >= 0
                    ? _currentWedgeSet.minRingsAtMaxDifficulty
                    : minRings;
                int maxTarget = _currentWedgeSet.maxRingsAtMaxDifficulty >= 0
                    ? _currentWedgeSet.maxRingsAtMaxDifficulty
                    : maxRings;
                minRings = Mathf.RoundToInt(Mathf.Lerp(minRings, minTarget, difficulty));
                maxRings = Mathf.RoundToInt(Mathf.Lerp(maxRings, maxTarget, difficulty));
            }
            maxRings = Mathf.Max(maxRings, minRings);
            _wedgeRunRingsRemaining = RandomRange(minRings, maxRings + 1);
            _wedgeCurrentRotationStep = RandomRange(0, Mathf.Max(1, sideCount));

            if (_currentWedgeSet.oneDirectionOnly)
            {
                _wedgeRotationDirectionSign = RandomValue() > 0.5f ? 1 : -1;
            }
            else
            {
                _wedgeRotationDirectionSign = 0;
            }
        }

        float angleStep = 360f / Mathf.Max(3, sideCount);
        float zAngle = _wedgeCurrentRotationStep * angleStep;

        GameObject prefab = GetPrefabForType(type);
        if (prefab != null && ring.obstacleInstance == null)
        {
            SetObstacleInstance(ring, Instantiate(prefab, ring.root));
        }

        ring.type = type;

        if (ring.obstacleInstance != null)
        {
            if (!ring.obstacleInstance.activeSelf)
                ring.obstacleInstance.SetActive(true);

            Transform t = ring.obstacleInstance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.Euler(0f, 0f, zAngle);

            var wedge = ring.obstacleInstance.GetComponent<WedgeObstacle>();
            if (wedge != null)
            {
                wedge.SetSideCount(sideCount);
                wedge.SetLocalPattern(_currentWedgeSet.localPattern);
                wedge.RegeneratePattern();
            }
        }

        _wedgeRunRingsRemaining--;

        if (_wedgeRunRingsRemaining > 0)
        {
            int maxStep = Mathf.Max(0, _currentWedgeSet.maxRotationStepPerRing);
            if (enableDifficultyScaling)
            {
                float difficulty = GetDifficulty01();
                int maxStepTarget = _currentWedgeSet.maxRotationStepPerRingAtMaxDifficulty >= 0
                    ? _currentWedgeSet.maxRotationStepPerRingAtMaxDifficulty
                    : maxStep;
                maxStep = Mathf.RoundToInt(Mathf.Lerp(maxStep, maxStepTarget, difficulty));
                float multiplier = Mathf.Lerp(1f, wedgeRotationStepMultiplierAtMaxDifficulty, difficulty);
                maxStep = Mathf.RoundToInt(maxStep * multiplier);
            }
            int delta = 0;

            if (maxStep > 0)
            {
                if (_currentWedgeSet.oneDirectionOnly)
                {
                    int steps = RandomRange(0, maxStep + 1);
                    int sign = (_wedgeRotationDirectionSign == 0) ? 1 : _wedgeRotationDirectionSign;
                    delta = steps * sign;
                }
                else
                {
                    delta = RandomRange(-maxStep, maxStep + 1);
                }
            }

            _wedgeCurrentRotationStep = Mod(_wedgeCurrentRotationStep + delta, sideCount);
        }
        else
        {
            _inWedgeRun = false;
            _currentWedgeSet = null;
        }
    }

    private WedgePatternSet ChooseWedgePatternSet()
    {
        if (wedgePatternSets == null || wedgePatternSets.Length == 0)
            return null;

        int totalWeight = 0;
        foreach (var set in wedgePatternSets)
        {
            if (set == null) continue;
            int baseWeight = Mathf.Max(0, set.weight);
            int bonus = 0;
            if (enableDifficultyScaling)
            {
                float difficulty = GetDifficulty01();
                bonus = Mathf.RoundToInt(Mathf.Max(0f, set.weightBonusAtMaxDifficulty) * difficulty);
            }
            totalWeight += Mathf.Max(0, baseWeight + bonus);
        }

        if (totalWeight <= 0)
        {
            foreach (var set in wedgePatternSets)
            {
                if (set != null) return set;
            }
            return null;
        }

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var set in wedgePatternSets)
        {
            if (set == null) continue;
            int baseWeight = Mathf.Max(0, set.weight);
            int bonus = 0;
            if (enableDifficultyScaling)
            {
                float difficulty = GetDifficulty01();
                bonus = Mathf.RoundToInt(Mathf.Max(0f, set.weightBonusAtMaxDifficulty) * difficulty);
            }
            int w = Mathf.Max(0, baseWeight + bonus);
            cumulative += w;
            if (roll < cumulative)
                return set;
        }

        return wedgePatternSets[0];
    }

    #endregion
}
