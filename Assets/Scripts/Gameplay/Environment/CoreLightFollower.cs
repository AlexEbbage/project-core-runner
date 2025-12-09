using UnityEngine;

/// <summary>
/// Keeps the core light / visual always a fixed distance
/// in front of the player along +Z, so you never fly past it.
/// </summary>
public class CoreLightFollower : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float distanceAhead = 200f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private void Awake()
    {
        if (player == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float targetZ = player.position.z + distanceAhead;
        transform.position = new Vector3(0f, 0f, targetZ) + offset;
    }
}
