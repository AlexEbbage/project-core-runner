using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Dot product threshold to consider a hit 'head-on' versus a side scrape.")]
    [SerializeField] private float headOnThreshold = 0.7f;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        // Auto-wire PlayerHealth if not set.
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        // In case some obstacles use triggers instead of solid colliders.
        HandleTriggerHit(other);
    }

    private void HandleHit(Collision collision)
    {
        // We are on the PLAYER, so collision.gameObject is the thing we hit.
        GameObject hitObject = collision.gameObject;

        // Only care about obstacles
        if (!hitObject.CompareTag("Obstacle"))
            return;

        if (playerHealth == null)
            return;

        // Get obstacle ring info
        ObstacleRingController ringController = hitObject.GetComponentInParent<ObstacleRingController>();
        string ringName = ringController != null ? ringController.RingName : "Unknown";

        // Use the first contact point for normal & hit position
        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;
        Vector3 hitPoint = contact.point;

        // Compare player's forward vs incoming normal to decide head-on vs scrape
        Transform playerTransform = transform;
        float alignment = Vector3.Dot(playerTransform.forward, -normal);

        if (alignment >= headOnThreshold)
        {
            playerHealth.HandleHeadOnHit(hitPoint, normal, ringName);
        }
        else
        {
            playerHealth.HandleSideScrapeHit(hitPoint, normal, ringName);
        }

        // Analytics
        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.ObstacleHit, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Type, ringName }
        });

        Debug.Log($"[PlayerCollisionHandler] Hit obstacle '{ringName}' (alignment={alignment})");
    }

    private void HandleTriggerHit(Collider other)
    {
        // For trigger-based obstacles (if you have any).
        if (!other.CompareTag("Obstacle"))
            return;

        if (playerHealth == null)
            return;

        ObstacleRingController ringController = other.GetComponentInParent<ObstacleRingController>();
        string ringName = ringController != null ? ringController.RingName : "Unknown";

        // For triggers we don't get a Collision/contact, so approximate position/normal
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 normal = (transform.position - hitPoint).sqrMagnitude > 0.001f
            ? (transform.position - hitPoint).normalized
            : -transform.forward;

        float alignment = Vector3.Dot(transform.forward, -normal);

        if (alignment >= headOnThreshold)
        {
            playerHealth.HandleHeadOnHit(hitPoint, normal, ringName);
        }
        else
        {
            playerHealth.HandleSideScrapeHit(hitPoint, normal, ringName);
        }

        gameManager?.LogAnalyticsEvent(AnalyticsEventNames.ObstacleHit, new Dictionary<string, object>
        {
            { AnalyticsEventNames.Params.Type, ringName }
        });

        Debug.Log($"[PlayerCollisionHandler] Trigger hit obstacle '{ringName}' (alignment={alignment})");
    }
}
