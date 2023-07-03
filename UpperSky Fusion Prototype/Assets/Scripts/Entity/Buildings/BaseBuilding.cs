using System.Collections;
using System.Collections.Generic;
using Custom_UI;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace Entity.Buildings
{
    public class BaseBuilding : BaseEntity
    {
        [field: SerializeField, Expandable] public BuildingData Data { get; private set; }
        
        private BuildingsManager _buildingsManager;
        private UIManager _uiManager;

        private float _tempMatToGenerate;
        private float _tempOrichalqueToGenerate;

        public Queue<UnitsManager.AllUnitsEnum> FormationQueue = new ();
        [HideInInspector] public float timeLeftToForm;

        private bool _mouseOverThisBuilding;

        public override void Spawned()
        {
            base.Spawned(); 
            
            SetUpHealtAndArmor(Data);

            _buildingsManager = BuildingsManager.Instance;
            _uiManager = UIManager.Instance;
        }

        public void Init(PlayerController owner)
        {
            Owner = owner;
            
            if (Data.UnlockedBuildings.Length > 0) UnlockBuildings();
            
            if (Data.DoesGenerateRessources && Data.AditionnalMaxSupplies > 0)
            {
                //TODO eventually add a check for game max supply if there's one
                Owner.ressources.CurrentMaxSupply += Data.AditionnalMaxSupplies;
            }

            StartCoroutine(CallEveryRealTimeSeconds());
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
                        UnitsManager.UnSelectAllUnits();
                    }
                }

                if (FormationQueue.Count > 0)
                {
                    ManageFormation();
                }
            }
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        private IEnumerator CallEveryRealTimeSeconds()
        {
            yield return new WaitForSecondsRealtime(1);
            
            if (Data.DoesGenerateRessources) GenerateRessources();
            if (Data.IsFormationBuilding) UpdateFormation();
            
            StartCoroutine(CallEveryRealTimeSeconds());
        }
        
        private void UnlockBuildings()
        {
            foreach (var building in Data.UnlockedBuildings)
            {
                _buildingsManager.allBuildingsIcons[(int) building].Unlock();
            }
        }
        
        private void GenerateRessources()
        {
            _tempMatToGenerate += Data.GeneratedMaterialPerSeconds;
            _tempOrichalqueToGenerate += Data.GeneratedOrichalquePerSeconds;

            if (_tempMatToGenerate >= 1 )
            {
                int x = Mathf.FloorToInt(_tempMatToGenerate);
                Owner.ressources.CurrentMaterials += x;
                _tempMatToGenerate -= x;
            }

            if (_tempOrichalqueToGenerate >= 1)
            {
                int y = Mathf.FloorToInt(_tempOrichalqueToGenerate);
                Owner.ressources.CurrentOrichalque += y;
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
            var prefab = UnitsManager.allUnitsPrefab[(int) FormationQueue.Dequeue()];
            
            Vector3 myPos = transform.position;
            Vector3 spawnPos = new Vector3(myPos.x, UnitsManager.flyingHeightOfUnits, myPos.z);
            NetworkObject obj = Runner.Spawn(prefab, spawnPos, Quaternion.identity, Object.StateAuthority);

            obj.GetComponent<BaseUnit>().Owner = Owner;
            obj.GetComponent<BaseUnit>().Colorize();
            
            if (FormationQueue.Count > 0)
            {
                timeLeftToForm = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
            }
            
            _uiManager.UpdateFormationQueueDisplay();
        }
        
        private void UpdateFormation()
        {
            timeLeftToForm--;

            if (FormationQueue.Count > 0)
            {
                if (_uiManager.CurrentlyOpenFormationBuilding == this)
                {
                    UpdateFormationQueueSliderWithNewValue();
                }
            }
        }

        public void UpdateFormationQueueSliderWithNewValue()
        {
            var totalTimeToRun = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
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
    }
}
