using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Entity.Military_Units
{
    public class UnitGroup : NetworkBehaviour
    {
        private UnitsManager _unitsManager;
        
        // Serialized for debug only
        [SerializeField] private Vector3 targetPos;
        [SerializeField] private  List<BaseUnit> unitsInGroup = new ();
        [SerializeField] private float groupSpeed;
        
        private bool _readyToGo;

        public override void Spawned()
        {
            _unitsManager = UnitsManager.Instance;
        }

        public void Init(Vector3 destination, List<BaseUnit> unitsToMove)
        {
            targetPos = destination;
            
            GameObject tempObj = null;

            foreach (var unit in  unitsToMove)
            {
                if (unit.transform.parent != null)
                {
                    tempObj = unit.transform.parent.gameObject;
                }

                unit.transform.parent = transform;
                unitsInGroup.Add(unit);
            } 
            
            if (tempObj != null) tempObj.SetActive(false);

            _unitsManager.currentlySelectedUnits = unitsInGroup;
            
            groupSpeed = 100;
            
            foreach (var unit in unitsInGroup)
            {
                if (unit.Data.MovementSpeed < groupSpeed) groupSpeed = unit.Data.MovementSpeed;
            }
            
            _readyToGo = true;
        }

        public override void FixedUpdateNetwork()
        {
          if(_readyToGo) MoveGroupToDesieredPos();
        }

        private void MoveGroupToDesieredPos()
        {
            foreach (var unit in unitsInGroup)
            {
                if (unit.targetedUnitIsInRange)
                {
                    unit.transform.parent = null;
                }
            }
            
            var step = groupSpeed * Runner.DeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

            foreach (var unit in unitsInGroup)
            {
               unit.transform.rotation = Quaternion.LookRotation(targetPos - transform.position);
            }

            if (Vector3.Distance(transform.position,  targetPos) < _unitsManager.distToTargetToStop)
            {
                transform.DetachChildren();
                gameObject.SetActive(false);
            }
        }
    }
}
