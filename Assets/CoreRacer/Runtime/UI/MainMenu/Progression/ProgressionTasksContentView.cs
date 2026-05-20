using System;
using System.Collections.Generic;
using CoreRacer.Meta.Progression;
using UnityEngine;

namespace CoreRacer.UI.MainMenu.Progression
{
    public sealed class ProgressionTasksContentView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ProgressionTaskRowView rowPrefab;
        private readonly List<ProgressionTaskRowView> _rows = new List<ProgressionTaskRowView>();

        public void Render(IList<ProgressionTaskDefinition> tasks, Func<string, int> currentValueProvider, Func<string, bool> claimedProvider, Action<string> onClaim)
        {
            Clear();
            if (tasks == null || rowPrefab == null)
                return;

            var parent = contentRoot != null ? contentRoot : transform;
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var row = Instantiate(rowPrefab, parent);
                row.Bind(task, currentValueProvider != null ? currentValueProvider(task.Id) : 0, claimedProvider != null && claimedProvider(task.Id), onClaim);
                _rows.Add(row);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i] != null)
                    Destroy(_rows[i].gameObject);
            _rows.Clear();
        }
    }
}
