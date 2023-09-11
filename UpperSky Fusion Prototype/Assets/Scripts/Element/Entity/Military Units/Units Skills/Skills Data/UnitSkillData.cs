using UnityEngine;

namespace Element.Entity.Military_Units.Units_Skills.Skills_Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Data/SkillData", order = 3)]
    public class UnitSkillData : ScriptableObject
    {
        [field: SerializeField]
        public UnitsManager.UnitSkillsEnum ThisSkill { get; private set; }
        
        [field: SerializeField]
        public Sprite SkillIcon { get; private set; }
        
        [field: SerializeField]
        public bool IsReadyAtStart { get; private set; }
        
        [field: SerializeField]
        public int CooldownDuration  { get; private set; }
    }
}