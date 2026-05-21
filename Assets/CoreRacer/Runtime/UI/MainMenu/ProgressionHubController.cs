using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.UI.MainMenu.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.UI.MainMenu
{
    public sealed class ProgressionHubController : MonoBehaviour
    {
        [SerializeField] private Button dailyLoginButton;
        [SerializeField] private Button tasksButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private GameObject dailyLoginPanel;
        [SerializeField] private GameObject tasksPanel;
        [SerializeField] private GameObject achievementsPanel;
        [SerializeField] private DailyLoginPageController dailyLoginPage;
        [SerializeField] private AchievementsPageController achievementsPage;
        [SerializeField] private RotatingTaskListView dailyTasks;
        [SerializeField] private RotatingTaskListView weeklyTasks;
        [SerializeField] private RotatingTaskListView monthlyTasks;

        private TutorialService _tutorial;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            if (dailyLoginButton != null) dailyLoginButton.onClick.AddListener(ShowDailyLogin);
            if (tasksButton != null) tasksButton.onClick.AddListener(ShowTasks);
            if (achievementsButton != null) achievementsButton.onClick.AddListener(ShowAchievements);
            GameServices.TryGet(out _tutorial);
            ShowDailyLogin();
        }

        private void OnDisable()
        {
            if (dailyLoginButton != null) dailyLoginButton.onClick.RemoveListener(ShowDailyLogin);
            if (tasksButton != null) tasksButton.onClick.RemoveListener(ShowTasks);
            if (achievementsButton != null) achievementsButton.onClick.RemoveListener(ShowAchievements);
        }

        public void ShowDailyLogin()
        {
            SetVisible(dailyLoginPanel, true);
            SetVisible(tasksPanel, false);
            SetVisible(achievementsPanel, false);
            dailyLoginPage?.Refresh();
            NotifyPromptOpened();
        }

        public void ShowTasks()
        {
            SetVisible(dailyLoginPanel, false);
            SetVisible(tasksPanel, true);
            SetVisible(achievementsPanel, false);
            dailyTasks?.Refresh();
            weeklyTasks?.Refresh();
            monthlyTasks?.Refresh();
            NotifyPromptOpened();
        }

        public void ShowAchievements()
        {
            SetVisible(dailyLoginPanel, false);
            SetVisible(tasksPanel, false);
            SetVisible(achievementsPanel, true);
            achievementsPage?.Refresh();
        }

        private void NotifyPromptOpened()
        {
            if (_tutorial == null) GameServices.TryGet(out _tutorial);
            _tutorial?.Notify(TutorialStepKind.WaitForDailyTaskRewardPromptOpened, "progression");
        }

        private static void SetVisible(GameObject panel, bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }
    }
}
