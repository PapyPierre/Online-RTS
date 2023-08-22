using System.Collections;
using AOSFogWar.Used_Scripts;
using Element.Entity.Military_Units.Units_Skills;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;

namespace Element.Entity.Military_Units
{
    public class BaseUnit : BaseEntity
    {
        [Space] public UnitSkill[] skills;
        [field: SerializeField, Expandable, Space] public UnitData Data { get; private set; }

        [HideInInspector] public UnitsManager.UnitStates myState;

        private Rigidbody _rb;
        
        [HideInInspector] public bool targetedUnitIsInRange;
        private bool _isReadyToShoot = true;
        
        [HideInInspector] public Vector3 targetPosToMoveTo;
        [HideInInspector] public GameObject myMoveIndicator;
        
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

            foreach (UnitSkill skill in skills)
            {
                if (skill.Data.ReadyAtStart) skill.isReady = true;
            }
        }

        public virtual void UseSkill(UnitSkill skill)
        {
            skill.isReady = false;
            UIManager.ShowSelectionInfoBox(this, Data, Owner);
        }

        private void ManageSkillsCooldowns()
        {
            foreach (var skill in skills)
            {
                if (skill.timeLeftOnCd > 0)
                {
                    skill.timeLeftOnCd -= Time.deltaTime;
                    int cdDuration = skill.Data.CooldownDuration;
                    skill.cdCompletion = (cdDuration - skill.timeLeftOnCd) / cdDuration;

                    if (skill.timeLeftOnCd <= 0 && skill.Data.ReadyAtStart) skill.isReady = true;

                    UIManager.UpdateSelectionInfobox(this, Data, Owner);
                }
            }
        }
        
        protected void StartSkillCooldown(UnitSkill skill)
        {
            skill.isActive = false;
            skill.timeLeftOnCd = skill.Data.CooldownDuration;
        }

        private void Update()
        {
            CheckIfTargetInRange();
            ShootAtEnemy();
            ManageSkillsCooldowns();
        }

        public override void FixedUpdateNetwork()
        {
            if (isDead) return;
            
            NullifyRbVelocity();

            if (myState == UnitsManager.UnitStates.Moving)
            {
                var step = Data.MovementSpeed * Runner.DeltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosToMoveTo, step);
                
                if (Data.AngularSpeed > 0)
                {
                    transform.rotation = Quaternion.LookRotation(targetPosToMoveTo - transform.position);
                }
                
                if (Vector3.Distance(transform.position,  targetPosToMoveTo) < UnitsManager.distToTargetToStop)
                {
                    // Stop
                    myMoveIndicator.SetActive(false);
                    myState = UnitsManager.UnitStates.Static;
                }
            }
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
                var distToTarget =
                    CustomHelper.ReturnDistanceInTopDown(transform.position, TargetedEntity.transform.position);

                targetedUnitIsInRange = distToTarget <= Data.ShootingRange;
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
            if (myState == UnitsManager.UnitStates.Moving) return;

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
        
        public override void DestroyEntity()
        {
            myMoveIndicator.SetActive(false);

            if (UnitsManager.currentlySelectedUnits.Contains(this)) UnitsManager.currentlySelectedUnits.Remove(this);

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
