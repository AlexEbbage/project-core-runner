//using UnityEngine;

///// <summary>
///// Camera that stays on the tube axis:
///// - X/Y = 0 (center of tube)
///// - Z follows player with an offset
///// - Rotation fixed looking down +Z
///// This feels closer to Super Hexagon and avoids nausea.
///// </summary>
//public class PlayerCameraFollow : MonoBehaviour
//{
//    [SerializeField] private Transform target;

//    [Header("Offsets")]
//    [Tooltip("How far behind the player along Z.")]
//    [SerializeField] private float zOffset = -15f;

//    [Header("Smoothing")]
//    [SerializeField] private float positionLerpSpeed = 10f;

//    private void LateUpdate()
//    {
//        if (target == null)
//            return;

//        float dt = Time.deltaTime;

//        float targetZ = target.position.z + zOffset;
//        Vector3 desiredPos = new Vector3(0f, 0f, targetZ);

//        transform.position = Vector3.Lerp(transform.position, desiredPos, positionLerpSpeed * dt);

//        // Always look straight down the tunnel
//        transform.rotation = Quaternion.identity; // forward = (0,0,1), up = (0,1,0)
//    }

//    public void SetTarget(Transform newTarget)
//    {
//        target = newTarget;
//    }
//}

//using UnityEngine;

///// <summary>
///// Camera that:
///// - Stays on the tube axis (x=y=0), following the player's Z.
///// - Spins slightly over time.
///// - Also follows a fraction of the player's angular rotation around the tube.
///// This feels closer to Super Hexagon (world spinning).
///// </summary>
//public class PlayerCameraFollow : MonoBehaviour
//{
//    [SerializeField] private PlayerController player;

//    [Header("Position")]
//    [Tooltip("How far behind the player along Z.")]
//    [SerializeField] private float zOffset = -15f;
//    [SerializeField] private float positionLerpSpeed = 12f;

//    [Header("Rotation")]
//    [Tooltip("How fast the camera spins constantly (degrees per second).")]
//    [SerializeField] private float baseSpinSpeedDeg = 20f;

//    [Tooltip("How much of the player's angle we add to the camera spin (0..1).")]
//    [Range(0f, 1f)]
//    [SerializeField] private float followPlayerAngleFactor = 0.3f;

//    [SerializeField] private float rotationLerpSpeed = 12f;

//    private float _currentZRot;   // current camera z-rotation in degrees

//    private void LateUpdate()
//    {
//        if (player == null)
//            return;

//        float dt = Time.deltaTime;

//        // --- Position: stay centered in tube, just follow Z ---
//        float targetZ = player.transform.position.z + zOffset;
//        Vector3 desiredPos = new Vector3(0f, 0f, targetZ);
//        transform.position = Vector3.Lerp(transform.position, desiredPos, positionLerpSpeed * dt);

//        // --- Rotation: base spin + a bit of player's angle ---
//        float baseSpin = baseSpinSpeedDeg * Time.time;
//        float targetZRot = baseSpin + player.CurrentAngleDegrees * followPlayerAngleFactor;

//        _currentZRot = Mathf.LerpAngle(_currentZRot, targetZRot, rotationLerpSpeed * dt);
//        Quaternion desiredRot = Quaternion.Euler(0f, 0f, _currentZRot);

//        transform.rotation = desiredRot;
//    }

//    public void SetPlayer(PlayerController p)
//    {
//        player = p;
//    }
//}

using UnityEngine;

/// <summary>
/// Camera that follows the player around the tube,
/// but keeps 'up' pointing towards the tunnel centre,
/// so orientation is stable and less nauseating.
/// </summary>
public class PlayerCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Offsets")]
    [Tooltip("How far behind the player along +Z.")]
    [SerializeField] private float distanceBack = 10f;

    [Tooltip("Offset away from the tunnel centre (radial). Positive moves further out.")]
    [SerializeField] private float radialOffset = 1f;

    [Tooltip("How high to offset along the radial 'up' direction.")]
    [SerializeField] private float heightAlongRadial = 0.2f;

    [Header("Smoothing")]
    [SerializeField] private float positionLerpSpeed = 16f;
    [SerializeField] private float rotationLerpSpeed = 16f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        float dt = Time.deltaTime;

        // Radial vector pointing from player towards tunnel centre
        Vector3 toCenter = new Vector3(-target.position.x, -target.position.y, 0f);
        Vector3 radialIn = toCenter.sqrMagnitude > 0.0001f
            ? toCenter.normalized
            : Vector3.up;

        // Define our up and forward axes
        Vector3 up = radialIn;             // points towards tube centre
        Vector3 forward = Vector3.forward; // always look down the tunnel

        // Desired position:
        // - Behind the player along the tunnel
        // - Slightly offset along the radial direction (so we're not exactly in the centre)
        Vector3 desiredPos =
            target.position
            - forward * distanceBack
            + up * heightAlongRadial
            - up * radialOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, positionLerpSpeed * dt);

        Quaternion desiredRot = Quaternion.LookRotation(forward, up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerpSpeed * dt);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
