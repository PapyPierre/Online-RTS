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

        [Networked] public int BuildingsCount { get; set; }
        
        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();

        public override void Spawned()
        {
            base.Spawned();

            _worldManager = WorldManager.Instance;

            _worldManager.allIslands.Add(this);

            if (transform.rotation.y != 0)
            {
               minimapIcon.transform.localRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            }
            
            GetComponent<FogAgentIsland>().Init(graphObject, null, minimapIcon.gameObject);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_NetworkInit(PlayerController owner, int TypeIndex)
        {
            // The code inside here will run on the client which owns this object (has state and input authority).

            data = _worldManager.allIslandsData[TypeIndex];
            
            ground.material.color =  data.GroundColor;
            rockMesh.material.color =  data.RockColor;
            
            _worldManager.islandGenerator.GeneratePropsOnIsland(this, data);
            Debug.Log(data);
            Init(owner, data);
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