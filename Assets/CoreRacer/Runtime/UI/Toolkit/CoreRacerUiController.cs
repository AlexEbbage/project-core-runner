using System;
using System.Collections.Generic;
using CoreRacer.Bootstrap;
using CoreRacer.Gameplay.Powerups;
using CoreRacer.Gameplay.Run;
using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.Levels;
using CoreRacer.Meta.Ships;
using CoreRacer.Meta.Shop;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    /// <summary>
    /// Runtime UI composition root. Structure lives in UXML, presentation in USS, feature behaviour in
    /// screen-specific views/presenters, and application state remains in Core Racer services.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class CoreRacerUiController : MonoBehaviour, IRunUiPresenter
    {
        [Header("Composition")]
        [SerializeField] private UIDocument document;
        [SerializeField] private RunController runController;
        [SerializeField] private RunSceneReferences runReferences;
        [SerializeField] private UiAnimationSettings animationSettings;

        [Header("Content")]
        [SerializeField] private LevelRoadmapConfigV2 levelRoadmap;
        [SerializeField] private BoosterCatalog boosterCatalog;
        [SerializeField] private ShipDatabase shipDatabase;
        [SerializeField] private ShopCatalog shopCatalog;
        [SerializeField] private PowerupUpgradeConfigV2 powerupUpgrades;

        private VisualElement _root;
        private VisualElement _gameRoot;
        private VisualElement _safeArea;
        private VisualElement _screenLayer;
        private VisualElement _mainMenu;
        private VisualElement _hudLayer;
        private IUiAnimationService _animations;
        private CoreRacerUiContext _context;
        private CoreRacerScreenRouter _router;
        private MenuShellPresenter _shell;
        private GameplayHudPresenter _hud;
        private RunOverlayPresenter _runOverlays;
        private UiModalService _modal;
        private UiToastService _toast;
        private ComponentGalleryPresenter _gallery;
        private UiSafeAreaController _safeAreaController;
        private bool _initialized;
        private bool _rebuildQueued;

        public bool IsInitialized => _initialized;
        public CoreRacerScreenId CurrentScreen => _router != null ? _router.Current : CoreRacerScreenId.Play;
        public bool IsModalOpen => _modal != null && _modal.IsOpen;

        private void Awake()
        {
            if (document == null)
                document = GetComponent<UIDocument>();
            if (runController == null)
                runController = FindObjectOfType<RunController>(true);
            if (runReferences == null)
                runReferences = FindObjectOfType<RunSceneReferences>(true);
        }

        private void OnEnable()
        {
            GameServices.RegistryChanged += OnServiceRegistryChanged;
            Initialize();
        }

        private void OnDisable()
        {
            GameServices.RegistryChanged -= OnServiceRegistryChanged;
            DisposeUi();
        }

        public void Initialize()
        {
            if (_initialized)
                return;
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

            try
            {
                _gameRoot = _root.Require<VisualElement>("GameUiRoot");
                _safeArea = _root.Require<VisualElement>("SafeArea");
                _screenLayer = _root.Require<VisualElement>("ScreenLayer");
                _mainMenu = _root.Require<VisualElement>("MainMenuScreen");
                _hudLayer = _root.Require<VisualElement>("HudLayer");

                _context = new CoreRacerUiContext(
                    runController,
                    runReferences,
                    levelRoadmap,
                    boosterCatalog,
                    shipDatabase,
                    shopCatalog,
                    powerupUpgrades);

                _animations = new LitMotionUiAnimationService(animationSettings)
                {
                    ReducedMotion = _context.Accessibility != null && _context.Accessibility.State.ReducedVfxMode
                };
                _toast = new UiToastService(_root.Require<Label>("Toast"), _animations);
                _modal = new UiModalService(_root.Require<VisualElement>("GenericModal"), _animations);
                _gallery = new ComponentGalleryPresenter(_root.Require<VisualElement>("ComponentGallery"), _animations);

                var play = new PlayScreenPresenter(
                    new PlayScreenView(_root.Require<VisualElement>("PlayScreen")),
                    _context,
                    _animations,
                    _toast);
                var shop = new ShopScreenPresenter(
                    new ShopScreenView(_root.Require<VisualElement>("ShopScreen")),
                    _context,
                    _animations,
                    _modal,
                    _toast);
                var hangar = new HangarScreenPresenter(
                    new HangarScreenView(_root.Require<VisualElement>("HangarScreen")),
                    _context,
                    _animations,
                    _toast);
                var lab = new LabScreenPresenter(
                    new LabScreenView(_root.Require<VisualElement>("LabScreen")),
                    _context,
                    _animations,
                    _toast);
                var progression = new ProgressionScreenPresenter(
                    new ProgressionScreenView(_root.Require<VisualElement>("ProgressionScreen")),
                    _context,
                    _animations,
                    _toast);
                var settings = new SettingsScreenPresenter(
                    new SettingsScreenView(_root.Require<VisualElement>("SettingsScreen")),
                    _context,
                    _animations,
                    _modal,
                    _toast,
                    _gallery.Open);

                var shellView = new MenuShellView(_mainMenu);
                _router = new CoreRacerScreenRouter(
                    new Dictionary<CoreRacerScreenId, IUiScreenPresenter>
                    {
                        [CoreRacerScreenId.Play] = play,
                        [CoreRacerScreenId.Shop] = shop,
                        [CoreRacerScreenId.Hangar] = hangar,
                        [CoreRacerScreenId.Lab] = lab,
                        [CoreRacerScreenId.Progression] = progression,
                        [CoreRacerScreenId.Settings] = settings
                    },
                    shellView.Navigation);
                _shell = new MenuShellPresenter(shellView, _context.Profile, _router);
                _shell.Initialize();

                _hud = new GameplayHudPresenter(new GameplayHudView(_hudLayer), _context, _animations);
                _hud.Initialize();
                _runOverlays = new RunOverlayPresenter(new RunOverlayView(_root), _context, _animations);
                _runOverlays.Initialize();

                _safeAreaController = new UiSafeAreaController(_safeArea);
                _gameRoot.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                ConfigurePointerRouting();
                _initialized = true;
                _router.Show(CoreRacerScreenId.Play);
                ShowMainMenu();
                _safeAreaController.Refresh();
                Debug.Log("[CoreRacer.UI] Final modular UI Toolkit root initialized.", this);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                DisposeUi();
            }
        }

        public void ShowMainMenu()
        {
            if (!EnsureReady())
                return;
            UiVisibility.SetVisible(_screenLayer, true, false);
            UiVisibility.SetVisible(_mainMenu, true);
            _hud.Hide();
            _runOverlays.HidePause();
            _runOverlays.HideGameOver();
            _shell.Refresh();
            _router.RefreshCurrent();
            _animations.ShowScreen(_mainMenu);
        }

        public void HideMainMenu()
        {
            if (!EnsureReady())
                return;
            _animations.Stop(_mainMenu);
            UiVisibility.SetVisible(_mainMenu, false);
            UiVisibility.SetVisible(_screenLayer, false, false);
        }

        public void ShowHud()
        {
            if (!EnsureReady())
                return;
            _hud.Reset();
            _hud.Show();
        }

        public void HideHud()
        {
            if (EnsureReady())
                _hud.Hide();
        }

        public void ShowPause()
        {
            if (EnsureReady())
                _runOverlays.ShowPause();
        }

        public void HidePause()
        {
            if (EnsureReady())
                _runOverlays.HidePause();
        }

        public void ShowContinueOffer()
        {
            if (EnsureReady())
                _runOverlays.ShowContinueOffer();
        }

        public void HideGameOver()
        {
            if (EnsureReady())
                _runOverlays.HideGameOver();
        }

        public void ShowContinueUnavailable()
        {
            if (EnsureReady())
                _runOverlays.ShowContinueUnavailable();
        }

        public void SetContinuePending(bool pending)
        {
            if (EnsureReady())
                _runOverlays.SetContinuePending(pending);
        }

        public void ShowGameOver(RunResult result)
        {
            if (EnsureReady())
                _runOverlays.ShowGameOver(result);
        }

        public void SetDoubleRewardPending(bool pending)
        {
            if (EnsureReady())
                _runOverlays.SetDoubleRewardPending(pending);
        }

        public void ShowDoubleRewardUnavailable()
        {
            if (EnsureReady())
                _runOverlays.ShowDoubleRewardUnavailable();
        }

        public void ShowDoubleRewardGranted(RunResult bonus)
        {
            if (!EnsureReady())
                return;
            _runOverlays.ShowDoubleRewardGranted(bonus);
            _shell.Refresh();
        }

        private void ConfigurePointerRouting()
        {
            _root.pickingMode = PickingMode.Ignore;
            _gameRoot.pickingMode = PickingMode.Ignore;
            _safeArea.pickingMode = PickingMode.Ignore;
            _root.Require<VisualElement>("OverlayLayer").pickingMode = PickingMode.Ignore;
            _root.Require<VisualElement>("PopupLayer").pickingMode = PickingMode.Ignore;
            _root.Require<VisualElement>("ToastLayer").pickingMode = PickingMode.Ignore;
            _root.Require<VisualElement>("EffectsLayer").pickingMode = PickingMode.Ignore;
            _root.Require<VisualElement>("LoadingLayer").pickingMode = PickingMode.Ignore;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _safeAreaController?.Refresh();
        }

        private void OnServiceRegistryChanged(CoreRacer.Common.ServiceRegistry registry)
        {
            if (!_initialized || !isActiveAndEnabled || _rebuildQueued)
                return;
            _rebuildQueued = true;
            _root.schedule.Execute(() =>
            {
                _rebuildQueued = false;
                if (!isActiveAndEnabled)
                    return;
                DisposeUi();
                Initialize();
            });
        }

        private bool EnsureReady()
        {
            if (_initialized)
                return true;
            Initialize();
            return _initialized;
        }

        private void DisposeUi()
        {
            if (_gameRoot != null)
                _gameRoot.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _shell?.Dispose();
            _router?.Dispose();
            _hud?.Dispose();
            _runOverlays?.Dispose();
            _modal?.Dispose();
            _gallery?.Dispose();
            _toast?.Dispose();
            _animations?.StopAll();

            _shell = null;
            _router = null;
            _hud = null;
            _runOverlays = null;
            _modal = null;
            _gallery = null;
            _toast = null;
            _animations = null;
            _context = null;
            _safeAreaController = null;
            _initialized = false;
        }
    }
}
