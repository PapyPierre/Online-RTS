using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UserInterface;
using World;

namespace Element.Entity.Military_Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;
        private GameManager _gameManager;
        private WorldManager _worldManager;
        private UIManager _uiManager;

        public NetworkPrefabRef[] allUnitsPrefab;
        public List<UnitData> allUnitsData;

        [Space] public List<BaseUnit> allActiveUnits;
        public List<BaseUnit> currentlySelectedUnits;

        [SerializeField] private GameObject unitMoveIndicator;
        
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

        public enum UnitStates
        {
            Static,
            Moving,
            Attacking
        }
        
        public enum UnitSkillsEnum
        {
            None = 0, 
            GomorrahBomb = 1,
            Colonisation = 2
        }

        public float distToTargetToStop;
        public float flyingHeightOfUnits;
        [SerializeField] private float maxDistToTargetCenter;

        public float distUnitToIslandToColonise;

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
            _gameManager = GameManager.Instance;
            _worldManager = WorldManager.Instance;
            _uiManager = UIManager.Instance;
        }
        
        public void SelectUnit(BaseUnit unit)
        {
            currentlySelectedUnits.Add(unit);
        }
      
        public void UnSelectAllUnits()
        {
            foreach (BaseUnit unit in currentlySelectedUnits)
            {
                unit.SetActiveSelectionCircle(false);
            }
      
            currentlySelectedUnits.Clear();
        }

        public void OrderSelectedUnitsToMoveTo(Vector3 positon)
        {
            Vector3 groupTarget = new Vector3(positon.x, flyingHeightOfUnits, positon.z);

            var groupCenter = Vector3.zero;

            foreach (var unit in  currentlySelectedUnits) groupCenter += unit.transform.position;
            
            groupCenter /= currentlySelectedUnits.Count;

            Vector3 centerToTarget = groupTarget - groupCenter;

            foreach (var unit in currentlySelectedUnits)
            {
                if (unit.isDead) return;

                if (unit.myMoveIndicator != null) unit.myMoveIndicator.SetActive(false);
                
                Vector3 unitPos = unit.transform.position;

                Vector3 unitTarget = unitPos + centerToTarget;

                if (Vector3.Distance(unitTarget, groupTarget) > maxDistToTargetCenter)
                {
                    unitTarget = groupTarget;
                }
                
                unit.myMoveIndicator = Instantiate(unitMoveIndicator, groupTarget, unitMoveIndicator.transform.rotation);

                unit.targetPosToMoveTo = unitTarget;
                
                unit.myState = UnitStates.Moving;
            }
        }
        
        // Call from inspector
        public void UseUnitSkill()
        {
            if (_gameManager.thisPlayer.lastSelectedElement != null)
            {
                _gameManager.thisPlayer.lastSelectedElement.GetComponent<BaseUnit>().UseSkill();
            }
            else
            {
                Debug.LogError("openedElementInInGameInfobox return null in UseUnitSkill()");
            }
        }
    }
}