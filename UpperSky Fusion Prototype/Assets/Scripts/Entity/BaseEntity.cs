using System.Collections.Generic;
using AOSFogWar;
using AOSFogWar.Used_Scripts;
using Custom_UI.InGame_UI;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;
using World;

namespace Entity
{
    public class BaseEntity : NetworkBehaviour
    {
        protected UnitsManager UnitsManager;
        protected GameManager GameManager;
        protected FogOfWar FogOfWar;

        protected int FogRevealerIndex;

        #region Ownership
        [field: SerializeField] [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        [SerializeField] protected GameObject selectionCircle;
        [SerializeField] private  List<MeshRenderer> meshToColor;
        #endregion

        #region Networked Health & Health Bar
        [field: SerializeField] [Networked(OnChanged = nameof(CurrentHealthChanged))]
        private float CurrentHealth { get; set; }
        
        [SerializeField] private StatBar healthBar;
        private static void CurrentHealthChanged(Changed<BaseEntity> changed) 
            => changed.Behaviour.healthBar.UpdateBar(changed.Behaviour.CurrentHealth);
        #endregion
        
        #region Networked Armor & Armor Bar
        [field: SerializeField] [Networked(OnChanged = nameof(CurrentArmorChanged))]
        private float CurrentArmor { get; set; }
        [SerializeField] private StatBar armorBar;
        private static void CurrentArmorChanged(Changed<BaseEntity> changed) 
            => changed.Behaviour.armorBar.UpdateBar(changed.Behaviour.CurrentArmor);
        #endregion

        [SerializeField] private GameObject graphObject;
        [SerializeField] private GameObject canvas;

        public override void Spawned()
        {
            GameManager = GameManager.Instance;
            UnitsManager = UnitsManager.Instance;
            FogOfWar = FogOfWar.Instance;
            GetComponent<FogAgent>().Init(graphObject, canvas);
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
        public void RPC_TakeDamage(float damageOnHealth, float damageOnArmor, BaseUnit shooter)
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

            if (CurrentHealth <= 0)
            {
                shooter.targetedEntity = null;
                shooter.targetedUnitIsInRange = false;
                DestroyEntity();
            }
        }
        
        public void DestroyEntity()
        {
            if (MouseAboveThisEntity()) GameManager.thisPlayer.mouseAboveThisEntity = null;

            if (this is BaseUnit unit)
            {
                if (UnitsManager.currentlySelectedUnits.Contains(unit)) UnitsManager.currentlySelectedUnits.Remove(unit);
                
                if (unit.currentGroup is not null) unit.currentGroup.RemoveUnitFromGroup(unit);

                GameManager.thisPlayer.ressources.CurrentSupply -= unit.Data.SupplyCost;
            }
            else if (this is BaseBuilding building)
            {
                building.myIsland.BuildingsCount--; //Possible erreur car pas la state authority, alors faire une RPC
                building.myIsland.buildingOnThisIsland.Remove(building);
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
