using UnityEngine;

namespace CoreRacer.Services.Settings
{
    public sealed class GraphicsSettingsManager : MonoBehaviour
    {
        [SerializeField] private int lowQualityIndex;
        [SerializeField] private int highQualityIndex = 1;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSyncOnMobile = true;

        private void Awake()
        {
            ApplyTargetFrameRate(targetFrameRate);
        }

        public void SetHighQuality(bool high)
        {
            var index = Mathf.Clamp(high ? highQualityIndex : lowQualityIndex, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(index, true);
        }

        public void ApplyTargetFrameRate(int fps)
        {
            Application.targetFrameRate = Mathf.Max(30, fps);
            if (disableVSyncOnMobile && (Application.isMobilePlatform || Application.isEditor))
                QualitySettings.vSyncCount = 0;
        }
    }
}
