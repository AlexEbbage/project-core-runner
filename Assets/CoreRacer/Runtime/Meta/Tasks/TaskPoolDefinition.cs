using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Meta.Tasks
{
    [CreateAssetMenu(menuName = "Core Racer/Progression/Rotating Task Pool")]
    public sealed class TaskPoolDefinition : ScriptableObject
    {
        public int DailySlots = 3;
        public int WeeklySlots = 5;
        public int MonthlySlots = 4;
        public List<RotatingTaskDefinition> Tasks = new List<RotatingTaskDefinition>();

        public List<RotatingTaskDefinition> GetTasksFor(TaskCadence cadence)
        {
            var result = new List<RotatingTaskDefinition>();
            for (int i = 0; i < Tasks.Count; i++)
            {
                var task = Tasks[i];
                if (task != null && task.Cadence == cadence)
                    result.Add(task);
            }
            return result;
        }
    }
}
