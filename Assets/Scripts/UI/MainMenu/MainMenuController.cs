using System.Collections;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private RectTransform hangarPage;
    [SerializeField] private RectTransform playPage;
    [SerializeField] private RectTransform shopPage;
    [SerializeField] private RectTransform challengesPage;
    [SerializeField] private RectTransform progressionPage;

    [Header("Hub")]
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private TopBarController topBarController;
    [SerializeField] private PlayerProfile profile;

    [Header("Navigation")]
    [SerializeField] private BottomNavBarController bottomNavBar;
    [SerializeField] private MainPage initialPage = MainPage.LevelSelect;

    [Header("Transitions")]
    [SerializeField] private float transitionDuration = 0.25f;

    private MainPage _currentPage;
    private bool _initialized;
    private Coroutine _transitionRoutine;

    public MainPage CurrentPage => _currentPage;

    private void Awake()
    {
        if (mainMenuUI == null)
            mainMenuUI = FindFirstObjectByType<MainMenuUI>();

        if (topBarController == null)
            topBarController = FindFirstObjectByType<TopBarController>();

        if (profile == null)
        {
            PlayerProfile[] profiles = Resources.FindObjectsOfTypeAll<PlayerProfile>();
            if (profiles != null && profiles.Length > 0)
                profile = profiles[0];
        }

        if (bottomNavBar != null)
            bottomNavBar.Initialize(this);

        EnsurePageControllers();
    }

    private void OnEnable()
    {
        RefreshHubChrome();
    }

    private void Start()
    {
        ShowPage(initialPage, true);
        if (bottomNavBar != null)
            bottomNavBar.SetSelected(initialPage);
    }

    public void ShowPage(MainPage page, bool instant = false, bool updateSelection = true)
    {
        page = NormalizeHubPage(page);

        if (page == MainPage.Lab)
        {
            mainMenuUI?.OpenLabPanelFromHub();
            if (updateSelection)
                bottomNavBar?.SetSelected(page);
            _currentPage = page;
            LogHubPageSelection(page);
            RefreshHubChrome();
            return;
        }

        if (_initialized && page == _currentPage)
            return;

        mainMenuUI?.CloseFeaturePanel();

        RectTransform current = GetPageRect(_currentPage);
        RectTransform target = GetPageRect(page);

        if (target == null)
            return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        if (instant || current == null)
        {
            ActivatePage(target, true);
            if (current != null && current != target)
                ActivatePage(current, false);

            _currentPage = page;
            _initialized = true;
            if (updateSelection)
                bottomNavBar?.SetSelected(page);
            LogHubPageSelection(page);
            RefreshHubChrome();
            return;
        }

        _transitionRoutine = StartCoroutine(TransitionPages(current, target));
        _currentPage = page;
        _initialized = true;
        if (updateSelection)
            bottomNavBar?.SetSelected(page);
        LogHubPageSelection(page);
        RefreshHubChrome();
    }

    public void ShowShopPage(ShopTab initialTab)
    {
        ShowPage(MainPage.Shop);

        if (shopPage != null && shopPage.TryGetComponent(out ShopPageController shopController))
        {
            shopController.SelectTab(initialTab);
        }

        mainMenuUI?.NotifyHubEntryOpened(AnalyticsEventNames.HubShopOpened, "currency");
    }

    public void OpenDailyLogin()
    {
        mainMenuUI?.OpenDailyLoginFromHub();
    }

    public void OpenTasks()
    {
        mainMenuUI?.OpenTasksFromHub();
    }

    public void OpenSpecialOffers()
    {
        mainMenuUI?.OpenSpecialOffersFromHub();
    }

    public void OpenNotifications()
    {
        mainMenuUI?.OpenNotificationsFromHub();
    }

    public void RefreshHubChrome()
    {
        if (topBarController != null)
            topBarController.RefreshFromProfile(profile);

        if (bottomNavBar != null)
            bottomNavBar.UpdateLocks(profile != null ? profile.level : 1);

        mainMenuUI?.RefreshHubState();
    }

    private IEnumerator TransitionPages(RectTransform current, RectTransform target)
    {
        CanvasGroup currentGroup = GetCanvasGroup(current);
        CanvasGroup targetGroup = GetCanvasGroup(target);

        Vector2 offscreenRight = new(Screen.width, 0f);
        Vector2 offscreenLeft = new(-Screen.width, 0f);

        ActivatePage(target, true);
        target.anchoredPosition = offscreenRight;
        if (targetGroup != null)
            targetGroup.alpha = 0f;

        float elapsed = 0f;
        Vector2 currentStart = current.anchoredPosition;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, transitionDuration));

            current.anchoredPosition = Vector2.Lerp(currentStart, offscreenLeft, t);
            target.anchoredPosition = Vector2.Lerp(offscreenRight, Vector2.zero, t);

            if (currentGroup != null)
                currentGroup.alpha = 1f - t;
            if (targetGroup != null)
                targetGroup.alpha = t;

            yield return null;
        }

        current.anchoredPosition = Vector2.zero;
        ActivatePage(current, false);
        target.anchoredPosition = Vector2.zero;
        if (targetGroup != null)
            targetGroup.alpha = 1f;

        _transitionRoutine = null;
    }

    private RectTransform GetPageRect(MainPage page)
    {
        return NormalizeHubPage(page) switch
        {
            MainPage.Shop => shopPage,
            MainPage.Hangar => hangarPage,
            MainPage.Play => playPage,
            MainPage.Challenges => challengesPage,
            MainPage.Progression => progressionPage,
            _ => null
        };
    }

    private void ActivatePage(RectTransform page, bool active)
    {
        if (page != null)
            page.gameObject.SetActive(active);

        if (page != null && active)
        {
            CanvasGroup group = GetCanvasGroup(page);
            if (group != null)
                group.alpha = 1f;
        }
    }

    private CanvasGroup GetCanvasGroup(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return null;

        if (rectTransform.TryGetComponent(out CanvasGroup group))
            return group;

        return rectTransform.gameObject.AddComponent<CanvasGroup>();
    }

    private void LogHubPageSelection(MainPage page)
    {
        page = NormalizeHubPage(page);
        mainMenuUI?.NotifyHubEntryOpened(AnalyticsEventNames.HubPageSelected, GetHubPageName(page));

        if (page == MainPage.Shop)
            mainMenuUI?.NotifyHubEntryOpened(AnalyticsEventNames.HubShopOpened, "shop");
        else if (page == MainPage.Hangar)
            mainMenuUI?.NotifyHubEntryOpened(AnalyticsEventNames.HubShipOpened, "ship");
        else if (page == MainPage.Lab)
            mainMenuUI?.NotifyHubEntryOpened(AnalyticsEventNames.HubLabOpened, "lab");
    }

    private void EnsurePageControllers()
    {
        if (challengesPage != null && !challengesPage.TryGetComponent(out AchievementsPageController _))
            challengesPage.gameObject.AddComponent<AchievementsPageController>();
    }

    private static MainPage NormalizeHubPage(MainPage page)
    {
        return page switch
        {
            MainPage.Ship => MainPage.Hangar,
            MainPage.LevelSelect => MainPage.Play,
            MainPage.Achievements => MainPage.Challenges,
            MainPage.Tasks => MainPage.Progression,
            _ => page
        };
    }

    private static string GetHubPageName(MainPage page)
    {
        return page switch
        {
            MainPage.Shop => "shop",
            MainPage.Hangar => "ship",
            MainPage.Play => "level_select",
            MainPage.Challenges => "achievements",
            MainPage.Progression => "tasks",
            MainPage.Lab => "lab",
            _ => page.ToString().ToLowerInvariant()
        };
    }
}
