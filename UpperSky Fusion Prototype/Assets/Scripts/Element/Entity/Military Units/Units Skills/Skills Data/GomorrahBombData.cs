using Element.Entity.Military_Units.Units_Skills.Skills_Data;
using Fusion;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Skills
{
    [CreateAssetMenu(fileName = "GomorrahBombData", menuName = "Data/GomorrahBombData", order = 4)]
    public class GomorrahBombData : UnitSkillData
    {
        [field: SerializeField]
        public NetworkPrefabRef NetworkPrefab { get; private set; }
        
        [field: SerializeField]
        public int NumberOfLaunch  { get; private set; }
        
        [field: SerializeField]
        public float TimeBetweenLaunch  { get; private set; }

        [field: SerializeField]
        public int ImpactDamage  { get; private set; }

        [field: SerializeField, Range(0,100)]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField]
        public float ImpactRange  { get; private set; }

        [field: SerializeField]
        public NetworkPrefabRef ExplosionVfx  { get; private set; }
    }
}