using System.Collections;
using Element.Entity.Military_Units.Units_Skills;
using Element.Entity.Military_Units.Units_Skills.Skills_Behaviour;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Specifics
{
    public class Galileo : BaseUnit
    {
        public override void UseSkill(UnitSkill skill)
        {
            base.UseSkill(skill);
            StartCoroutine(LaunchBombs(skill));
        }

        private IEnumerator LaunchBombs(UnitSkill skill)
        {
            if (skill.Data is GomorrahBombData data)
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
            }
            
            StartSkillCooldown(skill);
        }
    }
}
