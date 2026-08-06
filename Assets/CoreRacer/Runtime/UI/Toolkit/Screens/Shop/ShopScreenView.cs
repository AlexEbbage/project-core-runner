using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public sealed class ShopScreenView
    {
        public ShopScreenView(VisualElement root)
        {
            Root = root;
            Status = root.Require<Label>("ShopStatus");
            FeaturedOffer = root.Require<VisualElement>("FeaturedOffer");
            FeaturedBadge = root.Require<Label>("FeaturedBadge");
            FeaturedTitle = root.Require<Label>("FeaturedTitle");
            FeaturedDescription = root.Require<Label>("FeaturedDescription");
            FeaturedPrice = root.Require<Label>("FeaturedPrice");
            FeaturedAction = root.Require<Button>("FeaturedActionButton");
            ItemList = root.Require<VisualElement>("ShopList");
            FeaturedTab = root.Require<Button>("ShopFeaturedTab");
            BoostersTab = root.Require<Button>("ShopBoostersTab");
            CoresTab = root.Require<Button>("ShopCoresTab");
            CurrencyTab = root.Require<Button>("ShopCurrencyTab");
        }

        public VisualElement Root { get; }
        public Label Status { get; }
        public VisualElement FeaturedOffer { get; }
        public Label FeaturedBadge { get; }
        public Label FeaturedTitle { get; }
        public Label FeaturedDescription { get; }
        public Label FeaturedPrice { get; }
        public Button FeaturedAction { get; }
        public VisualElement ItemList { get; }
        public Button FeaturedTab { get; }
        public Button BoostersTab { get; }
        public Button CoresTab { get; }
        public Button CurrencyTab { get; }

        public void SetTab(string tab)
        {
            FeaturedTab.EnableInClassList(UiClassNames.Selected, tab == "featured");
            BoostersTab.EnableInClassList(UiClassNames.Selected, tab == "boosters");
            CoresTab.EnableInClassList(UiClassNames.Selected, tab == "cores");
            CurrencyTab.EnableInClassList(UiClassNames.Selected, tab == "currency");
        }
    }
}
