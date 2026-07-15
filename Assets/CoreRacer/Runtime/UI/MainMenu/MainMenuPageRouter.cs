using System;
using System.Collections.Generic;
using CoreRacer.UI.Shared;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public enum MainMenuPage
    {
        Play,
        Shop,
        Hangar,
        Lab,
        Progression,
        Settings
    }

    public sealed class MainMenuPageRouter : MonoBehaviour
    {
        [Serializable]
        public sealed class PageBinding
        {
            public MainMenuPage Page;
            public UiView View;
        }

        [SerializeField] private MainMenuPage defaultPage = MainMenuPage.Play;
        [SerializeField] private List<PageBinding> pages = new List<PageBinding>();
        [SerializeField] private bool keepCurrentPageWhenTargetMissing = true;

        private MainMenuPage _currentPage;
        private bool _hasCurrentPage;

        public event Action<MainMenuPage> PageChanged;

        public MainMenuPage DefaultPage => defaultPage;
        public MainMenuPage CurrentPage => _hasCurrentPage ? _currentPage : defaultPage;
        public bool HasCurrentPage => _hasCurrentPage;
        public IReadOnlyList<PageBinding> Pages => pages;

        public void ShowDefault()
        {
            if (!TryShow(defaultPage) && defaultPage != MainMenuPage.Play)
                TryShow(MainMenuPage.Play);
        }

        public void Show(MainMenuPage page)
        {
            if (!TryShow(page))
                Debug.LogWarning($"MainMenuPageRouter could not show {page}. Check the final menu page bindings.", this);
        }

        public bool TryShow(MainMenuPage page)
        {
            if (!FinalMenuSetRules.IsTopLevelPage(page))
            {
                Debug.LogWarning($"{page} is not part of the first-release final menu set.", this);
                return false;
            }

            var target = FindBinding(page);
            if (target == null || target.View == null)
            {
                if (!keepCurrentPageWhenTargetMissing)
                    HideAll();
                return false;
            }

            for (int i = 0; i < pages.Count; i++)
            {
                var binding = pages[i];
                if (binding == null || binding.View == null)
                    continue;

                if (binding == target)
                    binding.View.Show();
                else
                    binding.View.Hide();
            }

            var changed = !_hasCurrentPage || _currentPage != page;
            _currentPage = page;
            _hasCurrentPage = true;
            if (changed)
                PageChanged?.Invoke(page);
            return true;
        }

        public bool HasPage(MainMenuPage page)
        {
            var binding = FindBinding(page);
            return binding != null && binding.View != null;
        }

        public bool HasRequiredBottomNavigationPages()
        {
            var pagesToCheck = FinalMenuSetRules.BottomNavigationPages;
            for (int i = 0; i < pagesToCheck.Length; i++)
            {
                if (!HasPage(pagesToCheck[i]))
                    return false;
            }

            return true;
        }

        public void ShowPlay() => Show(MainMenuPage.Play);
        public void ShowHangar() => Show(MainMenuPage.Hangar);
        public void ShowLab() => Show(MainMenuPage.Lab);
        public void ShowShop() => Show(MainMenuPage.Shop);
        public void ShowProgression() => Show(MainMenuPage.Progression);
        public void ShowSettings() => Show(MainMenuPage.Settings);

        private PageBinding FindBinding(MainMenuPage page)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                var binding = pages[i];
                if (binding != null && binding.Page == page)
                    return binding;
            }

            return null;
        }

        private void HideAll()
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] != null && pages[i].View != null)
                    pages[i].View.Hide();
            }
        }
    }
}
