using System;
using UnityEngine;

namespace CoreRacer.Services.Assets
{
    public interface IAssetProvider
    {
        AssetLoadHandle<T> Load<T>(AssetReferenceId reference) where T : UnityEngine.Object;
        void LoadAsync<T>(AssetReferenceId reference, Action<AssetLoadHandle<T>> completed) where T : UnityEngine.Object;
        void Release(UnityEngine.Object asset);
    }
}
