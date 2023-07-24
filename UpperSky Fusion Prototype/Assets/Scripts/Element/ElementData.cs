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
    }
}
