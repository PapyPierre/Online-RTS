using Buildings;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Custom_UI.InGame_UI
{
    public class BuildingIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private UIManager _uiManager;

        [SerializeField, Expandable, Required()] private BuildingData data;

        private Button _myBtn;

        private bool _isLocked;

        private void Start()
        {
            _uiManager = UIManager.Instance;
            _myBtn = GetComponent<Button>();

            if (data.StartAsLocked)
            {
                _myBtn.interactable = false;
                _isLocked = true;
            }
        }

        public void Unlock()
        {
            _myBtn.interactable = true;
            _isLocked = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _uiManager.ShowInfoboxBuilding(data.Name, _isLocked ? data.LockedDescription : data.Description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _uiManager.HideInfoboxBuilding();
        }
    }
}
