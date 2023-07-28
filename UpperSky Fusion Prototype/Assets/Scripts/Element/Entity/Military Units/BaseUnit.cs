using System.Collections;
using AOSFogWar.Used_Scripts;
using Entity.Military_Units;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;

namespace Element.Entity.Military_Units
{
    public class BaseUnit : BaseEntity
    {
        [field: SerializeField, Expandable] public UnitData Data { get; private set; }

        private Rigidbody _rb;
        
        [HideInInspector] public bool targetedUnitIsInRange;
        private bool _isReadyToShoot = true;
        
        [HideInInspector] public UnitGroup currentGroup;
        
        [HideInInspector] public bool isSkillReady;

        public override void Spawned()
        {
            base.Spawned();
            UnitsManager.allActiveUnits.Add(this);
            SetUpHealtAndArmor(Data);
        }

        public void Init(PlayerController owner)
        {
            Owner = owner;
            
            if (PlayerIsOwner())
            {
                var fogRevealer = new FogOfWar.FogRevealer(transform, Data.SightRange, true);
                FogRevealerIndex = FogOfWar.AddFogRevealer(fogRevealer);
            }

            if (Data.SkillData.ReadyAtStart)
            {
                isSkillReady = true;
            }
        }

        public virtual void UseSkill()
        {
            isSkillReady = false;
            UIManager.ShowInGameInfoBox(this, Data, Owner);
        }

        private void Update()
        {
            CheckIfTargetInRange();
            ShootAtEnemy();
        }

        private void FixedUpdate()
        {
            NullifyRbVelocity();
        }

        private void NullifyRbVelocity()
        {
            _rb ??= GetComponent<Rigidbody>();
            
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        private void CheckIfTargetInRange()
        {
            if (TargetedEntity != null)
            {
                var distToTarget = Vector3.Distance(
                    CustomHelper.ReturnPosInTopDown(transform.position),
                    CustomHelper.ReturnPosInTopDown(TargetedEntity.transform.position));
                
                targetedUnitIsInRange = distToTarget <= Data.SightRange;
            }
            else targetedUnitIsInRange = false;
        }

        private void ShootAtEnemy()
        {
            if (TargetedEntity is null || !targetedUnitIsInRange || !_isReadyToShoot || !Data.CanShoot) return;

            ShowShootVfx();

            TargetedEntity.RPC_TakeDamage(Data.DamagePerShoot, Data.ArmorPenetration, this);

            _isReadyToShoot = false;
            StartCoroutine(Reload());
        }

        public void ReactToDamage(BaseEntity agressor)
        {
            if (TargetedEntity is null)
            {
                _isReadyToShoot = false;
                SetTarget(agressor);
                StartCoroutine(Reload());
            }
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSecondsRealtime(Data.RealodTime);
            _isReadyToShoot = true;
        }
        
        protected override void DestroyEntity()
        {
            if (UnitsManager.currentlySelectedUnits.Contains(this)) UnitsManager.currentlySelectedUnits.Remove(this);
                
            if (currentGroup is not null) currentGroup.RemoveUnitFromGroup(this);

            GameManager.thisPlayer.ressources.CurrentSupply -= Data.SupplyCost;
            base.DestroyEntity();
        }

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
