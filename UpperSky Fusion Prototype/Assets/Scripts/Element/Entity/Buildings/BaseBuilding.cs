using System.Collections;
using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Element.Entity.Military_Units;
using Element.Island;
using Entity.Buildings;
using Fusion;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;
using UserInterface;
using Random = UnityEngine.Random;

namespace Element.Entity.Buildings
{
    public class BaseBuilding : BaseEntity
    {
        private BuildingsManager _buildingsManager;

        [field: SerializeField, Expandable, Space] public BuildingData Data { get; private set; }

        [HideInInspector] public BaseIsland myIsland;
        
        [SerializeField, Header("Defense")] private Transform canonHead;
        private bool _isReadyToShoot = true;

        public override void Spawned()
        {
            base.Spawned(); 
            
            SetUpHealtAndArmor(Data);

            _buildingsManager = BuildingsManager.Instance;
        }

        public override void Init(PlayerController owner, ElementData elementData)
        {
            base.Init(owner, elementData);
            
            if (Data.UnlockedBuildings.Length > 0) UnlockBuildings();
            
            if (Data.DoesGenerateRessources)
            {
                Owner.ressources.CurrentWoodGain += Data.GeneratedWoodPerSeconds;
                Owner.ressources.CurrentMetalsGain += Data.GeneratedMetalsPerSeconds;
                Owner.ressources.CurrentOrichalqueGain += Data.GeneratedOrichalquePerSeconds;
                Owner.ressources.CurrentMaxSupply += Data.AditionnalMaxSupplies;
            }
        }

        public void SetIsland(BaseIsland buildOnThisIsland)
        {
            myIsland = buildOnThisIsland;
        }

        protected virtual void Update()
        {
            if (Data.IsDefenseBuilding)
            {
                if (TargetedEntity == null || TargetedEntity.IsDead)
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
                if (unit.Owner == Owner || unit.IsDead) continue;
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