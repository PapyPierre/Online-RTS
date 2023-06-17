using System;
using Fusion;
using NaughtyAttributes;
using UnityEngine;

namespace Buildings
{
    public class BaseBuilding : NetworkBehaviour
    {
        [SerializeField, Expandable] private BuildingData data;
        private BuildingsManager _buildingsManager;

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;
        
        private void Awake()
        {
            _maxHealth = data.MaxHealthPoints;
            currentHealthPoint = data.MaxHealthPoints;
        }
        
        public override void Spawned()
        {
            _buildingsManager = BuildingsManager.Instance;
            
            if (data.UnlockedBuildings.Length > 0)
            {
                UnlockBuildings();
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
    }
}
