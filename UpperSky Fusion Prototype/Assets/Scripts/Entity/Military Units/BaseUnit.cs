using Fusion;
using NaughtyAttributes;
using Network;
using Player;
using UnityEngine;

namespace Entity.Military_Units
{
    public class BaseUnit : NetworkBehaviour
    {
        private SelectionManager _selectionManager;
        private UnitsManager _unitsManager;
        private NetworkManager _networkManager;
        
        [SerializeField, Expandable] private UnitData data;

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;

        public Vector3 targetPosToMoveTo;
        public bool targetPosIsSet;

        [SerializeField] private GameObject selectionCircle;
        
        #region private variables for calculus
        private Vector3 _separationForce;
        private Vector3 _cohesionForce;
        private Vector3 _alignmentForce;

        private Vector3 _velocity;
        #endregion
        
        private void Awake()
        {
            _maxHealth = data.MaxHealthPoints;
            currentHealthPoint = data.MaxHealthPoints;
        }
        
        private void Start()
        {
            _selectionManager = SelectionManager.Instance;
            _networkManager = NetworkManager.Instance;
        }
        
        public override void Spawned()
        {
            _unitsManager = UnitsManager.Instance;
            _unitsManager.allActiveUnits.Add(this);
            Debug.Log(Object.InputAuthority);
        }

        public override void FixedUpdateNetwork()
        {
            if (targetPosIsSet) MoveToPosition();
        }

        public void OrderToMoveTo(Vector3 positon)
        {
            Vector3 correctedPos = new Vector3(positon.x, _unitsManager.flyingHeightOfUnits, positon.z);
            targetPosToMoveTo = correctedPos;
            targetPosIsSet = true;
        }

        private void MoveToPosition()
        {
            var step = data.MovementSpeed * Runner.DeltaTime;
            
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosToMoveTo, step);
           Quaternion newRot = Quaternion.LookRotation(newPos);
           
           _networkManager.thisPlayer.RPC_MoveNetworkObj(Object, newPos, newRot);

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
        
        public void SetActiveSelectionCircle(bool value)
        {
            selectionCircle.SetActive(value);
        }
        #endregion

        private void OnDrawGizmos()
        {
            Debug.DrawRay(transform.position, targetPosToMoveTo - transform.position, Color.yellow);
        }
    }
}
