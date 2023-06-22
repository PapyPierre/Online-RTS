using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace World.Island
{
    public class Island : NetworkBehaviour
    {
        private WorldManager _worldManager;
        [Networked] public PlayerRef Owner { get; set; }
        
        [Networked(OnChanged = nameof(NetworkTypeChanged))] public IslandTypesEnum NetworkType { get; set; }
        
        [SerializeField] private MeshRenderer meshRenderer;
        private static List<Color> possibleIslandColors = new ();

        public int buildingsCount;
        public int localOrichalRessources;

        public override void Spawned()
        {
            _worldManager = WorldManager.Instance;
            foreach (var islandTypesClass in _worldManager.islandTypes)
            {
                possibleIslandColors.Add(islandTypesClass.colorGradient.Evaluate(0));
            }
        }
        
        private static void NetworkTypeChanged(Changed<Island> changed)
        {
            changed.Behaviour.meshRenderer.material.color = changed.Behaviour.NetworkType switch
            {
                IslandTypesEnum.Uninitialized => possibleIslandColors[0],
                IslandTypesEnum.Starting => possibleIslandColors[1],
                IslandTypesEnum.Basic => possibleIslandColors[2],
                IslandTypesEnum.Living => possibleIslandColors[3],
                IslandTypesEnum.Mythic => possibleIslandColors[4],
                IslandTypesEnum.JungleSanctuary => possibleIslandColors[5],
                IslandTypesEnum.DesertSanctuary => possibleIslandColors[6],
                IslandTypesEnum.MontainSanctuary => possibleIslandColors[7],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
