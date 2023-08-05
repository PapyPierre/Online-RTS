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

        [field: SerializeField, Expandable] public BuildingData Data { get; private set; }

        [HideInInspector] public BaseIsland myIsland;

        public bool isStartBuilding;

        // Defense
        [SerializeField] private Transform canonHead;
        private bool _isReadyToShoot = true;

        public override void Spawned()
        {
            base.Spawned(); 
            
            SetUpHealtAndArmor(Data);

            _buildingsManager = BuildingsManager.Instance;
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
        }
        
        [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
        public void RPC_StartBuildingInit()
        {
            // The code inside here will run on the client which has input authority

            var fogRevealer = new FogOfWar.FogRevealer(transform, Data.SightRange, true);
            FogRevealerIndex = FogOfWar.AddFogRevealer(fogRevealer);
        }

        protected virtual void Update()
        {
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

        private void UnlockBuildings()
        {
            foreach (var building in Data.UnlockedBuildings)
            {
                _buildingsManager.allBuildingsIcons[(int) building].Unlock();
            }
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