using Fusion;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace Entity.Military_Units
{
    public class BaseUnit : NetworkBehaviour
    {
        private UnitsManager _unitsManager;
        private GameManager _gameManager;
        
        public PlayerController Owner { get; private set; }
        
        [field: SerializeField, Expandable] public UnitData Data { get; private set; }

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;

       
        [SerializeField] private GameObject selectionCircle;

        private void Awake()
        {
            _maxHealth = Data.MaxHealthPoints;
            currentHealthPoint = Data.MaxHealthPoints;
        }

        public override void Spawned()
        {
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;
            _unitsManager.allActiveUnits.Add(this);

            if (Object.HasInputAuthority) Owner = _gameManager.thisPlayer;
        }

        #region Selection
        private void OnMouseEnter()
        {
            _gameManager.thisPlayer.mouseAboveThisUnit = this;
        
            if (Owner == _gameManager.thisPlayer)
            {
                SetActiveSelectionCircle(true);
            }
        }
        
        private void OnMouseExit()
        {
            _gameManager.thisPlayer.mouseAboveThisUnit = null;
        
            if (!_unitsManager.currentlySelectedUnits.Contains(this) && Owner == _gameManager.thisPlayer)
            {
                SetActiveSelectionCircle(false);
            }
        }
        
        public void SetActiveSelectionCircle(bool value)
        {
            selectionCircle.SetActive(value);
        }
        #endregion
    }
}
