using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Nekwork_Objects.Islands
{
    public class IslandsManager : MonoBehaviour
    {
        public static IslandsManager instance;

        public Dictionary<PlayerRef, int> playersIslandsCount;
        
        [field: SerializeField]
        public int MaxBuildingsPerIslands { get; private set; }

        [SerializeField] private IslandTypesClass[] _islandTypes;

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

    public enum IslandTypesEnum
    {
        Autumn,
        Winter,
        Spring,
        Desert,
        Jungle,
        Toundra
    }
    
    [Serializable]
    public class IslandTypesClass
    {
        public IslandTypesEnum type;
        public Gradient colorGradient;
    }
}
