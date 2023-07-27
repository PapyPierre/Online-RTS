using UnityEngine;

namespace Element.Entity.Military_Units.Units_Skills
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Data/SkillData", order = 3)]
    public class UnitSkillData : ScriptableObject
    {
        [field: SerializeField]
        public UnitsManager.UnitSkillsEnum Skill { get; private set; }
        
        [field: SerializeField]
        public Sprite SkillIcon { get; private set; }
        
        [field: SerializeField]
        public bool ReadyAtStart { get; private set; }
    }
}