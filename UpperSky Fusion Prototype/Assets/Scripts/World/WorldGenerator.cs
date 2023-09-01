using System.Collections;
using Element.Entity.Buildings;
using Fusion;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

// Script responsible for procedural world generation

namespace World
{
    public class WorldGenerator : MonoBehaviour
    {
        private WorldManager _worldManager;
        private GameManager _gameManager;
        private BuildingsManager _buildingsManager;

        [SerializeField] private NetworkPrefabRef worldCenterPrefab;
        [HideInInspector] public Transform worldCenter;
        
        private int _numberOfPlayers;
        private int _numberOfIslandsPerPlayer;

        private int _currentlyPlacedIslands;

        private void Start()
        {
            _worldManager = GetComponent<WorldManager>();
            _gameManager = GameManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
        }
        
        public void GenerateWorld(int nbOfPlayers, int nbOfIslandsPerPlayer, int maxSpecialIslandsPerPlayer)
        {
            _numberOfPlayers = nbOfPlayers;
            _numberOfIslandsPerPlayer = nbOfIslandsPerPlayer;

            CheckForReset();

            worldCenter = _gameManager.thisPlayer.Runner.Spawn(
                worldCenterPrefab, Vector3.zero, Quaternion.identity, PlayerRef.None).transform;

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
            
            // Conversion en degrée
            float degreeAngle = radianAngle * 180 / Mathf.PI;
            
            SpawnPlayerPosAtEachAngle(degreeAngle);
        }

        private void SpawnPlayerPosAtEachAngle(float angle)
        {
            Vector3 playerIslandsSpawnPos = new Vector3(0, 0, _worldManager.innerBorderRadius);
            
           SpawnIsland(playerIslandsSpawnPos, IslandTypesEnum.Meadow, _gameManager.connectedPlayers[0]);
           
           for (int i = 0; i < _numberOfPlayers -1; i++) 
           {
               worldCenter.Rotate(Vector3.up, angle);
                
              SpawnIsland(playerIslandsSpawnPos, IslandTypesEnum.Meadow, _gameManager.connectedPlayers[i + 1]);
           }
            
            // Randomly rotate all the position around the center by moving the parent of the posistions
            worldCenter.Rotate(Vector3.up, Random.Range(0f,179f));
            
          SpawnSecondsIslands();
        }

        private void SpawnSecondsIslands()
        {
            for (var index = 0; index < _numberOfPlayers; index++)
            {
                var island = _worldManager.allIslands[index];
                Vector3 islandPos = island.transform.position;
                
                Vector3 pos = new Vector3(islandPos.x + RandomMinDist(), islandPos.y, islandPos.z + RandomMinDist());
                SpawnIsland(pos, RandomIslandType());
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
            int numberOfIslandToSpawn = _numberOfIslandsPerPlayer * _numberOfPlayers - _currentlyPlacedIslands;
            
            for (int i = 0; i < numberOfIslandToSpawn; i++)
            {
                SpawnIsland(NewIslandPos(), RandomIslandType());
            }
            
            _gameManager.thisPlayer.MakesPlayerReady();
            
            StartCoroutine(WaitForEndOfGeneration());
        }

        private IEnumerator WaitForEndOfGeneration()
        {
            yield return new WaitForSecondsRealtime(1f);
            
            var readyPlayersIndex = 0;
            
            foreach (var player in  _gameManager.connectedPlayers)
            {
                if (player.IsReadyToPlay)
                {
                    readyPlayersIndex++;
                }
            }

            if (readyPlayersIndex == _gameManager.expectedNumberOfPlayers) _gameManager.StartGame();
        }
        
        private void SpawnIsland(Vector3 position, IslandTypesEnum type, PlayerController owner = null)
        {
            _worldManager.islandGenerator.GenerateIsland(position, type, owner);
            _currentlyPlacedIslands++;
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
        
        private IslandTypesEnum RandomIslandType()
        {
            float maxValue = 0;
            
            foreach (var data in _worldManager.allIslandsData)
            {
                maxValue += data.Rarity;
            }
            
            float randomValue = Random.Range(0f, maxValue);
            
            foreach (var data in _worldManager.allIslandsData)
            {
                if (randomValue <= data.Rarity) return data.Type;
                else randomValue -= data.Rarity;
            }

            return IslandTypesEnum.Meadow;
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