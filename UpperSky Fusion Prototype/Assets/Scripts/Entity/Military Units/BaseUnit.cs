using Fusion;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace Entity.Military_Units
{
    public class BaseUnit : NetworkBehaviour
    {
        private SelectionManager _selectionManager;
        private UnitsManager _unitsManager;
        
        [SerializeField, Expandable] private UnitData data;

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;

        public Vector3 targetPosToMoveTo;

        [SerializeField] private GameObject selectionCircle;
        
        #region private variables for calculus
        private Vector3 _separationForce;
        private Vector3 _cohesionForce;
        private Vector3 _alignmentForce;
        
        private Vector3 _velocity;
        private Vector3 _force;
        #endregion
        
        private void Awake()
        {
            _maxHealth = data.MaxHealthPoints;
            currentHealthPoint = data.MaxHealthPoints;
        }
        
        private void Start()
        {
            _selectionManager = SelectionManager.Instance;
        }
        
        public override void Spawned()
        {
            _unitsManager = UnitsManager.Instance;
            _unitsManager.allActiveUnits.Add(this);
        }
        
        private void Update()
        {
            ManageBasicMovement();
            
            //  CalculateForces();
            //  MoveForward();
        }
        
        #region Movement Behaviour

        private void ManageBasicMovement()
        {
            if (Vector2.Distance(transform.position, targetPosToMoveTo) < 2) return;
            
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosToMoveTo, 
                data.MovementSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.LookRotation(targetPosToMoveTo - transform.position);
        }

        private void CalculateForces() 
        {
            Vector3 seperationSum = Vector3.zero;
            Vector3 positionSum = Vector3.zero;
            Vector3 headingSum = Vector3.zero;
        
            int unitsNearby = 0;

            foreach (var otherUnit in _unitsManager.allActiveUnits)
            {
                // Si l'untié verifié est cette unité, OU que l'unité vérifié n'a pas le même type que cette unité
                // OU que l'unité vérifié n'appartient pas au même joueur => passé la vérification
                if (this == otherUnit || otherUnit.Object.InputAuthority != Object.InputAuthority)
                {
                    continue;
                }

                Transform otherUnitTransform = otherUnit.transform;
                Vector3 otherUnitPos = otherUnitTransform.position;

                float distToOtherUnit = Vector2.Distance(transform.position, otherUnitPos);
                if (!(distToOtherUnit < data.AllyUnitsPerceptionRadius)) continue;
                
                seperationSum += -(otherUnitPos - transform.position) * (1f / Mathf.Max(distToOtherUnit, .0001f));
                positionSum += otherUnitPos;
                headingSum +=  otherUnit.transform.forward;
                    
                unitsNearby++;
            }
        
            if (unitsNearby > 0) 
            {
                _separationForce = seperationSum / unitsNearby;
                _cohesionForce   = (positionSum / unitsNearby) - transform.position;
                _alignmentForce  = headingSum / unitsNearby;
            }
            else 
            {
                _separationForce = Vector3.zero;
                _cohesionForce   = Vector3.zero;
                _alignmentForce  = Vector3.zero;
            }
        }
        
        private void MoveForward()
        {
            _force = _unitsManager.useWeights switch
            {
                true => _separationForce * -_unitsManager.separationWeight + _cohesionForce * _unitsManager.cohesionWeight +
                        _alignmentForce * _unitsManager.alignmentWeight,
                false => _separationForce + _cohesionForce + _alignmentForce
            };
            
            Vector3 correctedForce = _force;
            
            ApplyVelocity(data.MovementSpeed, correctedForce);
        }
        
        private void ApplyVelocity(float speed, Vector3 force)
        {
            _velocity = targetPosToMoveTo - transform.position * speed + force * Time.deltaTime;
            _velocity = _velocity.normalized * speed;
            transform.position += _velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(_velocity);
        }
        #endregion
        
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
