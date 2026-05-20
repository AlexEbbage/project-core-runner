using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Meta.Tasks;
using UnityEngine;

namespace CoreRacer.UI.MainMenu
{
    public sealed class RotatingTaskListView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private RotatingTaskRowView rowPrefab;

        private readonly List<RotatingTaskRowView> _rows = new List<RotatingTaskRowView>();
        private RotatingTaskService _tasks;

        private void OnEnable()
        {
            GameServices.TryGet(out _tasks);
            Refresh();
        }

        public void Refresh()
        {
            if (_tasks == null || rowPrefab == null || contentRoot == null)
                return;

            var models = _tasks.GetActiveTasks();
            EnsureRows(models.Count);
            for (int i = 0; i < _rows.Count; i++)
            {
                bool active = i < models.Count;
                _rows[i].gameObject.SetActive(active);
                if (active) _rows[i].Render(models[i], Claim);
            }
        }

        private void Claim(string taskId)
        {
            _tasks.TryClaim(taskId);
            Refresh();
        }

        private void EnsureRows(int count)
        {
            while (_rows.Count < count)
                _rows.Add(Instantiate(rowPrefab, contentRoot));
        }
    }
}
