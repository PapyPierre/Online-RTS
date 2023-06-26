using System.Collections;
using System.Collections.Generic;
using Custom_UI;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Network;
using UnityEngine;
using World.Island;

namespace Entity.Buildings
{
    public class BaseBuilding : NetworkBehaviour
    {
        [field: SerializeField, Expandable] public BuildingData Data { get; private set; }
        private BuildingsManager _buildingsManager;
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        private UnitsManager _unitsManager;

        private Island _myIsland;

        private int _maxHealth;
        [SerializeField, ProgressBar("Health", "_maxHealth", EColor.Red)] 
        private int currentHealthPoint;

        private float _tempMatToGenerate;
        private float _tempOrichalqueToGenerate;

        public Queue<UnitsManager.AllUnitsEnum> FormationQueue = new ();
        public float timeLeftToForm;

        private bool _mouseOverThisBuilding;
        
        private void Awake()
        {
            _maxHealth = Data.MaxHealthPoints;
            currentHealthPoint = Data.MaxHealthPoints;
        }
        
        public override void Spawned()
        {
            _buildingsManager = BuildingsManager.Instance;
            _networkManager = NetworkManager.Instance;
            _uiManager = UIManager.Instance;
            _unitsManager = UnitsManager.Instance;
            
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

            if (!PlayerIsOwner()) return;
            
            if (Data.UnlockedBuildings.Length > 0)
            {
                UnlockBuildings();
            }
            
            if (Data.DoesGenerateRessources)
            {
                InvokeRepeating("GenerateRessourcesEachSecond", 1,1);
                
                //TODO eventually add a check for game max supply if there's one
                _myIsland.Owner.ressources.CurrentMaxSupply += Data.AditionnalMaxSupplies;
            }

            if (Data.IsFormationBuilding) StartCoroutine(CallEveryRealTimeSeconds());
        }

        private void Update()
        {
            if (Data.IsFormationBuilding)
            {
                if (_mouseOverThisBuilding && PlayerIsOwner())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _uiManager.OpenFormationBuilding(Data.ThisBuilding, this);
                    }
                }

                if (FormationQueue.Count > 0)
                {
                    ManageFormation();
                }
            }
        }

        private bool PlayerIsOwner()
        {
            return _myIsland.Owner == _networkManager.thisPlayer;
        }

        private void UnlockBuildings()
        {
            foreach (var building in Data.UnlockedBuildings)
            {
                _buildingsManager.allBuildingsIcons[(int) building].Unlock();
            }
        }
        
        private void GenerateRessourcesEachSecond()
        {
            _tempMatToGenerate += Data.GeneratedMaterialPerSeconds;
            _tempOrichalqueToGenerate += Data.GeneratedOrichalquePerSeconds;

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

        
        private void ManageFormation()
        {
            if (timeLeftToForm <= 0)
            {
                timeLeftToForm = 100; // Par sécurité
                FormFirstUnitInQueue();
            }
        }

        private void FormFirstUnitInQueue()
        {
            // faire spawn la premier unité dans la queue
            var prefab = _unitsManager.allUnitsPrefab[(int) FormationQueue.Dequeue()];
            
            Vector3 myPos = transform.position;
            Vector3 spawnPos = new Vector3(myPos.x, _unitsManager.flyingHeightOfUnits, myPos.z);
            _networkManager.thisPlayer.RPC_SpawnNetworkObj(prefab, spawnPos, Quaternion.identity);
            
            if (FormationQueue.Count > 0)
            {
                timeLeftToForm = _unitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
            }
            
            _uiManager.UpdateFormationQueueDisplay();
        }

        private IEnumerator CallEveryRealTimeSeconds()
        {
            yield return new WaitForSecondsRealtime(1);
            timeLeftToForm--;

            if (FormationQueue.Count > 0)
            {
                if (_uiManager.CurrentlyOpenFormationBuilding == this)
                {
                    UpdateFormationQueueSliderWithNewValue();
                }
            }
            
            StartCoroutine(CallEveryRealTimeSeconds());
        }

        public void UpdateFormationQueueSliderWithNewValue()
        {
            var totalTimeToRun = _unitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
            var timeSpendAlready = totalTimeToRun - timeLeftToForm;
            var timeSpentOnTotalTime = timeSpendAlready / totalTimeToRun;
            
            _uiManager.UpdateFormationQueueSlider(timeSpentOnTotalTime);
        }
        
        private void OnMouseEnter()
        {
            _mouseOverThisBuilding = true;
        }

        private void OnMouseExit()
        {
            _mouseOverThisBuilding = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, -transform.up * 10);
        }
    }
}
