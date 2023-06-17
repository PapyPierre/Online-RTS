using System.Collections.Generic;
using UnityEngine;

namespace Military_Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;
        
        [Header("Units Params")]
        public List<BaseUnit> allActiveUnits;
        
        #region Boids Logic
        public bool useWeights;
        public float separationWeight;
        public float cohesionWeight;
        public float alignmentWeight;
        
        [Space] public float flyingHeightOfUnits;
        #endregion
        
     
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }
    }
}
