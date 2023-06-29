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
        
        public PlayerController Owner { get; private set; }
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

            if (Object.HasInputAuthority) Owner = _gameManager.thisPlayer;
            
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
            Debug.Log("took damage");
            
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

            var damageOnUnits = Data.DamagePerShootOnUnits; 
            var armorPenetration = Data.ArmorPenetration;
            
            Debug.Log(damageOnUnits + " : " + armorPenetration); // return 6 : 5
            
            float damageOnHealth = armorPenetration / 100 * damageOnUnits;
            float damageOnArmor = (100 - armorPenetration) / 100 * damageOnUnits;
            
            //BUG ICI
            Debug.Log(damageOnHealth + " : " + damageOnArmor); // return 0 : 0

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
