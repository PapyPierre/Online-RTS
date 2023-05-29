using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nekwork_Objects.Interactible.Military_Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager instance;
        
        public List<BaseUnit> allActiveUnits;
        
        #region Boids Logic
        public bool useWeights;
        public float separationWeight;
        public float cohesionWeight;
        public float alignmentWeight;
        
        [Space] 
        public float flyingHeightOfAerianUnits;
        
        #endregion
        
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            instance = this;
        }
    }
}
