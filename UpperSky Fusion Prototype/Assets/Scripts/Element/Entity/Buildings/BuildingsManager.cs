using Custom_UI;
using Custom_UI.InGame_UI;
using Element.Island;
using Entity.Buildings;
using Fusion;
using Player;
using UnityEngine;
using UserInterface;

namespace Element.Entity.Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance;
        private GameManager _gameManager;
        private UIManager _uiManager;
 
        public LayerMask terrainLayer;
        public LayerMask buildingLayer;

        [Header("Buildings")]
        public MenuIcon[] allBuildingsIcons;
        public NetworkPrefabRef[] allBuildingsPrefab;
        public BuildingData[] allBuildingsDatas;
        
        [Header("Blueprints")]
        [SerializeField] private GameObject[] allBuildingsBlueprints;
        [field: SerializeField] public Color BlueprintValidPosColor { get; private set; }
        [field: SerializeField] public Color BlueprintUnvalidPosColor { get; private set; }
        
        [HideInInspector] public bool haveBlueprintInHand;

        public enum AllBuildingsEnum
        {
            Foreuse = 0,
            Gisement = 1, 
            Habitation = 2,
            Menuiserie = 3,
            Lumberjack = 4,
            Fonderie = 5,
            Canon = 6,
            MineOrichalque = 7,
            Obusier = 8,
            CentreMeteo = 9,
            Usine = 10,
            Academie = 11,
            SiègeDuCommandement = 12
        }
        
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
            _uiManager = UIManager.Instance;
        }

        public void BuildBlueprint(int buildingIndex)
        {
            if (haveBlueprintInHand) return;

            PlayerController player = _gameManager.thisPlayer;
                
            var playerCurrentWood = player.ressources.CurrentWood;
            var playerCurrentMetals = player.ressources.CurrentMetals;
            var playerCurrentOri = player.ressources.CurrentOrichalque;
            
            var buildingWoodCost = allBuildingsDatas[buildingIndex].WoodCost;
            var buildingMetalsCost = allBuildingsDatas[buildingIndex].MetalsCost;
            var buildingOriCost = allBuildingsDatas[buildingIndex].OrichalqueCost;
            
            // Check if player have enough ressources to build this building
            if (playerCurrentWood >= buildingWoodCost 
                && playerCurrentMetals >= buildingMetalsCost 
                && playerCurrentOri >= buildingOriCost)
            {
                Instantiate(allBuildingsBlueprints[buildingIndex]);
                haveBlueprintInHand = true;
            }
            else Debug.Log("Not enough ressources");
        }

        public void PayForBuilding(int buildingIndex)
        {
            PlayerController player = _gameManager.thisPlayer;

            var woodCost = allBuildingsDatas[buildingIndex].WoodCost;
            if (woodCost > 0) player.ressources.CurrentWood -= woodCost;
            
            var metalsCost = allBuildingsDatas[buildingIndex].MetalsCost;
            if (metalsCost > 0) player.ressources.CurrentMetals -= metalsCost;

            var oriCost = allBuildingsDatas[buildingIndex].OrichalqueCost;
            if (oriCost > 0) player.ressources.CurrentOrichalque -= oriCost;
        }

        public void BuildBuilding(int buildingIndex, Vector3 pos, Quaternion rot, BaseIsland island)
        {
            island.BuildingsCount++;
            
            _uiManager.UpdateSelectionInfobox(island, island.Data, island.Owner);
            
            NetworkObject obj = _gameManager.thisPlayer.Runner.Spawn
                (allBuildingsPrefab[buildingIndex], pos, rot, island.Object.InputAuthority);

            BaseBuilding building = obj.GetComponent<BaseBuilding>();
            
            building.Init(island.Owner, building.Data);
            building.SetIsland(island);
            island.buildingOnThisIsland.Add(building);

            haveBlueprintInHand = false;
        }
    }
}
