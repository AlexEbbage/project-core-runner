using CoreRacer.Meta.Boosters;
using CoreRacer.Meta.Economy;
using UnityEngine;

namespace CoreRacer.UI.Toolkit
{
    public sealed class PlayScreenPresenter : UiScreenPresenterBase
    {
        private readonly PlayScreenView _view;
        private readonly CoreRacerUiContext _context;
        private readonly IUiAnimationService _animations;
        private readonly UiToastService _toast;
        private int _selectedIndex;

        public PlayScreenPresenter(PlayScreenView view, CoreRacerUiContext context, IUiAnimationService animations, UiToastService toast)
            : base(CoreRacerScreenId.Play, view.Root, animations)
        {
            _view = view;
            _context = context;
            _animations = animations;
            _toast = toast;
        }

        protected override void OnInitialize()
        {
            _view.PreviousButton.clicked += Previous;
            _view.NextButton.clicked += Next;
            _view.StartButton.clicked += Start;
            if (_context.Profile != null)
                _context.Profile.Changed += Refresh;
        }

        protected override void OnDispose()
        {
            _view.PreviousButton.clicked -= Previous;
            _view.NextButton.clicked -= Next;
            _view.StartButton.clicked -= Start;
            if (_context.Profile != null)
                _context.Profile.Changed -= Refresh;
        }

        public override void Refresh()
        {
            if (_context.LevelRoadmap == null || _context.LevelRoadmap.Levels == null || _context.LevelRoadmap.Levels.Count == 0)
            {
                RenderConfigurationError();
                return;
            }

            Select(Mathf.Clamp(_selectedIndex, 0, 1), false);
            RenderBoosters();
        }

        private void Previous() => Select(_selectedIndex - 1, true);
        private void Next() => Select(_selectedIndex + 1, true);

        private void Select(int index, bool animate)
        {
            _selectedIndex = Mathf.Clamp(index, 0, 1);
            UiVisibility.SetAvailable(_view.PreviousButton, _selectedIndex > 0, true);
            UiVisibility.SetAvailable(_view.NextButton, _selectedIndex < 1, true);

            var playable = _selectedIndex == 0;
            _view.LevelSurface.EnableInClassList(UiClassNames.ComingSoon, !playable);
            _view.StartButton.SetEnabled(playable);

            if (playable)
            {
                var level = _context.LevelRoadmap.Levels[0];
                if (level == null)
                {
                    RenderConfigurationError();
                    return;
                }

                _context.RunController?.SetSelectedLevel(level);
                _view.LevelTitle.text = "CORE RUN";
                _view.LevelDescription.text = "Survive, collect cores, and push your best score.";
                _view.LevelStatus.text = "READY";
                _view.LevelStatus.EnableInClassList(UiClassNames.Success, true);
                RenderScoreAndRewards();
            }
            else
            {
                _view.LevelTitle.text = "NEXT ZONE";
                _view.LevelDescription.text = "Coming soon... complete the Core Run while the next zone is prepared.";
                _view.LevelStatus.text = "COMING SOON";
                _view.LevelStatus.EnableInClassList(UiClassNames.Success, false);
                _view.HighScore.text = "---";
                _view.SetStars(0);
                _view.SetReward(1, false, false, "COMING SOON");
                _view.SetReward(2, false, false, "COMING SOON");
                _view.SetReward(3, false, false, "COMING SOON");
            }

            if (animate)
                _animations.ShowScreen(_view.LevelSurface);
        }

        private void RenderConfigurationError()
        {
            _view.LevelTitle.text = "NO CORE RUN";
            _view.LevelDescription.text = "No valid MVP route is configured.";
            _view.LevelStatus.text = "CONFIGURATION ERROR";
            _view.StartButton.SetEnabled(false);
        }

        private void RenderScoreAndRewards()
        {
            var best = _context.Profile != null ? _context.Profile.State.BestScore : 0;
            var stars = best >= 5000 ? 3 : best >= 2500 ? 2 : best >= 500 ? 1 : 0;
            _view.HighScore.text = best.ToString("N0");
            _view.SetStars(stars);
            _view.SetReward(1, stars >= 1, stars == 0, stars >= 1 ? "CLAIMED" : "AT 500");
            _view.SetReward(2, stars >= 2, stars == 1, stars >= 2 ? "CLAIMED" : "AT 2,500");
            _view.SetReward(3, stars >= 3, stars == 2, stars >= 3 ? "CLAIMED" : "AT 5,000");
        }

        private void RenderBoosters()
        {
            _view.BoosterList.Clear();
            var catalog = _context.BoosterCatalog;
            if (catalog == null || catalog.Boosters == null || catalog.Boosters.Count == 0)
            {
                _view.BoosterList.Add(UiDynamicElements.EmptyState("No boosters are configured."));
                return;
            }

            var equipped = 0;
            for (var i = 0; i < catalog.Boosters.Count; i++)
            {
                var booster = catalog.Boosters[i];
                if (booster == null)
                    continue;

                var isEquipped = _context.BoosterLoadout != null && _context.BoosterLoadout.IsEquipped(booster.Id);
                var owned = IsOwned(booster);
                if (isEquipped)
                    equipped++;

                var tile = new BoosterTileElement();
                tile.Bind(
                    booster.DisplayName.ToUpperInvariant(),
                    Description(booster),
                    booster.Icon,
                    booster.EffectType == BoosterEffectType.StartShield ? "S" : booster.EffectType == BoosterEffectType.CoinMultiplier ? "C" : "2X",
                    owned ? 1 : 0,
                    owned ? string.Empty : $"{booster.Price.Amount:N0} {(booster.Price.Type == CurrencyType.Premium ? "SHARDS" : "CREDITS")}",
                    isEquipped ? "EQUIPPED" : owned ? "EQUIP" : "BUY & EQUIP",
                    () => Toggle(booster.Id),
                    true,
                    isEquipped);
                _view.BoosterList.Add(tile);
            }

            _view.BoosterSummary.text = equipped == 0
                ? "Choose up to one booster from each family."
                : $"{equipped} booster {(equipped == 1 ? "family" : "families")} equipped";
        }

        private bool IsOwned(BoosterDefinition booster)
        {
            return booster != null &&
                   (booster.Price.Amount <= 0 ||
                    _context.Profile == null ||
                    _context.Profile.State.Inventory.IsUnlocked(booster.Id) ||
                    _context.Profile.State.EquippedBoosterIds.Contains(booster.Id));
        }

        private void Toggle(string id)
        {
            var booster = _context.BoosterCatalog != null ? _context.BoosterCatalog.Get(id) : null;
            var profile = _context.Profile;
            if (booster == null || profile == null)
            {
                _toast.Show("Booster could not be found.", true);
                return;
            }

            if (!IsOwned(booster))
            {
                var purchased = profile.TryMutate(state =>
                {
                    if (!state.Wallet.TrySpend(booster.Price))
                        return false;
                    state.Inventory.Unlock(booster.Id);
                    return true;
                });
                if (!purchased)
                {
                    _toast.Show("Not enough currency for this booster.", true);
                    _animations.PlayInvalidAction(_view.BoosterList);
                    return;
                }
            }

            if (_context.BoosterLoadout == null || !_context.BoosterLoadout.TryToggle(id))
            {
                _toast.Show("Booster could not be changed.", true);
                return;
            }

            RenderBoosters();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (_selectedIndex != 0)
            {
                _toast.Show("This zone is coming soon.", true);
                _animations.PlayInvalidAction(_view.StartButton);
                return;
            }

            if (_context.RunController == null || !_context.RunController.TryStartRun())
            {
                _toast.Show("Run could not start. See the Console for the missing reference.", true);
                _animations.PlayInvalidAction(_view.StartButton);
            }
        }

        private static string Description(BoosterDefinition booster)
        {
            switch (booster.EffectType)
            {
                case BoosterEffectType.StartShield: return "Start protected by a temporary shield.";
                case BoosterEffectType.CoinMultiplier: return $"Earn {booster.Value:0.#}x credits for this run.";
                case BoosterEffectType.ScoreMultiplier: return $"Score {booster.Value:0.#}x points for this run.";
                default: return "Equip this booster for the next Core Run.";
            }
        }
    }
}
