using UnityEngine;

public partial class ObstacleRingGenerator
{
    private WedgeObstacle.LocalPatternType _currentWedgePatternType = WedgeObstacle.LocalPatternType.EveryOther;

    public bool InWedgeRunDebug => IsCurrentPatternWedge() && _patternRingsRemaining > 0;

    public WedgeObstacle.LocalPatternType WedgeCurrentLocalPatternDebug => _currentWedgePatternType;

    public int WedgeRunRingsRemainingDebug => InWedgeRunDebug ? _patternRingsRemaining : 0;

    public int WedgeCurrentRotationStepDebug => InWedgeRunDebug ? _currentRotationStep : 0;

    public int MaxWedgeRotationStepsPerRingDebug => InWedgeRunDebug ? GetCurrentMaxRotationSteps() : 0;

    public int WedgeRotationDirectionSignDebug => InWedgeRunDebug ? _rotationDirectionSign : 0;

    private bool IsCurrentPatternWedge()
    {
        return _currentPatternPrefab != null
            && _currentPatternPrefab.prefab != null
            && _currentPatternPrefab.prefab.GetComponent<WedgeObstacle>() != null;
    }

    private WedgeObstacle.LocalPatternType GetWedgePatternType(ObstacleRingPrefab prefab)
    {
        if (prefab == null || prefab.prefab == null)
            return WedgeObstacle.LocalPatternType.EveryOther;

        var wedge = prefab.prefab.GetComponent<WedgeObstacle>();
        return wedge != null ? wedge.CurrentPatternType : WedgeObstacle.LocalPatternType.EveryOther;
    }

    private int GetCurrentMaxRotationSteps()
    {
        if (_currentRotationConfig == null)
            return 0;

        return Mathf.Max(0, _currentRotationConfig.maxRotationSteps);
    }
}
