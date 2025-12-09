using UnityEngine;

/// <summary>
/// Simple light that follows the player down the tunnel and spins slowly,
/// giving moving highlights on the tunnel walls.
/// </summary>
[RequireComponent(typeof(Light))]
public class TunnelLightFollower : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float spinSpeedDeg = 30f;
    [SerializeField] private float zOffset = -5f;

    private float _angleDeg;

    private void Start()
    {
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
                player = pc.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        _angleDeg += spinSpeedDeg * Time.deltaTime;
        float rad = _angleDeg * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * radius;
        float y = Mathf.Sin(rad) * radius + heightOffset;
        float z = player.position.z + zOffset;

        transform.position = new Vector3(x, y, z);
    }
}
