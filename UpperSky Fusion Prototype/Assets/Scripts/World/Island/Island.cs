using System.Collections.Generic;
using Fusion;
using Player;
using UnityEngine;

namespace World.Island
{
    public class Island : NetworkBehaviour
    {
        private WorldManager _worldManager;
        [Networked] public PlayerController Owner { get; set; }
        
        [Networked(OnChanged = nameof(NetworkTypeChanged))] public IslandTypesEnum Type { get; set; }
        
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
            _worldManager.allIslands.Add(this);
        }
        
        private static void NetworkTypeChanged(Changed<Island> changed)
        {
            changed.Behaviour.meshRenderer.material.color = possibleIslandColors[(int) changed.Behaviour.Type];
        }
    }
}
