using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreRacer.UI
{
    public sealed class UiInteractionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public event Action Hovered;
        public event Action Unhovered;
        public event Action Clicked;

        public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => Unhovered?.Invoke();
        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke();
    }
}
