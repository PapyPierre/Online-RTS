using System.Collections;
using Element.Island;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Specifics
{
    public class Darwin : BaseUnit
    {
       [HideInInspector] public BaseIsland targetIslandToColonise;

        public override void UseSkill()
        {
            base.UseSkill();
            DestroyEntity();
            targetIslandToColonise.CallToColonise();
        }
    }
}