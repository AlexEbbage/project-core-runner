using UnityEngine;

/// <summary>
/// Collectible pickup that grants currency or powerups.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType pickupType = PickupType.Coin;
    [SerializeField] private PowerupType powerupType = PowerupType.CoinMultiplier;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        var collider = GetComponent<Collider>();
        collider.isTrigger = true;

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var scoreManager = FindFirstObjectByType<RunScoreManager>();
        var currencyManager = FindFirstObjectByType<RunCurrencyManager>();
        var powerupController = other.GetComponent<PlayerPowerupController>();

        if (pickupType == PickupType.Coin)
        {
            scoreManager?.OnPickupCollected();
            if (currencyManager != null)
            {
                currencyManager.AddCoins(currencyManager.GetCoinValue());
            }
        }
        else if (pickupType == PickupType.Powerup)
        {
            powerupController?.ActivatePowerup(powerupType);
        }

        audioManager?.PlayPickup();
        Destroy(gameObject);
    }

    public void Configure(PickupType newType, PowerupType newPowerupType)
    {
        pickupType = newType;
        powerupType = newPowerupType;
    }
}
