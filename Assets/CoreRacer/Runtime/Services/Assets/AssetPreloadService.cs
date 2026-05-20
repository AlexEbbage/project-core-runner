using System;
using System.Collections;
using UnityEngine;

namespace CoreRacer.Services.Assets
{
    public sealed class AssetPreloadService : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour assetProviderBehaviour;
        [SerializeField] private AssetPreloadPlan preloadPlan;

        private IAssetProvider _provider;
        public bool IsComplete { get; private set; }
        public event Action<float> ProgressChanged;
        public event Action Completed;

        private void Awake()
        {
            _provider = assetProviderBehaviour as IAssetProvider;
            if (_provider == null)
                _provider = GetComponent<IAssetProvider>();
        }

        public void BeginPreload()
        {
            StartCoroutine(PreloadRoutine());
        }

        private IEnumerator PreloadRoutine()
        {
            IsComplete = false;
            var count = preloadPlan != null ? preloadPlan.Assets.Count : 0;
            if (_provider == null || count == 0)
            {
                IsComplete = true;
                ProgressChanged?.Invoke(1f);
                Completed?.Invoke();
                yield break;
            }

            for (int i = 0; i < count; i++)
            {
                bool done = false;
                _provider.LoadAsync<UnityEngine.Object>(preloadPlan.Assets[i], _ => done = true);
                while (!done) yield return null;
                ProgressChanged?.Invoke((i + 1f) / count);
            }

            IsComplete = true;
            Completed?.Invoke();
        }
    }
}
