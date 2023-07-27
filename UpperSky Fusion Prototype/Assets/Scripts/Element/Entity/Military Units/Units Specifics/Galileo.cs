using System.Collections;
using Element.Entity.Military_Units.Units_Skills;
using Element.Entity.Military_Units.Units_Skills.Skills_Behaviour;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Specifics
{
    public class Galileo : BaseUnit
    {
        public override void UseSkill()
        {
            base.UseSkill();
            StartCoroutine(LaunchBombs());
        }

        private IEnumerator LaunchBombs()
        {
            if (Data.SkillData is GomorrahBombData data)
            {
                for (int i = 0; i < data.NumberOfLaunch; i++)
                {
                    GameManager.thisPlayer.Runner.Spawn(data.NetworkPrefab, transform.position, Quaternion.identity, 
                        Object.InputAuthority, (runner, obj) => 
                        {
                           // Initialize before synchronizing it
                           obj.GetComponent<GomorrahBomb>().Init(Owner, obj);
                       });
                    
                    
                    yield return new WaitForSeconds(data.TimeBetweenLaunch);
                }

                StartCoroutine(SkillCooldown(data.CooldownDuration));
            }
        }

        private IEnumerator SkillCooldown(float delay)
        {
            yield return new WaitForSeconds(delay);
            isSkillReady = true;
            //TODO affiché le cd et réactiver le btn dans la ui à la fin
        }
    }
}
