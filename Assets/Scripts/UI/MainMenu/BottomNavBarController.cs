using System;
using UnityEngine;
using UnityEngine.UI;

public class BottomNavBarController : MonoBehaviour
{
    [Serializable]
    private class NavButton
    {
        public MainPage page;
        public Button button;
        public GameObject selectedState;
        public GameObject lockedState;
    }

    [Header("Buttons")]
    [SerializeField] private NavButton[] buttons;

    [Header("Locks")]
    [SerializeField] private int requiredLevelForShop = 2;
    [SerializeField] private int requiredLevelForHangar = 3;

    private MainMenuController _menu;
    private MainPage _currentPage;

    public void Initialize(MainMenuController menu)
    {
        _menu = menu;
        if (buttons == null)
            return;

        foreach (var navButton in buttons)
        {
            if (navButton == null || navButton.button == null)
                continue;

            var page = navButton.page;
            navButton.button.onClick.AddListener(() => HandlePagePressed(page));
        }
    }

    public void SetSelected(MainPage page)
    {
        _currentPage = page;
        if (buttons == null)
            return;

        foreach (var navButton in buttons)
        {
            if (navButton == null)
                continue;

            bool isSelected = navButton.page == page;
            if (navButton.selectedState != null)
                navButton.selectedState.SetActive(isSelected);
        }
    }

    public void UpdateLocks(int playerLevel)
    {
        if (buttons == null)
            return;

        foreach (var navButton in buttons)
        {
            if (navButton == null)
                continue;

            bool locked = IsLocked(navButton.page, playerLevel);
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
            MainPage.Hangar => playerLevel < requiredLevelForHangar,
            _ => false
        };
    }
}
