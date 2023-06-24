using Custom_UI;
using Custom_UI.InGame_UI;
using Fusion;
using Network;
using Player;
using UnityEngine;

namespace Entity.Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance;
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        
        public LayerMask terrainLayer;
        public LayerMask buildingLayer;

        [Header("Buildings")]
        public MenuIcon[] allBuildingsIcons;
        public NetworkPrefabRef[] allBuildingsPrefab;
        public BuildingData[] allBuildingsDatas;
        
        [Header("Blueprints")]
        [SerializeField] private GameObject[] allBuildingsBlueprints;
        [field: SerializeField] public Color BlueprintPossibleBuildColor { get; private set; }
        [field: SerializeField] public Color BlueprintOverlapColor { get; private set; }

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
            Academie = 12
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
            _networkManager = NetworkManager.Instance;
            _uiManager = UIManager.Instance;
        }

        public void BuildBlueprint(int buildingIndex)
        {
            PlayerController player = _networkManager.thisPlayer;
                
            var playerCurrentMat = player.ressources.CurrentMaterials;
            var playerCurrentOri = player.ressources.CurrentOrichalque;
            
            var buildingMatCost = allBuildingsDatas[buildingIndex].MaterialCost;
            var buildingOriCost = allBuildingsDatas[buildingIndex].OrichalqueCost;
            
            // Check if player have enough ressources to build this building
            if (playerCurrentMat >= buildingMatCost && playerCurrentOri >= buildingOriCost)
            {
                Instantiate(allBuildingsBlueprints[buildingIndex]);
            }
            else Debug.Log("not enough ressources");
           
        }

        public void BuildBuilding(int buildingIndex, Vector3 pos, Quaternion rot)
        {
            PlayerController player = _networkManager.thisPlayer;
            player.ressources.CurrentMaterials -= allBuildingsDatas[buildingIndex].MaterialCost;
            player.ressources.CurrentOrichalque -= allBuildingsDatas[buildingIndex].OrichalqueCost;
            
            _uiManager.ShowOrHideBuildMenu(false);
            _uiManager.HideInfobox();
            
            _networkManager.thisPlayer.RPC_SpawnNetworkObj(allBuildingsPrefab[buildingIndex], pos, rot);
        }
    }
}
