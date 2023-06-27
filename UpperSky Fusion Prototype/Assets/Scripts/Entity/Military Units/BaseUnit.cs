using Fusion;
using NaughtyAttributes;
using Network;
using Player;
using UnityEngine;

namespace Entity.Military_Units
{
    public class BaseUnit : NetworkBehaviour
    {
        private UnitsManager _unitsManager;
        private NetworkManager _networkManager;
        
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
            _networkManager = NetworkManager.Instance;
            _unitsManager = UnitsManager.Instance;
            _unitsManager.allActiveUnits.Add(this);

            if (Object.HasInputAuthority) Owner = _networkManager.thisPlayer;
        }

        #region Selection
        private void OnMouseEnter()
        {
            _networkManager.thisPlayer.mouseAboveThisUnit = this;
        
            if (Owner == _networkManager.thisPlayer)
            {
                SetActiveSelectionCircle(true);
            }
        }
        
        private void OnMouseExit()
        {
            _networkManager.thisPlayer.mouseAboveThisUnit = null;
        
            if (!_unitsManager.currentlySelectedUnits.Contains(this) && Owner == _networkManager.thisPlayer)
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
