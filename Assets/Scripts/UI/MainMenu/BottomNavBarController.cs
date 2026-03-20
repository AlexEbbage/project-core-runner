using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomNavBarController : MonoBehaviour
{
    [Serializable]
    private class NavButton
    {
        public MainPage page;
        public Button button;
        public TMP_Text label;
        public GameObject selectedState;
        public GameObject lockedState;
    }

    [Header("Buttons")]
    [SerializeField] private NavButton[] buttons;

    [Header("Locks")]
    [SerializeField] private int requiredLevelForShop = 2;
    [SerializeField] private int requiredLevelForShip = 3;
    [SerializeField] private int requiredLevelForLab = 2;
    [SerializeField] private int requiredLevelForAchievements = 2;

    private MainMenuController _menu;
    private MainPage _currentPage;

    public void Initialize(MainMenuController menu)
    {
        _menu = menu;
        if (buttons == null)
            return;

        foreach (NavButton navButton in buttons)
        {
            if (navButton == null || navButton.button == null)
                continue;

            if (navButton.label == null)
                navButton.label = navButton.button.GetComponentInChildren<TMP_Text>(true);

            MainPage page = ResolveTargetPage(navButton.page);
            navButton.button.onClick.AddListener(() => HandlePagePressed(page));
            UpdateLabel(navButton);
        }
    }

    public void SetSelected(MainPage page)
    {
        _currentPage = page;
        if (buttons == null)
            return;

        foreach (NavButton navButton in buttons)
        {
            if (navButton == null)
                continue;

            bool isSelected = ResolveTargetPage(navButton.page) == page;
            if (navButton.selectedState != null)
                navButton.selectedState.SetActive(isSelected);

            if (navButton.label != null)
                navButton.label.color = isSelected ? Color.white : new Color(0.85f, 0.85f, 0.85f, 1f);
        }
    }

    public void UpdateLocks(int playerLevel)
    {
        if (buttons == null)
            return;

        foreach (NavButton navButton in buttons)
        {
            if (navButton == null)
                continue;

            bool locked = IsLocked(ResolveTargetPage(navButton.page), playerLevel);
            if (navButton.button != null)
                navButton.button.interactable = !locked;
            if (navButton.lockedState != null)
                navButton.lockedState.SetActive(locked);
        }
    }

    private void HandlePagePressed(MainPage page)
    {
        if (_menu == null)
            return;

        _menu.ShowPage(page);
        SetSelected(page);
    }

    private bool IsLocked(MainPage page, int playerLevel)
    {
        return page switch
        {
            MainPage.Shop => playerLevel < requiredLevelForShop,
            MainPage.Ship => playerLevel < requiredLevelForShip,
            MainPage.Lab => playerLevel < requiredLevelForLab,
            MainPage.Achievements => playerLevel < requiredLevelForAchievements,
            _ => false
        };
    }

    private void UpdateLabel(NavButton navButton)
    {
        if (navButton == null || navButton.label == null)
            return;

        navButton.label.text = ResolveTargetPage(navButton.page) switch
        {
            MainPage.Shop => "SHOP",
            MainPage.Ship => "SHIP",
            MainPage.Lab => "LAB",
            MainPage.LevelSelect => "LEVELS",
            MainPage.Achievements => "ACHIEVEMENTS",
            _ => navButton.page.ToString().ToUpperInvariant()
        };
    }

    private static MainPage ResolveTargetPage(MainPage page)
    {
        return page switch
        {
            MainPage.Hangar => MainPage.Ship,
            MainPage.Play => MainPage.LevelSelect,
            MainPage.Challenges => MainPage.Achievements,
            MainPage.Progression => MainPage.Lab,
            _ => page
        };
    }
}
