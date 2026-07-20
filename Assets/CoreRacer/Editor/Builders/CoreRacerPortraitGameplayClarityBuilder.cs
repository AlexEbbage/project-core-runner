using CoreRacer.Gameplay.Run;
using CoreRacer.UI.Hud;
using CoreRacer.UI.Pause;
using CoreRacer.UI.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerPortraitGameplayClarityBuilder
    {
        [MenuItem("Tools/Core Racer/Playability/Rebuild Portrait Gameplay Clarity")]
        public static void Rebuild()
        {
            var references = Object.FindObjectOfType<RunSceneReferences>(true);
            var canvas = Object.FindObjectOfType<Canvas>(true);
            if (references == null || canvas == null || references.Hud == null || references.PauseMenu == null)
            {
                Debug.LogError("[CoreRacer.UI] Portrait clarity rebuild requires RunSceneReferences, Canvas, HUD, and PauseMenu in the active scene.");
                return;
            }

            ConfigureHud(canvas.transform, references);
            ConfigurePause(canvas.transform, references.PauseMenu);
            EditorSceneManager.MarkSceneDirty(references.gameObject.scene);
            EditorSceneManager.SaveScene(references.gameObject.scene);
            Debug.Log("[CoreRacer.UI] Saved portrait HUD, safe-area, powerup, and pause clarity changes.");
        }

        private static void ConfigureHud(Transform canvas, RunSceneReferences references)
        {
            var safeArea = FindOrCreateRect(canvas, "GameplaySafeArea");
            Stretch(safeArea);
            if (safeArea.GetComponent<SafeAreaRectTransform>() == null)
                safeArea.gameObject.AddComponent<SafeAreaRectTransform>();
            safeArea.SetAsFirstSibling();

            var hudRect = (RectTransform)references.Hud.transform;
            hudRect.SetParent(safeArea, false);
            SetRect(hudRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(960f, 300f), new Vector2(0f, -24f), new Vector2(0.5f, 1f));

            var score = FindText(hudRect, "ScoreText");
            var distance = FindOrCreateText(hudRect, "DistanceText");
            var coins = FindText(hudRect, "CoinsText");
            var health = FindText(hudRect, "HealthText");
            var powerupText = FindOrCreateText(hudRect, "PowerupStatusText");

            ConfigureMetric(score, new Vector2(0f, -50f), new Vector2(480f, 56f), 40, FontStyle.Bold, new Color(0.035f, 0.055f, 0.09f, 1f));
            ConfigureMetric(distance, new Vector2(0f, -100f), new Vector2(480f, 44f), 27, FontStyle.Normal, new Color(0.12f, 0.17f, 0.23f, 1f));
            ConfigureMetric(coins, new Vector2(-310f, -52f), new Vector2(260f, 50f), 28, FontStyle.Bold, new Color(0.75f, 0.31f, 0.02f, 1f));
            ConfigureMetric(health, new Vector2(-310f, -102f), new Vector2(260f, 44f), 25, FontStyle.Bold, new Color(0.62f, 0.08f, 0.08f, 1f));
            ConfigureMetric(powerupText, new Vector2(0f, -162f), new Vector2(820f, 54f), 25, FontStyle.Bold, new Color(0.035f, 0.055f, 0.09f, 1f));

            var strip = powerupText.GetComponent<PowerupStripView>();
            if (strip == null) strip = powerupText.gameObject.AddComponent<PowerupStripView>();
            SetObject(strip, "activeText", powerupText);
            SetObject(references.Hud, "statsTracker", references.StatsTracker);
            SetObject(references.Hud, "powerups", references.Powerups);
            SetObject(references.Hud, "distanceText", distance);
            SetObject(references.Hud, "powerupStrip", strip);

            var pauseButton = hudRect.Find("PauseButton") as RectTransform;
            if (pauseButton != null)
                SetRect(pauseButton, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(190f, 70f), new Vector2(365f, -52f), new Vector2(0.5f, 0.5f));
        }

        private static void ConfigurePause(Transform canvas, PauseMenuController pause)
        {
            var root = (RectTransform)pause.transform;
            root.SetParent(canvas, false);
            Stretch(root);
            var backdrop = root.GetComponent<Image>();
            if (backdrop == null) backdrop = root.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.015f, 0.025f, 0.05f, 0.86f);
            backdrop.raycastTarget = true;

            var title = FindText(root, "PauseTitle");
            var subtitle = FindText(root, "Title");
            if (title != null)
            {
                title.text = "RUN PAUSED";
                ConfigurePauseLabel(title, new Vector2(0f, 140f), new Vector2(760f, 72f), 48, FontStyle.Bold, Color.white);
            }
            if (subtitle != null)
            {
                subtitle.text = "Your run is safe";
                ConfigurePauseLabel(subtitle, new Vector2(0f, 78f), new Vector2(760f, 48f), 25, FontStyle.Normal, new Color(0.76f, 0.82f, 0.9f, 1f));
            }

            ConfigureButton(root.Find("ResumeButton") as RectTransform, new Vector2(0f, -20f), new Vector2(440f, 88f), "RESUME");
            ConfigureButton(root.Find("PauseMenuButton") as RectTransform, new Vector2(0f, -125f), new Vector2(440f, 88f), "HOME");
        }

        private static void ConfigureButton(RectTransform rect, Vector2 position, Vector2 size, string label)
        {
            if (rect == null) return;
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position, new Vector2(0.5f, 0.5f));
            var text = rect.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
                text.fontSize = 30;
                text.fontStyle = FontStyle.Bold;
                text.raycastTarget = false;
            }
        }

        private static void ConfigureMetric(Text text, Vector2 position, Vector2 size, int fontSize, FontStyle style, Color color)
        {
            if (text == null) return;
            SetRect(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), size, position, new Vector2(0.5f, 0.5f));
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
        }

        private static void ConfigurePauseLabel(Text text, Vector2 position, Vector2 size, int fontSize, FontStyle style, Color color)
        {
            ConfigureMetric(text, position, size, fontSize, style, color);
            SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position, new Vector2(0.5f, 0.5f));
        }

        private static RectTransform FindOrCreateRect(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null) return existing;
            var created = new GameObject(name, typeof(RectTransform));
            created.layer = LayerMask.NameToLayer("UI");
            created.transform.SetParent(parent, false);
            return (RectTransform)created.transform;
        }

        private static Text FindOrCreateText(Transform parent, string name)
        {
            var existing = FindText(parent, name);
            if (existing != null) return existing;
            var created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            created.layer = LayerMask.NameToLayer("UI");
            created.transform.SetParent(parent, false);
            var text = created.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = string.Empty;
            return text;
        }

        private static Text FindText(Transform parent, string name)
        {
            var child = parent.Find(name);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void SetObject(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[CoreRacer.UI] Missing serialized field {field} on {target.name}.", target);
                return;
            }
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
