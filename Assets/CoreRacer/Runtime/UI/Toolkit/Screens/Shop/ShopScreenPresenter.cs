using System;
using CoreRacer.Meta.Shop;

namespace CoreRacer.UI.Toolkit
{
    public sealed class ShopScreenPresenter : UiScreenPresenterBase
    {
        private readonly ShopScreenView _view;
        private readonly CoreRacerUiContext _context;
        private readonly UiModalService _modal;
        private readonly UiToastService _toast;
        private string _tab = "featured";
        private ShopItemDefinition _featured;

        public ShopScreenPresenter(ShopScreenView view, CoreRacerUiContext context, IUiAnimationService animations, UiModalService modal, UiToastService toast)
            : base(CoreRacerScreenId.Shop, view.Root, animations)
        {
            _view = view;
            _context = context;
            _modal = modal;
            _toast = toast;
        }

        protected override void OnInitialize()
        {
            _view.FeaturedTab.clicked += ShowFeatured;
            _view.BoostersTab.clicked += ShowBoosters;
            _view.CoresTab.clicked += ShowCores;
            _view.CurrencyTab.clicked += ShowCurrency;
            _view.FeaturedAction.clicked += PurchaseFeatured;
            if (_context.Profile != null)
                _context.Profile.Changed += Refresh;
        }

        protected override void OnDispose()
        {
            _view.FeaturedTab.clicked -= ShowFeatured;
            _view.BoostersTab.clicked -= ShowBoosters;
            _view.CoresTab.clicked -= ShowCores;
            _view.CurrencyTab.clicked -= ShowCurrency;
            _view.FeaturedAction.clicked -= PurchaseFeatured;
            if (_context.Profile != null)
                _context.Profile.Changed -= Refresh;
        }

        public override void Refresh()
        {
            _view.SetTab(_tab);
            _view.ItemList.Clear();
            var catalog = _context.ShopCatalog;
            if (catalog == null || catalog.Items == null || catalog.Items.Count == 0)
            {
                UiVisibility.SetVisible(_view.FeaturedOffer, false);
                _view.ItemList.Add(UiDynamicElements.EmptyState("Shop content is not configured."));
                return;
            }

            _featured = FindFeatured(catalog);
            RenderFeatured(_featured);
            for (var i = 0; i < catalog.Items.Count; i++)
            {
                var item = catalog.Items[i];
                if (item == null || ReferenceEquals(item, _featured) || !MatchesTab(item))
                    continue;
                RenderItem(item);
            }

            if (_view.ItemList.childCount == 0)
                _view.ItemList.Add(UiDynamicElements.EmptyState("No items are available in this category yet."));
        }

        private void RenderFeatured(ShopItemDefinition item)
        {
            if (item == null || (_tab != "featured" && !MatchesTab(item)))
            {
                UiVisibility.SetVisible(_view.FeaturedOffer, false);
                return;
            }

            UiVisibility.SetVisible(_view.FeaturedOffer, true);
            var owned = IsOwned(item);
            _view.FeaturedBadge.text = item.IsFeatured ? "FEATURED" : "CORE RACER PICK";
            _view.FeaturedTitle.text = item.DisplayName?.ToUpperInvariant() ?? "FEATURED OFFER";
            _view.FeaturedDescription.text = item.Description ?? string.Empty;
            _view.FeaturedPrice.text = owned ? "OWNED" : FormatPrice(item);
            _view.FeaturedAction.text = owned ? "OWNED" : "VIEW OFFER";
            _view.FeaturedAction.SetEnabled(!owned);
        }

        private void RenderItem(ShopItemDefinition item)
        {
            var owned = IsOwned(item);
            var tile = new ShopItemTileElement();
            tile.Bind(
                item.Icon,
                item.DisplayName,
                item.Description,
                owned ? "OWNED" : FormatPrice(item),
                item.IsFeatured ? "FEATURED" : string.Empty,
                owned ? "OWNED" : "DETAILS",
                () => OpenDetails(item),
                !owned,
                owned);
            _view.ItemList.Add(tile);
        }

        private void OpenDetails(ShopItemDefinition item)
        {
            _modal.Open(
                item.DisplayName,
                $"{item.Description}\n\nPrice: {FormatPrice(item)}",
                "PURCHASE",
                () => Purchase(item));
        }

        private void PurchaseFeatured()
        {
            if (_featured != null)
                OpenDetails(_featured);
        }

        private void Purchase(ShopItemDefinition item)
        {
            if (_context.Shop == null)
            {
                _view.Status.text = "Shop service is unavailable.";
                _toast.Show(_view.Status.text, true);
                _modal.Close();
                return;
            }

            var result = _context.Shop.TryPurchase(item.Id);
            _view.Status.text = result.Success
                ? $"Purchased {item.DisplayName}."
                : result.IsPending
                    ? "Purchase pending..."
                    : $"Purchase failed: {result.FailureReason}.";
            _toast.Show(_view.Status.text, !result.Success && !result.IsPending);
            _modal.Close();
            Refresh();
        }

        private bool IsOwned(ShopItemDefinition item)
        {
            if (_context.Profile == null || item == null)
                return false;
            var id = string.IsNullOrWhiteSpace(item.GrantItemId) ? item.Id : item.GrantItemId;
            return _context.Profile.State.Inventory.IsUnlocked(id);
        }

        private bool MatchesTab(ShopItemDefinition item)
        {
            if (_tab == "featured")
                return true;
            var text = $"{item.Id} {item.DisplayName}".ToLowerInvariant();
            if (_tab == "boosters")
                return text.Contains("boost") || text.Contains("shield");
            if (_tab == "cores")
                return text.Contains("core") || text.Contains("ship") || text.Contains("skin") || text.Contains("trail");
            if (_tab == "currency")
                return item.Kind == ShopItemKind.CurrencyPack || item.Kind == ShopItemKind.PremiumUser || item.Kind == ShopItemKind.RestorePurchases;
            return true;
        }

        private static ShopItemDefinition FindFeatured(ShopCatalog catalog)
        {
            for (var i = 0; i < catalog.Items.Count; i++)
                if (catalog.Items[i] != null && catalog.Items[i].IsFeatured)
                    return catalog.Items[i];
            return catalog.Items.Count > 0 ? catalog.Items[0] : null;
        }

        private static string FormatPrice(ShopItemDefinition item)
        {
            return item.Price.Amount > 0 ? $"{item.Price.Amount:N0} {item.Price.Type}" : "SPECIAL";
        }

        private void ShowFeatured() { _tab = "featured"; Refresh(); }
        private void ShowBoosters() { _tab = "boosters"; Refresh(); }
        private void ShowCores() { _tab = "cores"; Refresh(); }
        private void ShowCurrency() { _tab = "currency"; Refresh(); }
    }
}
