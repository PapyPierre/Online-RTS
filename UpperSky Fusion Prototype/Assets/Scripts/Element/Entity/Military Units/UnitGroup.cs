using System.Collections.Generic;
using Element.Entity.Military_Units;
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

        public void Init(Vector3 destination)
        {
            targetPos = destination;

            foreach (var unit in _unitsManager.currentlySelectedUnits)
            {
                if (unit.currentGroup is not null)
                {
                    unit.currentGroup.RemoveUnitFromGroup(unit);
                }

                unit.transform.parent = transform;
                unit.currentGroup = this;
                unitsInGroup.Add(unit);
            }

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
            // Dosen't call RemoveUnitFromGroup() here to not modify the collection during enumeration 
            foreach (var unit in unitsInGroup)
            {
                if (unit.isDead)
                {
                    unit.currentGroup = null;
                    continue;
                }

                if (unit.targetedUnitIsInRange)
                {
                    unit.currentGroup = null; 
                    unit.transform.parent = null;
                }
            }
            
            var step = groupSpeed * Runner.DeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

            foreach (var unit in unitsInGroup)
            {
                if (!unit.isDead) unit.transform.rotation = Quaternion.LookRotation(targetPos - transform.position);
            }

            if (Vector3.Distance(transform.position,  targetPos) < _unitsManager.distToTargetToStop)
            {
               DestroyGroup();
            }
        }

        private void DestroyGroup()
        {
            // Dosen't call RemoveUnitFromGroup() here to not modify the collection during enumeration 
            foreach (var unit in unitsInGroup)
            {
                unit.currentGroup = null;
            }
            
            unitsInGroup.Clear();
            transform.DetachChildren();
            gameObject.SetActive(false);
        }

        public void RemoveUnitFromGroup(BaseUnit unit)
        {
            unit.currentGroup = null;
            unit.transform.parent = null;
            unitsInGroup.Remove(unit);

            if (unitsInGroup.Count is 0) DestroyGroup();
        }
    }
}
