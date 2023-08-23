using System;
using Element.Entity.Military_Units.Units_Skills.Skills_Data;
using NaughtyAttributes;
using UnityEngine;

namespace Element.Entity.Military_Units.Units_Skills
{
    [Serializable]
    public class UnitSkill
    {
        [HideInInspector] public bool isReady;
        [HideInInspector] public bool isActive;
        [HideInInspector] public float timeLeftOnCd;
        [HideInInspector] public float cdCompletion; 
        [field: SerializeField, Expandable] public UnitSkillData Data { get; private set; }
    }
}