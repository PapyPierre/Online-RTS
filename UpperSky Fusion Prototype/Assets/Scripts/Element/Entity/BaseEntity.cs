using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Custom_UI;
using Custom_UI.InGame_UI;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Entity;
using Entity.Military_Units;
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
        protected FogOfWar FogOfWar;
        protected int FogRevealerIndex;
        
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

        public bool isDead;
        
        [HideInInspector] public List<BaseEntity> currentAgressor;

        [SerializeField, Header("VFX")] private NetworkPrefabRef deathVfx;
        [SerializeField] private NetworkPrefabRef lowHpVfx;
        private bool _lowHpVfxSpawned;
        
        [SerializeField] protected Transform[] canonsPos;
        [SerializeField] protected NetworkPrefabRef shootVfx;

        public override void Spawned()
        {
            base.Spawned();
            
            FogOfWar = FogOfWar.Instance;
            
            switch (this)
            {
                case BaseUnit unit:
                {
                    GetComponent<FogAgentUnit>().Init(graphObject, canvas, minimapIcon.gameObject);
                    break;
                }
                case BaseBuilding building:
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

            UIManager.ShowUnderAttackPopUp(transform.position);

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

        protected virtual void DestroyEntity()
        {
            isDead = true;
            
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
            entity.currentAgressor.Add(this);
            
            AimAtTarget(entity.transform, transform);
        }
        
        public void ResetTarget()
        {
            if (TargetedEntity is null) return;

            if (TargetedEntity.currentAgressor.Contains(this) && !TargetedEntity.isDead)
            {
                TargetedEntity.currentAgressor.Remove(this);
            }
            
            TargetedEntity = null;
        }

        protected void AimAtTarget(Transform target, Transform objToRotate)
        {
            float rotSpeed = 2;

            if (this is BaseUnit unit)
            {
                if (unit.Data.AngularSpeed > 0)
                {
                    rotSpeed = unit.Data.AngularSpeed;
                }
                else return;
            }

            Vector3 directionToTarget = target.position - objToRotate.position;
            Quaternion targetRot = Quaternion.LookRotation(directionToTarget, Vector3.up);

            objToRotate.rotation = Quaternion.Slerp(objToRotate.rotation, targetRot, rotSpeed * Time.deltaTime);
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