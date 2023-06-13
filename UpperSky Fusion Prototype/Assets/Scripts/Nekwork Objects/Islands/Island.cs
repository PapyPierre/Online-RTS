using System;
using Fusion;
using UnityEngine;
using World;

namespace Nekwork_Objects.Islands
{
    public class Island : NetworkBehaviour
    {
        private WorldManager _worldManager;
        public PlayerRef owner;
        
        public IslandTypesEnum type;
        [SerializeField] private MeshRenderer meshRenderer;

        public int buildingsCount;
        public int localOrichalRessources;

        private void Start()
        {
            _worldManager = WorldManager.instance;
            ColorIsland();
        }

        private void ColorIsland()
        {
            meshRenderer.material.color = type switch
            {
                IslandTypesEnum.Starting => _worldManager.islandTypes[0].colorGradient.Evaluate(0),
                IslandTypesEnum.Basic => _worldManager.islandTypes[1].colorGradient.Evaluate(0),
                IslandTypesEnum.Living => _worldManager.islandTypes[2].colorGradient.Evaluate(0),
                IslandTypesEnum.Mythic => _worldManager.islandTypes[3].colorGradient.Evaluate(0),
                IslandTypesEnum.JungleSanctuary => _worldManager.islandTypes[4].colorGradient.Evaluate(0),
                IslandTypesEnum.DesertSanctuary => _worldManager.islandTypes[5].colorGradient.Evaluate(0),
                IslandTypesEnum.MontainSanctuary => _worldManager.islandTypes[6].colorGradient.Evaluate(0),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
