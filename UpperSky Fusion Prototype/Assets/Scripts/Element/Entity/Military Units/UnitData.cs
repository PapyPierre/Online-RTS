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
        
       // [field: SerializeField, Expandable] public UnitsManager.UnitSkillsEnum[] Skills { get; private set; }

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
        public UnitsManager.ShootingMode ShootingMode { get; private set; }

        
        [field: SerializeField, ShowIf("Automatic")]
        public int ContinuiousDamage { get; private set; }
        
        
        [field: SerializeField, ShowIf("ShotByShot")]
        public int DamagePerShoot { get; private set; }
        
        
        [field: SerializeField, ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero"),
                ShowIf("ShotByShot")] 
        public float RealodTime { get; private set; }

        
        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField,
                ShowIf(EConditionOperator.Or, "ShotByShot", "Automatic")]
        public int ArmorPenetration { get; private set; }
        
        
        [field: SerializeField, ShowIf(EConditionOperator.Or, "ShotByShot", "Automatic"),
                ValidateInput("FloatIsGreaterThanZero", "Must be greater than zero")]
        public float ShootingRange { get; private set; }
        

        #endregion

        /// Callback Function For Inspector Purpose -------------------------------------------------------------------------

        private bool ShotByShot()
        {
            return ShootingMode == UnitsManager.ShootingMode.ShotByShot;
        }

        private bool Automatic()
        {
            return ShootingMode == UnitsManager.ShootingMode.Automatic;
        }
        
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
