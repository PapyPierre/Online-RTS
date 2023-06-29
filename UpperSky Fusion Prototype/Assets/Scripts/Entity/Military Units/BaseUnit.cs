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
        
        [field: SerializeField] [Networked] private float CurrentHealthPoint { get; set; }
        [SerializeField] private StatBar healthBar;
        
        [field: SerializeField] [Networked] private float CurrentArmor { get; set; }
        [SerializeField] private StatBar armorBar;
        
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
                  TakeDamage(10, 2);   
                }
            }

            CheckIfTargetInRange();
            ShootAtEnemy();
        }

        private void SetUpHealtAndArmor()
        {
            CurrentHealthPoint = Data.MaxHealthPoints;
            healthBar.Init(Data.MaxHealthPoints);
            
            CurrentArmor = Data.MaxArmorPoints;
            armorBar.Init(Data.MaxArmorPoints);
        }

        public void TakeDamage(float damageOnHealth, float damageOnArmor)
        {
            CurrentHealthPoint -= damageOnHealth;
            CurrentArmor -= damageOnArmor;
            
            if (CurrentHealthPoint <= 0) Kill();
            else
            {
                healthBar.UpdateBar(CurrentHealthPoint);
                armorBar.UpdateBar(CurrentArmor);
            }
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

            targetedUnit.TakeDamage(damageOnHealth, damageOnArmor);
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
