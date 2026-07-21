using System;
using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.FTUE;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Achievements;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.DailyRewards;
using CoreRacer.Meta.Economy;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Profile;
using CoreRacer.Meta.Progression;
using CoreRacer.Meta.Ships;
using CoreRacer.Meta.Shop;
using CoreRacer.Meta.Tasks;
using CoreRacer.Monetisation.Ads;
using CoreRacer.Services.Accessibility;
using CoreRacer.Services.Settings;
using CoreRacer.Services.Support;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class CoreRacerUiController : MonoBehaviour, IRunUiPresenter
    {
        [Header("Composition")]
        [SerializeField] private UIDocument document;
        [SerializeField] private RunController runController;
        [SerializeField] private RunSceneReferences runReferences;

        [Header("Content")]
        [SerializeField] private LevelRoadmapConfigV2 levelRoadmap;
        [SerializeField] private BoosterCatalog boosterCatalog;
        [SerializeField] private ShipDatabase shipDatabase;
        [SerializeField] private ShopCatalog shopCatalog;
        [SerializeField] private PowerupUpgradeConfigV2 powerupUpgrades;

        private VisualElement _root;
        private VisualElement _mainMenu;
        private VisualElement _screenLayer;
        private VisualElement _hud;
        private VisualElement _pause;
        private VisualElement _gameOver;
        private VisualElement _genericModal;
        private VisualElement _gallery;
        private VisualElement _tutorialOverlay;
        private Label _toast;
        private Label _hudScore;
        private Label _hudDistance;
        private Label _hudCoins;
        private Label _hudHealth;
        private VisualElement _powerupStrip;
        private Label _gameOverMessage;
        private VisualElement _continueActions;
        private VisualElement _finalActions;
        private Button _continueButton;
        private Button _doubleRewardsButton;
        private Label _settingsStatus;
        private Label _shopStatus;
        private Label _hangarStatus;
        private Label _labStatus;
        private Action _modalPrimaryAction;
        private IUiAnimationService _animations;
        private ICoreRacerScreenRouter _router;
        private PlayerProfileService _profile;
        private SettingsService _settings;
        private AccessibilitySettingsService _accessibility;
        private TutorialService _tutorial;
        private DailyRewardCalendarService _dailyRewards;
        private AchievementService _achievements;
        private ProgressionTaskService _progressionTasks;
        private RotatingTaskService _rotatingTasks;
        private ShopService _shop;
        private RewardedAdController _rewardedAds;
        private SupportBundleExporter _support;
        private BoosterLoadoutService _boosterLoadout;
        private bool _initialized;
        private int _selectedLevelIndex;
        private HangarSection _hangarSection;

        public bool IsInitialized => _initialized;
        public CoreRacerScreenId CurrentScreen => _router != null ? _router.Current : CoreRacerScreenId.Play;
        public bool IsModalOpen => _genericModal != null && !_genericModal.ClassListContains("is-hidden");

        private enum HangarSection { Ships, Skins, Trails, CoreFx }

        private void Awake()
        {
            if (document == null) document = GetComponent<UIDocument>();
            if (runController == null) runController = FindObjectOfType<RunController>();
            if (runReferences == null) runReferences = FindObjectOfType<RunSceneReferences>();
        }

        private void OnEnable()
        {
            Initialize();
            SubscribeModelEvents();
        }

        private void OnDisable()
        {
            UnsubscribeModelEvents();
            _animations?.StopAll();
        }

        public void Initialize()
        {
            if (_initialized) return;
            if (document == null || document.visualTreeAsset == null)
            {
                Debug.LogError("[CoreRacer.UI] UIDocument or source VisualTreeAsset is missing.", this);
                return;
            }

            _root = document.rootVisualElement;
            if (_root == null || _root.childCount == 0)
            {
                Debug.LogError("[CoreRacer.UI] UIDocument produced an empty visual tree.", this);
                return;
            }

            ResolveServices();
            CacheRequiredElements();
            _animations = new LitMotionUiAnimationService
            {
                ReducedMotion = _accessibility != null && _accessibility.State.ReducedVfxMode
            };
            BuildRouter();
            BindInteractions();
            _initialized = true;
            RefreshAll();
            _router.Show(CoreRacerScreenId.Play);
            ShowMainMenu();
            Debug.Log("[CoreRacer.UI] UI Toolkit root initialized.", this);
        }

        public void ShowMainMenu()
        {
            if (!EnsureReady()) return;
            SetVisible(_mainMenu, true);
            SetVisible(_screenLayer, true);
            HideHud();
            HidePause();
            HideGameOver();
            RefreshAll();
            _animations.ShowScreen(_mainMenu);
        }

        public void HideMainMenu()
        {
            if (!EnsureReady()) return;
            SetVisible(_mainMenu, false);
            SetVisible(_screenLayer, false);
        }
        public void ShowHud() { if (EnsureReady()) SetVisible(_hud, true); }
        public void HideHud() { if (EnsureReady()) SetVisible(_hud, false); }

        public void ShowPause()
        {
            if (!EnsureReady()) return;
            SetVisible(_pause, true);
            _animations.ShowPopup(_pause);
        }

        public void HidePause() { if (EnsureReady()) _animations.HidePopup(_pause); }
        public void HideGameOver() { if (EnsureReady()) _animations.HidePopup(_gameOver); }

        public void ShowContinueOffer()
        {
            if (!EnsureReady()) return;
            _gameOverMessage.text = "Watch a rewarded ad to continue this run, or end it now.";
            SetVisible(_continueActions, true);
            SetVisible(_finalActions, false);
            SetVisible(_gameOver, true);
            _animations.ShowPopup(_gameOver);
        }

        public void ShowContinueUnavailable()
        {
            if (!EnsureReady()) return;
            _gameOverMessage.text = "Continue is unavailable. Finalising this run.";
            _animations.PlayInvalidAction(_gameOver);
        }

        public void SetContinuePending(bool pending)
        {
            if (!EnsureReady()) return;
            _continueButton.SetEnabled(!pending);
            _continueButton.text = pending ? "PLEASE WAIT..." : "CONTINUE RUN";
        }

        public void ShowGameOver(RunResult result)
        {
            if (!EnsureReady()) return;
            _root.Require<Label>("ResultScore").text = $"SCORE  {result.Score:N0}";
            _root.Require<Label>("ResultCoins").text = $"COINS  {result.Coins:N0}";
            _root.Require<Label>("ResultXp").text = $"XP  {result.Experience:N0}";
            _root.Require<Label>("ResultPremium").text = $"CORE  {result.PremiumCurrency:N0}";
            _gameOverMessage.text = $"Distance {result.Distance:0} m  /  Powerups {result.PowerupsCollected}";
            SetVisible(_continueActions, false);
            SetVisible(_finalActions, true);
            SetVisible(_gameOver, true);
            _animations.ShowPopup(_gameOver);
        }

        public void SetDoubleRewardPending(bool pending)
        {
            if (!EnsureReady()) return;
            _doubleRewardsButton.SetEnabled(!pending);
            _doubleRewardsButton.text = pending ? "PLEASE WAIT..." : "DOUBLE REWARDS";
        }

        public void ShowDoubleRewardUnavailable()
        {
            if (!EnsureReady()) return;
            _gameOverMessage.text = "Double rewards are unavailable right now.";
            _animations.PlayInvalidAction(_doubleRewardsButton);
        }

        public void ShowDoubleRewardGranted(RunResult bonus)
        {
            if (!EnsureReady()) return;
            _gameOverMessage.text = $"Bonus granted: +{bonus.Coins:N0} coins, +{bonus.Experience:N0} XP.";
            _doubleRewardsButton.SetEnabled(false);
            _doubleRewardsButton.text = "REWARDS DOUBLED";
            _animations.PlaySuccess(_gameOver);
            RefreshProfileHeader();
        }

        private void CacheRequiredElements()
        {
            _mainMenu = _root.Require<VisualElement>("MainMenuScreen");
            _screenLayer = _root.Require<VisualElement>("ScreenLayer");
            _hud = _root.Require<VisualElement>("HudLayer");
            _pause = _root.Require<VisualElement>("PauseOverlay");
            _gameOver = _root.Require<VisualElement>("GameOverPopup");
            _genericModal = _root.Require<VisualElement>("GenericModal");
            _gallery = _root.Require<VisualElement>("ComponentGallery");
            _tutorialOverlay = _root.Require<VisualElement>("TutorialOverlay");
            _toast = _root.Require<Label>("Toast");
            _hudScore = _root.Require<Label>("HudScore");
            _hudDistance = _root.Require<Label>("HudDistance");
            _hudCoins = _root.Require<Label>("HudCoins");
            _hudHealth = _root.Require<Label>("HudHealth");
            _powerupStrip = _root.Require<VisualElement>("PowerupStrip");
            _gameOverMessage = _root.Require<Label>("GameOverMessage");
            _continueActions = _root.Require<VisualElement>("ContinueActions");
            _finalActions = _root.Require<VisualElement>("FinalActions");
            _continueButton = _root.Require<Button>("ContinueRunButton");
            _doubleRewardsButton = _root.Require<Button>("DoubleRewardsButton");
            _settingsStatus = _root.Require<Label>("SettingsStatus");
            _shopStatus = _root.Require<Label>("ShopStatus");
            _hangarStatus = _root.Require<Label>("HangarStatus");
            _labStatus = _root.Require<Label>("LabStatus");
        }

        private void BuildRouter()
        {
            var screens = new Dictionary<CoreRacerScreenId, VisualElement>
            {
                [CoreRacerScreenId.Play] = _root.Require<ScrollView>("PlayScreen"),
                [CoreRacerScreenId.Shop] = _root.Require<ScrollView>("ShopScreen"),
                [CoreRacerScreenId.Hangar] = _root.Require<ScrollView>("HangarScreen"),
                [CoreRacerScreenId.Lab] = _root.Require<ScrollView>("LabScreen"),
                [CoreRacerScreenId.Progression] = _root.Require<ScrollView>("ProgressionScreen"),
                [CoreRacerScreenId.Settings] = _root.Require<ScrollView>("SettingsScreen")
            };
            var navigation = new Dictionary<CoreRacerScreenId, Button>
            {
                [CoreRacerScreenId.Play] = _root.Require<Button>("NavPlay"),
                [CoreRacerScreenId.Shop] = _root.Require<Button>("NavShop"),
                [CoreRacerScreenId.Hangar] = _root.Require<Button>("NavHangar"),
                [CoreRacerScreenId.Lab] = _root.Require<Button>("NavLab"),
                [CoreRacerScreenId.Progression] = _root.Require<Button>("NavProgression")
            };
            _router = new CoreRacerScreenRouter(screens, navigation, _animations);
            _router.Changed += RefreshScreen;
        }

        private void BindInteractions()
        {
            BindNavigation("NavPlay", CoreRacerScreenId.Play);
            BindNavigation("NavShop", CoreRacerScreenId.Shop);
            BindNavigation("NavHangar", CoreRacerScreenId.Hangar);
            BindNavigation("NavLab", CoreRacerScreenId.Lab);
            BindNavigation("NavProgression", CoreRacerScreenId.Progression);
            _root.Require<Button>("ProfileButton").clicked += () => _router.Show(CoreRacerScreenId.Progression);
            _root.Require<Button>("SettingsShortcutButton").clicked += () => _router.Show(CoreRacerScreenId.Settings);
            _root.Require<Button>("PlayButton").clicked += StartSelectedRun;
            _root.Require<Button>("PauseButton").clicked += Pause;
            _root.Require<Button>("ResumeButton").clicked += Resume;
            _root.Require<Button>("PauseHomeButton").clicked += ReturnHome;
            _continueButton.clicked += () => runController?.ContinueRun();
            _root.Require<Button>("EndRunButton").clicked += () => runController?.DeclineContinue();
            _doubleRewardsButton.clicked += () => runController?.DoubleRunRewards();
            _root.Require<Button>("RetryButton").clicked += () => runController?.RetryRun();
            _root.Require<Button>("HomeButton").clicked += ReturnHome;
            _root.Require<Button>("ModalCloseButton").clicked += CloseModal;
            _root.Require<Button>("ModalPrimaryButton").clicked += InvokeModalPrimary;
            _root.Require<Button>("HangarShipsTab").clicked += () => ShowHangarSection(HangarSection.Ships);
            _root.Require<Button>("HangarSkinsTab").clicked += () => ShowHangarSection(HangarSection.Skins);
            _root.Require<Button>("HangarTrailsTab").clicked += () => ShowHangarSection(HangarSection.Trails);
            _root.Require<Button>("HangarCoreFxTab").clicked += () => ShowHangarSection(HangarSection.CoreFx);
            _root.Require<Button>("DailyTab").clicked += () => ShowProgressionPanel("DailyPanel", "DailyTab");
            _root.Require<Button>("TasksTab").clicked += () => ShowProgressionPanel("TasksPanel", "TasksTab");
            _root.Require<Button>("AchievementsTab").clicked += () => ShowProgressionPanel("AchievementsPanel", "AchievementsTab");
            _root.Require<Button>("ClaimDailyButton").clicked += () => ClaimDaily(false);
            _root.Require<Button>("ClaimDailyDoubleButton").clicked += () => ClaimDaily(true);
            _root.Require<Button>("PrivacyButton").clicked += OpenPrivacy;
            _root.Require<Button>("SupportButton").clicked += OpenSupport;
            _root.Require<Button>("ResetTutorialButton").clicked += ResetTutorial;
            _root.Require<Button>("GalleryButton").clicked += OpenGallery;
            _root.Require<Button>("GalleryCloseButton").clicked += CloseGallery;
            _root.Require<Button>("GalleryMotionButton").clicked += () => _animations.PlaySuccess(_gallery);
            BindSettings();
            BlockGameplayInput(_pause);
            BlockGameplayInput(_gameOver);
            BlockGameplayInput(_genericModal);
            BlockGameplayInput(_gallery);
        }

        private void BindNavigation(string buttonName, CoreRacerScreenId screen)
        {
            _root.Require<Button>(buttonName).clicked += () => _router.Show(screen);
        }

        private void BindSettings()
        {
            var music = _root.Require<Slider>("MusicSlider");
            var sfx = _root.Require<Slider>("SfxSlider");
            var haptics = _root.Require<Toggle>("HapticsToggle");
            var drag = _root.Require<Toggle>("DragControlsToggle");
            var reduced = _root.Require<Toggle>("ReducedMotionToggle");
            var contrast = _root.Require<Toggle>("HighContrastToggle");
            music.RegisterValueChangedCallback(e => _settings?.SetMusicVolume(e.newValue));
            sfx.RegisterValueChangedCallback(e => _settings?.SetSfxVolume(e.newValue));
            haptics.RegisterValueChangedCallback(e =>
            {
                _settings?.SetHaptics(e.newValue);
                _accessibility?.Update(s => s.HapticsEnabled = e.newValue);
            });
            drag.RegisterValueChangedCallback(e => _accessibility?.Update(s => s.DragControlsEnabled = e.newValue));
            reduced.RegisterValueChangedCallback(e =>
            {
                _accessibility?.Update(s => s.ReducedVfxMode = e.newValue);
                if (_animations != null) _animations.ReducedMotion = e.newValue;
            });
            contrast.RegisterValueChangedCallback(e =>
            {
                _accessibility?.Update(s => s.HighContrastMode = e.newValue);
                _root.EnableInClassList("theme--high-contrast", e.newValue);
            });
        }

        private void ResolveServices()
        {
            GameServices.TryGet(out _profile);
            GameServices.TryGet(out _settings);
            GameServices.TryGet(out _accessibility);
            GameServices.TryGet(out _tutorial);
            GameServices.TryGet(out _dailyRewards);
            GameServices.TryGet(out _achievements);
            GameServices.TryGet(out _progressionTasks);
            GameServices.TryGet(out _rotatingTasks);
            GameServices.TryGet(out _shop);
            GameServices.TryGet(out _rewardedAds);
            GameServices.TryGet(out _support);
            if (_profile != null && boosterCatalog != null) _boosterLoadout = new BoosterLoadoutService(_profile, boosterCatalog);
        }

        private void SubscribeModelEvents()
        {
            if (!_initialized) return;
            if (_profile != null) _profile.Changed += RefreshAll;
            if (_settings != null) _settings.Changed += RefreshSettings;
            if (_accessibility != null) _accessibility.Changed += OnComfortChanged;
            if (_tutorial != null)
            {
                _tutorial.StepChanged += ShowTutorialStep;
                _tutorial.Completed += HideTutorial;
            }
            if (runReferences == null) return;
            if (runReferences.ScoreTracker != null) runReferences.ScoreTracker.ScoreChanged += UpdateScore;
            if (runReferences.StatsTracker != null) runReferences.StatsTracker.DistanceChanged += UpdateDistance;
            if (runReferences.CurrencyTracker != null) runReferences.CurrencyTracker.CoinsChanged += UpdateCoins;
            if (runReferences.PlayerHealth != null) runReferences.PlayerHealth.HealthChanged += UpdateHealth;
            if (runReferences.Powerups != null)
            {
                runReferences.Powerups.PowerupActivated += AddPowerup;
                runReferences.Powerups.PowerupExpired += RemovePowerup;
            }
        }

        private void UnsubscribeModelEvents()
        {
            if (_profile != null) _profile.Changed -= RefreshAll;
            if (_settings != null) _settings.Changed -= RefreshSettings;
            if (_accessibility != null) _accessibility.Changed -= OnComfortChanged;
            if (_tutorial != null)
            {
                _tutorial.StepChanged -= ShowTutorialStep;
                _tutorial.Completed -= HideTutorial;
            }
            if (runReferences == null) return;
            if (runReferences.ScoreTracker != null) runReferences.ScoreTracker.ScoreChanged -= UpdateScore;
            if (runReferences.StatsTracker != null) runReferences.StatsTracker.DistanceChanged -= UpdateDistance;
            if (runReferences.CurrencyTracker != null) runReferences.CurrencyTracker.CoinsChanged -= UpdateCoins;
            if (runReferences.PlayerHealth != null) runReferences.PlayerHealth.HealthChanged -= UpdateHealth;
            if (runReferences.Powerups != null)
            {
                runReferences.Powerups.PowerupActivated -= AddPowerup;
                runReferences.Powerups.PowerupExpired -= RemovePowerup;
            }
        }

        private void RefreshAll()
        {
            if (!_initialized) return;
            ResolveServices();
            RefreshProfileHeader();
            RefreshScreen(CurrentScreen);
            RefreshSettings();
        }

        private void RefreshScreen(CoreRacerScreenId screen)
        {
            switch (screen)
            {
                case CoreRacerScreenId.Play: RefreshPlay(); break;
                case CoreRacerScreenId.Shop: RefreshShop(); break;
                case CoreRacerScreenId.Hangar: RefreshHangar(); break;
                case CoreRacerScreenId.Lab: RefreshLab(); break;
                case CoreRacerScreenId.Progression: RefreshProgression(); break;
                case CoreRacerScreenId.Settings: RefreshSettings(); break;
            }
        }

        private void RefreshProfileHeader()
        {
            if (_profile == null) return;
            _root.Require<Label>("LevelLabel").text = $"LV {_profile.State.Level}";
            _root.Require<Label>("SoftCurrencyLabel").text = _profile.State.Wallet.Soft.ToString("N0");
            _root.Require<Label>("PremiumCurrencyLabel").text = _profile.State.Wallet.Premium.ToString("N0");
        }

        private void RefreshPlay()
        {
            var list = _root.Require<VisualElement>("LevelList");
            list.Clear();
            if (levelRoadmap == null || levelRoadmap.Levels == null || levelRoadmap.Levels.Count == 0)
            {
                AddMessage(list, "No run routes are configured.", "state--error");
                _root.Require<Button>("PlayButton").SetEnabled(false);
                return;
            }
            _selectedLevelIndex = Mathf.Clamp(_profile != null ? _profile.State.SelectedLevelIndex : _selectedLevelIndex, 0, levelRoadmap.Levels.Count - 1);
            for (var i = 0; i < levelRoadmap.Levels.Count; i++)
            {
                var index = i;
                var level = levelRoadmap.Levels[i];
                if (level == null) continue;
                var unlocked = _profile == null || _profile.State.Level >= level.RequiredPlayerLevel;
                AddCard(list, level.DisplayName, level.Description, unlocked ? (i == _selectedLevelIndex ? "SELECTED" : "READY") : $"REQUIRES LV {level.RequiredPlayerLevel}", i == _selectedLevelIndex ? "SELECTED" : "SELECT", () => SelectLevel(index), unlocked);
            }
            SelectLevel(_selectedLevelIndex, false);
            RefreshBoosters();
        }

        private void SelectLevel(int index, bool animate = true)
        {
            if (levelRoadmap == null || index < 0 || index >= levelRoadmap.Levels.Count) return;
            var level = levelRoadmap.Levels[index];
            if (level == null || (_profile != null && _profile.State.Level < level.RequiredPlayerLevel)) return;
            _selectedLevelIndex = index;
            if (_profile != null && _profile.State.SelectedLevelIndex != index)
                _profile.Mutate(state => state.SelectedLevelIndex = index);
            runController?.SetSelectedLevel(level);
            _root.Require<Label>("SelectedLevelTitle").text = level.DisplayName.ToUpperInvariant();
            _root.Require<Label>("SelectedLevelDescription").text = level.Description;
            _root.Require<Label>("SelectedLevelStatus").text = "READY";
            _root.Require<Button>("PlayButton").SetEnabled(true);
            if (animate) RefreshPlay();
        }

        private void RefreshBoosters()
        {
            var list = _root.Require<VisualElement>("BoosterList");
            list.Clear();
            if (boosterCatalog == null || boosterCatalog.Boosters == null || boosterCatalog.Boosters.Count == 0)
            {
                AddMessage(list, "No boosters available.");
                return;
            }
            var equipped = 0;
            for (var i = 0; i < boosterCatalog.Boosters.Count; i++)
            {
                var booster = boosterCatalog.Boosters[i];
                if (booster == null) continue;
                var isEquipped = _boosterLoadout != null && _boosterLoadout.IsEquipped(booster.Id);
                if (isEquipped) equipped++;
                AddCard(list, booster.DisplayName, booster.EffectType.ToString(), isEquipped ? "EQUIPPED" : booster.Family.ToString(), isEquipped ? "REMOVE" : "EQUIP", () => ToggleBooster(booster.Id));
            }
            _root.Require<Label>("BoosterSummary").text = equipped == 0 ? "No boosters equipped" : $"{equipped} booster families equipped";
        }

        private void ToggleBooster(string id)
        {
            if (_boosterLoadout == null || !_boosterLoadout.TryToggle(id))
            {
                ShowToast("Booster could not be changed.", true);
                return;
            }
            RefreshBoosters();
        }

        private void StartSelectedRun()
        {
            Time.timeScale = 1f;
            if (runController == null || !runController.TryStartRun())
            {
                ShowToast("Run could not start. See the Console for the missing reference.", true);
                _animations.PlayInvalidAction(_root.Require<Button>("PlayButton"));
            }
        }

        private void RefreshShop()
        {
            var list = _root.Require<VisualElement>("ShopList");
            list.Clear();
            if (shopCatalog == null || shopCatalog.Items == null || shopCatalog.Items.Count == 0)
            {
                AddMessage(list, "Shop content is not configured.");
                return;
            }
            foreach (var item in shopCatalog.Items)
            {
                if (item == null) continue;
                var ownedId = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
                var owned = _profile != null && _profile.State.Inventory.IsUnlocked(ownedId);
                var price = item.Price.Amount > 0 ? $"{item.Price.Amount:N0} {item.Price.Type}" : "SPECIAL";
                AddCard(list, item.DisplayName, item.Description, owned ? "OWNED" : price, owned ? "OWNED" : "DETAILS", () => OpenShopItem(item), !owned);
            }
        }

        private void OpenShopItem(ShopItemDefinition item)
        {
            OpenModal(item.DisplayName, $"{item.Description}\n\nPrice: {item.Price.Amount:N0} {item.Price.Type}", "PURCHASE", () => PurchaseShopItem(item));
        }

        private void PurchaseShopItem(ShopItemDefinition item)
        {
            if (_shop == null)
            {
                _shopStatus.text = "Shop service is unavailable.";
                CloseModal();
                return;
            }
            var result = _shop.TryPurchase(item.Id);
            _shopStatus.text = result.Success ? $"Purchased {item.DisplayName}." : result.IsPending ? "Purchase pending..." : $"Purchase failed: {result.FailureReason}.";
            CloseModal();
            RefreshShop();
        }

        private void RefreshHangar() { ShowHangarSection(_hangarSection, false); }

        private void ShowHangarSection(HangarSection section, bool animate = true)
        {
            _hangarSection = section;
            SetSelectedTab("HangarShipsTab", section == HangarSection.Ships);
            SetSelectedTab("HangarSkinsTab", section == HangarSection.Skins);
            SetSelectedTab("HangarTrailsTab", section == HangarSection.Trails);
            SetSelectedTab("HangarCoreFxTab", section == HangarSection.CoreFx);
            var list = _root.Require<VisualElement>("HangarList");
            list.Clear();
            if (shipDatabase == null || _profile == null)
            {
                AddMessage(list, "Hangar content is not configured.");
                return;
            }
            switch (section)
            {
                case HangarSection.Ships: RenderUnlockables(list, shipDatabase.Ships, _profile.State.SelectedShipId, id => _profile.Mutate(s => s.SelectedShipId = id)); break;
                case HangarSection.Skins: RenderUnlockables(list, shipDatabase.Skins, _profile.State.SelectedSkinId, id => _profile.Mutate(s => s.SelectedSkinId = id)); break;
                case HangarSection.Trails: RenderUnlockables(list, shipDatabase.Trails, _profile.State.SelectedTrailId, id => _profile.Mutate(s => s.SelectedTrailId = id)); break;
                case HangarSection.CoreFx: RenderUnlockables(list, shipDatabase.CoreFx, _profile.State.SelectedCoreFxId, id => _profile.Mutate(s => s.SelectedCoreFxId = id)); break;
            }
            var ship = shipDatabase.GetShip(_profile.State.SelectedShipId);
            _root.Require<Label>("HangarSelectionTitle").text = ship != null ? ship.DisplayName.ToUpperInvariant() : "NO SHIP";
            _root.Require<Label>("HangarSelectionStatus").text = "EQUIPPED";
            _hangarStatus.text = "Unlocked items can be equipped immediately.";
            if (animate) _animations.ShowScreen(list);
        }

        private void RenderUnlockables<T>(VisualElement list, IList<T> definitions, string selectedId, Action<string> equip) where T : UnlockableDefinition
        {
            if (definitions == null) return;
            foreach (var item in definitions)
            {
                if (item == null) continue;
                var unlocked = _profile.State.Inventory.IsUnlocked(item.Id);
                var selected = item.Id == selectedId;
                AddCard(list, item.DisplayName, item.Price.Amount > 0 ? $"Value {item.Price.Amount:N0} {item.Price.Type}" : "Core Racer equipment", selected ? "EQUIPPED" : unlocked ? "UNLOCKED" : "LOCKED", selected ? "EQUIPPED" : "EQUIP", () =>
                {
                    if (!unlocked) { ShowToast("This item is still locked.", true); return; }
                    equip(item.Id);
                    ShowHangarSection(_hangarSection);
                }, unlocked && !selected);
            }
        }

        private void RefreshLab()
        {
            var list = _root.Require<VisualElement>("LabList");
            list.Clear();
            if (powerupUpgrades == null || _profile == null)
            {
                AddMessage(list, "Lab upgrades are not configured.");
                return;
            }
            foreach (var entry in powerupUpgrades.Upgrades)
            {
                if (entry == null) continue;
                var level = _profile.GetUpgradeLevel(_profile.State.PowerupUpgradeLevels, entry.Type.ToString());
                var max = entry.MaxLevel;
                var cost = entry.GetCostForLevel(level);
                AddCard(list, entry.DisplayName, $"Level {level}/{max}", level >= max ? "MAXIMUM" : $"{cost:N0} COINS", level >= max ? "MAX" : "UPGRADE", () => UpgradePowerup(entry), level < max);
            }
        }

        private void UpgradePowerup(PowerupUpgradeEntry entry)
        {
            var id = entry.Type.ToString();
            var current = _profile.GetUpgradeLevel(_profile.State.PowerupUpgradeLevels, id);
            if (current >= entry.MaxLevel) return;
            var cost = new CurrencyAmount(CurrencyType.Soft, entry.GetCostForLevel(current));
            if (!_profile.TrySpend(cost))
            {
                _labStatus.text = "Not enough coins.";
                _animations.PlayInvalidAction(_root.Require<VisualElement>("LabList"));
                return;
            }
            _profile.SetUpgradeLevel(_profile.State.PowerupUpgradeLevels, id, current + 1);
            _labStatus.text = $"{entry.DisplayName} upgraded to level {current + 1}.";
            _tutorial?.Notify(TutorialStepKind.WaitForUpgradePurchased, "lab");
            RefreshLab();
        }

        private void RefreshProgression()
        {
            if (_profile != null)
                _root.Require<Label>("ProgressionSummary").text = $"Level {_profile.State.Level} / Best {_profile.State.BestScore:N0} / {_profile.State.TotalRuns:N0} runs";
            RefreshDaily();
            RefreshTasks();
            RefreshAchievements();
        }

        private void RefreshDaily()
        {
            var list = _root.Require<VisualElement>("DailyList");
            list.Clear();
            var preview = _dailyRewards?.GetCalendarPreview();
            if (preview == null || preview.Count == 0)
            {
                AddMessage(list, "Daily rewards are not configured.");
                return;
            }
            var current = _dailyRewards.GetCurrentCalendarIndex();
            for (var i = 0; i < preview.Count; i++)
            {
                var reward = preview[i];
                var detail = reward.Rewards.Count > 0 ? $"{reward.Rewards[0].Amount:N0} {reward.Rewards[0].Type}" : "Reward";
                AddCard(list, $"DAY {i + 1}", reward.DisplayName, i == current ? "TODAY" : detail, string.Empty, null, false);
            }
            var canClaim = _dailyRewards.CanClaimToday();
            _root.Require<Button>("ClaimDailyButton").SetEnabled(canClaim);
            _root.Require<Button>("ClaimDailyDoubleButton").SetEnabled(canClaim && (_rewardedAds == null || _rewardedAds.CanShow(AdPlacement.DailyLoginDoubleReward)));
            _root.Require<Label>("DailyStatus").text = canClaim ? "Today's reward is ready." : "Today's reward is already claimed.";
        }

        private void ClaimDaily(bool doubled)
        {
            if (_dailyRewards == null) return;
            if (doubled && _rewardedAds != null)
            {
                _rewardedAds.ShowOrBypass(AdPlacement.DailyLoginDoubleReward, result =>
                {
                    if (_rewardedAds.ShouldGrantReward(result)) _dailyRewards.TryClaim(true);
                    RefreshDaily();
                });
                return;
            }
            if (_dailyRewards.TryClaim(doubled)) ShowToast("Daily reward claimed.");
            RefreshDaily();
        }

        private void RefreshTasks()
        {
            var list = _root.Require<VisualElement>("TaskList");
            list.Clear();
            var rotating = _rotatingTasks?.GetActiveTasks();
            if (rotating != null && rotating.Count > 0)
            {
                foreach (var task in rotating)
                {
                    var claimable = task.Status == RotatingTaskStatus.Completed;
                    AddCard(list, task.DisplayName, task.Description, $"{task.Progress}/{task.Target} / {task.Status}", claimable ? "CLAIM" : task.Status.ToString().ToUpperInvariant(), () => ClaimRotatingTask(task.Id), claimable);
                }
                return;
            }
            var definitions = _progressionTasks?.Definitions;
            if (definitions == null || definitions.Count == 0) { AddMessage(list, "No tasks are active."); return; }
            foreach (var task in definitions)
            {
                if (task == null) continue;
                var progress = _progressionTasks.GetProgress(task);
                var claimed = _progressionTasks.IsClaimed(task.Id);
                var claimable = _progressionTasks.IsComplete(task) && !claimed;
                AddCard(list, task.DisplayName, task.Description, $"{Mathf.Min(progress, task.TargetValue)}/{task.TargetValue}", claimed ? "CLAIMED" : claimable ? "CLAIM" : "IN PROGRESS", () => ClaimProgressionTask(task.Id), claimable);
            }
        }

        private void ClaimRotatingTask(string id) { if (_rotatingTasks != null && _rotatingTasks.TryClaim(id)) ShowToast("Task reward claimed."); RefreshTasks(); }
        private void ClaimProgressionTask(string id) { if (_progressionTasks != null && _progressionTasks.TryClaim(id)) ShowToast("Task reward claimed."); RefreshTasks(); }

        private void RefreshAchievements()
        {
            var list = _root.Require<VisualElement>("AchievementList");
            list.Clear();
            var definitions = _achievements?.Definitions;
            if (definitions == null || definitions.Count == 0) { AddMessage(list, "No achievements are configured."); return; }
            foreach (var achievement in definitions)
            {
                if (achievement == null) continue;
                var progress = _achievements.GetProgress(achievement);
                var claimed = _achievements.IsClaimed(achievement.Id);
                var claimable = _achievements.IsComplete(achievement) && !claimed;
                AddCard(list, achievement.DisplayName, achievement.Description, $"{Mathf.Min(progress, achievement.RequiredValue)}/{achievement.RequiredValue}", claimed ? "CLAIMED" : claimable ? "CLAIM" : "IN PROGRESS", () => ClaimAchievement(achievement.Id), claimable);
            }
        }

        private void ClaimAchievement(string id) { if (_achievements != null && _achievements.TryClaim(id)) ShowToast("Achievement reward claimed."); RefreshAchievements(); }

        private void ShowProgressionPanel(string panelName, string tabName)
        {
            foreach (var name in new[] { "DailyPanel", "TasksPanel", "AchievementsPanel" }) SetVisible(_root.Require<VisualElement>(name), name == panelName);
            foreach (var name in new[] { "DailyTab", "TasksTab", "AchievementsTab" }) SetSelectedTab(name, name == tabName);
        }

        private void RefreshSettings()
        {
            if (!_initialized) return;
            if (_settings != null)
            {
                _root.Require<Slider>("MusicSlider").SetValueWithoutNotify(_settings.State.MusicVolume);
                _root.Require<Slider>("SfxSlider").SetValueWithoutNotify(_settings.State.SfxVolume);
                _root.Require<Toggle>("HapticsToggle").SetValueWithoutNotify(_settings.State.HapticsEnabled);
            }
            if (_accessibility != null)
            {
                var s = _accessibility.State;
                _root.Require<Toggle>("DragControlsToggle").SetValueWithoutNotify(s.DragControlsEnabled);
                _root.Require<Toggle>("ReducedMotionToggle").SetValueWithoutNotify(s.ReducedVfxMode);
                _root.Require<Toggle>("HighContrastToggle").SetValueWithoutNotify(s.HighContrastMode);
                _root.EnableInClassList("theme--high-contrast", s.HighContrastMode);
            }
        }

        private void OnComfortChanged(ComfortSettingsState state)
        {
            if (_animations != null) _animations.ReducedMotion = state.ReducedVfxMode;
            RefreshSettings();
        }

        private void OpenPrivacy()
        {
            OpenModal("PRIVACY CONTROLS", "Privacy preferences are managed by Core Racer's consent service. Open the policy links from the platform release build.", "CLOSE", CloseModal);
        }

        private void OpenSupport()
        {
            var summary = _support != null ? _support.BuildTextBundle(PlayerSupportInfo.Create()) : "Support exporter is not registered.";
            OpenModal("SUPPORT SUMMARY", summary, "CLOSE", CloseModal);
        }

        private void ResetTutorial()
        {
            ResolveServices();
            _tutorial?.ResetForTesting();
            _settingsStatus.text = _tutorial != null ? "Tutorial progress reset." : "Tutorial service is unavailable.";
            ShowToast(_settingsStatus.text, _tutorial == null);
        }

        private void OpenGallery()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            SetVisible(_gallery, true);
            _animations.ShowScreen(_gallery);
#else
            ShowToast("Component gallery is available in development builds only.", true);
#endif
        }

        private void CloseGallery() { if (EnsureReady()) SetVisible(_gallery, false); }

        private void OpenModal(string title, string body, string actionText, Action primary)
        {
            _root.Require<Label>("ModalTitle").text = title;
            _root.Require<Label>("ModalBody").text = body;
            var primaryButton = _root.Require<Button>("ModalPrimaryButton");
            primaryButton.text = actionText;
            _modalPrimaryAction = primary;
            SetVisible(_genericModal, true);
            _animations.ShowPopup(_genericModal);
        }

        private void InvokeModalPrimary() { _modalPrimaryAction?.Invoke(); }
        private void CloseModal() { _modalPrimaryAction = null; if (EnsureReady()) _animations.HidePopup(_genericModal); }

        private void Pause()
        {
            if (runController == null || runController.State != RunState.Running) return;
            runController.PauseRun();
            ShowPause();
        }

        private void Resume()
        {
            Time.timeScale = 1f;
            runController?.ResumeRun();
            HidePause();
        }

        private void ReturnHome()
        {
            Time.timeScale = 1f;
            runController?.ReturnToMenu();
        }

        private void ShowTutorialStep(TutorialStepDefinition step)
        {
            if (step == null) { HideTutorial(); return; }
            _root.Require<Label>("TutorialTitle").text = string.IsNullOrWhiteSpace(step.TitleKey) ? "CORE TRAINING" : step.TitleKey;
            _root.Require<Label>("TutorialBody").text = string.IsNullOrWhiteSpace(step.BodyKey) ? "Follow the highlighted action." : step.BodyKey;
            var button = _root.Require<Button>("TutorialContinueButton");
            button.clicked -= AdvanceTutorial;
            button.clicked += AdvanceTutorial;
            SetVisible(_tutorialOverlay, true);
            _animations.ShowPopup(_tutorialOverlay);
        }

        private void AdvanceTutorial() { _tutorial?.Advance(); }
        private void HideTutorial() { if (_initialized) _animations.HidePopup(_tutorialOverlay); }

        private void UpdateScore(int score) { if (_initialized) _hudScore.text = $"SCORE  {score:N0}"; }
        private void UpdateDistance(int distance) { if (_initialized) _hudDistance.text = $"DIST  {distance:N0} m"; }
        private void UpdateCoins(int coins, int _) { if (_initialized) _hudCoins.text = $"COINS  {coins:N0}"; }
        private void UpdateHealth(float current, float max)
        {
            if (!_initialized) return;
            _hudHealth.text = current >= max ? string.Empty : $"HULL  {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void AddPowerup(PowerupType type, float seconds)
        {
            if (!_initialized) return;
            RemovePowerup(type);
            var label = new Label($"{type.ToString().ToUpperInvariant()}  {seconds:0}s") { name = "Powerup_" + type };
            label.AddToClassList("status-pill");
            label.AddToClassList("status-pill--success");
            _powerupStrip.Add(label);
            _animations.PlaySuccess(label);
        }

        private void RemovePowerup(PowerupType type)
        {
            if (!_initialized) return;
            _powerupStrip.Q<Label>("Powerup_" + type)?.RemoveFromHierarchy();
        }

        private void ShowToast(string message, bool error = false)
        {
            _toast.text = message;
            _toast.EnableInClassList("state--error", error);
            SetVisible(_toast, true);
            _animations.ShowToast(_toast);
            _toast.schedule.Execute(() => SetVisible(_toast, false)).StartingIn(2400);
        }

        private static void AddMessage(VisualElement parent, string text, string stateClass = null)
        {
            var label = new Label(text);
            label.AddToClassList("content-card");
            label.AddToClassList("body-copy");
            if (!string.IsNullOrWhiteSpace(stateClass)) label.AddToClassList(stateClass);
            parent.Add(label);
        }

        private static void AddCard(VisualElement parent, string title, string description, string status, string actionText, Action action, bool enabled = true)
        {
            var card = new VisualElement();
            card.AddToClassList("content-card");
            var titleLabel = new Label(title ?? string.Empty);
            titleLabel.AddToClassList("card-title");
            card.Add(titleLabel);
            if (!string.IsNullOrWhiteSpace(description))
            {
                var descriptionLabel = new Label(description);
                descriptionLabel.AddToClassList("body-copy");
                card.Add(descriptionLabel);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLabel = new Label(status);
                statusLabel.AddToClassList("status-pill");
                card.Add(statusLabel);
            }
            if (!string.IsNullOrWhiteSpace(actionText))
            {
                var button = new Button(action) { text = actionText };
                button.AddToClassList("secondary-button");
                button.SetEnabled(enabled);
                card.Add(button);
            }
            parent.Add(card);
        }

        private void SetSelectedTab(string name, bool selected) { _root.Require<Button>(name).EnableInClassList("is-selected", selected); }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element == null) return;
            element.EnableInClassList("is-hidden", !visible);
            element.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
        }

        private static void BlockGameplayInput(VisualElement element)
        {
            element.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
            element.RegisterCallback<PointerMoveEvent>(e => e.StopPropagation());
            element.RegisterCallback<PointerUpEvent>(e => e.StopPropagation());
            element.RegisterCallback<KeyDownEvent>(e => e.StopPropagation());
        }

        private bool EnsureReady()
        {
            if (_initialized) return true;
            Initialize();
            return _initialized;
        }
    }
}
