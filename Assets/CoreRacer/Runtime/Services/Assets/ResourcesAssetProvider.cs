using System;
using System.Collections;
using UnityEngine;

namespace CoreRacer.Services.Assets
{
    public sealed class ResourcesAssetProvider : MonoBehaviour, IAssetProvider
    {
        public AssetLoadHandle<T> Load<T>(AssetReferenceId reference) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(reference.ResourcesPath))
                return new AssetLoadHandle<T>(null, "ResourcesPath is empty.");

            var asset = Resources.Load<T>(reference.ResourcesPath);
            return asset != null ? new AssetLoadHandle<T>(asset) : new AssetLoadHandle<T>(null, "Asset not found: " + reference.ResourcesPath);
        }

        public void LoadAsync<T>(AssetReferenceId reference, Action<AssetLoadHandle<T>> completed) where T : UnityEngine.Object
        {
            StartCoroutine(LoadAsyncRoutine(reference, completed));
        }

        private IEnumerator LoadAsyncRoutine<T>(AssetReferenceId reference, Action<AssetLoadHandle<T>> completed) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(reference.ResourcesPath))
            {
                completed?.Invoke(new AssetLoadHandle<T>(null, "ResourcesPath is empty."));
                yield break;
            }

            var request = Resources.LoadAsync<T>(reference.ResourcesPath);
            yield return request;
            completed?.Invoke(new AssetLoadHandle<T>(request.asset as T, request.asset == null ? "Asset not found: " + reference.ResourcesPath : null));
        }

        public void Release(UnityEngine.Object asset)
        {
            // Resources assets are managed by Unity. UnloadUnusedAssets can be triggered from a memory pressure handler if required.
        }
    }
}
