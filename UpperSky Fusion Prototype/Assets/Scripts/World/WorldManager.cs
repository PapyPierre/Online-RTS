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
        
        [Header("Border Radius")]
        public float innerBorderRadius;
        public float outerBorderRadius;

        [Header("Islands"), SerializeField]
        private int numberOfIslandsPerPlayer; 
        [SerializeField] private int maxSpecialIslandsPerPlayer;
        public IslandTypesClass[] islandTypes;
        public NetworkPrefabRef islandPrefab;
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
        }

        public void CallWorldGeneration(int numberOfPlayers)
        {
            _worldGenerator.GenerateWorld(numberOfPlayers, numberOfIslandsPerPlayer, maxSpecialIslandsPerPlayer);
        }
    }

    public enum IslandTypesEnum
    {
        Uninitialized,
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
        [MinMaxSlider(0,100)] public Vector2 rarity;
    }
}
