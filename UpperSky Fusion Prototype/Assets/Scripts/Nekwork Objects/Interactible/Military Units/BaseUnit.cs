using Gameplay;
using UnityEngine;

namespace Nekwork_Objects.Interactible.Military_Units
{
    public class BaseUnit : BaseInteractibleObjects
    {
        private SelectionManager _selectionManager;
        [SerializeField] private GameObject selectionCircle;

        private void Start()
        {
            _selectionManager = SelectionManager.instance;
        }

        #region Selection
        private void OnMouseEnter()
        {
            _selectionManager.mouseAboveThisUnit = this;

            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                SetActiveSelectionCircle(true);
            }
        }

        private void OnMouseExit()
        {
            _selectionManager.mouseAboveThisUnit = null;

            if (!_selectionManager.currentlySelectedUnits.Contains(this)
                && Object.InputAuthority == Runner.LocalPlayer)
            {
                SetActiveSelectionCircle(false);
            }
        }
        #endregion

        public void SetActiveSelectionCircle(bool value)
        {
            selectionCircle.SetActive(value);
        }
    }
}
