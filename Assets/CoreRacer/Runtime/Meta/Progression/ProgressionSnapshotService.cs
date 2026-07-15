using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Tasks;

namespace CoreRacer.Meta.Progression
{
    public sealed class ProgressionSnapshotService
    {
        private readonly PlayerProfileService _profile;
        private readonly ProgressionTaskService _tasks;
        private readonly RotatingTaskService _rotatingTasks;

        public ProgressionSnapshotService(PlayerProfileService profile, ProgressionTaskService tasks = null)
        {
            _profile = profile;
            _tasks = tasks;
        }

        public ProgressionSnapshotService(PlayerProfileService profile, RotatingTaskService rotatingTasks)
        {
            _profile = profile;
            _rotatingTasks = rotatingTasks;
        }

        public ProgressionSnapshot Build()
        {
            var state = _profile != null ? _profile.State : PlayerProfileDefaults.CreateNew();
            var nextLevelXp = _profile != null ? _profile.ExperienceForNextLevel(state.Level) : 500;
            return new ProgressionSnapshot(state, nextLevelXp, BuildTaskSummary());
        }

        private ProgressionTaskSummary BuildTaskSummary()
        {
            if (_tasks != null)
                return _tasks.GetSummary();

            if (_rotatingTasks == null)
                return new ProgressionTaskSummary(0, 0, 0);

            var tasks = _rotatingTasks.GetActiveTasks();
            var ready = 0;
            var claimed = 0;
            for (var i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].Status == RotatingTaskStatus.Completed)
                    ready++;
                else if (tasks[i].Status == RotatingTaskStatus.Claimed)
                    claimed++;
            }

            return new ProgressionTaskSummary(tasks.Count, ready, claimed);
        }
    }
}
