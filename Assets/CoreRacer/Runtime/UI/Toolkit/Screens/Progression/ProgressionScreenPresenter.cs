using CoreRacer.Meta.Tasks;
using CoreRacer.Monetisation.Ads;
using UnityEngine;

namespace CoreRacer.UI.Toolkit
{
    public sealed class ProgressionScreenPresenter : UiScreenPresenterBase
    {
        private readonly ProgressionScreenView _view;
        private readonly CoreRacerUiContext _context;
        private readonly UiToastService _toast;
        private string _panel = "daily";

        public ProgressionScreenPresenter(ProgressionScreenView view, CoreRacerUiContext context, IUiAnimationService animations, UiToastService toast)
            : base(CoreRacerScreenId.Progression, view.Root, animations)
        {
            _view = view;
            _context = context;
            _toast = toast;
        }

        protected override void OnInitialize()
        {
            _view.DailyTab.clicked += ShowDaily;
            _view.TasksTab.clicked += ShowTasks;
            _view.AchievementsTab.clicked += ShowAchievements;
            _view.ClaimDaily.clicked += ClaimDaily;
            _view.ClaimDailyDouble.clicked += ClaimDailyDouble;
            if (_context.Profile != null)
                _context.Profile.Changed += Refresh;
        }

        protected override void OnDispose()
        {
            _view.DailyTab.clicked -= ShowDaily;
            _view.TasksTab.clicked -= ShowTasks;
            _view.AchievementsTab.clicked -= ShowAchievements;
            _view.ClaimDaily.clicked -= ClaimDaily;
            _view.ClaimDailyDouble.clicked -= ClaimDailyDouble;
            if (_context.Profile != null)
                _context.Profile.Changed -= Refresh;
        }

        public override void Refresh()
        {
            RenderSummary();
            RenderDaily();
            RenderTasks();
            RenderAchievements();
            _view.ShowPanel(_panel);
        }

        private void RenderSummary()
        {
            var profile = _context.Profile;
            if (profile == null)
            {
                _view.Summary.text = "Progression is unavailable.";
                return;
            }

            var state = profile.State;
            var required = Mathf.Max(1, profile.ExperienceForNextLevel(state.Level));
            var stars = state.BestScore >= 5000 ? 3 : state.BestScore >= 2500 ? 2 : state.BestScore >= 500 ? 1 : 0;
            _view.Summary.text = $"Best {state.BestScore:N0} · {state.TotalRuns:N0} runs · {state.TotalPowerupsCollected:N0} powerups";
            _view.LevelValue.text = state.Level.ToString();
            _view.XpValue.text = $"{state.Experience:N0} / {required:N0} XP";
            _view.XpBar.value = Mathf.Clamp01((float)state.Experience / required) * 100f;
            _view.StarsValue.text = $"{stars} / 3";
        }

        private void RenderDaily()
        {
            _view.DailyList.Clear();
            var service = _context.DailyRewards;
            var preview = service?.GetCalendarPreview();
            if (preview == null || preview.Count == 0)
            {
                _view.DailyList.Add(UiDynamicElements.EmptyState("Daily rewards are not configured."));
                _view.ClaimDaily.SetEnabled(false);
                _view.ClaimDailyDouble.SetEnabled(false);
                return;
            }

            var current = service.GetCurrentCalendarIndex();
            for (var i = 0; i < preview.Count; i++)
            {
                var reward = preview[i];
                var detail = reward.Rewards.Count > 0 ? $"{reward.Rewards[0].Amount:N0} {reward.Rewards[0].Type}" : "Reward";
                var row = new ActionListItemElement();
                row.Bind(
                    null,
                    $"DAY {i + 1}",
                    reward.DisplayName,
                    i == current ? "TODAY" : detail,
                    -1f,
                    string.Empty,
                    null,
                    false,
                    i == current ? UiClassNames.Attention : null);
                _view.DailyList.Add(row);
            }

            var canClaim = service.CanClaimToday();
            var canDouble = canClaim && (_context.RewardedAds == null || _context.RewardedAds.CanShow(AdPlacement.DailyLoginDoubleReward));
            _view.ClaimDaily.SetEnabled(canClaim);
            _view.ClaimDailyDouble.SetEnabled(canDouble);
            _view.DailyStatus.text = canClaim ? "Today's reward is ready." : "Today's reward is already claimed.";
        }

        private void RenderTasks()
        {
            _view.TaskList.Clear();
            var rotating = _context.RotatingTasks?.GetActiveTasks();
            if (rotating != null && rotating.Count > 0)
            {
                for (var i = 0; i < rotating.Count; i++)
                {
                    var task = rotating[i];
                    var claimable = task.Status == RotatingTaskStatus.Completed;
                    var row = new ActionListItemElement();
                    row.Bind(
                        null,
                        task.DisplayName,
                        task.Description,
                        $"{task.Progress}/{task.Target}",
                        task.Target > 0 ? (float)task.Progress / task.Target : 0f,
                        claimable ? "CLAIM" : task.Status.ToString().ToUpperInvariant(),
                        () => ClaimRotating(task.Id),
                        claimable,
                        claimable ? UiClassNames.Claimable : null);
                    _view.TaskList.Add(row);
                }
                return;
            }

            var definitions = _context.ProgressionTasks?.Definitions;
            if (definitions == null || definitions.Count == 0)
            {
                _view.TaskList.Add(UiDynamicElements.EmptyState("No tasks are active."));
                return;
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                var task = definitions[i];
                if (task == null)
                    continue;
                var progress = _context.ProgressionTasks.GetProgress(task);
                var claimed = _context.ProgressionTasks.IsClaimed(task.Id);
                var claimable = _context.ProgressionTasks.IsComplete(task) && !claimed;
                var row = new ActionListItemElement();
                row.Bind(
                    null,
                    task.DisplayName,
                    task.Description,
                    $"{Mathf.Min(progress, task.TargetValue)}/{task.TargetValue}",
                    task.TargetValue > 0 ? (float)progress / task.TargetValue : 0f,
                    claimed ? "CLAIMED" : claimable ? "CLAIM" : "IN PROGRESS",
                    () => ClaimProgression(task.Id),
                    claimable,
                    claimed ? UiClassNames.Claimed : claimable ? UiClassNames.Claimable : null);
                _view.TaskList.Add(row);
            }
        }

        private void RenderAchievements()
        {
            _view.AchievementList.Clear();
            var definitions = _context.Achievements?.Definitions;
            if (definitions == null || definitions.Count == 0)
            {
                _view.AchievementList.Add(UiDynamicElements.EmptyState("No achievements are configured."));
                return;
            }

            for (var i = 0; i < definitions.Count; i++)
            {
                var achievement = definitions[i];
                if (achievement == null)
                    continue;
                var progress = _context.Achievements.GetProgress(achievement);
                var claimed = _context.Achievements.IsClaimed(achievement.Id);
                var claimable = _context.Achievements.IsComplete(achievement) && !claimed;
                var row = new ActionListItemElement();
                row.Bind(
                    null,
                    achievement.DisplayName,
                    achievement.Description,
                    $"{Mathf.Min(progress, achievement.RequiredValue)}/{achievement.RequiredValue}",
                    achievement.RequiredValue > 0 ? (float)progress / achievement.RequiredValue : 0f,
                    claimed ? "CLAIMED" : claimable ? "CLAIM" : "IN PROGRESS",
                    () => ClaimAchievement(achievement.Id),
                    claimable,
                    claimed ? UiClassNames.Claimed : claimable ? UiClassNames.Claimable : null);
                _view.AchievementList.Add(row);
            }
        }

        private void ClaimDaily()
        {
            if (_context.DailyRewards != null && _context.DailyRewards.TryClaim(false))
                _toast.Show("Daily reward claimed.");
            RenderDaily();
        }

        private void ClaimDailyDouble()
        {
            if (_context.DailyRewards == null)
                return;
            if (_context.RewardedAds == null)
            {
                if (_context.DailyRewards.TryClaim(true))
                    _toast.Show("Double daily reward claimed.");
                RenderDaily();
                return;
            }

            _context.RewardedAds.ShowOrBypass(AdPlacement.DailyLoginDoubleReward, result =>
            {
                if (_context.RewardedAds.ShouldGrantReward(result) && _context.DailyRewards.TryClaim(true))
                    _toast.Show("Double daily reward claimed.");
                else
                    _toast.Show("Double reward is unavailable.", true);
                RenderDaily();
            });
        }

        private void ClaimRotating(string id)
        {
            if (_context.RotatingTasks != null && _context.RotatingTasks.TryClaim(id))
                _toast.Show("Task reward claimed.");
            RenderTasks();
        }

        private void ClaimProgression(string id)
        {
            if (_context.ProgressionTasks != null && _context.ProgressionTasks.TryClaim(id))
                _toast.Show("Task reward claimed.");
            RenderTasks();
        }

        private void ClaimAchievement(string id)
        {
            if (_context.Achievements != null && _context.Achievements.TryClaim(id))
                _toast.Show("Achievement reward claimed.");
            RenderAchievements();
        }

        private void ShowDaily() { _panel = "daily"; _view.ShowPanel(_panel); }
        private void ShowTasks() { _panel = "tasks"; _view.ShowPanel(_panel); }
        private void ShowAchievements() { _panel = "achievements"; _view.ShowPanel(_panel); }
    }
}
