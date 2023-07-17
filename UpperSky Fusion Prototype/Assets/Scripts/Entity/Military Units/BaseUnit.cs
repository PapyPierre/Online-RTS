using System.Collections;
using AOSFogWar;
using AOSFogWar.Used_Scripts;
using Fusion;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;

namespace Entity.Military_Units
{
    public class BaseUnit : BaseEntity
    {
        [field: SerializeField, Expandable] public UnitData Data { get; private set; }
        
        [HideInInspector] public bool targetedUnitIsInRange;
        private bool _isReadyToShoot = true;

        [HideInInspector] public UnitGroup currentGroup;
        
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

        public void Init(PlayerController owner)
        {
            Owner = owner;
            
            if (PlayerIsOwner())
            {
                var fogRevealer = new FogOfWar.FogRevealer(transform, Data.SightRange, true);
                FogRevealerIndex = FogOfWar.AddFogRevealer(fogRevealer);
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
            if (TargetedEntity != null)
            {
                var distToTarget = Vector3.Distance(
                    CustomHelper.ReturnPosInTopDown(transform.position),
                    CustomHelper.ReturnPosInTopDown(TargetedEntity.transform.position));
                
                targetedUnitIsInRange = distToTarget <= Data.ShootingRange;
            }
            else targetedUnitIsInRange = false;
        }

        private void ShootAtEnemy()
        {
            if (TargetedEntity is null || !targetedUnitIsInRange || !_isReadyToShoot) return;

            int damageOnUnits = Data.DamagePerShootOnUnits; 
            int armorPenetration = Data.ArmorPenetration;

            float damageOnHealth =  armorPenetration / 100f * damageOnUnits;
            float damageOnArmor = (100f - armorPenetration) / 100f * damageOnUnits;

            TargetedEntity.RPC_TakeDamage(damageOnHealth, damageOnArmor,  this);
            if (TargetedEntity is BaseUnit unit) unit.ReactToDamage();

            _isReadyToShoot = false;
            StartCoroutine(Reload());
        }

        public void ReactToDamage()
        {
            //TODO Réaction, voir gdd
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSecondsRealtime(Data.RealodTime);
            _isReadyToShoot = true;
        }

        #region Selection

        protected override void OnMouseEnter()
        {
            base.OnMouseEnter();
            
            if (PlayerIsOwner())
            {
                SetActiveSelectionCircle(true);
            }
        }

        protected override void OnMouseExit()
        {
            base.OnMouseExit();

            if (!UnitsManager.currentlySelectedUnits.Contains(this) && PlayerIsOwner())
            {
                SetActiveSelectionCircle(false);
            }
        }
        #endregion

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Handles.color = Color.green;
            Handles.DrawWireDisc(transform.position,Vector3.up, UnitsManager.distUnitToIslandToColonise);
        }
        #endif
    }
}
