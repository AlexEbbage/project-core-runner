using System.Collections.Generic;
using CoreRacer.Services.Audio;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerAudioFeedbackBuilder
    {
        private const string LibraryPath = "Assets/CoreRacer/Generated/Configs/AudioEventLibrary.asset";
        [MenuItem("Tools/Core Racer/Playability/Rebuild Core Audio Feedback")]
        public static void Rebuild()
        {
            var library = AssetDatabase.LoadAssetAtPath<AudioEventLibrary>(LibraryPath);
            if (library == null)
            {
                Debug.LogError($"[CoreRacer.Audio] Missing audio event library at {LibraryPath}.");
                return;
            }

            Populate(library);
            AssetDatabase.SaveAssets();
            Debug.Log("[CoreRacer.Audio] Saved the core-loop audio event catalog.");
        }

        public static void Populate(AudioEventLibrary library)
        {
            if (library == null)
                return;

            library.Events = BuildEvents();
            EditorUtility.SetDirty(library);
        }

        private static List<AudioEventDefinition> BuildEvents()
        {
            return new List<AudioEventDefinition>
            {
                Event(AudioEventId.UiClick, "Assets/Audio/SFX/UiClickSfx.mp3", 0.65f),
                Event(AudioEventId.RunStart, "Assets/Audio/SFX/WarpSfx.mp3", 0.75f),
                Event(AudioEventId.RunEnd, "Assets/Audio/SFX/countdown_go.mp3", 0.6f, 0.85f),
                Event(AudioEventId.PickupCoin, "Assets/Audio/SFX/pickupSfx1.mp3", 0.55f, 1.15f),
                Event(AudioEventId.PickupPowerup, "Assets/Audio/SFX/pickupSfx1.mp3", 0.8f, 0.85f),
                Event(AudioEventId.PlayerHit, "Assets/Audio/SFX/HitSfx.mp3", 0.7f, 1.1f),
                Event(AudioEventId.PlayerDeath, "Assets/Audio/SFX/HitSfx.mp3", 1f, 0.78f),
                Event(AudioEventId.RewardClaimed, "Assets/Audio/SFX/UiClickSfx.mp3", 0.7f, 1.1f),
                Event(AudioEventId.UpgradePurchased, "Assets/Audio/SFX/UiClickSfx.mp3", 0.8f, 0.9f),
                Event(AudioEventId.ObstaclePassed, "Assets/Audio/SFX/UiClickSfx.mp3", 0.12f, 1.55f),
                Event(AudioEventId.ShieldActivated, "Assets/Audio/SFX/WarpSfx.mp3", 0.5f, 1.25f),
                Event(AudioEventId.ShieldBroken, "Assets/Audio/SFX/HitSfx.mp3", 0.65f, 1.35f),
                Event(AudioEventId.SpeedTierReached, "Assets/Audio/SFX/countdown_tone.mp3", 0.45f, 1.2f),
                Event(AudioEventId.MenuMusic, "Assets/Audio/Music/MenuTrack.mp3", 0.55f, 1f, true, true),
                Event(AudioEventId.RunMusic, "Assets/Audio/Music/Zone1Music.mp3", 0.6f, 1f, true, true)
            };
        }

        private static AudioEventDefinition Event(
            AudioEventId id,
            string clipPath,
            float volume,
            float pitch = 1f,
            bool loop = false,
            bool useMusicSource = false)
        {
            return new AudioEventDefinition
            {
                Id = id,
                Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath),
                Volume = volume,
                Pitch = pitch,
                Loop = loop,
                UseMusicSource = useMusicSource
            };
        }
    }
}
