using System;
using System.Linq;
using CoreRacer.Services.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CoreRacer.Tests.EditMode
{
    public sealed class AudioFeedbackTests
    {
        [Test]
        public void GeneratedLibrary_CoversEveryCoreAudioEvent()
        {
            var library = AssetDatabase.LoadAssetAtPath<AudioEventLibrary>(
                "Assets/CoreRacer/Generated/Configs/AudioEventLibrary.asset");

            Assert.That(library, Is.Not.Null);
            var ids = Enum.GetValues(typeof(AudioEventId)).Cast<AudioEventId>().ToArray();
            Assert.That(library.Events, Has.Count.EqualTo(ids.Length));
            Assert.That(library.Events.Select(item => item.Id).Distinct().Count(), Is.EqualTo(ids.Length));
            Assert.That(library.Events.All(item => item != null && item.Clip != null && item.Volume > 0f), Is.True);
            Assert.That(library.Events.Single(item => item.Id == AudioEventId.MenuMusic).UseMusicSource, Is.True);
            Assert.That(library.Events.Single(item => item.Id == AudioEventId.RunMusic).UseMusicSource, Is.True);
        }

        [Test]
        public void AudioService_RoutesCatalogEventsToAuthoredSources()
        {
            var root = new GameObject("AudioServiceTest");
            var music = root.AddComponent<AudioSource>();
            var sfxObject = new GameObject("Sfx");
            sfxObject.transform.SetParent(root.transform);
            var sfx = sfxObject.AddComponent<AudioSource>();
            var clip = AudioClip.Create("test", 256, 1, 22050, false);
            var library = ScriptableObject.CreateInstance<AudioEventLibrary>();
            library.Events.Add(new AudioEventDefinition
            {
                Id = AudioEventId.MenuMusic,
                Clip = clip,
                Volume = 0.5f,
                Pitch = 1f,
                Loop = true,
                UseMusicSource = true
            });

            try
            {
                var service = new AudioService(null, library);
                service.Bind(music, sfx);

                Assert.That(service.PlayEvent(AudioEventId.MenuMusic), Is.True);
                Assert.That(service.IsBound, Is.True);
                Assert.That(service.LastPlayedEventId, Is.EqualTo(AudioEventId.MenuMusic));
                Assert.That(service.PlayedEventCount, Is.EqualTo(1));
                Assert.That(music.clip, Is.SameAs(clip));
                Assert.That(music.loop, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(library);
                UnityEngine.Object.DestroyImmediate(clip);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
