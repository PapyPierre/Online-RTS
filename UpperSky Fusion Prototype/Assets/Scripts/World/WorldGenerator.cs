using System.Collections;
using Element.Entity.Buildings;
using Element.Island;
using Entity.Buildings;
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
        private Transform _worldCenter;
        
        private int _numberOfPlayers;
        private int _numberOfIslandsPerPlayer;
        private int _maxSpecialIslands;
        
        private int _currentlyPlacedIslands;
        private int _currentlyPlacedSpecialIslands;

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
            _maxSpecialIslands = maxSpecialIslandsPerPlayer * nbOfPlayers;

            CheckForReset();

            _worldCenter = _gameManager.thisPlayer.Runner.Spawn(
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
            
            // Conversion en degrée
            float degreeAngle = radianAngle * 180 / Mathf.PI;
            
            SpawnPlayerPosAtEachAngle(degreeAngle);
        }

        private void SpawnPlayerPosAtEachAngle(float angle)
        {
           SpawnIsland(new Vector3(0, 0, _worldManager.innerBorderRadius), 
               IslandTypesEnum.Home, _gameManager.connectedPlayers[0]);
           
           for (int i = 0; i < _numberOfPlayers -1; i++) 
           {
               _worldCenter.Rotate(Vector3.up, angle);
                
              SpawnIsland(new Vector3(0, 0, _worldManager.innerBorderRadius), 
                  IslandTypesEnum.Home, _gameManager.connectedPlayers[i + 1]);
           }
            
            // Randomly rotate all the position around the center by moving the parent of the posistions
            _worldCenter.Rotate(Vector3.up, Random.Range(0f,179f));
            
            SpawnSecondsIslands();
        }

        private void SpawnSecondsIslands()
        {
            for (var index = 0; index < _numberOfPlayers; index++)
            {
                var island = _worldManager.allIslands[index];
                Vector3 islandPos = island.transform.position;
                Vector3 pos = new Vector3(islandPos.x + RandomMinDist(), islandPos.y, islandPos.z + RandomMinDist());
                SpawnIsland(pos, IslandTypesEnum.Basic);
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
                if (_currentlyPlacedSpecialIslands < _maxSpecialIslands)
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

            if (readyPlayersIndex == _gameManager.expectedNumberOfPlayers)
            {
                _gameManager.StartGame();
            }
        }
        
        private void SpawnIsland(Vector3 position, IslandTypesEnum type, PlayerController owner = null)
        {
            NetworkObject islandObject = _gameManager.thisPlayer.Runner.Spawn(
               _worldManager.islandPrefabs[owner != null ? 1 : 0], 
                position, 
                Quaternion.identity,
                owner != null ? owner.Object.StateAuthority : PlayerRef.None);
            
            islandObject.GetComponent<BaseIsland>().Init(_worldCenter, owner);

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