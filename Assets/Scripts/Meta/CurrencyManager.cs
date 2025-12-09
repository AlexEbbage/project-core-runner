using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private const string CurrencyKey = "Currency";

    public int TotalCurrency => PlayerPrefs.GetInt(CurrencyKey, 0);

    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;
        int current = PlayerPrefs.GetInt(CurrencyKey, 0);
        current += amount;
        PlayerPrefs.SetInt(CurrencyKey, current);
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Currency")]
    public void ResetCurrency()
    {
        PlayerPrefs.DeleteKey(CurrencyKey);
    }
}
