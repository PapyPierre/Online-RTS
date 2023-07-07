using Custom_UI;
using Custom_UI.InGame_UI;
using Fusion;
using Player;
using UnityEngine;
using World.Island;

namespace Entity.Buildings
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
        
        [field: SerializeField, Space] public int MaxBuildingsPerIslands { get; private set; }

        [Header("Blueprints")]
        [SerializeField] private GameObject[] allBuildingsBlueprints;
        [field: SerializeField] public Color BlueprintValidPosColor { get; private set; }
        [field: SerializeField] public Color BlueprintUnvalidPosColor { get; private set; }
        
        [HideInInspector] public bool haveBlueprintInHand;

        public enum AllBuildingsEnum
        {
            Foreuse = 0,
            ExploitationOrichalque = 1, 
            Habitation = 2,
            Menuiserie = 3,
            Tisserand = 4,
            Fonderie = 5,
            Baliste = 6,
            Canon = 7,
            MineOrichalque = 8,
            Obusier = 9,
            CentreMeteo = 10,
            Usine = 11,
            Academie = 12,
            SiègeDuCommandement = 13
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
                
            var playerCurrentMat = player.ressources.CurrentMaterials;
            var playerCurrentOri = player.ressources.CurrentOrichalque;
            
            var buildingMatCost = allBuildingsDatas[buildingIndex].MaterialCost;
            var buildingOriCost = allBuildingsDatas[buildingIndex].OrichalqueCost;
            
            // Check if player have enough ressources to build this building
            if (playerCurrentMat >= buildingMatCost && playerCurrentOri >= buildingOriCost)
            {
                Instantiate(allBuildingsBlueprints[buildingIndex]);
                haveBlueprintInHand = true;
            }
            else Debug.Log("not enough ressources");
        }

        public NetworkObject BuildBuilding(int buildingIndex, Vector3 pos, Quaternion rot, Island island, 
            bool isStartBuilding = false)
        {          
            PlayerController player = _gameManager.thisPlayer;

            var matCost = allBuildingsDatas[buildingIndex].MaterialCost;
            if (matCost > 0) player.ressources.CurrentMaterials -= matCost;

            var oriCost = allBuildingsDatas[buildingIndex].OrichalqueCost;
            if (oriCost > 0) player.ressources.CurrentOrichalque -= oriCost;

            if (!isStartBuilding) island.BuildingsCount++;
            
            NetworkObject obj = _gameManager.thisPlayer.Runner.Spawn
                (allBuildingsPrefab[buildingIndex], pos, rot, island.Object.InputAuthority);

            BaseBuilding building = obj.GetComponent<BaseBuilding>();
            
            building.Init(island, isStartBuilding);
            island.buildingOnThisIsland.Add(building);
           
            haveBlueprintInHand = false;
            
            return obj;
        }
    }
}
