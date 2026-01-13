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

    [Header("Navigation")]
    [SerializeField] private BottomNavBarController bottomNavBar;
    [SerializeField] private MainPage initialPage = MainPage.Play;

    [Header("Transitions")]
    [SerializeField] private float transitionDuration = 0.25f;

    private MainPage _currentPage;
    private bool _initialized;
    private Coroutine _transitionRoutine;

    private void Awake()
    {
        if (bottomNavBar != null)
            bottomNavBar.Initialize(this);
    }

    private void Start()
    {
        ShowPage(initialPage, true);
        if (bottomNavBar != null)
            bottomNavBar.SetSelected(initialPage);
    }

    public void ShowPage(MainPage page, bool instant = false)
    {
        if (_initialized && page == _currentPage)
            return;

        var current = GetPageRect(_currentPage);
        var target = GetPageRect(page);

        if (target == null)
            return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        if (instant || current == null)
        {
            ActivatePage(target, true);
            if (current != null)
                ActivatePage(current, false);
            _currentPage = page;
            _initialized = true;
            return;
        }

        _transitionRoutine = StartCoroutine(TransitionPages(current, target));
        _currentPage = page;
        _initialized = true;
    }

    public void ShowShopPage(ShopTab initialTab)
    {
        ShowPage(MainPage.Shop);

        if (shopPage != null && shopPage.TryGetComponent(out ShopPageController shopController))
        {
            shopController.SelectTab(initialTab);
        }
    }

    private IEnumerator TransitionPages(RectTransform current, RectTransform target)
    {
        var currentGroup = GetCanvasGroup(current);
        var targetGroup = GetCanvasGroup(target);

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
        return page switch
        {
            MainPage.Hangar => hangarPage,
            MainPage.Play => playPage,
            MainPage.Shop => shopPage,
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
            var group = GetCanvasGroup(page);
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
}
