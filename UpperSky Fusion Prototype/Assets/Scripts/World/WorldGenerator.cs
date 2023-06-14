using Fusion;
using Network;
using UnityEngine;
using Random = UnityEngine.Random;

// Script responsible for procedural world generation

namespace World
{
    public class WorldGenerator : NetworkBehaviour
    {
        public static WorldGenerator Instance;
        private WorldManager _worldManager;
        private NetworkManager _networkManager;

        public float innerBorderRadius;
        public float outerBorderRadius;

        private int _numberOfPlayers;
        private int _numberOfIslands;
        [SerializeField] private float minDistBetweenIslands;
        [SerializeField] private AnimationCurve islandDistFormCenterRepartition;
        private int _maxSpecialIslands;
        private int _currentlyPlacedSpecialIslands;
        
        [SerializeField] private NetworkPrefabRef islandPrefab;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        } 

        public override void Spawned()
        {
            _worldManager = WorldManager.instance;
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
            float innerBorderPerimeter = 2 * Mathf.PI * innerBorderRadius;
            
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
            SpawnIsland(new Vector3(0, 0, innerBorderRadius), IslandTypesEnum.Starting, 
                _networkManager.ConnectedPlayers[0].Ref);

            for (int i = 0; i < _numberOfPlayers -1; i++)
            {
                transform.Rotate(Vector3.up, angle);
                SpawnIsland(new Vector3(0, 0, innerBorderRadius), IslandTypesEnum.Starting, 
                    _networkManager.ConnectedPlayers[i + 1].Ref);
            }
            
            // Randomly rotate all the position around the center by moving the parent of the posistions
            transform.Rotate(Vector3.up, Random.Range(0,359));
            SpawnSecondsIslands();
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
            return a >= 0.5f ? -minDistBetweenIslands : minDistBetweenIslands;
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
            
            TeleportPlayerToTheirIslands();
        }

        private void TeleportPlayerToTheirIslands()
        {
            foreach (var island in _worldManager.allIslands)
            {
                if (island.owner == PlayerRef.None) continue;
                
                
                for (int i = 0; i < _networkManager.ConnectedPlayers.Count; i++)
                {
                    if (island.owner == _networkManager.ConnectedPlayers[i].Ref)
                    {
                        Vector3 islandPos = island.transform.position;
                        
                        Vector3 currentPlayerPos = _networkManager.ConnectedPlayers[i].Controller.cam.transform.position;

                        currentPlayerPos = new Vector3(islandPos.x, currentPlayerPos.y, islandPos.z);

                        _networkManager.ConnectedPlayers[i].Controller.cam.transform.position = currentPlayerPos;
                        Debug.Log("read");

                    }
                }
            }
        }

        private void SpawnIsland(Vector3 position, IslandTypesEnum type, PlayerRef owner)
        {
            NetworkObject island = _networkManager.RPC_SpawnNetworkObject(
                islandPrefab, 
                position, 
                Quaternion.identity,
                Object.StateAuthority, 
                Runner);
            
            island.transform.parent = transform;
            Island.Island islandComponent = island.GetComponent<Island.Island>();
            islandComponent.owner = owner;
            islandComponent.NetworkType = type;
            _worldManager.allIslands.Add(islandComponent);
        }

        private Vector3 NewIslandPos()
        {
            Vector3 possiblePos = CalculateNewRandomPos();
                
            foreach (var island in _worldManager.allIslands)
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
            
            return new Vector3(x, 0, z);
        }
    }
}