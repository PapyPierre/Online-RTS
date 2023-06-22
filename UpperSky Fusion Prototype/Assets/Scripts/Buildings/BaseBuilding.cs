using System;
using Fusion;
using NaughtyAttributes;
using Network;
using UnityEngine;
using World.Island;

namespace Buildings
{
    public class BaseBuilding : NetworkBehaviour
    {
        [SerializeField, Expandable] private BuildingData data;
        private BuildingsManager _buildingsManager;
        private NetworkManager _networkManager;

        private Island _myIsland;

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;

       [SerializeField] private float _tempMatToGenerate;
       [SerializeField] private float _tempOrichalqueToGenerate;
        
        private void Awake()
        {
            _maxHealth = data.MaxHealthPoints;
            currentHealthPoint = data.MaxHealthPoints;
        }
        
        public override void Spawned()
        {
            _buildingsManager = BuildingsManager.Instance;
            _networkManager = NetworkManager.Instance;

            var pos = transform.position;
            Ray ray = new Ray(new Vector3(pos.x, pos.y + 3, pos.z), -transform.up); 
            RaycastHit hit;

            Physics.Raycast(ray, out hit, 5000, _buildingsManager.terrainLayer);
            if (hit.collider is null)
            {
                Debug.LogError("Building didn't detect his island");
                return;
            }
            
            _myIsland = hit.collider.GetComponentInParent<Island>();

            if (_myIsland.Owner != _networkManager.thisPlayer)
            {
                Debug.Log("blou");
                return;
            }
            
            if (data.UnlockedBuildings.Length > 0)
            {
                UnlockBuildings();
            }
            
            if (data.DoesGenerateRessources)
            {
                InvokeRepeating("GenerateRessourcesEachSecond", 1,1);
                
                //TODO eventually add a check for game max supply if there's one
                _myIsland.Owner.ressources.CurrentMaxSupply += data.AditionnalMaxSupplies;
            }
        }

        private void UnlockBuildings()
        {
            foreach (var building in data.UnlockedBuildings)
            {
                switch (building)
                {
                    case BuildingsManager.AllBuildingsEnum.Menuiserie:
                        _buildingsManager.allBuildingsIcons[3].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.Tisserand: 
                        _buildingsManager.allBuildingsIcons[4].Unlock();
                        break;
                    case BuildingsManager.AllBuildingsEnum.Fonderie:
                        _buildingsManager.allBuildingsIcons[5].Unlock();
                        break;
                    case BuildingsManager.AllBuildingsEnum.Manufacture:
                        _buildingsManager.allBuildingsIcons[6].Unlock();
                            break; 
                    case BuildingsManager.AllBuildingsEnum.Baliste:
                        _buildingsManager.allBuildingsIcons[7].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.Canon:
                        _buildingsManager.allBuildingsIcons[8].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.MineOrichalque: 
                        _buildingsManager.allBuildingsIcons[9].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.Obusier:
                        _buildingsManager.allBuildingsIcons[10].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.CentreMeteo: 
                        _buildingsManager.allBuildingsIcons[11].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.Usine: 
                        _buildingsManager.allBuildingsIcons[12].Unlock();
                            break;
                    case BuildingsManager.AllBuildingsEnum.Academie: 
                        _buildingsManager.allBuildingsIcons[13].Unlock();
                            break;
                    
                    case BuildingsManager.AllBuildingsEnum.Foreuse:
                    case BuildingsManager.AllBuildingsEnum.ExploitationOrichalque:
                    case BuildingsManager.AllBuildingsEnum.Habitation:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        private void GenerateRessourcesEachSecond()
        {
            _tempMatToGenerate += data.GeneratedMaterialPerSeconds;
            _tempOrichalqueToGenerate += data.GeneratedOrichalquePerSeconds;

            if (_tempMatToGenerate >= 1 )
            {
                int x = Mathf.FloorToInt(_tempMatToGenerate);
                _myIsland.Owner.ressources.CurrentMaterials += x;
                _tempMatToGenerate -= x;
            }

            if (_tempOrichalqueToGenerate >= 1)
            {
                int y = Mathf.FloorToInt(_tempOrichalqueToGenerate);
                _myIsland.Owner.ressources.CurrentOrichalque += y;
                _tempOrichalqueToGenerate -= y;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, -transform.up * 10);
        }
    }
}
