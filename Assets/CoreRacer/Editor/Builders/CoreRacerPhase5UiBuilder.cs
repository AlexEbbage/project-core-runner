#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Obstacles;
using CoreRacer.Gameplay.Pickups;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Localization;
using CoreRacer.Meta.Achievements;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Ships;
using CoreRacer.Meta.Shop;
using CoreRacer.Meta.Tasks;
using CoreRacer.Services.Compliance;
using CoreRacer.UI.Compliance;
using CoreRacer.UI.Debugging;
using CoreRacer.UI.FTUE;
using CoreRacer.UI.MainMenu;
using CoreRacer.UI.MainMenu.Progression;
using CoreRacer.UI.Settings;
using CoreRacer.UI.Shared;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoreRacer.Editor.Builders
{
    public static class CoreRacerPhase5UiBuilder
    {
        private const string GeneratedConfigFolder = "Assets/CoreRacer/Generated/Configs";
        private const string StringTablePath = GeneratedConfigFolder + "/StringTable.asset";

        [Serializable]
        private sealed class LocalizationJson
        {
            public List<LocalizationEntryJson> entries = new List<LocalizationEntryJson>();
        }

        [Serializable]
        private sealed class LocalizationEntryJson
        {
            public string key;
            public string value;
        }

        [MenuItem("Tools/Core Racer/Phase 5 Build Main UI Flow")]
        public static void BuildMainUiFlow()
        {
            EnsureFolder("Assets/CoreRacer/Generated");
            EnsureFolder(GeneratedConfigFolder);

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/CoreRacer/Scenes/CoreRacer_Main.unity")
            {
                Debug.LogError("Open Assets/CoreRacer/Scenes/CoreRacer_Main.unity before running Phase 5 UI build.");
                return;
            }

            var bootstrapper = UnityEngine.Object.FindObjectOfType<GameBootstrapper>();
            var runController = UnityEngine.Object.FindObjectOfType<RunController>();
            var references = UnityEngine.Object.FindObjectOfType<RunSceneReferences>();
            var canvas = GameObject.Find("Canvas");

            if (bootstrapper == null || runController == null || references == null || canvas == null)
            {
                Debug.LogError("CoreRacer_Main is missing bootstrap, run, references, or canvas objects required for Phase 5.");
                return;
            }

            var stringTable = CreateOrUpdateStringTable();
            var achievements = CreateOrUpdateAchievements();
            var tutorialConfig = CreateOrUpdateTutorialConfig();

            var existingMenuRoot = GameObject.Find("Canvas/MainMenu");
            if (existingMenuRoot != null)
                Undo.DestroyObjectImmediate(existingMenuRoot);

            var menuRoot = CreatePanel("MainMenu", canvas.transform as RectTransform, new Color(0.06f, 0.08f, 0.12f, 0.92f));
            Stretch(menuRoot, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);

            var shell = menuRoot.gameObject.GetComponent<MainMenuShell>() ?? menuRoot.gameObject.AddComponent<MainMenuShell>();
            RemoveComponentIfPresent<PlayPageController>(menuRoot.gameObject);
            RemoveComponentIfPresent<ShopPageController>(menuRoot.gameObject);
            RemoveComponentIfPresent<HangarPageController>(menuRoot.gameObject);
            RemoveComponentIfPresent<LabPageController>(menuRoot.gameObject);
            RemoveComponentIfPresent<ProgressionPageController>(menuRoot.gameObject);
            RemoveComponentIfPresent<SettingsMenuController>(menuRoot.gameObject);

            var topBar = BuildTopBar(menuRoot.transform as RectTransform);
            var pagesRoot = CreateUiObject("Pages", menuRoot.transform as RectTransform);
            Stretch(pagesRoot, 0f, 0f, 1f, 1f, 24f, 150f, -24f, -130f);
            var router = pagesRoot.gameObject.AddComponent<MainMenuPageRouter>();

            var playPage = BuildPlayPage(pagesRoot, runController);
            var shopPage = BuildShopPage(pagesRoot);
            var hangarPage = BuildHangarPage(pagesRoot);
            var labPage = BuildLabPage(pagesRoot);
            var progressionPage = BuildProgressionPage(pagesRoot);
            var settingsPage = BuildSettingsPage(pagesRoot);
            BuildBottomNav(menuRoot.transform as RectTransform, router);
            BuildTutorialOverlayAndDirector(canvas.transform as RectTransform, runController, router);

            SetObject(shell, "topBar", topBar);
            SetObject(shell, "router", router);
            SetRouterPages(router, playPage, shopPage, hangarPage, labPage, progressionPage, settingsPage);

            references.MainMenu = shell;
            EditorUtility.SetDirty(references);

            WireBootstrapper(bootstrapper, stringTable, achievements, tutorialConfig);
            EditorUtility.SetDirty(bootstrapper);
            EditorUtility.SetDirty(shell);
            EditorUtility.SetDirty(router);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Core Racer Phase 5 main UI flow build completed.");
        }

        [MenuItem("Tools/Core Racer/Phase 6 Wire FTUE Tutorial")]
        public static void WireFtueTutorial()
        {
            BuildMainUiFlow();
            Debug.Log("Core Racer Phase 6 FTUE tutorial wiring completed.");
        }

        private static TopBarController BuildTopBar(RectTransform parent)
        {
            var root = CreatePanel("TopBar", parent, new Color(0.1f, 0.13f, 0.18f, 0.98f));
            Stretch(root, 0f, 1f, 1f, 1f, 24f, -120f, -24f, -24f);
            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var soft = CreateInfoBlock(root, "SoftBlock", "Coins", "0");
            var premium = CreateInfoBlock(root, "PremiumBlock", "Gems", "0");
            var level = CreateInfoBlock(root, "LevelBlock", "Level", "Lv 1");

            var controller = root.gameObject.AddComponent<TopBarController>();
            SetObject(controller, "softCurrencyText", soft);
            SetObject(controller, "premiumCurrencyText", premium);
            SetObject(controller, "levelText", level);
            return controller;
        }

        private static UiView BuildPlayPage(RectTransform parent, RunController runController)
        {
            var page = CreateScrollPage("PlayPage", parent, out var content);
            var pageController = page.gameObject.AddComponent<PlayPageController>();
            var levelController = page.gameObject.AddComponent<LevelSelectPageController>();

            var title = CreateText(content, "Title", "Level Select", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var subtitle = CreateText(content, "Subtitle", "Choose the next tunnel route and start a run.", 24, TextAnchor.MiddleLeft);

            var selectedTitle = CreateText(content, "SelectedTitle", "Hex Sector", 34, TextAnchor.MiddleLeft);
            selectedTitle.fontStyle = FontStyle.Bold;
            var selectedDescription = CreateText(content, "SelectedDescription", "Classic six-sided tunnel run.", 24, TextAnchor.MiddleLeft);
            var selectedStatus = CreateText(content, "SelectedStatus", "Unlocked at Lv 1", 22, TextAnchor.MiddleLeft);

            var cardsRoot = CreateLayoutContainer("Cards", content);
            var cardTemplate = BuildLevelCardTemplate(cardsRoot);
            cardTemplate.gameObject.SetActive(false);

            var playButton = CreateButton(content, "PlayButton", "Play", out var playLabel);
            SetPreferredHeight(playButton.GetComponent<RectTransform>(), 110f);

            var roadmap = LoadAsset<LevelRoadmapConfigV2>(GeneratedConfigFolder + "/LevelRoadmap.asset");
            SetObject(pageController, "runController", runController);
            SetObject(levelController, "roadmap", roadmap);
            SetObject(levelController, "runController", runController);
            SetObject(levelController, "contentRoot", cardsRoot);
            SetObject(levelController, "cardPrefab", cardTemplate);
            SetObject(levelController, "selectedTitleText", selectedTitle);
            SetObject(levelController, "selectedDescriptionText", selectedDescription);
            SetObject(levelController, "selectedStatusText", selectedStatus);
            SetObject(levelController, "playButtonLabelText", playLabel);
            SetObject(levelController, "playButton", playButton.GetComponent<Button>());

            AddLocalizedText(title.gameObject, "menu.play.title");
            AddLocalizedText(playLabel.gameObject, "menu.play.action");
            return pageController;
        }

        private static UiView BuildShopPage(RectTransform parent)
        {
            var page = CreateScrollPage("ShopPage", parent, out var content);
            var controller = page.gameObject.AddComponent<ShopPageController>();

            var title = CreateText(content, "Title", "Shop", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var status = CreateText(content, "Status", "Browse unlocks and premium actions.", 22, TextAnchor.MiddleLeft);
            var cardsRoot = CreateLayoutContainer("CardList", content);
            var cardTemplate = BuildShopCardTemplate(cardsRoot);
            cardTemplate.gameObject.SetActive(false);
            var modal = BuildShopModal(page.transform as RectTransform);
            modal.gameObject.SetActive(false);

            SetObject(controller, "fallbackCatalog", LoadAsset<ShopCatalog>(GeneratedConfigFolder + "/ShopCatalog.asset"));
            SetObject(controller, "contentRoot", cardsRoot);
            SetObject(controller, "cardPrefab", cardTemplate);
            SetObject(controller, "detailsModal", modal);
            SetObject(controller, "statusText", status);

            AddLocalizedText(title.gameObject, "menu.shop.title");
            return controller;
        }

        private static UiView BuildHangarPage(RectTransform parent)
        {
            var page = CreateScrollPage("HangarPage", parent, out var content);
            var controller = page.gameObject.AddComponent<HangarPageController>();

            var title = CreateText(content, "Title", "Hangar", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var previewShip = CreateText(content, "PreviewShip", "Current Ship", 30, TextAnchor.MiddleLeft);
            previewShip.fontStyle = FontStyle.Bold;
            var previewCosmetics = CreateText(content, "PreviewCosmetics", "Cosmetics", 22, TextAnchor.MiddleLeft);
            var status = CreateText(content, "Status", "Ship upgrades remain disabled until the clean runtime consumes them.", 20, TextAnchor.MiddleLeft);

            var statsRoot = CreateLayoutContainer("StatsRoot", content, true);
            var statTemplate = BuildStatRowTemplate(statsRoot);
            statTemplate.gameObject.SetActive(false);

            var tabRow = CreateHorizontalContainer("Tabs", content);
            var shipsButton = CreateButton(tabRow, "ShipsButton", "Ships", out _).GetComponent<Button>();
            var skinsButton = CreateButton(tabRow, "SkinsButton", "Skins", out _).GetComponent<Button>();
            var trailsButton = CreateButton(tabRow, "TrailsButton", "Trails", out _).GetComponent<Button>();
            var coreFxButton = CreateButton(tabRow, "CoreFxButton", "Core FX", out _).GetComponent<Button>();
            var upgradesButton = CreateButton(tabRow, "UpgradesButton", "Upgrades", out _).GetComponent<Button>();

            var shipsPanel = CreateLayoutContainer("ShipsPanel", content);
            var skinsPanel = CreateLayoutContainer("SkinsPanel", content);
            var trailsPanel = CreateLayoutContainer("TrailsPanel", content);
            var coreFxPanel = CreateLayoutContainer("CoreFxPanel", content);
            var upgradesPanel = CreateLayoutContainer("UpgradesPanel", content);

            var cosmeticTemplate = BuildCosmeticRowTemplate(shipsPanel);
            cosmeticTemplate.gameObject.SetActive(false);
            var skinsTemplate = BuildCosmeticRowTemplate(skinsPanel);
            skinsTemplate.gameObject.SetActive(false);
            var trailsTemplate = BuildCosmeticRowTemplate(trailsPanel);
            trailsTemplate.gameObject.SetActive(false);
            var coreFxTemplate = BuildCosmeticRowTemplate(coreFxPanel);
            coreFxTemplate.gameObject.SetActive(false);
            var upgradeTemplate = BuildHangarUpgradeTemplate(upgradesPanel);
            upgradeTemplate.gameObject.SetActive(false);

            SetObject(controller, "shipDatabase", LoadAsset<ShipDatabase>("Assets/Config/ShipDatabase.asset"));
            SetObject(controller, "shipsButton", shipsButton);
            SetObject(controller, "skinsButton", skinsButton);
            SetObject(controller, "trailsButton", trailsButton);
            SetObject(controller, "coreFxButton", coreFxButton);
            SetObject(controller, "upgradesButton", upgradesButton);
            SetObject(controller, "shipsPanel", shipsPanel.gameObject);
            SetObject(controller, "skinsPanel", skinsPanel.gameObject);
            SetObject(controller, "trailsPanel", trailsPanel.gameObject);
            SetObject(controller, "coreFxPanel", coreFxPanel.gameObject);
            SetObject(controller, "upgradesPanel", upgradesPanel.gameObject);
            SetObject(controller, "shipsRoot", shipsPanel);
            SetObject(controller, "skinsRoot", skinsPanel);
            SetObject(controller, "trailsRoot", trailsPanel);
            SetObject(controller, "coreFxRoot", coreFxPanel);
            SetObject(controller, "upgradesRoot", upgradesPanel);
            SetObject(controller, "cosmeticPrefab", cosmeticTemplate);
            SetObject(controller, "upgradePrefab", upgradeTemplate);
            SetObject(controller, "statRowPrefab", statTemplate);
            SetObject(controller, "statsRoot", statsRoot);
            SetObject(controller, "previewShipText", previewShip);
            SetObject(controller, "previewCosmeticsText", previewCosmetics);
            SetObject(controller, "statusText", status);

            AddLocalizedText(title.gameObject, "menu.hangar.title");
            return controller;
        }

        private static UiView BuildLabPage(RectTransform parent)
        {
            var page = CreateScrollPage("LabPage", parent, out var content);
            var controller = page.gameObject.AddComponent<LabPageController>();
            var title = CreateText(content, "Title", "Lab", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var status = CreateText(content, "Status", "Upgrade supported powerups here.", 22, TextAnchor.MiddleLeft);
            var listRoot = CreateLayoutContainer("UpgradeList", content);
            var template = BuildLabUpgradeTemplate(listRoot);
            template.gameObject.SetActive(false);

            SetObject(controller, "upgradeConfig", LoadAsset<PowerupUpgradeConfigV2>(GeneratedConfigFolder + "/PowerupUpgrades.asset"));
            SetObject(controller, "contentRoot", listRoot);
            SetObject(controller, "rowPrefab", template);
            SetObject(controller, "statusText", status);
            AddLocalizedText(title.gameObject, "menu.lab.title");
            return controller;
        }

        private static UiView BuildProgressionPage(RectTransform parent)
        {
            var page = CreateScrollPage("ProgressionPage", parent, out var content);
            var pageController = page.gameObject.AddComponent<ProgressionPageController>();
            var hubController = page.gameObject.AddComponent<ProgressionHubController>();

            var title = CreateText(content, "Title", "Progression", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var buttonRow = CreateHorizontalContainer("SectionButtons", content);
            var dailyButton = CreateButton(buttonRow, "DailyLoginButton", "Daily Login", out _).GetComponent<Button>();
            var tasksButton = CreateButton(buttonRow, "TasksButton", "Tasks", out _).GetComponent<Button>();
            var achievementsButton = CreateButton(buttonRow, "AchievementsButton", "Achievements", out _).GetComponent<Button>();

            var dailyPanel = CreateLayoutContainer("DailyLoginPanel", content);
            var dailyController = dailyPanel.gameObject.AddComponent<DailyLoginPageController>();
            var dailyStatus = CreateText(dailyPanel, "Status", "Today's reward is ready.", 22, TextAnchor.MiddleLeft);
            var dailyPreviewRoot = CreateLayoutContainer("DailyPreviewRoot", dailyPanel);
            var dailyPreviewTemplate = BuildDailyPreviewTemplate(dailyPreviewRoot);
            dailyPreviewTemplate.gameObject.SetActive(false);
            var claimButton = CreateButton(dailyPanel, "ClaimButton", "Claim", out var claimLabel).GetComponent<Button>();
            var claimX2Button = CreateButton(dailyPanel, "ClaimX2Button", "Claim x2", out var claimX2Label).GetComponent<Button>();

            SetObject(dailyController, "contentRoot", dailyPreviewRoot);
            SetObject(dailyController, "rowPrefab", dailyPreviewTemplate);
            SetObject(dailyController, "statusText", dailyStatus);
            SetObject(dailyController, "claimButtonLabelText", claimLabel);
            SetObject(dailyController, "claimX2ButtonLabelText", claimX2Label);
            SetObject(dailyController, "claimButton", claimButton);
            SetObject(dailyController, "claimX2Button", claimX2Button);

            var tasksPanel = CreateLayoutContainer("TasksPanel", content);
            var dailyTasks = BuildTaskSection(tasksPanel, "Daily Tasks", TaskCadence.Daily);
            var weeklyTasks = BuildTaskSection(tasksPanel, "Weekly Tasks", TaskCadence.Weekly);
            var monthlyTasks = BuildTaskSection(tasksPanel, "Monthly Tasks", TaskCadence.Monthly);

            var achievementsPanel = CreateLayoutContainer("AchievementsPanel", content);
            var achievementsController = achievementsPanel.gameObject.AddComponent<AchievementsPageController>();
            var achievementsStatus = CreateText(achievementsPanel, "Status", "Achievements update from profile progress.", 22, TextAnchor.MiddleLeft);
            var achievementsRoot = CreateLayoutContainer("AchievementList", achievementsPanel);
            var achievementTemplate = BuildAchievementTemplate(achievementsRoot);
            achievementTemplate.gameObject.SetActive(false);
            SetObject(achievementsController, "contentRoot", achievementsRoot);
            SetObject(achievementsController, "rowPrefab", achievementTemplate);
            SetObject(achievementsController, "statusText", achievementsStatus);

            SetObject(hubController, "dailyLoginButton", dailyButton);
            SetObject(hubController, "tasksButton", tasksButton);
            SetObject(hubController, "achievementsButton", achievementsButton);
            SetObject(hubController, "dailyLoginPanel", dailyPanel.gameObject);
            SetObject(hubController, "tasksPanel", tasksPanel.gameObject);
            SetObject(hubController, "achievementsPanel", achievementsPanel.gameObject);
            SetObject(hubController, "dailyLoginPage", dailyController);
            SetObject(hubController, "achievementsPage", achievementsController);
            SetObject(hubController, "dailyTasks", dailyTasks);
            SetObject(hubController, "weeklyTasks", weeklyTasks);
            SetObject(hubController, "monthlyTasks", monthlyTasks);

            AddLocalizedText(title.gameObject, "menu.progression.title");
            return pageController;
        }

        private static UiView BuildSettingsPage(RectTransform parent)
        {
            var page = CreateScrollPage("SettingsPage", parent, out var content);
            var settingsController = page.gameObject.AddComponent<SettingsMenuController>();
            var hubController = page.gameObject.AddComponent<SettingsHubController>();

            var title = CreateText(content, "Title", "Settings", 40, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var buttonRow = CreateHorizontalContainer("SectionButtons", content);
            var generalButton = CreateButton(buttonRow, "GeneralButton", "General", out _).GetComponent<Button>();
            var comfortButton = CreateButton(buttonRow, "ComfortButton", "Comfort", out _).GetComponent<Button>();
            var privacyButton = CreateButton(buttonRow, "PrivacyButton", "Privacy", out _).GetComponent<Button>();
            var supportButton = CreateButton(buttonRow, "SupportButton", "Support / Debug", out _).GetComponent<Button>();

            var generalPanel = CreateLayoutContainer("GeneralPanel", content);
            var musicSlider = CreateSliderRow(generalPanel, "Music");
            var sfxSlider = CreateSliderRow(generalPanel, "SFX");
            var hapticsToggle = CreateToggleRow(generalPanel, "Haptics");
            SetObject(settingsController, "musicSlider", musicSlider);
            SetObject(settingsController, "sfxSlider", sfxSlider);
            SetObject(settingsController, "hapticsToggle", hapticsToggle);

            var comfortPanel = CreateLayoutContainer("ComfortPanel", content);
            var comfortController = comfortPanel.gameObject.AddComponent<ComfortSettingsController>();
            SetObject(comfortController, "screenShakeSlider", CreateSliderRow(comfortPanel, "Screen Shake"));
            SetObject(comfortController, "flashSlider", CreateSliderRow(comfortPanel, "Flashes"));
            SetObject(comfortController, "reducedVfxToggle", CreateToggleRow(comfortPanel, "Reduced VFX"));
            SetObject(comfortController, "highContrastToggle", CreateToggleRow(comfortPanel, "High Contrast"));
            SetObject(comfortController, "hapticsToggle", CreateToggleRow(comfortPanel, "Comfort Haptics"));
            SetObject(comfortController, "dragControlsToggle", CreateToggleRow(comfortPanel, "Drag Controls"));
            SetObject(comfortController, "inputSensitivitySlider", CreateSliderRow(comfortPanel, "Input Sensitivity"));

            var privacyPanel = CreateLayoutContainer("PrivacyPanel", content);
            var privacyController = privacyPanel.gameObject.AddComponent<PrivacySettingsController>();
            var exportSummary = CreateText(privacyPanel, "ExportSummary", "Local data summary appears here.", 20, TextAnchor.MiddleLeft);
            SetObject(privacyController, "privacyPolicyButton", CreateButton(privacyPanel, "PrivacyPolicyButton", "Open Privacy Policy", out _).GetComponent<Button>());
            SetObject(privacyController, "termsButton", CreateButton(privacyPanel, "TermsButton", "Open Terms", out _).GetComponent<Button>());
            SetObject(privacyController, "deleteLocalProgressButton", CreateButton(privacyPanel, "DeleteProgressButton", "Delete Local Progress", out _).GetComponent<Button>());
            SetObject(privacyController, "exportSummaryText", exportSummary);

            var consentRoot = CreateLayoutContainer("ConsentPrompt", privacyPanel);
            var consentController = consentRoot.gameObject.AddComponent<ConsentPromptController>();
            SetObject(consentController, "root", consentRoot.gameObject);
            SetObject(consentController, "acceptAllButton", CreateButton(consentRoot, "AcceptAllButton", "Accept All", out _).GetComponent<Button>());
            SetObject(consentController, "rejectPersonalizedAdsButton", CreateButton(consentRoot, "RejectAdsButton", "Reject Personalized Ads", out _).GetComponent<Button>());
            SetObject(consentController, "privacyPolicyButton", CreateButton(consentRoot, "ConsentPrivacyButton", "Privacy Policy", out _).GetComponent<Button>());
            SetObject(consentController, "termsButton", CreateButton(consentRoot, "ConsentTermsButton", "Terms", out _).GetComponent<Button>());

            var supportPanel = CreateLayoutContainer("SupportPanel", content);
            var supportController = supportPanel.gameObject.AddComponent<SupportDebugPanel>();
            var supportOutput = CreateText(supportPanel, "SupportOutput", "Support bundle output appears here.", 18, TextAnchor.UpperLeft);
            SetObject(supportController, "outputText", supportOutput);
            SetObject(supportController, "generateButton", CreateButton(supportPanel, "GenerateSupportButton", "Generate Support Bundle", out _).GetComponent<Button>());
            SetObject(supportController, "resetTutorialButton", CreateButton(supportPanel, "ResetTutorialButton", "Reset Tutorial", out _).GetComponent<Button>());

            SetObject(hubController, "generalButton", generalButton);
            SetObject(hubController, "comfortButton", comfortButton);
            SetObject(hubController, "privacyButton", privacyButton);
            SetObject(hubController, "supportButton", supportButton);
            SetObject(hubController, "generalPanel", generalPanel.gameObject);
            SetObject(hubController, "comfortPanel", comfortPanel.gameObject);
            SetObject(hubController, "privacyPanel", privacyPanel.gameObject);
            SetObject(hubController, "supportPanel", supportPanel.gameObject);
            SetObject(hubController, "settingsMenu", settingsController);
            SetObject(hubController, "comfortSettings", comfortController);

            AddLocalizedText(title.gameObject, "menu.settings.title");
            return settingsController;
        }

        private static void BuildBottomNav(RectTransform parent, MainMenuPageRouter router)
        {
            var root = CreatePanel("BottomNav", parent, new Color(0.1f, 0.13f, 0.18f, 0.98f));
            Stretch(root, 0f, 0f, 1f, 0f, 24f, 24f, -24f, 120f);
            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var controller = root.gameObject.AddComponent<BottomNavBarController>();
            SetObject(controller, "router", router);

            AddNavButton(root, "Play", controller.ShowPlay);
            AddNavButton(root, "Shop", controller.ShowShop);
            AddNavButton(root, "Hangar", controller.ShowHangar);
            AddNavButton(root, "Lab", controller.ShowLab);
            AddNavButton(root, "Progression", controller.ShowProgression);
            AddNavButton(root, "Settings", controller.ShowSettings);
        }

        private static void AddNavButton(RectTransform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var button = CreateButton(parent, label + "Button", label, out _).GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static RotatingTaskListView BuildTaskSection(RectTransform parent, string titleText, TaskCadence cadence)
        {
            var section = CreateLayoutContainer(cadence + "Section", parent);
            var title = CreateText(section, "Title", titleText, 28, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var listRoot = CreateLayoutContainer("ListRoot", section);
            var template = BuildTaskRowTemplate(listRoot);
            template.gameObject.SetActive(false);
            var list = section.gameObject.AddComponent<RotatingTaskListView>();
            SetObject(list, "contentRoot", listRoot);
            SetObject(list, "rowPrefab", template);
            SetEnum(list, "cadenceFilter", cadence);
            return list;
        }

        private static LevelSelectCardView BuildLevelCardTemplate(RectTransform parent)
        {
            var row = CreatePanel("LevelCardTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 210f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childForceExpandHeight = false;

            var title = CreateText(row, "Title", "Hex Sector", 28, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var description = CreateText(row, "Description", "Tunnel route with 6 sides.", 20, TextAnchor.MiddleLeft);
            var status = CreateText(row, "Status", "Unlocked at Lv 1", 18, TextAnchor.MiddleLeft);
            var badge = CreateText(row, "SelectedBadge", "Selected", 18, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "SelectButton", "Select", out var actionLabel).GetComponent<Button>();

            var view = row.gameObject.AddComponent<LevelSelectCardView>();
            SetObject(view, "titleText", title);
            SetObject(view, "descriptionText", description);
            SetObject(view, "statusText", status);
            SetObject(view, "actionLabelText", actionLabel);
            SetObject(view, "selectedBadge", badge.gameObject);
            SetObject(view, "selectButton", button);
            return view;
        }

        private static ShopItemCardView BuildShopCardTemplate(RectTransform parent)
        {
            var row = CreatePanel("ShopCardTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 260f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childForceExpandHeight = false;

            var icon = CreateSpritePlaceholder(row, "Icon");
            var title = CreateText(row, "Title", "Item", 26, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var description = CreateText(row, "Description", "Description", 20, TextAnchor.MiddleLeft);
            var price = CreateText(row, "Price", "0 Coins", 20, TextAnchor.MiddleLeft);
            var status = CreateText(row, "Status", "Ready to purchase.", 18, TextAnchor.MiddleLeft);
            var featured = CreateText(row, "FeaturedBadge", "Featured", 16, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "ActionButton", "View", out var actionLabel).GetComponent<Button>();

            var view = row.gameObject.AddComponent<ShopItemCardView>();
            SetObject(view, "icon", icon);
            SetObject(view, "titleText", title);
            SetObject(view, "descriptionText", description);
            SetObject(view, "priceText", price);
            SetObject(view, "statusText", status);
            SetObject(view, "actionLabelText", actionLabel);
            SetObject(view, "featuredBadge", featured.gameObject);
            SetObject(view, "buyButton", button);
            return view;
        }

        private static ShopItemDetailsModal BuildShopModal(RectTransform parent)
        {
            var root = CreatePanel("ShopItemModal", parent, new Color(0f, 0f, 0f, 0.86f));
            Stretch(root, 0f, 0f, 1f, 1f, 40f, 200f, -40f, -200f);
            var body = CreatePanel("Body", root, new Color(0.15f, 0.18f, 0.24f, 1f));
            Stretch(body, 0f, 0f, 1f, 1f, 32f, 32f, -32f, -32f);
            var layout = body.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.childForceExpandHeight = false;

            var icon = CreateSpritePlaceholder(body, "Icon");
            var title = CreateText(body, "Title", "Item", 30, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var description = CreateText(body, "Description", "Description", 22, TextAnchor.MiddleLeft);
            var price = CreateText(body, "Price", "0 Coins", 22, TextAnchor.MiddleLeft);
            var status = CreateText(body, "Status", "Ready to purchase.", 20, TextAnchor.MiddleLeft);
            var buyButton = CreateButton(body, "BuyButton", "Buy", out var actionLabel).GetComponent<Button>();
            var closeButton = CreateButton(body, "CloseButton", "Close", out _).GetComponent<Button>();

            var modal = root.gameObject.AddComponent<ShopItemDetailsModal>();
            SetObject(modal, "icon", icon);
            SetObject(modal, "titleText", title);
            SetObject(modal, "descriptionText", description);
            SetObject(modal, "priceText", price);
            SetObject(modal, "statusText", status);
            SetObject(modal, "actionLabelText", actionLabel);
            SetObject(modal, "buyButton", buyButton);
            SetObject(modal, "closeButton", closeButton);
            return modal;
        }

        private static HangarCosmeticItemView BuildCosmeticRowTemplate(RectTransform parent)
        {
            var row = CreatePanel("CosmeticRowTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 170f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            var icon = CreateSpritePlaceholder(row, "Icon");
            var title = CreateText(row, "Title", "Item", 24, TextAnchor.MiddleLeft);
            var selectedBadge = CreateText(row, "SelectedBadge", "Selected", 18, TextAnchor.MiddleLeft);
            var lockedBadge = CreateText(row, "LockedBadge", "Locked", 18, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "SelectButton", "Equip", out var actionLabel).GetComponent<Button>();

            var view = row.gameObject.AddComponent<HangarCosmeticItemView>();
            SetObject(view, "icon", icon);
            SetObject(view, "titleText", title);
            SetObject(view, "actionLabelText", actionLabel);
            SetObject(view, "selectedBadge", selectedBadge.gameObject);
            SetObject(view, "lockedBadge", lockedBadge.gameObject);
            SetObject(view, "selectButton", button);
            return view;
        }

        private static HangarUpgradeItemView BuildHangarUpgradeTemplate(RectTransform parent)
        {
            var row = CreatePanel("HangarUpgradeTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 190f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            var icon = CreateSpritePlaceholder(row, "Icon");
            var title = CreateText(row, "Title", "Upgrade", 24, TextAnchor.MiddleLeft);
            var level = CreateText(row, "Level", "Lv 0/5", 20, TextAnchor.MiddleLeft);
            var cost = CreateText(row, "Cost", "100", 20, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "UpgradeButton", "Pending", out var actionLabel).GetComponent<Button>();

            var view = row.gameObject.AddComponent<HangarUpgradeItemView>();
            SetObject(view, "icon", icon);
            SetObject(view, "titleText", title);
            SetObject(view, "levelText", level);
            SetObject(view, "costText", cost);
            SetObject(view, "actionLabelText", actionLabel);
            SetObject(view, "upgradeButton", button);
            return view;
        }

        private static HangarStatRowView BuildStatRowTemplate(RectTransform parent)
        {
            var row = CreatePanel("StatRowTemplate", parent, new Color(0.12f, 0.14f, 0.18f, 1f));
            SetPreferredHeight(row, 120f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            var label = CreateText(row, "Label", "SPD", 20, TextAnchor.MiddleLeft);
            var value = CreateText(row, "Value", "0.0", 18, TextAnchor.MiddleLeft);
            var sliderGo = CreateUiObject("Slider", row);
            var slider = sliderGo.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 10f;
            slider.value = 0f;
            var view = row.gameObject.AddComponent<HangarStatRowView>();
            SetObject(view, "labelText", label);
            SetObject(view, "valueText", value);
            SetObject(view, "slider", slider);
            return view;
        }

        private static LabUpgradeItemView BuildLabUpgradeTemplate(RectTransform parent)
        {
            var row = CreatePanel("LabUpgradeTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 210f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            var icon = CreateSpritePlaceholder(row, "Icon");
            var title = CreateText(row, "Title", "Shield", 24, TextAnchor.MiddleLeft);
            var level = CreateText(row, "Level", "Lv 0/3", 20, TextAnchor.MiddleLeft);
            var cost = CreateText(row, "Cost", "100 Coins", 20, TextAnchor.MiddleLeft);
            var status = CreateText(row, "Status", "Ready to upgrade.", 18, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "UpgradeButton", "Upgrade", out var actionLabel).GetComponent<Button>();

            var view = row.gameObject.AddComponent<LabUpgradeItemView>();
            SetObject(view, "icon", icon);
            SetObject(view, "titleText", title);
            SetObject(view, "levelText", level);
            SetObject(view, "costText", cost);
            SetObject(view, "statusText", status);
            SetObject(view, "actionLabelText", actionLabel);
            SetObject(view, "upgradeButton", button);
            return view;
        }

        private static DailyLoginRewardPreviewView BuildDailyPreviewTemplate(RectTransform parent)
        {
            var row = CreatePanel("DailyPreviewTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 150f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            var day = CreateText(row, "Day", "Day 1", 24, TextAnchor.MiddleLeft);
            var reward = CreateText(row, "Reward", "100 SoftCurrency", 20, TextAnchor.MiddleLeft);
            var claimed = CreateText(row, "ClaimedBadge", "Claimed", 18, TextAnchor.MiddleLeft);
            var current = CreateText(row, "CurrentBadge", "Current", 18, TextAnchor.MiddleLeft);

            var view = row.gameObject.AddComponent<DailyLoginRewardPreviewView>();
            SetObject(view, "dayText", day);
            SetObject(view, "rewardText", reward);
            SetObject(view, "claimedBadge", claimed.gameObject);
            SetObject(view, "currentBadge", current.gameObject);
            return view;
        }

        private static RotatingTaskRowView BuildTaskRowTemplate(RectTransform parent)
        {
            var row = CreatePanel("TaskRowTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 220f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            var title = CreateText(row, "Title", "Task", 24, TextAnchor.MiddleLeft);
            var description = CreateText(row, "Description", "Description", 20, TextAnchor.MiddleLeft);
            var progress = CreateText(row, "Progress", "0/1", 18, TextAnchor.MiddleLeft);
            var expiry = CreateText(row, "Expiry", "Expires tomorrow", 18, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "ClaimButton", "Claim", out _).GetComponent<Button>();

            var view = row.gameObject.AddComponent<RotatingTaskRowView>();
            SetObject(view, "titleText", title);
            SetObject(view, "descriptionText", description);
            SetObject(view, "progressText", progress);
            SetObject(view, "expiryText", expiry);
            SetObject(view, "claimButton", button);
            return view;
        }

        private static AchievementRowView BuildAchievementTemplate(RectTransform parent)
        {
            var row = CreatePanel("AchievementTemplate", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(row, 220f);
            var layout = row.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            var title = CreateText(row, "Title", "Achievement", 24, TextAnchor.MiddleLeft);
            var description = CreateText(row, "Description", "Description", 20, TextAnchor.MiddleLeft);
            var progress = CreateText(row, "Progress", "0/1", 18, TextAnchor.MiddleLeft);
            var button = CreateButton(row, "ClaimButton", "Locked", out var actionLabel).GetComponent<Button>();

            var view = row.gameObject.AddComponent<AchievementRowView>();
            SetObject(view, "titleText", title);
            SetObject(view, "descriptionText", description);
            SetObject(view, "progressText", progress);
            SetObject(view, "actionLabelText", actionLabel);
            SetObject(view, "claimButton", button);
            return view;
        }

        private static StringTable CreateOrUpdateStringTable()
        {
            var table = LoadAsset<StringTable>(StringTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<StringTable>();
                AssetDatabase.CreateAsset(table, StringTablePath);
            }

            var entries = new Dictionary<string, string>(StringComparer.Ordinal);
            var jsonPath = "Assets/Resources/Localization/en.json";
            var jsonText = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : string.Empty;
            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                var parsed = JsonUtility.FromJson<LocalizationJson>(jsonText);
                if (parsed != null && parsed.entries != null)
                {
                    for (int i = 0; i < parsed.entries.Count; i++)
                    {
                        var entry = parsed.entries[i];
                        if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                            continue;
                        entries[entry.key] = entry.value ?? entry.key;
                    }
                }
            }

            AddExtra(entries, "menu.play.title", "Level Select");
            AddExtra(entries, "menu.play.action", "Play");
            AddExtra(entries, "menu.shop.title", "Shop");
            AddExtra(entries, "menu.hangar.title", "Hangar");
            AddExtra(entries, "menu.lab.title", "Lab");
            AddExtra(entries, "menu.progression.title", "Progression");
            AddExtra(entries, "menu.settings.title", "Settings");
            AddFtueStrings(entries);

            table.Entries = new List<StringTable.Entry>();
            foreach (var pair in entries)
                table.Entries.Add(new StringTable.Entry { Key = pair.Key, Value = pair.Value });
            EditorUtility.SetDirty(table);
            return table;
        }

        private static List<AchievementDefinition> CreateOrUpdateAchievements()
        {
            var specs = new[]
            {
                new AchievementSpec("achievement_profile_5", "Pilot Rank 5", "Reach profile level 5.", AchievementMetricType.ProfileLevel, 5),
                new AchievementSpec("achievement_25_runs", "Tunnel Regular", "Complete 25 runs.", AchievementMetricType.TotalRuns, 25),
                new AchievementSpec("achievement_5000_score", "Speed Focus", "Reach a best score of 5,000.", AchievementMetricType.BestScore, 5000),
                new AchievementSpec("achievement_5000_coins", "Credit Cache", "Collect 5,000 coins in total.", AchievementMetricType.TotalCoinsCollected, 5000),
                new AchievementSpec("achievement_20_powerups", "System Hunter", "Collect 20 powerups in total.", AchievementMetricType.TotalPowerupsCollected, 20)
            };

            var result = new List<AchievementDefinition>();
            for (int i = 0; i < specs.Length; i++)
            {
                var spec = specs[i];
                var path = $"{GeneratedConfigFolder}/{spec.Id}.asset";
                var asset = LoadAsset<AchievementDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<AchievementDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.Id = spec.Id;
                asset.DisplayName = spec.DisplayName;
                asset.Description = spec.Description;
                asset.Metric = spec.Metric;
                asset.RequiredValue = spec.RequiredValue;
                asset.Rewards.Clear();
                asset.Rewards.Add(CoreRacer.Meta.Economy.RewardGrant.Soft(250 + i * 50));
                if (i == specs.Length - 1)
                    asset.Rewards.Add(CoreRacer.Meta.Economy.RewardGrant.Premium(5));
                EditorUtility.SetDirty(asset);
                result.Add(asset);
            }

            return result;
        }

        private static TutorialConfig CreateOrUpdateTutorialConfig()
        {
            var config = LoadAsset<TutorialConfig>(GeneratedConfigFolder + "/TutorialConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<TutorialConfig>();
                AssetDatabase.CreateAsset(config, GeneratedConfigFolder + "/TutorialConfig.asset");
            }

            config.TutorialId = "core_racer_ftue_v4";
            config.RunOnFreshInstall = true;
            config.Steps = new List<TutorialStepDefinition>
            {
                Step("welcome", TutorialStepKind.WaitForRunStarted, "ftue.welcome.title", "ftue.welcome.body", "play", false, false),
                Step("move", TutorialStepKind.WaitForInput, "ftue.move.title", "ftue.move.body", "player", false, false),
                Step("dodge_first_obstacle", TutorialStepKind.WaitForObstacleAvoided, "ftue.dodge.title", "ftue.dodge.body", "obstacle", false, false),
                Step("collect_currency", TutorialStepKind.WaitForPickup, "ftue.currency.title", "ftue.currency.body", "coin", false, false),
                Step("collect_powerup", TutorialStepKind.WaitForPowerup, "ftue.powerup.title", "ftue.powerup.body", "powerup", false, false),
                Step("crash_continue_explanation", TutorialStepKind.WaitForCrash, "ftue.crash.title", "ftue.crash.body", "continue", false, false),
                Step("continue_first_run", TutorialStepKind.WaitForContinue, "ftue.continue.title", "ftue.continue.body", "continue", false, false),
                Step("complete", TutorialStepKind.Complete, "ftue.complete.title", "ftue.complete.body", string.Empty, false, true)
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static TutorialStepDefinition Step(string id, TutorialStepKind kind, string titleKey, string bodyKey, string targetId, bool pauseGame, bool explicitContinue)
        {
            return new TutorialStepDefinition
            {
                Id = id,
                Kind = kind,
                TitleKey = titleKey,
                BodyKey = bodyKey,
                HighlightTargetId = targetId,
                PauseGame = pauseGame,
                RequiresExplicitContinue = explicitContinue,
                MinimumDisplaySeconds = 0.5f
            };
        }

        private static void BuildTutorialOverlayAndDirector(RectTransform canvas, RunController runController, MainMenuPageRouter router)
        {
            var existingOverlay = GameObject.Find("Canvas/TutorialOverlay");
            if (existingOverlay != null)
                Undo.DestroyObjectImmediate(existingOverlay);

            var overlayRoot = CreateUiObject("TutorialOverlay", canvas);
            Stretch(overlayRoot, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);

            var panel = CreatePanel("TutorialPanel", overlayRoot, new Color(0.02f, 0.04f, 0.07f, 0.92f));
            Stretch(panel, 0f, 0f, 1f, 0f, 40f, 160f, -40f, 460f);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.childForceExpandHeight = false;
            var title = CreateText(panel, "Title", "Welcome", 32, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            var body = CreateText(panel, "Body", "Survive the tunnel.", 22, TextAnchor.MiddleLeft);
            var continueButton = CreateButton(panel, "ContinueButton", "Continue", out _).GetComponent<Button>();

            var overlay = overlayRoot.gameObject.AddComponent<TutorialOverlayController>();
            SetObject(overlay, "root", panel.gameObject);
            SetObject(overlay, "titleText", title);
            SetObject(overlay, "bodyText", body);
            SetObject(overlay, "continueButton", continueButton);
            panel.gameObject.SetActive(false);

            var existingDirector = GameObject.Find("TutorialDirector");
            if (existingDirector != null)
                Undo.DestroyObjectImmediate(existingDirector);

            var directorObject = new GameObject("TutorialDirector");
            var director = directorObject.AddComponent<TutorialDirector>();
            SetObject(director, "runController", runController);
            SetObject(director, "obstacleWorld", UnityEngine.Object.FindObjectOfType<ObstacleWorldController>());
            SetObject(director, "pickupWorld", UnityEngine.Object.FindObjectOfType<PickupWorldController>());
            SetObject(director, "router", router);
            SetObject(director, "overlay", overlay);
        }

        private static void WireBootstrapper(GameBootstrapper bootstrapper, StringTable table, List<AchievementDefinition> achievements, TutorialConfig tutorialConfig)
        {
            SetObject(bootstrapper, "shopCatalog", LoadAsset<ShopCatalog>(GeneratedConfigFolder + "/ShopCatalog.asset"));
            SetObject(bootstrapper, "stringTable", table);
            SetObject(bootstrapper, "rotatingTaskPool", LoadAsset<TaskPoolDefinition>(GeneratedConfigFolder + "/RotatingTaskPool.asset"));
            SetObject(bootstrapper, "dailyRewardCalendar", LoadAsset<DailyRewardCalendarConfig>(GeneratedConfigFolder + "/DailyRewardCalendar.asset"));
            SetObject(bootstrapper, "privacyLinks", LoadAsset<PrivacyLinksConfig>(GeneratedConfigFolder + "/PrivacyLinks.asset"));
            SetObject(bootstrapper, "tutorialConfig", tutorialConfig);
            SetList(bootstrapper, "achievementDefinitions", achievements);
        }

        private static void SetRouterPages(MainMenuPageRouter router, UiView play, UiView shop, UiView hangar, UiView lab, UiView progression, UiView settings)
        {
            var pages = new[]
            {
                Tuple.Create(MainMenuPage.Play, play),
                Tuple.Create(MainMenuPage.Shop, shop),
                Tuple.Create(MainMenuPage.Hangar, hangar),
                Tuple.Create(MainMenuPage.Lab, lab),
                Tuple.Create(MainMenuPage.Progression, progression),
                Tuple.Create(MainMenuPage.Settings, settings)
            };

            var so = new SerializedObject(router);
            so.FindProperty("defaultPage").enumValueIndex = (int)MainMenuPage.Play;
            var prop = so.FindProperty("pages");
            prop.arraySize = pages.Length;
            for (int i = 0; i < pages.Length; i++)
            {
                var binding = prop.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("Page").enumValueIndex = (int)pages[i].Item1;
                binding.FindPropertyRelative("View").objectReferenceValue = pages[i].Item2;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform CreateScrollPage(string name, RectTransform parent, out RectTransform content)
        {
            var root = CreateUiObject(name, parent);
            Stretch(root, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
            var viewport = CreatePanel("Viewport", root, new Color(0f, 0f, 0f, 0f));
            Stretch(viewport, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            content = CreateLayoutContainer("Content", viewport);
            Stretch(content, 0f, 1f, 1f, 1f, 0f, 0f, 0f, 0f);
            content.pivot = new Vector2(0.5f, 1f);
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            return root;
        }

        private static Text CreateInfoBlock(RectTransform parent, string name, string label, string value)
        {
            var block = CreatePanel(name, parent, new Color(0.16f, 0.19f, 0.25f, 1f));
            var layout = block.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            var labelText = CreateText(block, "Label", label, 16, TextAnchor.MiddleCenter);
            labelText.color = new Color(0.72f, 0.76f, 0.84f, 1f);
            var valueText = CreateText(block, "Value", value, 24, TextAnchor.MiddleCenter);
            valueText.fontStyle = FontStyle.Bold;
            return valueText;
        }

        private static RectTransform CreateLayoutContainer(string name, RectTransform parent, bool horizontal = false)
        {
            var root = CreateUiObject(name, parent);
            var layout = horizontal
                ? (LayoutGroup)root.gameObject.AddComponent<HorizontalLayoutGroup>()
                : root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            if (layout is HorizontalLayoutGroup horizontalLayout)
            {
                horizontalLayout.spacing = 12f;
                horizontalLayout.childForceExpandWidth = true;
                horizontalLayout.childForceExpandHeight = false;
            }
            else if (layout is VerticalLayoutGroup verticalLayout)
            {
                verticalLayout.spacing = 12f;
                verticalLayout.childForceExpandWidth = true;
                verticalLayout.childForceExpandHeight = false;
            }

            var fitter = root.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            return root;
        }

        private static RectTransform CreateHorizontalContainer(string name, RectTransform parent)
        {
            return CreateLayoutContainer(name, parent, true);
        }

        private static Slider CreateSliderRow(RectTransform parent, string label)
        {
            var root = CreatePanel(label.Replace(" ", string.Empty) + "Row", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(root, 120f);
            var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            CreateText(root, "Label", label, 20, TextAnchor.MiddleLeft);
            var sliderGo = CreatePanel("Slider", root, new Color(0.2f, 0.24f, 0.32f, 1f));
            SetPreferredHeight(sliderGo, 32f);
            var fillArea = CreateUiObject("FillArea", sliderGo);
            Stretch(fillArea, 0f, 0f, 1f, 1f, 6f, 6f, -32f, -6f);
            var fill = CreatePanel("Fill", fillArea, new Color(0.23f, 0.38f, 0.74f, 1f));
            Stretch(fill, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
            var handleArea = CreateUiObject("HandleArea", sliderGo);
            Stretch(handleArea, 0f, 0f, 1f, 1f, 16f, 0f, -16f, 0f);
            var handle = CreatePanel("Handle", handleArea, Color.white);
            handle.sizeDelta = new Vector2(24f, 24f);

            var slider = sliderGo.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static Toggle CreateToggleRow(RectTransform parent, string label)
        {
            var root = CreatePanel(label.Replace(" ", string.Empty) + "Row", parent, new Color(0.14f, 0.17f, 0.23f, 1f));
            SetPreferredHeight(root, 120f);
            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childAlignment = TextAnchor.MiddleLeft;
            CreateText(root, "Label", label, 20, TextAnchor.MiddleLeft);
            var toggleGo = CreatePanel("Toggle", root, new Color(0.2f, 0.24f, 0.32f, 1f));
            toggleGo.sizeDelta = new Vector2(40f, 40f);
            var checkmark = CreatePanel("Checkmark", toggleGo, new Color(0.23f, 0.38f, 0.74f, 1f));
            Stretch(checkmark, 0f, 0f, 1f, 1f, 8f, 8f, -8f, -8f);
            var toggle = toggleGo.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = toggleGo.GetComponent<Image>();
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.isOn = true;
            return toggle;
        }

        private static RectTransform CreateButton(RectTransform parent, string name, string label, out Text labelText)
        {
            var root = CreatePanel(name, parent, new Color(0.23f, 0.38f, 0.74f, 1f));
            SetPreferredHeight(root, 92f);
            var button = root.gameObject.AddComponent<Button>();
            var text = CreateText(root, "Label", label, 22, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            Stretch(text.rectTransform, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
            labelText = text;
            return root;
        }

        private static Text CreateText(RectTransform parent, string name, string text, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = anchor;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static Image CreateSpritePlaceholder(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.35f, 0.42f, 0.54f, 1f);
            SetPreferredHeight(go.GetComponent<RectTransform>(), 80f);
            return image;
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform CreateUiObject(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void AddLocalizedText(GameObject target, string key)
        {
            var localized = target.AddComponent<LocalizedTextV2>();
            SetString(localized, "key", key);
            var text = target.GetComponent<Text>();
            if (text != null)
                SetObject(localized, "target", text);
        }

        private static void SetPreferredHeight(RectTransform target, float height)
        {
            var layoutElement = target.GetComponent<LayoutElement>() ?? target.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
        }

        private static void Stretch(RectTransform rect, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' not found on '{target.name}'.");
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' not found on '{target.name}'.");
            property.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(UnityEngine.Object target, string propertyName, Enum value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' not found on '{target.name}'.");
            property.enumValueIndex = Convert.ToInt32(value);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetList<T>(UnityEngine.Object target, string propertyName, List<T> values) where T : UnityEngine.Object
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property '{propertyName}' not found on '{target.name}'.");
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RemoveComponentIfPresent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
                Undo.DestroyObjectImmediate(component);
        }

        private static void AddExtra(Dictionary<string, string> entries, string key, string value)
        {
            entries[key] = value;
        }

        private static void AddFtueStrings(Dictionary<string, string> entries)
        {
            AddExtra(entries, "ftue.welcome.title", "Welcome, pilot");
            AddExtra(entries, "ftue.welcome.body", "Pick a route and start your first tunnel run.");
            AddExtra(entries, "ftue.move.title", "Move left and right");
            AddExtra(entries, "ftue.move.body", "Touch either side to steer. Drag Controls in Settings enables analog steering.");
            AddExtra(entries, "ftue.dodge.title", "Dodge the first obstacle");
            AddExtra(entries, "ftue.dodge.body", "Slip through the open lane and keep moving forward.");
            AddExtra(entries, "ftue.currency.title", "Collect currency");
            AddExtra(entries, "ftue.currency.body", "Grab coins during runs to fund upgrades and unlocks.");
            AddExtra(entries, "ftue.powerup.title", "Collect a powerup");
            AddExtra(entries, "ftue.powerup.body", "Powerups give short boosts that help you survive longer.");
            AddExtra(entries, "ftue.crash.title", "Crashes and continues");
            AddExtra(entries, "ftue.crash.body", "Keep flying until you crash. Your first run includes one continue.");
            AddExtra(entries, "ftue.continue.title", "Continue the run");
            AddExtra(entries, "ftue.continue.body", "Press Continue to respawn and keep your score and distance.");
            AddExtra(entries, "ftue.upgrade.title", "Upgrade in the Lab");
            AddExtra(entries, "ftue.upgrade.body", "The Lab is where powerups become stronger over time.");
            AddExtra(entries, "ftue.tasks.title", "Claim daily rewards");
            AddExtra(entries, "ftue.tasks.body", "Daily rewards and rotating tasks give you goals between runs.");
            AddExtra(entries, "ftue.complete.title", "Ready to race");
            AddExtra(entries, "ftue.complete.body", "Keep running, upgrading, and claiming rewards.");
        }

        private readonly struct AchievementSpec
        {
            public AchievementSpec(string id, string displayName, string description, AchievementMetricType metric, int requiredValue)
            {
                Id = id;
                DisplayName = displayName;
                Description = description;
                Metric = metric;
                RequiredValue = requiredValue;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public string Description { get; }
            public AchievementMetricType Metric { get; }
            public int RequiredValue { get; }
        }
    }
}
#endif
