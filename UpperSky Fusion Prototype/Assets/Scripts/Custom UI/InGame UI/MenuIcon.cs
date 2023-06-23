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

        protected bool IsLocked;
        
        protected Button MyBtn;

        protected virtual void Start()
        {
            UIManager = UIManager.Instance;
            MyBtn = GetComponent<Button>();

            if (data.StartAsLocked)
            {
                MyBtn.interactable = false;
                IsLocked = true;
            }
        }

        public void Unlock()
        {
            MyBtn.interactable = true;
            IsLocked = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            UIManager.ShowInfobox(data, IsLocked);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.HideInfobox();
        }
    }
}
