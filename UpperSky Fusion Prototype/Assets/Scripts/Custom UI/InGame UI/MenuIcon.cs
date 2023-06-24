using Entity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Custom_UI.InGame_UI
{
    public class MenuIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        protected UIManager UIManager;
        [Expandable] public EntityData data;

        private bool _isLocked;
        
        protected Button MyBtn;

        protected virtual void Start()
        {
            UIManager = UIManager.Instance;
            MyBtn = GetComponent<Button>();

            if (data.StartAsLocked)
            {
                MyBtn.interactable = false;
                _isLocked = true;
            }
        }

        public void Unlock()
        {
            MyBtn.interactable = true;
            _isLocked = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            UIManager.ShowInfobox(data, _isLocked);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.HideInfobox();
        }
    }
}
