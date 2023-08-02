using System.Collections;
using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Element.Entity.Military_Units;
using Element.Island;
using Entity.Buildings;
using Fusion;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UserInterface;
using Random = UnityEngine.Random;

namespace Element.Entity.Buildings
{
    public class BaseBuilding : BaseEntity
    {
        private BuildingsManager _buildingsManager;
        private UIManager _uiManager;
        
        [field: SerializeField, Expandable] public BuildingData Data { get; private set; }

        [HideInInspector] public BaseIsland myIsland;

        public bool isStartBuilding;

        // Formation d'unités
        public Queue<UnitsManager.AllUnitsEnum> FormationQueue = new ();
        [HideInInspector] public float timeLeftToForm;
        private bool _NotEnoughSupplies;

        // Defense
        [SerializeField] private Transform canonHead;
        [SerializeField] private float headAngularSpeed;
        private bool _isReadyToShoot = true;

        public override void Spawned()
        {
            base.Spawned(); 
            
            SetUpHealtAndArmor(Data);

            _buildingsManager = BuildingsManager.Instance;
            _uiManager = UIManager.Instance;
        }

        public void Init(BaseIsland buildOnThisIsland, bool startBuilding = false)
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
            
            if (Data.DoesGenerateRessources)
            {
                Owner.ressources.CurrentWoodGain += Data.GeneratedWoodPerSeconds;
                Owner.ressources.CurrentMetalsGain += Data.GeneratedMetalsPerSeconds;
                Owner.ressources.CurrentOrichalqueGain += Data.GeneratedOrichalquePerSeconds;
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
                if (FormationQueue.Count > 0) ManageFormation();
            }

            if (Data.IsDefenseBuilding)
            {
                if (TargetedEntity == null || TargetedEntity.isDead)
                {
                    FindTarget();
                    return;
                }
                
                AimAtTarget(TargetedEntity.transform, canonHead);
                ShootAtTarget();
            }
        }

        public void FindTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Data.ShootingRange, 
                LayerMask.GetMask("Units"));

            foreach (var col in colliders)
            {
                var unit = col.GetComponent<BaseUnit>();
                if (unit.Owner == Owner || unit.isDead) continue;
                SetTarget(unit);
                break;
            }
        }

        private void ShootAtTarget()
        {
            if (!_isReadyToShoot) return;
            
            ShowShootVfx();
            
            TargetedEntity.RPC_TakeDamage(Data.DamagePerShootOnUnits, Data.ArmorPenetration, this);
            if (TargetedEntity is BaseUnit unit) unit.ReactToDamage(this);
            
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

        private void ManageFormation()
        {
            if (timeLeftToForm <= 0)
            {
                TryFormFirstUnitInQueue();
            }
        }

        private void TryFormFirstUnitInQueue()
        {
            var unitSupplyCost = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].SupplyCost;

            if (unitSupplyCost + Owner.ressources.CurrentSupply > Owner.ressources.CurrentMaxSupply)
            {
                if (!_NotEnoughSupplies)
                {
                    Debug.Log("not enough available supplies");
                    _NotEnoughSupplies = true;
                }
                return;
            }
            
            Owner.ressources.CurrentSupply += unitSupplyCost;
            _NotEnoughSupplies = false;
                
            timeLeftToForm = 100; // Par sécurité
            FormFirstUnitInQueue();
        }

        private void FormFirstUnitInQueue()
        {
            // faire spawn la premier unité dans la queue
            var prefab = UnitsManager.allUnitsPrefab[(int) FormationQueue.Dequeue()];
            
            Vector3 myPos = transform.position;
            var randomX = Random.Range(-2f, 2f);
            var randomZ = Random.Range(-2f, 2f);
            Vector3 spawnPos = new Vector3(myPos.x + randomX, UnitsManager.flyingHeightOfUnits, myPos.z + randomZ);
            NetworkObject obj = Runner.Spawn(prefab, spawnPos, Quaternion.identity, Object.StateAuthority);

            obj.GetComponent<BaseUnit>().Init(Owner);
            
            if (FormationQueue.Count > 0)
            {
                timeLeftToForm = UnitsManager.allUnitsData[(int) FormationQueue.Peek()].ProductionTime;
            }
            
            _uiManager.UpdateFormationQueueDisplay(this);
        }
        
        private void UpdateFormation()
        {
            timeLeftToForm--;

            if (FormationQueue.Count > 0)
            {
                if (GameManager.thisPlayer.lastSelectedElement == this && !_NotEnoughSupplies)
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

        public override void DestroyEntity()
        {
            myIsland.BuildingsCount--;
            myIsland.buildingOnThisIsland.Remove(this);

            if (Data.DoesGenerateRessources)
            {
                Owner.ressources.CurrentWoodGain -= Data.GeneratedWoodPerSeconds;
                Owner.ressources.CurrentMetalsGain -= Data.GeneratedMetalsPerSeconds;
                Owner.ressources.CurrentOrichalqueGain -= Data.GeneratedOrichalquePerSeconds;
                Owner.ressources.CurrentMaxSupply -= Data.AditionnalMaxSupplies;
            }

            if (isStartBuilding) GameManager.DefeatPlayer(Owner);
            base.DestroyEntity();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.DrawWireDisc(transform.position,Vector3.up, Data.ShootingRange);
        }
        #endif
    }
}