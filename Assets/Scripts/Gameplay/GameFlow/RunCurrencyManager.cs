using UnityEngine;

/// <summary>
/// Tracks in-run currency earned from pickups.
/// </summary>
public class RunCurrencyManager : MonoBehaviour
{
    [Header("Config (optional)")]
    [SerializeField] private GameBalanceConfig balanceConfig;

    [Header("Currency")]
    [SerializeField] private int coinValue = 1;

    private int _currentCoins;

    public int CurrentCoins => _currentCoins;

    private void Awake()
    {
        if (balanceConfig != null)
        {
            coinValue = balanceConfig.coinValue;
        }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        _currentCoins += amount;
    }

    public int GetCoinValue() => coinValue;

    public void ResetRun()
    {
        _currentCoins = 0;
    }
}
