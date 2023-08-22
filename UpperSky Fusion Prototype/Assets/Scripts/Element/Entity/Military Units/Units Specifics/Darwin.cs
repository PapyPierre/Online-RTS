using System.Collections;
using Element.Entity.Military_Units.Units_Skills;
using Element.Island;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Specifics
{
    public class Darwin : BaseUnit
    {
       [HideInInspector] public BaseIsland targetIslandToColonise;

        public override void UseSkill(UnitSkill skill)
        {
            base.UseSkill(skill);
            StartSkillCooldown(skill);
            targetIslandToColonise.CallToColonise();
        }
    }
}