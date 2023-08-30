using NaughtyAttributes;
using UnityEngine;

namespace Element
{
    /// <summary>
    /// Elements include Islands, Buildings and Units
    /// </summary>

    public class ElementData : ScriptableObject
    {
        [field: Header("Main Info"), SerializeField] 
        public string Name { get; private set; }
        
        [field: TextArea(2, 5), SerializeField] 
        public string Description { get; private set; }
    
        [field: SerializeField, Required()]
        public Sprite Icon { get; private set; }
        
        [field: SerializeField, ValidateInput("IntIsGreaterThanZero", "Must be greater than zero")] 
        public int SightRange { get; private set; }
        
        /// Function For Inspector Purpose -----------------------------------------------------------------------------

        private bool IntIsGreaterThanZero(int value)
        {
            return value > 0;
        }
    }
}
