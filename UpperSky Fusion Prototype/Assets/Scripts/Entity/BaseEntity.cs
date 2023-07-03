using System;
using Custom_UI.InGame_UI;
using Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;

namespace Entity
{
    public class BaseEntity : NetworkBehaviour
    {
        protected UnitsManager UnitsManager;
        protected GameManager GameManager;

        [field: SerializeField] [Networked] public PlayerController Owner { get; set; }
        [SerializeField] protected GameObject selectionCircle;

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

        public override void Spawned()
        {
            GameManager = GameManager.Instance;
            UnitsManager = UnitsManager.Instance;
        }

        protected void SetUpHealtAndArmor(EntityData data)
        {
            CurrentHealth = data.MaxHealthPoints;
            healthBar.Init(data.MaxHealthPoints);
            
            CurrentArmor = data.MaxArmorPoints;
            armorBar.Init(data.MaxArmorPoints);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_TakeDamage(float damageOnHealth, float damageOnArmor)
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
            
            if (CurrentHealth <= 0) DestroyEntity();
        }
        
        public void DestroyEntity()
        {
            if (this is BaseUnit)
            {
                BaseUnit thisUnit = GetComponent<BaseUnit>();
                
                if (UnitsManager.currentlySelectedUnits.Contains(thisUnit))
                {
                    UnitsManager.currentlySelectedUnits.Remove(thisUnit);
                }
            }

            Runner.Despawn(Object);
        }
    }
}
