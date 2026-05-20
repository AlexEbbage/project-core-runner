using System;
using UnityEngine;

namespace CoreRacer.Services.Assets
{
    /// <summary>
    /// Compile-safe Addressables seam. Define CORE_RACER_ADDRESSABLES and replace the guarded body with real Addressables calls.
    /// </summary>
    public sealed class AddressablesAssetProvider : MonoBehaviour, IAssetProvider
    {
        [SerializeField] private ResourcesAssetProvider fallbackResourcesProvider;

        private void Awake()
        {
            if (fallbackResourcesProvider == null)
                fallbackResourcesProvider = GetComponent<ResourcesAssetProvider>() ?? gameObject.AddComponent<ResourcesAssetProvider>();
        }

        public AssetLoadHandle<T> Load<T>(AssetReferenceId reference) where T : UnityEngine.Object
        {
#if CORE_RACER_ADDRESSABLES
            // Add Unity Addressables synchronous load strategy here if needed, or prefer LoadAsync.
#endif
            return fallbackResourcesProvider.Load<T>(reference);
        }

        public void LoadAsync<T>(AssetReferenceId reference, Action<AssetLoadHandle<T>> completed) where T : UnityEngine.Object
        {
#if CORE_RACER_ADDRESSABLES
            // Add Addressables.LoadAssetAsync<T>(reference.AddressablesKey) implementation here.
#endif
            fallbackResourcesProvider.LoadAsync(reference, completed);
        }

        public void Release(UnityEngine.Object asset)
        {
#if CORE_RACER_ADDRESSABLES
            // Addressables.Release(asset) when using Addressables.
#endif
            fallbackResourcesProvider.Release(asset);
        }
    }
}
