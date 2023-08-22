using Element.Entity.Military_Units;
using Element.Entity.Military_Units.Units_Specifics;
using UnityEngine;
using UserInterface;

namespace Element.Island
{
   public class ColoniserCheck : MonoBehaviour
   {
      [SerializeField] private BaseIsland myIsland;

      private void OnTriggerEnter(Collider other)
      {
         if (myIsland.BuildingsCount > 0 || myIsland.PlayerIsOwner()) return;

         if (other.CompareTag("Unit"))
         {
            BaseUnit unit = other.GetComponent<BaseUnit>();
            
            if (unit.isDead) return;

            foreach (var skill in unit.skills)
            {
               if (skill.Data.ThisSkill is UnitsManager.UnitSkillsEnum.Colonisation && unit.Owner != myIsland.Owner)
               {
                  if (skill.timeLeftOnCd <= 0)
                  {
                     skill.isReady = true;
                     unit.GetComponent<Darwin>().targetIslandToColonise = myIsland;
                     UIManager.Instance.UpdateSelectionInfobox(unit, unit.Data, unit.Owner);
                  }
               }
            }
         }
      }

      private void OnTriggerExit(Collider other)
      {
         if (other.CompareTag("Unit"))
         {
            BaseUnit unit = other.GetComponent<BaseUnit>();

            if (unit.isDead || unit.Object == null) return;
            
            foreach (var skill in unit.skills)
            {
               if (skill.Data.ThisSkill is UnitsManager.UnitSkillsEnum.Colonisation && unit.Owner != myIsland.Owner)
               {
                  skill.isReady = false;
                  unit.GetComponent<Darwin>().targetIslandToColonise = null;
                  UIManager.Instance.UpdateSelectionInfobox(unit, unit.Data, unit.Owner);
               }
            }
         }
      }
   }
}
