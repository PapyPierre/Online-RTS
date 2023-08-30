using System.Collections;
using AOSFogWar.Used_Scripts;
using Element.Entity.Military_Units.Units_Skills;
using Fusion;
using NaughtyAttributes;
using Player;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Element.Entity.Military_Units
{
    public class BaseUnit : BaseEntity
    {
        [Space] public UnitSkill[] skills;
        [field: SerializeField, Expandable, Space] public UnitData Data { get; private set; }

        [Networked] public UnitsManager.UnitStates MyState { get; set; }
        [SerializeField] private TextMeshProUGUI stateDebugTMP;

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

        public override void Init(PlayerController owner, ElementData data)
        {
            base.Init(owner, data);

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

            if (DebugManager.Instance.showUnitsStateDebugText) stateDebugTMP.text = MyState.ToString();
            else stateDebugTMP.gameObject.SetActive(false);
        }

        public override void FixedUpdateNetwork()
        {
            if (IsDead) return;
            
            NullifyRbVelocity();

            if (MyState == UnitsManager.UnitStates.Moving)
            {
              ManageMovement();
            }
        }

        private void ManageMovement()
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
                if (myMoveIndicator != null) myMoveIndicator.SetActive(false);
                MyState = UnitsManager.UnitStates.Static;
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

        public override void ResetTarget()
        {
           base.ResetTarget();
           MyState = UnitsManager.UnitStates.Static;
        }

        private void ShootAtEnemy()
        {
            if (TargetedEntity is null || !targetedUnitIsInRange || Data.ShootingMode == UnitsManager.ShootingMode.None)
            {
                return;
            }

            if (Data.ShootingMode == UnitsManager.ShootingMode.ShotByShot)
            {
                AimAtTarget(TargetedEntity.transform, transform);

                if (!_isReadyToShoot) return;

                ShootProjectile(Data.DamagePerShoot, Data.ArmorPenetration, this);

                _isReadyToShoot = false;
                StartCoroutine(Reload());
            }
            else if (Data.ShootingMode == UnitsManager.ShootingMode.Automatic)
            {
                ShowShootVfx();
                TargetedEntity.RPC_TakeDamage(Data.ContinuiousDamage, Data.ArmorPenetration, this);
            }
            
            MyState = UnitsManager.UnitStates.Shooting;
        }

        public void ReactToDamage(BaseEntity agressor)
        {
            if (MyState == UnitsManager.UnitStates.Moving) return;

            if (TargetedEntity is null)
            {
                _isReadyToShoot = false;
                SetTarget(agressor);
                StartCoroutine(Reload());
            }
            else if (TargetedEntity == agressor)
            {
                AimAtTarget(agressor.transform, transform);
            }
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSecondsRealtime(Data.RealodTime);
            _isReadyToShoot = true;
        }
        
        public override void DestroyEntity()
        {
            if (myMoveIndicator != null) myMoveIndicator.SetActive(false);
            
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
