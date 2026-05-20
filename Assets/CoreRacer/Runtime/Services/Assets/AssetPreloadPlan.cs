using System.Collections.Generic;
using UnityEngine;

namespace CoreRacer.Services.Assets
{
    [CreateAssetMenu(menuName = "Core Racer/Assets/Preload Plan")]
    public sealed class AssetPreloadPlan : ScriptableObject
    {
        public List<AssetReferenceId> Assets = new List<AssetReferenceId>();
    }
}
