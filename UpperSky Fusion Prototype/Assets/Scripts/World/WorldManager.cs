using System;
using System.Collections.Generic;
using Element.Island;
using Fusion;
using NaughtyAttributes;
using UnityEngine;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;
        private WorldGenerator _worldGenerator;
        private IslandGenerator _islandGenerator;
        
        [Header("Border Radius")]
        public float innerBorderRadius;
        public float outerBorderRadius;

        [Header("Islands"), SerializeField]
        private int numberOfIslandsPerPlayer; 
        [SerializeField] private int maxSpecialIslandsPerPlayer;
        public IslandTypesClass[] islandTypes;
        public NetworkPrefabRef[] islandPrefabs;
        public float minDistBetweenIslands;
        public AnimationCurve islandDistFormCenterRepartition;
        
        [HideInInspector] public List<BaseIsland> allIslands = new();

        [Header("Players")] 
        public List<Color> playersColors;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }

        private void Start()
        {
            _worldGenerator = GetComponent<WorldGenerator>();
            _islandGenerator = GetComponent<IslandGenerator>();
        }

        public void CallWorldGeneration(int numberOfPlayers)
        {
            _worldGenerator.GenerateWorld(numberOfPlayers, numberOfIslandsPerPlayer, maxSpecialIslandsPerPlayer);
        }

        #region Test
        [SerializeField] private IslandTypesEnum typeToGenerate;
        [SerializeField] private Vector3 pos;

        
        [Button()]
        public void CallToGenerateIsland()
        {
            _islandGenerator.GenerateIsland(pos, typeToGenerate);
        }
        #endregion
       
    }

    public enum IslandTypesEnum
    {
        Grassland,
        Forester,
        Tropical,
        Winter,
        Cursed,
        Mythic,
        Mineral,
        Random
    }
    
    [Serializable]
    public class IslandTypesClass
    {
        public IslandTypesEnum type;
        [Expandable] public IslandTypeData data;
    }
}
