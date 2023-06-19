using Custom_UI.InGame_UI;
using Military_Units;
using NaughtyAttributes;
using UnityEngine;

namespace Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Data/BuildingData", order = 1)]
    public class BuildingData : ScriptableObject
    {
        /// Main Data Section -------------------------------------------------------------------------

        #region Main Data
        
        [field: Header("Main Data"), SerializeField] 
        public string Name { get; private set; }
        
        [field: TextArea(2, 5), SerializeField] 
        public string Description { get; private set; }
        
        [field: SerializeField]
        public bool StartAsLocked { get; private set; }
        
        [field: TextArea(2, 5), SerializeField, ShowIf("StartAsLocked")] 
        public string LockedDescription { get; private set; }
        
        [field: SerializeField, ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int MaterialCost { get; private set; }
        
        [field: SerializeField]
        public int OrichalqueCost { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float BuildingTime { get; private set; }
        
        [field: SerializeField, ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int MaxHealthPoints { get; private set; }
        
        [field: SerializeField]
        public int ArmorPoints { get; private set; }
        
        #endregion
        
        /// Optional Data Section -------------------------------------------------------------------------

        #region Obtional Data

        [field: Header("Optional Data"), SerializeField] 
        public BuildingsManager.AllBuildingsEnum[] UnlockedBuildings { get; private set; }
        
        [field: SerializeField]
        public BaseUnit[] UnlockedUnits { get; private set; }
        
        [field: SerializeField]
        public float GeneratedMaterialPerSeconds { get; private set; }
        
        [field: SerializeField]
        public float GeneratedOrichalcPerSeconds { get; private set; }
        
        [field: SerializeField]
        public int AditionnalMaxSupplies { get; private set; }
        
        [field: Tooltip("% du temps de formation soustrait"), Range(0,100), SerializeField]
        public int UnitFormationBonus { get; private set; }
        
        [field: Tooltip("% du temps de construction soustrait"), Range(0,100), SerializeField]
        public int BuildingConstructionBonus { get; private set; }
        
        [field: Tooltip("% du temps de recherche soustrait"), Range(0,100), SerializeField]
        public int TechResearchBonus { get; private set; }

        #endregion

        /// Defense Properties Section -------------------------------------------------------------------------

        #region Defense Properties

        [field: Header("Defense Properties"), SerializeField]
        public bool IsDefenseBuilding { get; private set; }

        [field: SerializeField, ShowIf("IsDefenseBuilding")]
        public int DamagePerShootOnUnits { get; private set; }
        
        [field: Tooltip("Range de modulation des dégâts à chaque tirs"), SerializeField, ShowIf("IsDefenseBuilding")]
        public int DamageRangeOfVariation { get; private set; }
        
        [field: Tooltip("% des tirs qui atteint leurs cibles"), Range(0,100), SerializeField, ShowIf("IsDefenseBuilding")]
        public int ShootAccuracy { get; private set; }
        
        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField, ShowIf("IsDefenseBuilding")]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField, ShowIf("IsDefenseBuilding")] 
        public float ShotPerSeconds { get; private set; }
        
        [field: SerializeField, ShowIf("IsDefenseBuilding")] 
        public float ShootingRange { get; private set; }
        
        [field: SerializeField, ShowIf("IsDefenseBuilding")] 
        public GameManager.EntityType TargetableType { get; private set; }

        #endregion
        
        /// Callback Function For Inspector Purpose -------------------------------------------------------------------------

        private bool IntIsGreaterThanZero(int value)
        {
            return value > 0;
        }
        
        private bool FloatIsGreaterThanZero(float value)
        {
            return value > 0f;
        }
    }
}
