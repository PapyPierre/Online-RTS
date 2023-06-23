using NaughtyAttributes;
using UnityEngine;

namespace Entity.Military_Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "Data/UnitData", order = 2)]
    public class UnitData : EntityData
    {
        /// Additional Cost Section -------------------------------------------------------------------------

        #region Additional Cost

        [field: SerializeField, ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int SupplyCost { get; private set; }
        
        [field: SerializeField, Required()]
        public Sprite Icon { get; private set; }

        #endregion
        
        
        /// Movement Data Section -------------------------------------------------------------------------

        #region Movement Data

        [field: Header("Movement Data"), SerializeField, 
                ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float MovementSpeed { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float AllyUnitsPerceptionRadius { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float WeatherResistance { get; private set; }

        #endregion
      
        
        /// Combat Data Section -------------------------------------------------------------------------

        #region Combat Data

        [field: Header("Combat Data"),  SerializeField]
        public int DamagePerShootOnUnits { get; private set; }
        
        [field: SerializeField] 
        public int DamagePerShootOnBuildings { get; private set; } 
        
        [field: Tooltip("Range de modulation des dégâts à chaque tirs"), SerializeField]
        public int DamageRangeOfVariation { get; private set; }
        
        [field: Tooltip("% des tirs qui atteint leurs cibles"), Range(0,100), SerializeField, 
                ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int ShootAccuracy { get; private set; }
        
        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float ShotPerSeconds { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float ShootingRange { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float VisionRange { get; private set; }
        
        [field: SerializeField] 
        public GameManager.EntityType TargetableEntityType { get; private set; }

        #endregion
        

        // Optional Data Section -------------------------------------------------------------------------

        #region Optional Data

        [field:Header("Optional Data"), SerializeField]
        public int NumberOfTransportableTroops { get; private set; }

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
