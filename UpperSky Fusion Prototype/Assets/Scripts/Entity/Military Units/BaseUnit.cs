using System.Collections;
using Custom_UI.InGame_UI;
using Fusion;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace Entity.Military_Units
{
    public class BaseUnit : NetworkBehaviour
    {
        private UnitsManager _unitsManager;
        private GameManager _gameManager;
        
        [field: SerializeField] [Networked] public PlayerController Owner { get; set; }
        [SerializeField] private GameObject selectionCircle;

        [field: SerializeField, Expandable] public UnitData Data { get; private set; }

        #region Networked Health & Health Bar
        [field: SerializeField] [Networked(OnChanged = nameof(CurrentHealthChanged))]
        private float CurrentHealth { get; set; }
        
        [SerializeField] private StatBar healthBar;
        private static void CurrentHealthChanged(Changed<BaseUnit> changed) 
            => changed.Behaviour.healthBar.UpdateBar(changed.Behaviour.CurrentHealth);
        #endregion
        
        #region Networked Armor & Armor Bar
        [field: SerializeField] [Networked(OnChanged = nameof(CurrentArmorChanged))]
        private float CurrentArmor { get; set; }
        [SerializeField] private StatBar armorBar;
        private static void CurrentArmorChanged(Changed<BaseUnit> changed) 
            => changed.Behaviour.armorBar.UpdateBar(changed.Behaviour.CurrentArmor);
        #endregion

        // Serialized for debug
        public BaseUnit targetedUnit;
        public bool targetedUnitIsInRange;
        
        private bool _isReadyToShoot = true;
        
        public override void Spawned()
        {
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;
            _unitsManager.allActiveUnits.Add(this);

            SetUpHealtAndArmor();
        }

        private void Update()
        {
            // For Debug
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                if (HasStateAuthority)
                {
                  RPC_TakeDamage(10, 2);   
                }
            }

            CheckIfTargetInRange();
            ShootAtEnemy();
        }

        private void SetUpHealtAndArmor()
        {
            CurrentHealth = Data.MaxHealthPoints;
            healthBar.Init(Data.MaxHealthPoints);
            
            CurrentArmor = Data.MaxArmorPoints;
            armorBar.Init(Data.MaxArmorPoints);
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
            
            if (CurrentHealth <= 0) Kill();
        }

        private void Kill()
        {
            gameObject.SetActive(false);
        }

        private void CheckIfTargetInRange()
        {
            if (targetedUnit is not null)
            {
                targetedUnitIsInRange =
                    Vector3.Distance(transform.position, targetedUnit.transform.position) <= Data.ShootingRange;
            }
            else targetedUnitIsInRange = false;
        }

        private void ShootAtEnemy()
        {
            if (targetedUnit is null || !targetedUnitIsInRange || !_isReadyToShoot) return;

            int damageOnUnits = Data.DamagePerShootOnUnits; 
            int armorPenetration = Data.ArmorPenetration;

            float damageOnHealth =  armorPenetration / 100f * damageOnUnits;
            float damageOnArmor = (100f - armorPenetration) / 100f * damageOnUnits;

            targetedUnit.RPC_TakeDamage(damageOnHealth, damageOnArmor);
            _isReadyToShoot = false;
            StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSecondsRealtime(Data.RealodTime);
            _isReadyToShoot = true;
        }

        #region Selection
        private void OnMouseEnter()
        {
            _gameManager.thisPlayer.mouseAboveThisUnit = this;
        
            if (Owner == _gameManager.thisPlayer)
            {
                SetActiveSelectionCircle(true);
            }
        }
        
        private void OnMouseExit()
        {
            _gameManager.thisPlayer.mouseAboveThisUnit = null;
        
            if (!_unitsManager.currentlySelectedUnits.Contains(this) && Owner == _gameManager.thisPlayer)
            {
                SetActiveSelectionCircle(false);
            }
        }
        
        public void SetActiveSelectionCircle(bool value)
        {
            selectionCircle.SetActive(value);
        }
        #endregion
    }
}
