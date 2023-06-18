using System;
using Custom_UI.InGame_UI;
using Fusion;
using Network;
using UnityEngine;

namespace Buildings
{
    public class BuildingsManager : MonoBehaviour
    {
        public static BuildingsManager Instance;
        private NetworkManager _networkManager;

        public BuildingIcon[] allBuildingsIcons;

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

        public void BuildBlueprint(GameObject buildingBlueprint)
        {
            Instantiate(buildingBlueprint);
        }

        public void BuildBuilding(NetworkPrefabRef buildingPrefab, Vector3 pos, Quaternion rot, PlayerRef owner)
        {
            _networkManager.RPC_SpawnNetworkObject(buildingPrefab, pos, rot, owner);
        }
    }
}
