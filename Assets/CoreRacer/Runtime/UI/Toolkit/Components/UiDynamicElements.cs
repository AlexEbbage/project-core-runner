using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoreRacer.UI.Toolkit
{
    public static class UiDynamicElements
    {
        public static Label EmptyState(string message)
        {
            var label = new Label(message ?? string.Empty);
            label.AddToClassList("empty-state");
            return label;
        }

        public static VisualElement CreateIcon(Sprite sprite, string fallback, string className)
        {
            var icon = new VisualElement();
            icon.AddToClassList("dynamic-icon");
            if (!string.IsNullOrWhiteSpace(className))
                icon.AddToClassList(className);

            if (sprite != null)
            {
                icon.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                var label = new Label(fallback ?? string.Empty);
                label.AddToClassList("dynamic-icon__fallback");
                icon.Add(label);
            }
            return icon;
        }
    }

    public sealed class BoosterTileElement : VisualElement
    {
        private readonly VisualElement _iconHost;
        private readonly Label _quantity;
        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _price;
        private readonly Button _action;
        private Action _actionCallback;

        public BoosterTileElement()
        {
            AddToClassList("booster-tile");
            var top = new VisualElement();
            top.AddToClassList("booster-tile__top");
            _iconHost = new VisualElement();
            _iconHost.AddToClassList("booster-tile__icon-host");
            _quantity = new Label("0");
            _quantity.AddToClassList("quantity-badge");
            _iconHost.Add(_quantity);
            top.Add(_iconHost);
            var copy = new VisualElement();
            copy.AddToClassList("booster-tile__copy");
            _title = new Label();
            _title.AddToClassList("booster-tile__title");
            _description = new Label();
            _description.AddToClassList("booster-tile__description");
            copy.Add(_title);
            copy.Add(_description);
            top.Add(copy);
            Add(top);
            _price = new Label();
            _price.AddToClassList("booster-tile__price");
            Add(_price);
            _action = new Button(InvokeAction);
            _action.AddToClassList("button");
            _action.AddToClassList("button--compact");
            Add(_action);
        }

        public void Bind(string title, string description, Sprite icon, string fallbackIcon, int quantity, string price, string actionText, Action action, bool enabled, bool equipped)
        {
            _iconHost.Q<VisualElement>(className: "dynamic-icon")?.RemoveFromHierarchy();
            var iconElement = UiDynamicElements.CreateIcon(icon, fallbackIcon, "booster-tile__icon");
            _iconHost.Insert(0, iconElement);
            _quantity.text = Mathf.Max(0, quantity).ToString();
            _title.text = title ?? string.Empty;
            _description.text = description ?? string.Empty;
            _price.text = price ?? string.Empty;
            _action.text = actionText ?? string.Empty;
            _actionCallback = action;
            _action.SetEnabled(enabled);
            _action.EnableInClassList("button--primary", !equipped);
            _action.EnableInClassList("button--success", equipped);
            EnableInClassList(UiClassNames.Equipped, equipped);
        }

        private void InvokeAction()
        {
            _actionCallback?.Invoke();
        }
    }

    public sealed class ShopItemTileElement : VisualElement
    {
        private readonly VisualElement _icon;
        private readonly Label _badge;
        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _price;
        private readonly Button _action;
        private Action _actionCallback;

        public ShopItemTileElement()
        {
            AddToClassList("shop-item");
            _badge = new Label();
            _badge.AddToClassList("shop-item__badge");
            Add(_badge);
            _icon = new VisualElement();
            _icon.AddToClassList("shop-item__icon");
            Add(_icon);
            _title = new Label();
            _title.AddToClassList("shop-item__title");
            Add(_title);
            _description = new Label();
            _description.AddToClassList("shop-item__description");
            Add(_description);
            _price = new Label();
            _price.AddToClassList("shop-item__price");
            Add(_price);
            _action = new Button(InvokeAction);
            _action.AddToClassList("button");
            _action.AddToClassList("button--compact");
            _action.AddToClassList("button--primary");
            Add(_action);
        }

        public void Bind(Sprite icon, string title, string description, string price, string badge, string actionText, Action action, bool enabled, bool owned)
        {
            if (icon != null)
                _icon.style.backgroundImage = new StyleBackground(icon);
            else
                _icon.style.backgroundImage = StyleKeyword.None;
            _icon.EnableInClassList("has-image", icon != null);
            _title.text = title ?? string.Empty;
            _description.text = description ?? string.Empty;
            _price.text = price ?? string.Empty;
            _badge.text = badge ?? string.Empty;
            _badge.EnableInClassList(UiClassNames.Hidden, string.IsNullOrWhiteSpace(badge));
            _action.text = actionText ?? string.Empty;
            _actionCallback = action;
            _action.SetEnabled(enabled);
            EnableInClassList(UiClassNames.Claimed, owned);
        }

        private void InvokeAction()
        {
            _actionCallback?.Invoke();
        }
    }

    public sealed class ActionListItemElement : VisualElement
    {
        private readonly VisualElement _icon;
        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _status;
        private readonly ProgressBar _progress;
        private readonly Button _action;
        private Action _actionCallback;

        public ActionListItemElement()
        {
            AddToClassList("action-list-item");
            _icon = new VisualElement();
            _icon.AddToClassList("action-list-item__icon");
            Add(_icon);
            var copy = new VisualElement();
            copy.AddToClassList("action-list-item__copy");
            _title = new Label();
            _title.AddToClassList("action-list-item__title");
            _description = new Label();
            _description.AddToClassList("action-list-item__description");
            _progress = new ProgressBar { lowValue = 0f, highValue = 1f };
            _progress.AddToClassList("progress-meter");
            copy.Add(_title);
            copy.Add(_description);
            copy.Add(_progress);
            Add(copy);
            var trailing = new VisualElement();
            trailing.AddToClassList("action-list-item__trailing");
            _status = new Label();
            _status.AddToClassList("status-text");
            _action = new Button(InvokeAction);
            _action.AddToClassList("button");
            _action.AddToClassList("button--compact");
            trailing.Add(_status);
            trailing.Add(_action);
            Add(trailing);
        }

        public void Bind(Sprite icon, string title, string description, string status, float progress, string actionText, Action action, bool enabled, string stateClass = null)
        {
            if (icon != null)
                _icon.style.backgroundImage = new StyleBackground(icon);
            else
                _icon.style.backgroundImage = StyleKeyword.None;
            _icon.EnableInClassList("has-image", icon != null);
            _title.text = title ?? string.Empty;
            _description.text = description ?? string.Empty;
            _status.text = status ?? string.Empty;
            _progress.value = Mathf.Clamp01(progress);
            _progress.EnableInClassList(UiClassNames.Hidden, progress < 0f);
            _action.text = actionText ?? string.Empty;
            _actionCallback = action;
            _action.SetEnabled(enabled);
            _action.EnableInClassList(UiClassNames.Hidden, string.IsNullOrWhiteSpace(actionText));
            if (!string.IsNullOrWhiteSpace(stateClass))
                AddToClassList(stateClass);
        }

        private void InvokeAction()
        {
            _actionCallback?.Invoke();
        }
    }
}
