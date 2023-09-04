using System;
using System.Collections.Generic;
using Element.Island;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;
        [HideInInspector] public WorldGenerator worldGenerator;
        [HideInInspector] public IslandGenerator islandGenerator;
        
        [Header("Border Radius")]
        public float innerBorderRadius;
        public float outerBorderRadius;

        [Header("Islands"), SerializeField]
        public int numberOfIslandsPerPlayer; 
        [SerializeField] private int maxSpecialIslandsPerPlayer;
        public IslandData[] allIslandsData;
        public float minDistBetweenIslands;
        public AnimationCurve islandDistFormCenterRepartition;
        public IslandTypesEnum startIslandType;
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
            worldGenerator = GetComponent<WorldGenerator>();
            islandGenerator = GetComponent<IslandGenerator>();
        }

        public void CallWorldGeneration(int numberOfPlayers)
        {
            worldGenerator.GenerateWorld(numberOfPlayers, numberOfIslandsPerPlayer, maxSpecialIslandsPerPlayer);
        }
    }

    public enum IslandTypesEnum
    {
        Meadow,
        Forester,
        Mineral,
        Tropical,
        Winter,
        Cursed,
        Mythic
    }
}
