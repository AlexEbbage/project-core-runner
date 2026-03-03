using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PickupRing : MonoBehaviour
{
    [Tooltip("Assign all transforms around the ring where pickups can spawn.")]
    [SerializeField] private List<Transform> pickupSpawnPoints = new List<Transform>();

    public IReadOnlyList<Transform> PickupSpawnPoints => pickupSpawnPoints;

    /// <summary>
    /// Convenience helper if you just need one random spawn point.
    /// </summary>
    public Transform GetRandomSpawnPoint()
    {
        if (pickupSpawnPoints == null || pickupSpawnPoints.Count == 0)
            return null;

        int index = Random.Range(0, pickupSpawnPoints.Count);
        return pickupSpawnPoints[index];
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pickupSpawnPoints == null)
            return;

        Gizmos.color = Color.yellow;

        foreach (var t in pickupSpawnPoints)
        {
            if (t == null) continue;
            Gizmos.DrawWireSphere(t.position, 0.1f);
        }
    }
#endif
}
