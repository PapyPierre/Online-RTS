using System;
using Gameplay;
using NaughtyAttributes;
using UnityEngine;

namespace Nekwork_Objects.Interactible.Military_Units
{
    public class BaseUnit : BaseInteractibleObjects
    {
        private SelectionManager _selectionManager;
        private UnitsManager _unitsManager;
        
        [SerializeField, Expandable] private UnitData data;
        
        [SerializeField] private float currentHealthPoint;
        
        [SerializeField] private GameObject selectionCircle;
        
        #region private variables for calculus
        private Vector2 _separationForce;
        private Vector2 _cohesionForce;
        private Vector2 _alignmentForce;
        
        private Vector2 _velocity;
        private Vector2 _force;
        #endregion
        
        private void Awake()
        {
            currentHealthPoint = data.MaxHealthPoints;
        }
        
        private void Start()
        {
            _selectionManager = SelectionManager.instance;
            _unitsManager = UnitsManager.instance;
        }
        
        public override void Spawned()
        {
            _unitsManager.allActiveUnits.Add(this);
        }
        
        private void Update()
        {
            CalculateForces();
            MoveForward();
        }
        
        #region Movement Behaviour
        private void CalculateForces() 
        {
            Vector2 seperationSum = Vector2.zero;
            Vector2 positionSum = Vector2.zero;
            Vector2 headingSum = Vector2.zero;
        
            int unitsNearby = 0;
        
            Vector2 myPositionIn2D = new Vector2(transform.position.x, transform.position.z);
        
            foreach (var unit in _unitsManager.allActiveUnits)
            {
                // Si l'untié verifié est cette unité, OU que l'unité vérifié n'a pas le même type que cette unité
                // OU que l'unité vérifié n'appartient pas au même joueur => passé la vérification
                if (this == unit || unit.data.UnitType != data.UnitType 
                                 || unit.Object.InputAuthority != Object.InputAuthority)
                {
                    continue;
                }
        
                Vector2 otherUnitPositionIn2D =  new Vector2(unit.transform.position.x, unit.transform.position.z);
                float distToOtherUnit = Vector2.Distance(myPositionIn2D, otherUnitPositionIn2D);
                
             
                if (!(distToOtherUnit < data.AllyUnitsPerceptionRadius)) continue;
                
                    
                seperationSum += -(otherUnitPositionIn2D - myPositionIn2D) * (1f / Mathf.Max(distToOtherUnit, .0001f));
                positionSum += otherUnitPositionIn2D;
                headingSum +=  new Vector2(unit.transform.forward.z, _unitsManager.flyingHeightOfAerianUnits);
                    
                unitsNearby++;
            }
        
            if (unitsNearby > 0) 
            {
                _separationForce = seperationSum / unitsNearby;
                _cohesionForce   = (positionSum / unitsNearby) - myPositionIn2D;
                _alignmentForce  = headingSum / unitsNearby;
            }
            else 
            {
                _separationForce = Vector2.zero;
                _cohesionForce   = Vector2.zero;
                _alignmentForce  = Vector2.zero;
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
            
            var correctedForce = _force;
            
            ApplyVelocity(data.MovementSpeed, correctedForce);
        }
        
        private void ApplyVelocity(float speed, Vector2 force)
        {
            _velocity =  new Vector2(transform.forward.z, _unitsManager.flyingHeightOfAerianUnits)
                * speed + force * Time.deltaTime; 
            
            _velocity = _velocity.normalized * speed;
            
            transform.position += (Vector3) _velocity * Time.deltaTime;
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
    }
}
