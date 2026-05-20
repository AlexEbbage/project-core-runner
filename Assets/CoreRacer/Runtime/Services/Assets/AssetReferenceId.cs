using System;

namespace CoreRacer.Services.Assets
{
    [Serializable]
    public struct AssetReferenceId
    {
        public string Id;
        public string ResourcesPath;
        public string AddressablesKey;

        public bool IsValid => !string.IsNullOrWhiteSpace(ResourcesPath) || !string.IsNullOrWhiteSpace(AddressablesKey);
    }
}
