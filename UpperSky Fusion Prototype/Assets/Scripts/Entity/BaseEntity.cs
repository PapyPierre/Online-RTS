using System.Collections;
using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Custom_UI.InGame_UI;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;

namespace Entity
{
    public abstract class BaseEntity : NetworkBehaviour
    {
        protected UnitsManager UnitsManager;
        protected GameManager GameManager;
        protected FogOfWar FogOfWar;

        protected int FogRevealerIndex;

        #region Ownership
        [field: SerializeField, Header("Ownership")] [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        [SerializeField] protected GameObject selectionCircle;
        [SerializeField] private  List<MeshRenderer> meshToColor;
        #endregion

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

        public BaseEntity TargetedEntity { get; set; }

        [SerializeField, Header("VFX")] private NetworkPrefabRef deathVfx;
        [SerializeField] private NetworkPrefabRef lowHpVfx;
        private bool _lowHpVfxSpawned;
        
        [SerializeField, Space] private GameObject graphObject;
        
        [SerializeField, Space] private GameObject canvas;

        private List<NetworkObject> _objToDestroyOnDeath = new ();

        public override void Spawned()
        {
            GameManager = GameManager.Instance;
            UnitsManager = UnitsManager.Instance;
            FogOfWar = FogOfWar.Instance;
            GetComponent<FogAgent>().Init(graphObject, canvas);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                RPC_TakeDamage(30,10, this);
            }
        }

        private static void OwnerChanged(Changed<BaseEntity> changed)
        {
            if (changed.Behaviour.Owner == null) return;

            foreach (var meshRenderer in changed.Behaviour.meshToColor)
            {
                meshRenderer.material.color = changed.Behaviour.Owner.myColor;
            }
        }

        protected void SetUpHealtAndArmor(EntityData data)
        {
            CurrentHealth = data.MaxHealthPoints;
            healthBar.Init(data.MaxHealthPoints);
            
            CurrentArmor = data.MaxArmorPoints;
            armorBar.Init(data.MaxArmorPoints);
        }
        
        protected bool PlayerIsOwner()
        {
            return Owner == GameManager.thisPlayer;
        }
        
        protected bool MouseAboveThisEntity()
        {
            return GameManager.thisPlayer.mouseAboveThisEntity == this;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_TakeDamage(float damageOnHealth, float damageOnArmor, BaseEntity shooter)
        {
            // The code inside here will run on the client which owns this object (has state and input authority).

            CurrentHealth -= damageOnHealth;
            
            if (CurrentArmor - damageOnArmor >= 0) CurrentArmor -= damageOnArmor;
            else
            {     
                var x = damageOnArmor - CurrentArmor;
                CurrentHealth -= x;
                CurrentArmor = 0;
            }

            // Need to access slider max value to avoid doing twice this code in children class to access data
            if (CurrentHealth < healthBar.Slider.maxValue/2 && !_lowHpVfxSpawned)
            {
                _lowHpVfxSpawned = true;
                var obj = Runner.Spawn(lowHpVfx, transform.position);
                obj.transform.parent = transform;
                _objToDestroyOnDeath.Add(obj);
            }

            if (CurrentHealth <= 0)
            {
                shooter.TargetedEntity = null;
                if (shooter is BaseUnit unit) unit.targetedUnitIsInRange = false;
                
                DestroyEntity();
            }
        }
        
        public void DestroyEntity()
        {
            foreach (var obj in _objToDestroyOnDeath) Runner.Despawn(obj);

            graphObject.SetActive(false);
            Runner.Spawn(deathVfx, transform.position);
        
            if (MouseAboveThisEntity()) GameManager.thisPlayer.mouseAboveThisEntity = null;

            switch (this)
            {
                case BaseUnit unit:
                {
                    if (UnitsManager.currentlySelectedUnits.Contains(unit)) UnitsManager.currentlySelectedUnits.Remove(unit);
                
                    if (unit.currentGroup is not null) unit.currentGroup.RemoveUnitFromGroup(unit);

                    GameManager.thisPlayer.ressources.CurrentSupply -= unit.Data.SupplyCost;
                    break;
                }
                case BaseBuilding building:
                {
                    building.myIsland.BuildingsCount--;
                    building.myIsland.buildingOnThisIsland.Remove(building);

                    if (building.isStartBuilding) GameManager.KillPlayer(Owner);
                    break;
                }
            }

            FogOfWar.RemoveFogRevealer(FogRevealerIndex);
            Runner.Despawn(Object);
        }

        #region Selection
        protected virtual void OnMouseEnter()
        {
            GameManager.thisPlayer.mouseAboveThisEntity = this;
        }
        
        protected virtual void OnMouseExit()
        {
            GameManager.thisPlayer.mouseAboveThisEntity = null;
        }
        
        public void SetActiveSelectionCircle(bool value)
        {
            selectionCircle.SetActive(value);
        }
        #endregion
    }
}
