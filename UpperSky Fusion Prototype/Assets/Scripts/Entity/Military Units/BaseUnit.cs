using System.Collections.Generic;
using Fusion;
using NaughtyAttributes;
using Network;
using Player;
using UnityEditor;
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
        private Vector3 _currentDir;
        private bool isGoingForStop;
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
            if (targetPosIsSet || isGoingForStop) MoveToPosition();
        }

        public void OrderToMoveTo(Vector3 positon)
        {
            Vector3 correctedPos = new Vector3(positon.x, _unitsManager.flyingHeightOfUnits, positon.z);
            targetPosToMoveTo = correctedPos;
            targetPosIsSet = true;
            isGoingForStop = false;
        }

        private void MoveToPosition()
        {
            if (Vector3.Distance(transform.position, targetPosToMoveTo) < _unitsManager.distToTargetToStop)
            {
                targetPosIsSet = false;
                return;
            }

            Vector3 seperationSum = Vector3.zero;
            Vector3 positionSum = Vector3.zero;
            Vector3 headingSum = Vector3.zero;
        
            int unitsNearby = 0;

            foreach (var otherUnit in _unitsManager.allActiveUnits)
            {
                // Si l'untié verifié est cette unité, OU que l'unité vérifié n'a pas le même type que cette unité
                // OU que l'unité vérifié n'appartient pas au même joueur => passé la vérification
                if (this == otherUnit || otherUnit.Object.InputAuthority != Object.InputAuthority) continue;
                
                Vector3 otherUnitPos = otherUnit.transform.position;

                float distToOtherUnit = Vector3.Distance(transform.position, otherUnitPos);
                if (!(distToOtherUnit < _unitsManager.unitsPerceptionRadius)) continue;
                
                if (Vector3.Distance(transform.position, targetPosToMoveTo) < _unitsManager.distToTargetToTryStop)
                {
                    targetPosIsSet = false;
                    return;
                }
                
                seperationSum += -(otherUnitPos - transform.position) * (1f / Mathf.Max(distToOtherUnit, .0001f));
                positionSum += otherUnitPos;
                headingSum +=  otherUnit.transform.forward;
                    
                unitsNearby++;

                Debug.DrawLine(transform.position, otherUnitPos, Color.magenta);
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
                _alignmentForce = Vector3.zero;
            }
            
            Vector3 force = _unitsManager.useWeights switch
            {
                true => _separationForce * _unitsManager.separationWeight 
                        + _cohesionForce * _unitsManager.cohesionWeight 
                        + _alignmentForce * _unitsManager.alignmentWeight,
                
                false => _separationForce + _cohesionForce + _alignmentForce
            };
            
            _currentDir = targetPosToMoveTo - transform.position;
            _velocity = _currentDir * data.MovementSpeed + Vector3.forward + force * Runner.DeltaTime;
            _velocity = _velocity.normalized * data.MovementSpeed;
            transform.position += _velocity * Runner.DeltaTime;
            transform.rotation = Quaternion.SlerpUnclamped(
                transform.rotation, Quaternion.LookRotation(_velocity), Runner.DeltaTime * data.AngularSpeed);

            _networkManager.thisPlayer.RPC_MoveNetworkObj(Object, transform.position, transform.rotation);
        }
        
        private Vector3 GenerateRandomPosAroundPoint(Vector3 point, float radius)
        {
            // Générer des angles aléatoires pour les axes x, y et z
            float angleX = Random.Range(0f, 2f * Mathf.PI);
            float angleY = Random.Range(0f, 2f * Mathf.PI);

            // Générer une distance radiale aléatoire entre 0 et le rayon maximal
            float distance = Random.Range(0f, radius);

            // Convertir les angles et la distance en coordonnées cartésiennes
            float deltaX = distance * Mathf.Sin(angleY) * Mathf.Cos(angleX);
            float deltaZ = distance * Mathf.Cos(angleY);

            // Calculer la position aléatoire finale en ajoutant les coordonnées au centre donné
            Vector3 randomPosition = point + new Vector3(deltaX, 0, deltaZ);

            return randomPosition;
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

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {            
            Vector3 myPos = transform.position;

            Gizmos.color =  Color.white;
            Gizmos.DrawRay(myPos, _currentDir);
            Gizmos.color =  Color.green;
            Gizmos.DrawRay(myPos, _velocity);
            
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(myPos, Vector3.up, _unitsManager.unitsPerceptionRadius);
        }
        #endif
    }
}
