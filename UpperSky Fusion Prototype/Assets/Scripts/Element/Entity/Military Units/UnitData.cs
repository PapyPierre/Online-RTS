using Element.Entity.Military_Units.Units_Skills;
using Entity;
using NaughtyAttributes;
using UnityEngine;

namespace Element.Entity.Military_Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "Data/UnitData", order = 3)]
    public class UnitData : EntityData
    {
        /// Additional Data Section -------------------------------------------------------------------------

        #region Additional Data

        [field: Header("Additional Unit Data"), SerializeField, 
                ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int SupplyCost { get; private set; }
        
        [field: SerializeField, Expandable] public UnitSkillData SkillData { get; private set; }

        #endregion
        
        
        /// Movement Data Section -------------------------------------------------------------------------

        #region Movement Data

        [field: Header("Movement Data"), SerializeField, 
                ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")] 
        public float MovementSpeed { get; private set; }
        
        [field: SerializeField] 
        public float AngularSpeed { get; private set; }

        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float WeatherResistance { get; private set; }

        #endregion
      
        
        /// Combat Data Section -------------------------------------------------------------------------

        #region Combat Data
        
        [field: Header("Combat Data"),  SerializeField]
        public bool CanShoot { get; private set; }

        [field: SerializeField, ShowIf("CanShoot")]
        public int DamagePerShoot { get; private set; }

        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField,
                ShowIf("CanShoot")]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero"),
                ShowIf("CanShoot")] 
        public float RealodTime { get; private set; }

        #endregion
        
        /// Base Status Section -------------------------------------------------------------------------

        #region Base Status

        [field: Header("Base Unit Status"),  SerializeField]
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
