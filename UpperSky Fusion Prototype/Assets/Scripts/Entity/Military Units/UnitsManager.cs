using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Entity.Military_Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;

        public NetworkPrefabRef[] allUnitsPrefab;
        public List<UnitData> allUnitsData;

        public enum AllUnitsEnum
        {
          Darwin = 0, 
          Beethoven = 1,
          Galileo = 2,
          Hokusai = 3,
          Magellan = 4,
          Wagner = 5,
          Oppenheimer = 6
        }
        
        [Header("Units Params")]
        public List<BaseUnit> allActiveUnits;
        
        #region Boids Logic
        public bool useWeights;
        public float separationWeight;
        public float cohesionWeight;
        public float alignmentWeight;

        public float distToTargetToTryStop;
        public float distToTargetToStop;
        public float unitsPerceptionRadius;
        public float flyingHeightOfUnits;
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
