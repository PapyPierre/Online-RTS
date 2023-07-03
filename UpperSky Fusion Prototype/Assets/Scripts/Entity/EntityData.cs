using NaughtyAttributes;
using UnityEngine;

namespace Entity
{
    public class EntityData : ScriptableObject
    {
        /// Main Info Section -------------------------------------------------------------------------

        #region Main Info

        [field: Header("Main Info"), SerializeField] 
        public string Name { get; private set; }
        
        [field: TextArea(2, 5), SerializeField] 
        public string Description { get; private set; }
        
        [field: SerializeField]
        public bool StartAsLocked { get; private set; }
        
        [field: TextArea(2, 5), SerializeField, ShowIf("StartAsLocked")] 
        public string LockedDescription { get; private set; }
        
        [field: SerializeField]
        public int MaterialCost { get; private set; }
        
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
        
        private bool FloatIsGreaterThanZero(float value)
        {
            return value > 0f;
        }
    }
}
