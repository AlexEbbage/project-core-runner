using System.Collections.Generic;
using UnityEngine;

public partial class ObstacleRingGenerator
{
    #region Pickups

    private void ConfigurePickupRing(RingInstance ring)
    {
        if (ring == null || ring.root == null || pickupPrefab == null)
            return;

        if (!EnsurePickupChain())
            return;

        int slotCount = Mathf.Max(1, pickupSlotsPerRing);
        int slotsToFill = RandomRange(pickupSlotsToFillMin, pickupSlotsToFillMax + 1);
        slotsToFill = Mathf.Clamp(slotsToFill, 0, slotCount);

        var usedSlots = new HashSet<int>();
        for (int i = 0; i < slotsToFill; i++)
        {
            float spawnChance = Mathf.Clamp01(pickupSlotSpawnChance * _pickupSpawnChanceMultiplier);
            if (RandomValue() > spawnChance)
                continue;

            bool spawned = TrySpawnPickup(ring, slotCount, usedSlots, ShouldSpawnPowerupInChain());
            if (!spawned)
                break;
        }

        AdvancePickupChain();
    }

    private void ConfigureObstacleRingPickups(RingInstance ring)
    {
        if (ring == null || ring.root == null || pickupPrefab == null)
            return;

        float obstacleChance = Mathf.Clamp01(obstacleRingPickupChance * _pickupSpawnChanceMultiplier);
        if (RandomValue() > obstacleChance)
            return;

        int slotCount = Mathf.Max(1, pickupSlotsPerRing);
        var usedSlots = new HashSet<int>();
        TrySpawnPickup(ring, slotCount, usedSlots, ShouldSpawnPowerupInChain());
    }

    private bool TrySpawnPickup(RingInstance ring, int slotCount, HashSet<int> usedSlots, bool allowPowerup)
    {
        int attempts = Mathf.Max(1, pickupPlacementAttempts);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int slotIndex = RandomRange(0, slotCount);
            if (!usedSlots.Add(slotIndex))
                continue;

            float angleStep = 360f / slotCount;
            float angleDeg = slotIndex * angleStep;
            float radius = pickupSlotRadiusOverride > 0f ? pickupSlotRadiusOverride : GetDefaultPickupRadius();

            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius, 0f);
            localPos += Vector3.up * pickupFloatHeight;
            Vector3 worldPos = ring.root.TransformPoint(localPos);
            if (IsPickupBlocked(worldPos))
                continue;

            var pickup = Instantiate(pickupPrefab, ring.root);
            pickup.transform.localPosition = localPos;
            pickup.transform.localRotation = Quaternion.LookRotation(Vector3.forward, -localPos.normalized);

            bool spawnPowerup = allowPowerup && RandomValue() <= powerupSpawnChance;
            if (spawnPowerup)
            {
                pickup.Configure(PickupType.Powerup, ChooseRandomPowerup());
            }
            else
            {
                pickup.Configure(PickupType.Coin, PowerupType.CoinMultiplier);
            }

            ring.pickups.Add(pickup.gameObject);
            return true;
        }

        return false;
    }

    private bool EnsurePickupChain()
    {
        if (_pickupChainRemaining > 0)
            return true;

        if (_pickupChainGapRemaining > 0)
        {
            _pickupChainGapRemaining--;
            return false;
        }

        if (RandomValue() > pickupChainStartChance)
            return false;

        _pickupChainLength = RandomRange(pickupChainMinRings, pickupChainMaxRings + 1);
        _pickupChainRemaining = _pickupChainLength;
        return true;
    }

    private void AdvancePickupChain()
    {
        if (_pickupChainRemaining <= 0)
            return;

        _pickupChainRemaining--;
        if (_pickupChainRemaining <= 0)
        {
            _pickupChainGapRemaining = RandomRange(pickupChainGapMin, pickupChainGapMax + 1);
        }
    }

    private bool ShouldSpawnPowerupInChain()
    {
        if (_pickupChainRemaining <= 0)
            return false;

        int chainIndex = _pickupChainLength - _pickupChainRemaining;
        int midIndex = _pickupChainLength / 2;
        return chainIndex == 0 || chainIndex == midIndex || _pickupChainRemaining == 1;
    }

    private PowerupType ChooseRandomPowerup()
    {
        if (powerupEntries == null || powerupEntries.Length == 0)
            return PowerupType.CoinMultiplier;

        int totalWeight = 0;
        foreach (var entry in powerupEntries)
        {
            if (entry == null)
                continue;
            totalWeight += Mathf.Max(0, entry.weight);
        }

        if (totalWeight <= 0)
            return powerupEntries[0].type;

        int roll = RandomRange(0, totalWeight);
        int cumulative = 0;
        foreach (var entry in powerupEntries)
        {
            if (entry == null)
                continue;
            int w = Mathf.Max(0, entry.weight);
            cumulative += w;
            if (roll < cumulative)
                return entry.type;
        }

        return powerupEntries[0].type;
    }

    private float GetDefaultPickupRadius()
    {
        return playerController != null ? playerController.TubeRadius : 5f;
    }

    private void ClearPickups(RingInstance ring)
    {
        if (ring == null)
            return;

        foreach (var pickup in ring.pickups)
        {
            if (pickup != null)
                Destroy(pickup);
        }
        ring.pickups.Clear();
    }

    private bool ShouldSpawnObstacleRing()
    {
        int interval = Mathf.RoundToInt(Mathf.Lerp(obstacleRingIntervalStart, obstacleRingIntervalAtMaxDifficulty, GetDifficulty01()));
        interval = Mathf.Max(1, interval);
        _ringSequenceIndex++;
        return _ringSequenceIndex % interval == 0;
    }

    public void SetPickupSpawnChanceMultiplier(float multiplier)
    {
        _pickupSpawnChanceMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetPickupFloatHeight(float height)
    {
        pickupFloatHeight = Mathf.Max(0f, height);
    }

    private bool IsPickupBlocked(Vector3 worldPosition)
    {
        return Physics.CheckSphere(worldPosition, pickupClearanceRadius, obstacleOverlapMask);
    }

    #endregion
}
