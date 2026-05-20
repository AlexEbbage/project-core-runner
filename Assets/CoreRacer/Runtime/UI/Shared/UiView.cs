using UnityEngine;

namespace CoreRacer.UI.Shared
{
    public class UiView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        protected GameObject Root => root != null ? root : gameObject;
        public virtual void Show() => Root.SetActive(true);
        public virtual void Hide() => Root.SetActive(false);
    }
}
