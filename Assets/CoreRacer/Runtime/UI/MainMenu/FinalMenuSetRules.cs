namespace CoreRacer.UI.MainMenu
{
    /// <summary>
    /// Product-level menu contract for the first release.
    /// Keep this as the source of truth for bottom navigation and top-level routing.
    /// </summary>
    public static class FinalMenuSetRules
    {
        private static readonly MainMenuPage[] BottomNavigationPagesInternal =
        {
            MainMenuPage.Play,
            MainMenuPage.Hangar,
            MainMenuPage.Lab,
            MainMenuPage.Shop,
            MainMenuPage.Progression
        };

        private static readonly MainMenuPage[] TopLevelPagesInternal =
        {
            MainMenuPage.Play,
            MainMenuPage.Hangar,
            MainMenuPage.Lab,
            MainMenuPage.Shop,
            MainMenuPage.Progression,
            MainMenuPage.Settings
        };

        public static MainMenuPage[] BottomNavigationPages => BottomNavigationPagesInternal;
        public static MainMenuPage[] TopLevelPages => TopLevelPagesInternal;

        public static bool IsBottomNavigationPage(MainMenuPage page)
        {
            for (int i = 0; i < BottomNavigationPagesInternal.Length; i++)
            {
                if (BottomNavigationPagesInternal[i] == page)
                    return true;
            }

            return false;
        }

        public static bool IsTopLevelPage(MainMenuPage page)
        {
            for (int i = 0; i < TopLevelPagesInternal.Length; i++)
            {
                if (TopLevelPagesInternal[i] == page)
                    return true;
            }

            return false;
        }

        public static int GetBottomNavigationIndex(MainMenuPage page)
        {
            for (int i = 0; i < BottomNavigationPagesInternal.Length; i++)
            {
                if (BottomNavigationPagesInternal[i] == page)
                    return i;
            }

            return -1;
        }

        public static string GetPageLabel(MainMenuPage page)
        {
            switch (page)
            {
                case MainMenuPage.Play:
                    return "Play";
                case MainMenuPage.Hangar:
                    return "Hangar";
                case MainMenuPage.Lab:
                    return "Lab";
                case MainMenuPage.Shop:
                    return "Shop";
                case MainMenuPage.Progression:
                    return "Progression";
                case MainMenuPage.Settings:
                    return "Settings";
                default:
                    return page.ToString();
            }
        }
    }
}
