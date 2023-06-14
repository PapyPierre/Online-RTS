using System.Collections.Generic;
using Fusion;
using NaughtyAttributes;
using Nekwork_Objects.Islands;
using Network_Logic;
using UnityEngine;
using Random = UnityEngine.Random;

// Sciprt resonsable de la génération procedural du monde

namespace World
{
    public class WorldGenerator : NetworkBehaviour
    {
        public static WorldGenerator instance;
        private WorldManager _worldManager;
        private NetworkManager _networkManager;

        [SerializeField] private bool autoGenerate;

        public float innerBorderRadius;
        public float outerBorderRadius;

        public int numberOfPlayers;
        private int numberOfIslands;
        [SerializeField] private float minDistBetweenIslands;
        [SerializeField] private AnimationCurve islandDistFormCenterRepartition;
        private int maxSpecialIslands;
        private int _currentlyPlacedSpecialIslands;
        
        [SerializeField] private NetworkPrefabRef islandPrefab;
        private List<Island> _allIslands = new();
        

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            instance = this;
        }

        public override void Spawned()
        {
            _worldManager = WorldManager.instance;
            _networkManager = NetworkManager.instance;

            if (Runner.IsServer && autoGenerate) GenerateWorld();
        }

        [Button()]
        private void GenerateWorld()
        {
            numberOfIslands = numberOfPlayers * 3;
            maxSpecialIslands = numberOfPlayers - Mathf.RoundToInt(numberOfPlayers/4);

            CheckForReset();
            
            SpawnIsland(new Vector3(0, 0, innerBorderRadius), IslandTypesEnum.Starting);
            CalculatePlayersPosOnInnerBorder();
        }

        private void CheckForReset()
        {
            if (_allIslands.Count > 0)
            {
                foreach (var island in _allIslands)
                {
                    island.gameObject.SetActive(false);
                }
                
                _allIslands.Clear();
                _currentlyPlacedSpecialIslands = 0;
            }
        }

        // Le but de cette fonction est de calculé x positions equidistante sur la bordure du cercle
        private void CalculatePlayersPosOnInnerBorder()
        {
            // Calcule du périmètre
            float innerBorderPerimeter = 2 * Mathf.PI * innerBorderRadius;
            
            // Calcule de la distance entre chaque joueur raporté sur le périmètre
            float distancePerPlayer = innerBorderPerimeter / numberOfPlayers;
            
            // Calcule de l'angle en radian avec cette distance sur le périmètre du cercle
            float radianAngle = (distancePerPlayer / innerBorderPerimeter) * 2 * Mathf.PI;
            
            // Conversion en degré
            float degreeAngle = radianAngle * 180 / Mathf.PI;
            
            SpawnPlayerPosAtEachAngle(degreeAngle);
        }

        private void SpawnPlayerPosAtEachAngle(float angle)
        {
            for (int i = 0; i < numberOfPlayers -1; i++)
            {
                transform.Rotate(Vector3.up, angle);
                SpawnIsland(new Vector3(0, 0, innerBorderRadius), IslandTypesEnum.Starting);
            }
            
            // Randomly rotate all the position around the center by moving the parent of the posistions
            transform.Rotate(Vector3.up, Random.Range(0,359));
            SpawnSecondsIslands();
        }

        private void SpawnSecondsIslands()
        {
            for (var index = 0; index < numberOfPlayers; index++)
            {
                var island = _allIslands[index];
                Vector3 islandPos = island.transform.position;
                Vector3 pos = new Vector3(islandPos.x + RandowMinDist(), islandPos.y, islandPos.z + RandowMinDist());
                SpawnIsland(pos, IslandTypesEnum.Basic);
            }

            SpawnThirdsIslands();
        }
        
        private void SpawnThirdsIslands()
        {
            for (var index = 0; index < numberOfPlayers; index++)
            {
                var island = _allIslands[index];
                Vector3 islandPos = island.transform.position;
                Vector3 pos = new Vector3(islandPos.x + RandowMinDist() *2, islandPos.y, islandPos.z + RandowMinDist() *2);
                SpawnIsland(pos, IslandTypesEnum.Basic);
            }

            SpawnOtherIslands();
        }

        private float RandowMinDist()
        {
            float a = Random.Range(0f, 1f);
            return a >= 0.5f ? -minDistBetweenIslands : minDistBetweenIslands;
        }
        
        private void SpawnOtherIslands()
        {
            for (int i = 0; i < numberOfIslands; i++)
            {
                if (_currentlyPlacedSpecialIslands < maxSpecialIslands)
                {
                    int r = Random.Range(1, 101);

                    foreach (var islandTypes in  _worldManager.islandTypes)
                    {
                        if (!(r >= islandTypes.rarity.x) || !(r <= islandTypes.rarity.y)) continue;
                        SpawnIsland(NewIslandPos(), islandTypes.type);
                        if (islandTypes.type != IslandTypesEnum.Basic) _currentlyPlacedSpecialIslands++;
                    }
                }
                else SpawnIsland(NewIslandPos(),IslandTypesEnum.Basic);
            }
        }

        private void SpawnIsland(Vector3 position, IslandTypesEnum type)
        {
            NetworkObject island = _networkManager.RPC_SpawnNetworkObject(
                islandPrefab, 
                position, 
                Quaternion.identity,
                Object.StateAuthority, 
                Runner);
            
            island.transform.parent = transform;
            Island islandComponent = island.GetComponent<Island>();
            islandComponent.NetworkType = type;
            _allIslands.Add(islandComponent);
        }

        private Vector3 NewIslandPos()
        {
            Vector3 possiblePos = CalculateNewRandomPos();
                
            foreach (var island in _allIslands)
            {
                if (Vector3.Distance(island.transform.position, possiblePos) < minDistBetweenIslands)
                {
                    return NewIslandPos();
                }
            }

            return possiblePos;
        }

        private Vector3 CalculateNewRandomPos()
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            float distance = islandDistFormCenterRepartition.Evaluate(Random.Range(0f,1f)) * outerBorderRadius;

            float x = distance * Mathf.Cos(angle);
            float z = distance * Mathf.Sin(angle);
            
            return new Vector3(x, 0, z);;
        }
    }
}