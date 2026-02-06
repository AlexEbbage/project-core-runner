using System;
using UnityEngine;

[Serializable]
public class PowerupEntry
{
    [Tooltip("Type of powerup to spawn.")]
    public PowerupType type = PowerupType.CoinMultiplier;

    [Tooltip("Relative weight for this powerup in the random selection. Higher = more common.")]
    public int weight = 1;
}
