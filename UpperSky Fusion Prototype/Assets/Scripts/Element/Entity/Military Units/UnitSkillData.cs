using Fusion;
using UnityEngine;

namespace Element.Entity.Military_Units
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Data/SkillData", order = 3)]
    public class UnitSkillData : ScriptableObject
    {
        [field: SerializeField]
        public UnitsManager.UnitSkillsEnum Skill { get; private set; }
        
        [field: SerializeField]
        public Sprite SkillIcon { get; private set; }

        [field: SerializeField]
        public NetworkPrefabRef NetworkPrefab { get; private set; }
        
        [field: SerializeField]
        public int NumberOfLaunch  { get; private set; }
        
        [field: SerializeField]
        public float TimeBetweenLaunch  { get; private set; }

        [field: SerializeField]
        public float ImpactDamage  { get; private set; }

        [field: SerializeField, Range(0,100)]
        public float ArmorPenetration  { get; private set; }
        
        [field: SerializeField]
        public float ImpactRange  { get; private set; }
    }
}