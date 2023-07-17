using Entity.Military_Units;
using NaughtyAttributes;
using UnityEngine;

namespace Entity.Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Data/BuildingData", order = 1)]
    public class BuildingData : EntityData
    {
        /// Additional Main Data Section ------------------------------------------------------------------------------------------

        #region Additional Main Data
        
        [field: SerializeField] 
        public BuildingsManager.AllBuildingsEnum ThisBuilding { get; private set; }

        [field: SerializeField] 
        public BuildingsManager.AllBuildingsEnum[] UnlockedBuildings { get; private set; }
        
        [field: SerializeField, Tooltip("Is this building forming units ?")]
        public bool IsFormationBuilding { get; private set; }
        
        [field: SerializeField, ShowIf("IsFormationBuilding")] 
        public UnitsManager.AllUnitsEnum[] FormableUnits { get; private set; }

        #endregion
        
        /// Generated Ressources Section -------------------------------------------------------------------------------
        
        #region Generated Ressources

        [field: Header("Generated Ressources"), SerializeField]
        public bool DoesGenerateRessources { get; private set; }
        
        [field: SerializeField, ShowIf("DoesGenerateRessources")]
        public float GeneratedMaterialPerSeconds { get; private set; }
        
        [field: SerializeField, ShowIf("DoesGenerateRessources")]
        public float GeneratedOrichalquePerSeconds { get; private set; }
        
        [field: SerializeField, ShowIf("DoesGenerateRessources")]
        public int AditionnalMaxSupplies { get; private set; }
        
        #endregion
        
        /// Timing Reduction Section -----------------------------------------------------------------------------------

        #region Timing Reduction

        [field: Header("Timing Reduction"),  SerializeField]
        public bool HasTimingReduction { get; private set; }
        
        [field: Tooltip("% du temps de formation soustrait"), Range(0,100), SerializeField, ShowIf("HasTimingReduction")]
        public int UnitFormationBonus { get; private set; }
        
        [field: Tooltip("% du temps de construction soustrait"), Range(0,100), SerializeField, ShowIf("HasTimingReduction")]
        public int BuildingConstructionBonus { get; private set; }
        
        [field: Tooltip("% du temps de recherche soustrait"), Range(0,100), SerializeField, ShowIf("HasTimingReduction")]
        public int TechResearchBonus { get; private set; }

        #endregion

        /// Defense Properties Section ---------------------------------------------------------------------------------

        #region Defense Properties

        [field: Header("Defense Properties"), SerializeField]
        public bool IsDefenseBuilding { get; private set; }

        [field: SerializeField, ShowIf("IsDefenseBuilding")]
        public int DamagePerShootOnUnits { get; private set; }

        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField, ShowIf("IsDefenseBuilding")]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField, ShowIf("IsDefenseBuilding"), ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float RealodTime { get; private set; }

        #endregion
        
        /// Function For Inspector Purpose -----------------------------------------------------------------------------

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
