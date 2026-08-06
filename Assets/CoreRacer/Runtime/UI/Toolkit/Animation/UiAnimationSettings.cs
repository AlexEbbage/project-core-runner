using LitMotion;
using UnityEngine;

namespace CoreRacer.UI.Toolkit
{
    [CreateAssetMenu(menuName = "Core Racer/UI/Animation Settings")]
    public sealed class UiAnimationSettings : ScriptableObject
    {
        [Min(0f)] public float ScreenDuration = 0.22f;
        [Min(0f)] public float PopupDuration = 0.2f;
        [Min(0f)] public float ToastDuration = 0.16f;
        [Min(0f)] public float FeedbackDuration = 0.3f;
        public float ScreenTravel = 22f;
        public float BottomSheetTravel = 72f;
        public float InvalidShakeDistance = 12f;
        public float SuccessPunchScale = 0.07f;
        public Ease ScreenEase = Ease.OutCubic;
        public Ease PopupEase = Ease.OutBack;
        public Ease FeedbackEase = Ease.OutQuad;

        public static UiAnimationSettings CreateRuntimeDefaults()
        {
            var settings = CreateInstance<UiAnimationSettings>();
            settings.hideFlags = HideFlags.HideAndDontSave;
            return settings;
        }
    }
}
