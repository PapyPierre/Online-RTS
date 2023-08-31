using System.Collections.Generic;
using Element.Entity.Buildings;
using Fusion;
using NaughtyAttributes;
using Player;
using Ressources.AOSFogWar.Used_Scripts;
using UnityEngine;
using World;

namespace Element.Island
{
    public class BaseIsland : BaseElement
    {
        private WorldManager _worldManager;

      //  [field: SerializeField, Expandable]
       [Networked] public int TypeIndex { get; set; }
       [HideInInspector] public IslandData Data { get; set; }

        public MeshRenderer ground;
        public MeshRenderer rockMesh;

        [Networked] public int BuildingsCount { get; set; }
        
        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();

        public override void Spawned()
        {
            base.Spawned();

            _worldManager = WorldManager.Instance;

            if (Data == null)
            {
                Data = _worldManager.allIslandsData[TypeIndex];
            }

            _worldManager.allIslands.Add(this);

            if (transform.rotation.y != 0)
            {
               minimapIcon.transform.localRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            }
            
            GetComponent<FogAgentIsland>().Init(graphObject, canvas, minimapIcon.gameObject);
        }
        
      

        // Call from coloniser
        public void CallToColonise()
        {
            if (Object.HasStateAuthority) UpdateOwner();
            else Object.RequestStateAuthority();
        }
        
        public void StateAuthorityChanged() => UpdateOwner();
        
        private void UpdateOwner()
        { 
            Owner = GameManager.thisPlayer;
        }
    }
}