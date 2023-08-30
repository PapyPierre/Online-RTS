using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Custom_UI;
using Custom_UI.InGame_UI;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Entity;
using Fusion;
using Ressources.AOSFogWar.Used_Scripts;
using UnityEngine;
using UserInterface;

namespace Element.Entity
{
    /// <summary>
    /// Entity include Buildings and Units
    /// </summary>
    
    public abstract class BaseEntity : BaseElement
    {
        #region Networked Health & Health Bar
        [field: SerializeField, Header("Health")] [Networked(OnChanged = nameof(CurrentHealthChanged))]
        private float CurrentHealth { get; set; }
        
        [SerializeField] private StatBar healthBar;
        private static void CurrentHealthChanged(Changed<BaseEntity> changed) 
            => changed.Behaviour.healthBar.UpdateBar(changed.Behaviour.CurrentHealth);
        #endregion
        
        #region Networked Armor & Armor Bar
        [field: SerializeField, Header("Armor")] [Networked(OnChanged = nameof(CurrentArmorChanged))]
        private float CurrentArmor { get; set; }
        [SerializeField] private StatBar armorBar;
        private static void CurrentArmorChanged(Changed<BaseEntity> changed) 
            => changed.Behaviour.armorBar.UpdateBar(changed.Behaviour.CurrentArmor);
        #endregion

        protected BaseEntity TargetedEntity { get; private set; }

        [Networked] public bool IsDead { get; private set; }
        
        [HideInInspector] public List<BaseEntity> currentAgressor;

        [SerializeField, Header("VFX")] private NetworkPrefabRef deathVfx;
        [SerializeField] private NetworkPrefabRef lowHpVfx;
        private bool _lowHpVfxSpawned;
        [SerializeField] protected NetworkPrefabRef shootVfx;

        [SerializeField, Header("Other")] protected Transform[] canonsPos;
        [SerializeField] private NetworkPrefabRef shootProjectile;

        public override void Spawned()
        {
            base.Spawned();
            
            
            switch (this)
            {
                case BaseUnit:
                {
                    GetComponent<FogAgentUnit>().Init(graphObject, canvas, minimapIcon.gameObject);
                    break;
                }
                case BaseBuilding:
                {
                    GetComponent<FogAgent>().Init(graphObject, canvas);
                    break;
                }
            }
        }
        
        protected void SetUpHealtAndArmor(EntityData data)
        {
            CurrentHealth = data.MaxHealthPoints;
            healthBar.Init(data.MaxHealthPoints);
            
            CurrentArmor = data.MaxArmorPoints;
            armorBar.Init(data.MaxArmorPoints);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_TakeDamage(int damage, int armorPenetration, BaseEntity shooter = null)
        {
            // The code inside here will run on the client which owns this object (has state and input authority).

            float damageOnHealth =  armorPenetration / 100f * damage;
            float damageOnArmor = (100f - armorPenetration) / 100f * damage;
            
            CurrentHealth -= damageOnHealth;
            
            if (CurrentArmor - damageOnArmor >= 0) CurrentArmor -= damageOnArmor;
            else
            {     
                var x = damageOnArmor - CurrentArmor;
                CurrentHealth -= x;
                CurrentArmor = 0;
            }

            UIManager.ShowUnderAttackPopUp(transform.position, this);

            // Need to access slider max value to avoid doing twice this code in children class to access data
            if (CurrentHealth < healthBar.Slider.maxValue/2 && !_lowHpVfxSpawned)
            {
                _lowHpVfxSpawned = true;
                var obj = Runner.Spawn(lowHpVfx, transform.position);
                obj.transform.parent = transform;
            }

            if (CurrentHealth <= 0)
            {
                if (shooter != null)
                {
                    shooter.ResetTarget();
                    if (shooter is BaseUnit unit) unit.targetedUnitIsInRange = false;
                    else if(shooter is BaseBuilding building) building.FindTarget();
                }
                
                DestroyEntity();
            }
            else
            {
                if (this is BaseUnit unit) unit.ReactToDamage(shooter);
            }
        }
        
        public virtual void DestroyEntity()
        {
            IsDead = true;

            if (GameManager.thisPlayer.lastSelectedElement == this)
            {
                UIManager.HideSelectionInfoBox();
            }
            
            Runner.Spawn(deathVfx, transform.position);
        
            if (MouseAboveThisElement()) GameManager.thisPlayer.mouseAboveThisElement = null;

            foreach (var entity in currentAgressor)
            {
                entity.ResetTarget();
            }

            FogOfWar.RemoveFogRevealer(FogRevealerIndex);
            Runner.Despawn(Object);
        }

        public void SetTarget(BaseEntity entity)
        {
            TargetedEntity = entity;
            AimAtTarget(entity.transform, transform);
        }
        
        public virtual void ResetTarget()
        {
            if (TargetedEntity is null) return;

            if (TargetedEntity.currentAgressor.Contains(this) && !TargetedEntity.IsDead)
            {
                TargetedEntity.currentAgressor.Remove(this);
            }
            
            TargetedEntity = null;
        }

        protected void AimAtTarget(Transform target, Transform objToRotate)
        {
            Vector3 directionToTarget = target.position - objToRotate.position;
            objToRotate.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        }

        protected void ShootProjectile( int damage, int armorPen, BaseEntity shooter)
        {
            GameManager.thisPlayer.Runner.Spawn(shootProjectile, canonsPos[0].position, Quaternion.identity, 
               Object.InputAuthority, (runner, obj) => 
            {
                // Initialize before synchronizing it
                obj.GetComponent<ShootProjectile>().Init(TargetedEntity, damage, armorPen, shooter);
            });
        }
        
        protected void ShowShootVfx()
        {
            foreach (var t in canonsPos)
            {
                Runner.Spawn(shootVfx, t.position, Quaternion.identity);
            }
        }
    }
}