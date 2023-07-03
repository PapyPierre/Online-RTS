using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;
using World;

namespace Entity.Military_Units
{
    public class BaseUnit : BaseEntity
    {

        [field: SerializeField, Expandable] public UnitData Data { get; private set; }
        
        [HideInInspector] public BaseUnit targetedUnit;
        [HideInInspector] public bool targetedUnitIsInRange;
        private bool _isReadyToShoot = true;

        [SerializeField] private List<MeshRenderer> meshToColor;

        [Header("Status")] 
        public bool isColonizer;
        public bool isCamouflaged; //TODO
        [SerializeField] private float currentRegeneration; //TODO
        [SerializeField] private float currentAcid; //TODO
        [SerializeField] private float currentParasite; //TODO

        public override void Spawned()
        {
            base.Spawned();
            UnitsManager.allActiveUnits.Add(this);
            SetUpHealtAndArmor(Data);
            SetUpStatus();
        }

        public void Colorize()
        {
            for (var i = 0; i < GameManager.Instance.connectedPlayers.Count; i++)
            {
                PlayerController player = GameManager.Instance.connectedPlayers[i];
                if (player == Owner)
                {
                    foreach (var meshRenderer in meshToColor)
                    {
                        meshRenderer.material.color = WorldManager.Instance.playersColors[i];
                    }
                    return;
                }
            }
        }

        private void SetUpStatus()
        {
            isColonizer = Data.IsBaseColonizer;
            isCamouflaged = Data.IsBaseCamouflaged;
            currentRegeneration = Data.BaseRegeneration;
            currentAcid = Data.BaseAcid;
            currentParasite = Data.BaseParasite;
        }

        private void Update()
        {
            CheckIfTargetInRange();
            ShootAtEnemy();
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
            GameManager.thisPlayer.mouseAboveThisUnit = this;
        
            if (PlayerIsOwner())
            {
                SetActiveSelectionCircle(true);
            }
        }
        
        private void OnMouseExit()
        {
            GameManager.thisPlayer.mouseAboveThisUnit = null;
        
            if (!UnitsManager.currentlySelectedUnits.Contains(this) && Owner == GameManager.thisPlayer)
            {
                SetActiveSelectionCircle(false);
            }
        }
        
        public void SetActiveSelectionCircle(bool value)
        {
            selectionCircle.SetActive(value);
        }
        #endregion

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(transform.position,Vector3.up, UnitsManager.distUnitToIslandToColonise);
        }
        #endif
    }
}
