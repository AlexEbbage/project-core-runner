using System;

namespace CoreRacer.Services.Assets
{
    public sealed class AssetLoadHandle<T> where T : UnityEngine.Object
    {
        public T Asset { get; }
        public bool Succeeded => Asset != null;
        public string Error { get; }

        public AssetLoadHandle(T asset, string error = null)
        {
            Asset = asset;
            Error = error;
        }
    }
}
