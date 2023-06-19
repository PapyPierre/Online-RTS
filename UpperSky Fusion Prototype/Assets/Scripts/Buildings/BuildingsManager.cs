using Custom_UI.InGame_UI;
using Fusion;
using Network;
using Player;
using UnityEngine;

namespace Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance;
        private NetworkManager _networkManager;

        public BuildingIcon[] allBuildingsIcons;
        [SerializeField] private GameObject[] allBuildingsBlueprints;
        [SerializeField] private NetworkPrefabRef[] allBuildingsPrefab;
        [SerializeField] private BuildingData[] allBuildingsDatas;
        
        public enum AllBuildingsEnum
        {
            Foreuse = 0,
            ExploitationOrichalque = 1, 
            Habitation = 2,
            Menuiserie = 3,
            Tisserand = 4,
            Fonderie = 5,
            Manufacture = 6,
            Baliste = 7,
            Canon = 8,
            MineOrichalque = 9,
            Obusier = 10,
            CentreMeteo = 11,
            Usine = 12,
            Academie = 13
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

        public void BuildBuilding(int buildingIndex, Vector3 pos, Quaternion rot, PlayerRef owner, NetworkRunner runner)
        {
            PlayerController player = _networkManager.thisPlayer;
            player.ressources.CurrentMaterials -= allBuildingsDatas[buildingIndex].MaterialCost;
            player.ressources.CurrentOrichalque -= allBuildingsDatas[buildingIndex].OrichalqueCost;
            
            RPC_SpawnNetworkObject(allBuildingsPrefab[buildingIndex], pos, rot, owner);
        }
        
        public NetworkObject RPC_SpawnNetworkObject(NetworkPrefabRef prefab, Vector3 position, Quaternion rotation,
            PlayerRef owner, NetworkRunner networkRunner = null, RpcInfo info = default)
        {
            Debug.Log(_networkManager.connectedPlayers[0].Runner.GameMode);

            
            if (networkRunner != null)
            {
                Debug.Log(networkRunner);
                return networkRunner.Spawn(prefab, position, rotation, owner);
            }
            else return _networkManager.connectedPlayers[0].Runner.Spawn(prefab, position, rotation, owner);
        }
    }
}
