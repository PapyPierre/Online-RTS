using Element;
using NaughtyAttributes;
using UnityEngine;

namespace Entity
{
    /// <summary>
    /// Entity include Buildings and Units
    /// </summary>

    public class EntityData : ElementData
    {
        /// Main Info Section -------------------------------------------------------------------------

        #region Entity Main Info

        [field: Header("Entity Main Info"), SerializeField]
        public bool StartAsLocked { get; private set; }
        
        [field: TextArea(2, 5), SerializeField, ShowIf("StartAsLocked")] 
        public string LockedDescription { get; private set; }

        [field: SerializeField]
        public int WoodCost { get; private set; }
        
        [field: SerializeField]
        public int MetalsCost { get; private set; }
        
        [field: SerializeField]
        public int OrichalqueCost { get; private set; }
        
        [field: SerializeField]
        public float ProductionTime { get; private set; }
        
        [field: SerializeField, ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")]
        public int MaxHealthPoints { get; private set; }
        
        [field: SerializeField]
        public int MaxArmorPoints { get; private set; }
        

        #endregion
        
        /// Function For Inspector Purpose -----------------------------------------------------------------------------

        private bool IntIsGreaterThanZero(int value)
        {
            return value > 0;
        }
    }
}
