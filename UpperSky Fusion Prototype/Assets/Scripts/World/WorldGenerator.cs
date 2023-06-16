using System.Net.NetworkInformation;
using Fusion;
using Network;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

// Script responsible for procedural world generation

namespace World
{
    public class WorldGenerator : MonoBehaviour
    {
        private WorldManager _worldManager;
        private NetworkManager _networkManager;

        private int _numberOfPlayers;
        private int _numberOfIslands;
        private int _maxSpecialIslands;
        private int _currentlyPlacedSpecialIslands;

        private void Start()
        {
            _worldManager = GetComponent<WorldManager>();
            _networkManager = NetworkManager.Instance;
        }
        
        public void GenerateWorld(int nbOfPlayers)
        {
            _numberOfPlayers = nbOfPlayers;
            _numberOfIslands = _numberOfPlayers * 3;
    
            // ReSharper disable once PossibleLossOfFraction
            _maxSpecialIslands = _numberOfPlayers - Mathf.RoundToInt(_numberOfPlayers/4);

            CheckForReset();

            CalculatePlayersPosOnInnerBorder();
        }

        private void CheckForReset()
        {
            if (_worldManager.allIslands.Count > 0)
            {
                foreach (var island in _worldManager.allIslands)
                {
                    island.gameObject.SetActive(false);
                }
                
                _worldManager.allIslands.Clear();
                _currentlyPlacedSpecialIslands = 0;
            }
        }

        // Le but de cette fonction est de calculé x positions equidistante sur la bordure du cercle
        private void CalculatePlayersPosOnInnerBorder()
        {
            // Calcule du périmètre
            float innerBorderPerimeter = 2 * Mathf.PI * _worldManager.innerBorderRadius;
            
            // Calcule de la distance entre chaque joueur raporté sur le périmètre
            float distancePerPlayer = innerBorderPerimeter / _numberOfPlayers;
            
            // Calcule de l'angle en radian avec cette distance sur le périmètre du cercle
            float radianAngle = (distancePerPlayer / innerBorderPerimeter) * 2 * Mathf.PI;
            
            // Conversion en degré
            float degreeAngle = radianAngle * 180 / Mathf.PI;
            
            SpawnPlayerPosAtEachAngle(degreeAngle);
        }

        private void SpawnPlayerPosAtEachAngle(float angle)
        {
           NetworkObject networkObject =  SpawnIsland(new Vector3(0, 0, _worldManager.innerBorderRadius),
               IslandTypesEnum.Starting, _networkManager.connectedPlayers[0].MyPlayerRef);

           _networkManager.connectedPlayers[0].transform.parent = networkObject.transform;
           _networkManager.connectedPlayers[0].transform.localPosition = new Vector3(0, 5, 0);
           
            for (int i = 0; i < _numberOfPlayers -1; i++)
            {
                transform.Rotate(Vector3.up, angle);
                NetworkObject networkObject2 = SpawnIsland(new Vector3(0, 0, _worldManager.innerBorderRadius), 
                    IslandTypesEnum.Starting, _networkManager.connectedPlayers[i + 1].MyPlayerRef);
                
                _networkManager.connectedPlayers[i+1].transform.parent = networkObject2.transform;
                _networkManager.connectedPlayers[i+1].transform.localPosition = new Vector3(0, 5, 0);

            }
            
            // Randomly rotate all the position around the center by moving the parent of the posistions
            transform.Rotate(Vector3.up, Random.Range(0,359));
            
            UpdatePlayersCam();
            SpawnSecondsIslands();
        }

        private void UpdatePlayersCam()
        {
            foreach (var playerController in _networkManager.connectedPlayers)
            {
                playerController.cam.transform.position = playerController.transform.position;
            }
        }
        
        private void SpawnSecondsIslands()
        {
            for (var index = 0; index < _numberOfPlayers; index++)
            {
                var island = _worldManager.allIslands[index];
                Vector3 islandPos = island.transform.position;
                Vector3 pos = new Vector3(islandPos.x + RandomMinDist(), islandPos.y, islandPos.z + RandomMinDist());
                SpawnIsland(pos, IslandTypesEnum.Basic, PlayerRef.None);
            }

            SpawnThirdsIslands();
        }
        
        private void SpawnThirdsIslands()
        {
            for (var index = 0; index < _numberOfPlayers; index++)
            {
                var island = _worldManager.allIslands[index];
                Vector3 islandPos = island.transform.position;
                Vector3 pos = new Vector3(islandPos.x + RandomMinDist() *2, islandPos.y, islandPos.z + RandomMinDist() *2);
                SpawnIsland(pos, IslandTypesEnum.Basic, PlayerRef.None);
            }

            SpawnOtherIslands();
        }

        private float RandomMinDist()
        {
            float a = Random.Range(0f, 1f);
            return a >= 0.5f ? -_worldManager.minDistBetweenIslands : _worldManager.minDistBetweenIslands;
        }
        
        private void SpawnOtherIslands()
        {
            for (int i = 0; i < _numberOfIslands; i++)
            {
                if (_currentlyPlacedSpecialIslands < _maxSpecialIslands)
                {
                    int r = Random.Range(1, 101);

                    foreach (var islandTypes in  _worldManager.islandTypes)
                    {
                        if (!(r >= islandTypes.rarity.x) || !(r <= islandTypes.rarity.y)) continue;
                        SpawnIsland(NewIslandPos(), islandTypes.type, PlayerRef.None);
                        if (islandTypes.type != IslandTypesEnum.Basic) _currentlyPlacedSpecialIslands++;
                    }
                }
                else SpawnIsland(NewIslandPos(),IslandTypesEnum.Basic, PlayerRef.None);
            }
        }

        private NetworkObject SpawnIsland(Vector3 position, IslandTypesEnum type, PlayerRef owner)
        {
            NetworkObject islandObject = _networkManager.RPC_SpawnNetworkObject(
                _worldManager.islandPrefab, position, Quaternion.identity, owner);
            Island.Island islandComponent = islandObject.GetComponent<Island.Island>();
            islandComponent.transform.parent = transform;
            islandComponent.owner = owner;
            islandComponent.ownerId = owner.PlayerId;

            islandComponent.NetworkType = type;
            _worldManager.allIslands.Add(islandComponent);

            return islandObject;
        }

        private Vector3 NewIslandPos()
        {
            Vector3 possiblePos = CalculateNewRandomPos();
                
            foreach (var island in _worldManager.allIslands)
            {
                if (Vector3.Distance(island.transform.position, possiblePos) < _worldManager.minDistBetweenIslands)
                {
                    return NewIslandPos();
                }
            }

            return possiblePos;
        }

        private Vector3 CalculateNewRandomPos()
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            float distance = _worldManager.islandDistFormCenterRepartition.Evaluate(Random.Range(0f,1f)) * _worldManager.outerBorderRadius;

            float x = distance * Mathf.Cos(angle);
            float z = distance * Mathf.Sin(angle);
            
            return new Vector3(x, 0, z);
        }
    }
}