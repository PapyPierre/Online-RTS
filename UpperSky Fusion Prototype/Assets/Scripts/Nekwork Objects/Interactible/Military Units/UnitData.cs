using UnityEngine;

namespace Nekwork_Objects.Interactible.Military_Units
{
    public enum UnitType
    {
        GroundUnit,
        AerialUnit,
        Both
    }

    [CreateAssetMenu(fileName = "UnitData", menuName = "Unit/UnitData", order = 1)]
    public class UnitData : ScriptableObject
    {
        /// Info Section -------------------------------------------------------------------------
        
        [field: Header("Info"), SerializeField] 
        public string UnitName { get; private set; }
        
        [field: TextArea(2, 5), SerializeField] 
        public string UnitDescription { get; private set; }
        
        [field: SerializeField]
        public UnitType UnitType { get; private set; }
        

        /// Cost Section -------------------------------------------------------------------------

        [field: Header("Cost"), SerializeField]
        public int MaterialCost { get; private set; }
        
        [field: SerializeField] 
        public int OrichalcCost { get; private set; }
        
        [field: SerializeField]
        public int SupplyCost { get; private set; }

        
        /// Movement Section -------------------------------------------------------------------------

        [field:Header("Movement Data"), SerializeField] 
        public float MovementSpeed { get; private set; }
        
        [field: SerializeField]
        public float AllyUnitsPerceptionRadius { get; private set; }
        
        
        /// Combat Section -------------------------------------------------------------------------
 
        [field:Header("Combat Data"), SerializeField]
        public int MaxHealthPoints { get; private set; }
        
        [field: SerializeField]
        public int ArmorPoints { get; private set; }
        
        [field: SerializeField]
        public int DamagePerShootOnUnits { get; private set; }
        
        [field: SerializeField] 
        public int DamagePerShootOnBuildings { get; private set; } 
        
        [field: Tooltip("Range de modulation des dégâts à chaque tirs"), SerializeField]
        public int DamageRangeOfVariation { get; private set; }
        
        [field: Tooltip("% des tirs qui atteint leurs cibles"), Range(0,100), SerializeField]
        public int Accuracy { get; private set; }
        
        [field: Tooltip("% des dégâts infligé qui vont ignoré l’armure"), Range(0,100), SerializeField]
        public int ArmorPenetration { get; private set; }
        
        [field: SerializeField] 
        public float ShotPerSeconds { get; private set; }
        
        [field: SerializeField] 
        public float ShootingRange { get; private set; }
        
        [field: SerializeField] 
        public float VisionRange { get; private set; }
        
        [field: SerializeField] 
        public UnitType TargetableUnitType { get; private set; }
        

        // Optional Data Section -------------------------------------------------------------------------
        
        [field:Header("Optional Data"), SerializeField]
        public int NumberOfTransportableTroops { get; private set; }
        
        [field: SerializeField] 
        public UnitType TypeOfTransportableTroops { get; private set; }
    }
}
