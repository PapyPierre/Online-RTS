using System;
using System.Collections.Generic;
using Fusion;
using Network;
using UnityEngine;

namespace Entity.Military_Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;
        private NetworkManager _networkManager;

        public NetworkPrefabRef[] allUnitsPrefab;
        public List<UnitData> allUnitsData;

        [SerializeField] private NetworkPrefabRef unitGroupPrefab;

        [Space] public List<BaseUnit> allActiveUnits;
        public List<BaseUnit> currentlySelectedUnits;
        
        public List<BaseUnit> tempUnitsToMove;
        public Vector3 tempTargetPos;
        
        public enum AllUnitsEnum
        {
          Darwin = 0, 
          Beethoven = 1,
          Galileo = 2,
          Hokusai = 3,
          Magellan = 4,
          Wagner = 5,
          Oppenheimer = 6
        }
        
        public float distToTargetToStop;
        public float flyingHeightOfUnits;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }

        private void Start()
        {
            _networkManager = NetworkManager.Instance;
        }

        public void SelectUnit(BaseUnit unit)
        {
            currentlySelectedUnits.Add(unit); 
            unit.SetActiveSelectionCircle(true);
        }
      
        public void UnSelectAllUnits()
        {
            foreach (var unit in currentlySelectedUnits)
            {
                unit.SetActiveSelectionCircle(false);
            }
      
            currentlySelectedUnits.Clear();
        }
        
        public void OrderToMoveUnitsTo(List<BaseUnit> unitsToMove, Vector3 positon)
        {
            Vector3 correctedPos = new Vector3(positon.x, flyingHeightOfUnits, positon.z);
            var targetPosToMoveTo = correctedPos;

            var selectedUnitsCenter = Vector3.zero;

            foreach (var unit in  unitsToMove)
            {
                selectedUnitsCenter += unit.transform.position;
            }

            selectedUnitsCenter /= unitsToMove.Count;
     
            tempTargetPos = targetPosToMoveTo;
            tempUnitsToMove = unitsToMove;
            
            _networkManager.thisPlayer.RPC_SpawnNetworkObj(unitGroupPrefab, selectedUnitsCenter, Quaternion.identity);
        }
    }
}