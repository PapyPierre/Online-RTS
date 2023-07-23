using System.Collections.Generic;
using Custom_UI;
using Fusion;
using UnityEngine;
using World;
using World.Island;

namespace Entity.Military_Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;
        private GameManager _gameManager;
        private WorldManager _worldManager;
        private UIManager _uiManager;

        public NetworkPrefabRef[] allUnitsPrefab;
        public List<UnitData> allUnitsData;

        [SerializeField] private NetworkPrefabRef unitGroupPrefab;

        [Space] public List<BaseUnit> allActiveUnits;
        public List<BaseUnit> currentlySelectedUnits;
        
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
            unit.SetActiveSelectionCircle(true);
        }
      
        public void UnSelectAllUnits()
        {
            foreach (BaseUnit unit in currentlySelectedUnits)
            {
                unit.SetActiveSelectionCircle(false);
            }
      
            currentlySelectedUnits.Clear();
            HideIslandsColoniseBtn();
        }

        private void HideIslandsColoniseBtn()
        {
            foreach (Island island in _worldManager.allIslands)
            {
                if (island.coloniseBtn.activeSelf) island.coloniseBtn.SetActive(false);
            }
        }
        
        public void OrderSelectedUnitsToMoveTo(Vector3 positon)
        {
            Vector3 targetPos = new Vector3(positon.x, flyingHeightOfUnits, positon.z);

            var center = Vector3.zero;

            foreach (var unit in  currentlySelectedUnits)
            {
                center += unit.transform.position;
            }

            center /= currentlySelectedUnits.Count;

            var netObj = _gameManager.thisPlayer.Runner.Spawn(unitGroupPrefab, center, Quaternion.identity, 
                _gameManager.thisPlayer.Object.StateAuthority);

            netObj.GetComponent<UnitGroup>().Init(targetPos);
        }
    }
}