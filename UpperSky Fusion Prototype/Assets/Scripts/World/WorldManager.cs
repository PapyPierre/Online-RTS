using System;
using System.Collections.Generic;
using Fusion;
using NaughtyAttributes;
using UnityEngine;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;
        
        [Header("Border Radius")]
        public float innerBorderRadius;
        public float outerBorderRadius;

        [Header("Islands")]
        public IslandTypesClass[] islandTypes;
        public NetworkPrefabRef islandPrefab;
        public float minDistBetweenIslands;
        public AnimationCurve islandDistFormCenterRepartition;
        [field: SerializeField] public int MaxBuildingsPerIslands { get; private set; }
        
        [HideInInspector] public List<Island.Island> allIslands = new();
        public Dictionary<PlayerRef, int> PlayersIslandsCount;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }

        public void CallWorldGeneration(int numberOfPlayers)
        {
            GetComponent<WorldGenerator>().GenerateWorld(numberOfPlayers);
        }
    }

    public enum IslandTypesEnum
    {
        Uninitialized,
        Starting,
        Basic,
        Living,
        Mythic,
        JungleSanctuary,
        DesertSanctuary,
        MontainSanctuary
    }
    
    [Serializable]
    public class IslandTypesClass
    {
        public IslandTypesEnum type;
        public Gradient colorGradient;
        [MinMaxSlider(0,100)] public Vector2 rarity;
    }
}
