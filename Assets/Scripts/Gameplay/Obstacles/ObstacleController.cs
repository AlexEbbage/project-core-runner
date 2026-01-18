using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to obstacle objects. Detects collisions with the Player
/// and decides whether it's a head-on collision or a side scrape,
/// then notifies PlayerHealth accordingly.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ObstacleController : MonoBehaviour
{
    [Header("Collision Detection")]
    [Tooltip("If the alignment between player's forward and collision normal is above this, treat as head-on.")]
    [Range(0f, 1f)]
    [SerializeField] private float headOnThreshold = 0.7f;

    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        PlayerHealth health = collision.collider.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;
        Vector3 hitPoint = contact.point;

        Transform playerTransform = collision.collider.transform;

        float alignment = Vector3.Dot(playerTransform.forward, -normal);
        string obstacleType = GetObstacleTypeName();

        if (alignment >= headOnThreshold)
        {
            health.HandleHeadOnHit(hitPoint, normal, obstacleType);
        }
        else
        {
            health.HandleSideScrapeHit(hitPoint, normal, obstacleType);
        }

        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.ObstacleHit, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Type, obstacleType }
        });
    }

    private string GetObstacleTypeName()
    {
        if (GetComponent<FanObstacle>() != null)
            return "Fan";
        if (GetComponent<LaserObstacle>() != null)
            return "Laser";
        if (GetComponent<WedgeObstacle>() != null)
            return "Wedge";

        return gameObject.name;
    }
}
