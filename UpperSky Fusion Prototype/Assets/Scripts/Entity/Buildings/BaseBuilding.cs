using System;
using System.Collections;
using System.Collections.Generic;
using AOSFogWar;
using AOSFogWar.Used_Scripts;
using Custom_UI;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Player;
using UnityEngine;
using World.Island;

namespace Entity.Buildings
{
    public class BaseBuilding : BaseEntity
    {
        private BuildingsManager _buildingsManager;
        private UIManager _uiManager;
        
        [field: SerializeField, Expandable] public BuildingData Data { get; private set; }

        [HideInInspector] public Island myIsland;

        public bool isStartBuilding;
        
        // Génération de ressources
        private float _tempMatToGenerate;
        private float _tempOrichalqueToGenerate;

        // Formation d'unités
        [HideInInspector] public bool isOpen;
        public Queue<UnitsManager.AllUnitsEnum> FormationQueue = new ();
        [HideInInspector] public float timeLeftToForm;

        // Defense
        private List<BaseUnit> _enemyInRange = new();
        private bool _isReadyToShoot = true;

        public override void Spawned()
        {
            base.Spawned(); 
            
            SetUpHealtAndArmor(Data);

            _buildingsManager = BuildingsManager.Instance;
            _uiManager = UIManager.Instance;
        }

        public void Init(Island buildOnThisIsland, bool startBuilding = false)
        {
            Owner = buildOnThisIsland.Owner;
            myIsland = buildOnThisIsland;
            isStartBuilding = startBuilding;
            
            if (PlayerIsOwner())
            {
                if (!HasStateAuthority) Object.RequestStateAuthority();
                var fogRevealer = new FogOfWar.FogRevealer(transform, Data.SightRange, true);
                FogRevealerIndex = FogOfWar.AddFogRevealer(fogRevealer);
            }
            else if (isStartBuilding) RPC_StartBuildingInit();
            
            if (Data.UnlockedBuildings.Length > 0) UnlockBuildings();
            
            if (Data.DoesGenerateRessources && Data.AditionnalMaxSupplies > 0)
            {
                //TODO eventually add a check for game max supply if there's one
                Owner.ressources.CurrentMaxSupply += Data.AditionnalMaxSupplies;
            }
            
            StartCoroutine(CallEveryRealTimeSeconds());
        }
        
        [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
        public void RPC_StartBuildingInit()
        {
            // The code inside here will run on the client which has input authority.

            var fogRevealer = new FogOfWar.FogRevealer(transform, Data.SightRange, true);
            FogRevealerIndex = FogOfWar.AddFogRevealer(fogRevealer);
        }
        

        private void Update()
        {
            if (Data.IsFormationBuilding)
            {
                if (MouseAboveThisEntity() && PlayerIsOwner())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        isOpen = true;
                        _uiManager.OpenFormationBuilding(Data.ThisBuilding, this);
                        UnitsManager.UnSelectAllUnits();
                    }
                }

                if (FormationQueue.Count > 0) ManageFormation();
            }

            if (Data.IsDefenseBuilding && TargetedEntity is not null) ShootAtTarget();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Data.IsDefenseBuilding) return;
            
            if (other.CompareTag("Unit"))
            {
                var unit = other.GetComponent<BaseUnit>();
                if (unit.Owner == Owner) return;
                
                _enemyInRange.Add(unit);
                TargetedEntity ??= unit;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Data.IsDefenseBuilding) return;

            if (other.CompareTag("Unit"))
            {
                var unit = other.GetComponent<BaseUnit>();
                if (unit.Owner == Owner) return;

                if (_enemyInRange.Contains(unit)) _enemyInRange.Remove(unit);

                if (unit == TargetedEntity)
                {
                    if (_enemyInRange.Count > 0)
                    {
                        TargetedEntity = _enemyInRange[0];
                    }
                    else TargetedEntity = null;
                }
            }
        }

        private void ShootAtTarget()
        {
            if (!_isReadyToShoot) return;

            int damageOnUnits = Data.DamagePerShootOnUnits; 
            int armorPenetration = Data.ArmorPenetration;

            float damageOnHealth =  armorPenetration / 100f * damageOnUnits;
            float damageOnArmor = (100f - armorPenetration) / 100f * damageOnUnits;

            TargetedEntity.RPC_TakeDamage(damageOnHealth, damageOnArmor,  this);
            if (TargetedEntity is BaseUnit unit) unit.ReactToDamage();
            
            _isReadyToShoot = false;
            StartCoroutine(Reload());
        }
        
        private IEnumerator Reload()
        {
            yield return new WaitForSecondsRealtime(Data.RealodTime);
            _isReadyToShoot = true;
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

            obj.GetComponent<BaseUnit>().Init(Owner);
            
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
                if (_uiManager.CurrentlyOpenBuilding == this)
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
        
        protected override void OnMouseEnter()
        {
            base.OnMouseEnter();
            
            if (PlayerIsOwner()) SetActiveSelectionCircle(true);
        }
        
        protected override void OnMouseExit()
        {
            base.OnMouseExit();

            if (PlayerIsOwner())
            {
                if (Data.IsFormationBuilding)
                {
                    if (!isOpen) SetActiveSelectionCircle(false);
                }
                else SetActiveSelectionCircle(false);
            }
        }
    }
}
