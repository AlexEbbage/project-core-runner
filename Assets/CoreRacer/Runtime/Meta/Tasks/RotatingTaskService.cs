using System;
using System.Collections.Generic;
using CoreRacer.Common.Time;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;
using CoreRacer.Services.Analytics;
using CoreRacer.Services.Logging;
using CoreRacer.Services.Save;

namespace CoreRacer.Meta.Tasks
{
    public sealed class RotatingTaskService
    {
        private const string ClaimPrefix = "rotating:";

        private readonly PlayerProfileService _profile;
        private readonly RewardGrantService _rewards;
        private readonly ISaveStorage _storage;
        private readonly JsonSaveSerializer _serializer;
        private readonly IGameClock _clock;
        private readonly TaskPoolDefinition _pool;
        private readonly IAnalyticsService _analytics;
        private readonly IGameLogger _logger;

        private RotatingTaskSaveData _state;

        public RotatingTaskService(
            PlayerProfileService profile,
            RewardGrantService rewards,
            ISaveStorage storage,
            JsonSaveSerializer serializer,
            IGameClock clock,
            TaskPoolDefinition pool,
            IAnalyticsService analytics = null,
            IGameLogger logger = null)
        {
            _profile = profile;
            _rewards = rewards;
            _storage = storage;
            _serializer = serializer;
            _clock = clock;
            _pool = pool;
            _analytics = analytics;
            _logger = logger;
            _state = Load();
            RefreshRotations();
        }

        public IReadOnlyList<RotatingTaskViewModel> GetActiveTasks()
        {
            RefreshRotations();
            var result = new List<RotatingTaskViewModel>();
            for (var i = 0; i < _state.ActiveSlots.Count; i++)
            {
                var slot = _state.ActiveSlots[i];
                var definition = FindDefinition(slot.TaskId);
                if (definition != null)
                    result.Add(BuildViewModel(slot, definition));
            }
            return result;
        }

        public bool TryClaim(string taskId)
        {
            RefreshRotations();
            var slot = _state.ActiveSlots.Find(x => x.TaskId == taskId);
            var definition = FindDefinition(taskId);
            if (slot == null || definition == null || slot.Claimed)
                return false;

            if (BuildViewModel(slot, definition).Status != RotatingTaskStatus.Completed)
                return false;

            var claimId = BuildClaimId(slot);
            var committed = _profile.TryMutate(state =>
            {
                if (HasClaim(state.ClaimedTasks, claimId))
                    return false;

                _rewards.ApplyManyToState(state, definition.Rewards);
                state.ClaimedTasks.Add(new SerializableBoolById(claimId, true));
                return true;
            });

            if (!committed)
            {
                // Recover the task UI if the profile commit already succeeded before a task-save interruption.
                if (HasClaim(_profile.State.ClaimedTasks, claimId))
                {
                    slot.Claimed = true;
                    Save();
                }
                return false;
            }

            slot.Claimed = true;
            Save();
            _analytics?.Track(AnalyticsEventNames.TaskClaimed, new Dictionary<string, object>
            {
                ["task_id"] = taskId,
                ["cadence"] = definition.Cadence.ToString()
            });
            _logger?.Info(LogCategory.Tasks, $"Claimed rotating task '{taskId}'.");
            return true;
        }

        public void ForceRotateAll()
        {
            _state.ActiveSlots.Clear();
            AssignForCadence(TaskCadence.Daily);
            AssignForCadence(TaskCadence.Weekly);
            AssignForCadence(TaskCadence.Monthly);
            Save();
        }

        private void RefreshRotations()
        {
            var changed = false;
            changed |= RemoveExpired(TaskCadence.Daily);
            changed |= RemoveExpired(TaskCadence.Weekly);
            changed |= RemoveExpired(TaskCadence.Monthly);
            changed |= EnsureSlots(TaskCadence.Daily, _pool != null ? _pool.DailySlots : 0);
            changed |= EnsureSlots(TaskCadence.Weekly, _pool != null ? _pool.WeeklySlots : 0);
            changed |= EnsureSlots(TaskCadence.Monthly, _pool != null ? _pool.MonthlySlots : 0);
            if (changed)
                Save();
        }

        private bool RemoveExpired(TaskCadence cadence)
        {
            var now = _clock.UtcNow;
            return _state.ActiveSlots.RemoveAll(slot => slot.Cadence == cadence &&
                TryParse(slot.ExpiresAtUtcIso, out var expiresAt) && now >= expiresAt) > 0;
        }

        private bool EnsureSlots(TaskCadence cadence, int targetCount)
        {
            if (targetCount <= 0 || _pool == null)
                return false;

            var current = 0;
            for (var i = 0; i < _state.ActiveSlots.Count; i++)
                if (_state.ActiveSlots[i].Cadence == cadence)
                    current++;

            var changed = false;
            while (current < targetCount)
            {
                if (!AssignForCadence(cadence))
                    break;
                current++;
                changed = true;
            }
            return changed;
        }

        private bool AssignForCadence(TaskCadence cadence)
        {
            var source = _pool?.GetTasksFor(cadence) ?? new List<RotatingTaskDefinition>();
            if (source.Count == 0)
                return false;

            var candidates = new List<RotatingTaskDefinition>(source);
            var used = new HashSet<string>();
            for (var i = 0; i < _state.ActiveSlots.Count; i++)
                used.Add(_state.ActiveSlots[i].TaskId);

            candidates.RemoveAll(x => x == null || string.IsNullOrEmpty(x.Id) || used.Contains(x.Id));
            if (candidates.Count == 0)
                return false;

            var seedText = _clock.UtcNow.Date.ToString("yyyyMMdd") + ":" + cadence;
            var random = new Random(StableHash(seedText) + _state.ActiveSlots.Count);
            var chosen = WeightedPick(candidates, random);
            var now = _clock.UtcNow;

            _state.ActiveSlots.Add(new RotatingTaskSlot
            {
                TaskId = chosen.Id,
                Cadence = cadence,
                ProgressStartValue = GetAbsoluteProgress(chosen),
                Claimed = false,
                AssignedAtUtcIso = now.ToString("o"),
                ExpiresAtUtcIso = GetExpiry(now, cadence).ToString("o")
            });
            _logger?.Info(LogCategory.Tasks, $"Assigned {cadence} task '{chosen.Id}'.");
            return true;
        }

        private RotatingTaskViewModel BuildViewModel(RotatingTaskSlot slot, RotatingTaskDefinition definition)
        {
            var absolute = GetAbsoluteProgress(definition);
            var progress = definition.Metric == ProgressionTaskMetric.BestScore
                ? absolute
                : Math.Max(0, absolute - slot.ProgressStartValue);
            var target = Math.Max(1, definition.TargetValue);
            var status = slot.Claimed ? RotatingTaskStatus.Claimed : progress >= target ? RotatingTaskStatus.Completed : RotatingTaskStatus.Active;
            if (TryParse(slot.ExpiresAtUtcIso, out var expiresAt) && _clock.UtcNow >= expiresAt && !slot.Claimed)
                status = RotatingTaskStatus.Expired;

            return new RotatingTaskViewModel
            {
                Id = definition.Id,
                DisplayName = definition.DisplayName,
                Description = definition.Description,
                Cadence = definition.Cadence,
                Status = status,
                Progress = Math.Min(progress, target),
                Target = target,
                ExpiresAtUtc = expiresAt,
                Rewards = definition.Rewards
            };
        }

        private int GetAbsoluteProgress(RotatingTaskDefinition definition)
        {
            var state = _profile.State;
            switch (definition.Metric)
            {
                case ProgressionTaskMetric.RunsCompleted: return state.TotalRuns;
                case ProgressionTaskMetric.CoinsCollected: return state.TotalCoinsCollected;
                case ProgressionTaskMetric.PowerupsCollected: return state.TotalPowerupsCollected;
                case ProgressionTaskMetric.BestScore: return state.BestScore;
                default: return 0;
            }
        }

        private RotatingTaskDefinition FindDefinition(string id)
        {
            if (_pool == null)
                return null;
            for (var i = 0; i < _pool.Tasks.Count; i++)
            {
                var task = _pool.Tasks[i];
                if (task != null && task.Id == id)
                    return task;
            }
            return null;
        }

        private RotatingTaskSaveData Load()
        {
            if (_storage == null || !_storage.Exists(SaveKeys.RotatingTasks))
                return new RotatingTaskSaveData();
            var loaded = _serializer.Deserialize<RotatingTaskSaveData>(_storage.Load(SaveKeys.RotatingTasks));
            loaded = loaded ?? new RotatingTaskSaveData();
            loaded.ActiveSlots = loaded.ActiveSlots ?? new List<RotatingTaskSlot>();
            return loaded;
        }

        private void Save()
        {
            _storage?.Save(SaveKeys.RotatingTasks, _serializer.Serialize(_state));
        }

        private static bool HasClaim(List<SerializableBoolById> claims, string id)
        {
            if (claims == null)
                return false;
            for (var i = 0; i < claims.Count; i++)
                if (claims[i].Id == id && claims[i].Value)
                    return true;
            return false;
        }

        private static string BuildClaimId(RotatingTaskSlot slot)
        {
            return ClaimPrefix + slot.TaskId + ":" + slot.AssignedAtUtcIso;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = (int)2166136261;
                for (var i = 0; i < value.Length; i++)
                    hash = (hash ^ value[i]) * 16777619;
                return hash;
            }
        }

        private static RotatingTaskDefinition WeightedPick(List<RotatingTaskDefinition> tasks, Random random)
        {
            var total = 0;
            for (var i = 0; i < tasks.Count; i++)
                total += Math.Max(1, tasks[i].Weight);

            var roll = random.Next(0, Math.Max(1, total));
            var cursor = 0;
            for (var i = 0; i < tasks.Count; i++)
            {
                cursor += Math.Max(1, tasks[i].Weight);
                if (roll < cursor)
                    return tasks[i];
            }
            return tasks[0];
        }

        private static DateTimeOffset GetExpiry(DateTimeOffset now, TaskCadence cadence)
        {
            switch (cadence)
            {
                case TaskCadence.Weekly:
                    var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                    if (daysUntilMonday == 0) daysUntilMonday = 7;
                    return now.Date.AddDays(daysUntilMonday);
                case TaskCadence.Monthly:
                    return new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1);
                default:
                    return now.Date.AddDays(1);
            }
        }

        private static bool TryParse(string value, out DateTimeOffset date) => DateTimeOffset.TryParse(value, out date);
    }
}
