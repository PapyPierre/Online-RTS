using System.Collections;
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
        
        [HideInInspector] public IslandData data;

        public MeshRenderer ground;
        public MeshRenderer rockMesh;

        [ReadOnly] public bool hasGeneratedProps;

        [Networked] public int BuildingsCount { get; set; }
        
        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();

        public override void Spawned()
        {
            base.Spawned();

            _worldManager = WorldManager.Instance;
            _worldManager.allIslands.Add(this);
        }

        public void SetUp(PlayerController owner, IslandData islandData)
        {
            data = islandData;
            Init(owner, data);
            
            ground.material.color =  data.GroundColor;
            rockMesh.material.color =  data.RockColor;
            
            _worldManager.islandGenerator.GeneratePropsOnIsland(this, data);
            transform.parent = _worldManager.worldGenerator.worldCenter;
        }

        public void FogOfWarInit()
        {
           GetComponent<FogAgentIsland>().Init(graphObject, null, minimapIcon.gameObject);
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