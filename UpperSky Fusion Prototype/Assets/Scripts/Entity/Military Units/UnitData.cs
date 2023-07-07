using NaughtyAttributes;
using UnityEngine;

namespace Entity.Military_Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "Data/UnitData", order = 2)]
    public class UnitData : EntityData
    {
        /// Additional Data Section -------------------------------------------------------------------------

        #region Additional Data

        [field: Header("Additional Data"), SerializeField, 
                ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int SupplyCost { get; private set; }
        
        [field: SerializeField, Required()]
        public Sprite Icon { get; private set; }

        #endregion
        
        
        /// Movement Data Section -------------------------------------------------------------------------

        #region Movement Data

        [field: Header("Movement Data"), SerializeField, 
                ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float MovementSpeed { get; private set; }
        
        [field: SerializeField, 
                ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float AngularSpeed { get; private set; }

        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float WeatherResistance { get; private set; }

        #endregion
      
        
        /// Combat Data Section -------------------------------------------------------------------------

        #region Combat Data

        [field: Header("Combat Data"),  SerializeField]
        public int DamagePerShootOnUnits { get; private set; }
        
        [field: SerializeField] 
        public int DamagePerShootOnBuildings { get; private set; }

        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float RealodTime { get; private set; }

        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float ShootingRange { get; private set; }

        [field: SerializeField] 
        public GameManager.EntityType TargetableEntityType { get; private set; }

        #endregion
        
        /// Base Status Section -------------------------------------------------------------------------

        #region Base Status

        [field: Header("Base Unit Status"),  SerializeField]
        public bool IsBaseColonizer { get; private set; }
        
        [field: SerializeField] 
        public bool IsBaseCamouflaged { get; private set; }
        
        [field: SerializeField] 
        public float BaseRegeneration { get; private set; }
       
        [field: SerializeField] 
        public float BaseAcid { get; private set; }
        
        [field: SerializeField] 
        public float BaseParasite { get; private set; }

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
