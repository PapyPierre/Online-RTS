using System.Collections.Generic;
using Fusion;
using Network;
using UnityEngine;

namespace Entity.Military_Units
{
    public class UnitGroup : NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private UnitsManager _unitsManager;
        
        // Serialized for debug only
        [SerializeField] private Vector3 _targetPos;
        [SerializeField] private  List<BaseUnit> _unitsInGroup = new ();
        [SerializeField]private float _groupSpeed;
        
        private bool _readyToGo;

        public override void Spawned()
        {
            _networkManager = NetworkManager.Instance;
            _unitsManager = UnitsManager.Instance;
            
            _targetPos = _unitsManager.tempTargetPos;
            _unitsManager.tempTargetPos = Vector3.zero;
            
            foreach (var unit in  _unitsManager.tempUnitsToMove)
            {
                unit.transform.parent = transform;
                _unitsInGroup.Add(unit);
            }
            
            _unitsManager.tempUnitsToMove.Clear();
            
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
            transform.rotation = Quaternion.LookRotation(_targetPos - transform.position);

            _networkManager.thisPlayer.RPC_MoveNetworkObj(Object, transform.position, transform.rotation);
            
            if (Vector3.Distance(transform.position,  _targetPos) < _unitsManager.distToTargetToStop)
            {
                transform.DetachChildren();
                Destroy(gameObject);
            }
        }
    }
}
