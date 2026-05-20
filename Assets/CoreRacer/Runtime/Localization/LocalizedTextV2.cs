using CoreRacer.Bootstrap;
using CoreRacer.UI.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CoreRacer.Localization
{
    public sealed class LocalizedTextV2 : MonoBehaviour
    {
        [SerializeField] private string key;
        [SerializeField] private Text target;

        private void Awake()
        {
            if (target == null) target = GetComponent<Text>();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (GameServices.TryGet<LocalizationServiceV2>(out var localization))
                UiTextBinder.SetText(target, localization.Get(key));
            else
                UiTextBinder.SetText(target, key);
        }
    }
}
