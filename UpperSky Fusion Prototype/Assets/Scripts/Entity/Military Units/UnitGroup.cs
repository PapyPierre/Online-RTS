using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Entity.Military_Units
{
    public class UnitGroup : NetworkBehaviour
    {
        private GameManager _gameManager;
        private UnitsManager _unitsManager;
        
        // Serialized for debug only
        [SerializeField] private Vector3 _targetPos;
        [SerializeField] private  List<BaseUnit> _unitsInGroup = new ();
        [SerializeField] private float _groupSpeed;
        
        private bool _readyToGo;

        public override void Spawned()
        {
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;
        }

        public void Init(Vector3 targetPos, List<BaseUnit> unitsToMove)
        {
            _targetPos = targetPos;
            foreach (var unit in  unitsToMove)
            {
                unit.transform.parent = transform;
                _unitsInGroup.Add(unit);
            }
            
            _unitsManager.currentlySelectedUnits = _unitsInGroup;
            
            _groupSpeed = 100;
            
            foreach (var unit in _unitsInGroup)
            {
                if (unit.Data.MovementSpeed < _groupSpeed) _groupSpeed = unit.Data.MovementSpeed;
            }
            
            _readyToGo = true;
        }

        public override void FixedUpdateNetwork()
        {
          if(_readyToGo) MoveGroupToDesieredPos();
        }

        private void MoveGroupToDesieredPos()
        {
            var step = _groupSpeed * Runner.DeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, step);

            foreach (var unit in _unitsInGroup)
            {
               unit.transform.rotation = Quaternion.LookRotation(_targetPos - transform.position);
            }

            if (Vector3.Distance(transform.position,  _targetPos) < _unitsManager.distToTargetToStop)
            {
                transform.DetachChildren();
                Destroy(gameObject);
            }
        }
    }
}
