using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface.MiniMap
{
    public class MinimapHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _isMouseOver = false;

        public bool IsMouseOverMinimap()
        {
            return _isMouseOver;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }
    }
}