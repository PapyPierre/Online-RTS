using System;
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
    public class BaseIsland : BaseElement, IStateAuthorityChanged
    {
        private WorldManager _worldManager;
        
        [Networked] private PlayerController TempOwner { get; set; }
        [Networked] private IslandTypesEnum Type { get; set; }
        [HideInInspector] public IslandData data;
        
        [SerializeField, Header("Other")] private MeshRenderer ground;
        [SerializeField] private MeshRenderer rockMesh;
        [SerializeField] private BoxCollider boxCollider;
        
        [ReadOnly] public bool hasGeneratedProps;

        [Networked] public int BuildingsCount { get; set; }
        
        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();
        
        public void BeforeSpawnInit(PlayerController owner, IslandTypesEnum islandType)
        {
            // The code inside here will run only on the client that called the spawn of this object
            TempOwner = owner;
            Type = islandType;
        }

        public override void Spawned()
        {
            base.Spawned();

            _worldManager = WorldManager.Instance;
            _worldManager.allIslands.Add(this);
            
            transform.parent = _worldManager.worldGenerator.worldCenter;

            data = _worldManager.allIslandsData[(int) Type]; 
            
            Init(TempOwner, data);

            minimapIcon.sprite = data.Icon;
            
            ground.material.color =  data.GroundColor;
            rockMesh.material.color =  data.RockColor;

            StartCoroutine(WaitNetworkSync());
        }

        // Wait for network synchronisation before generating props on island
        private IEnumerator WaitNetworkSync()
        {
            yield return new WaitForSeconds(2);
            _worldManager.islandGenerator.GeneratePropsOnIsland(this, data, boxCollider);

            Transform worldCenter = _worldManager.worldGenerator.worldCenter;

            if (worldCenter == null)
            {
                worldCenter = GameObject.FindWithTag("WorldCenter").transform;
            }
            
            minimapCanvas.transform.rotation = Quaternion.LookRotation(worldCenter.up, Vector3.up); 
           // Quaternion.Euler(90, 180, -transform.rotation.y -worldCenter.rotation.y);
        }
        
        public void FogOfWarInit()
        {
           GetComponent<FogAgentIsland>().Init(graphObject, null, minimapCanvas.gameObject);
        }

        // Call from coloniser
        public void Colonise()
        {            
            if (Object.HasStateAuthority) UpdateOwner();
            else Object.RequestStateAuthority();
        }

        public void StateAuthorityChanged()
        {
            if (!GameManager.gameIsStarted) return;
            UpdateOwner();
        } 
        
        private void UpdateOwner()
        {
            if (Owner != null)
            {
                CheckIfOwnerHaveOtherIslandsOfGivenType(IslandTypesEnum.Winter);
                Owner.RPC_LostAnIsland();
            }

            Owner = GameManager.thisPlayer;
            
            if (GameManager.gameIsStarted)
            {
                Owner.RPC_GainAnIsland();
                OnColonisationEffect();
            }
        }

        private void OnColonisationEffect()
        {
            switch (data.Type)
            {
                case IslandTypesEnum.Winter:
                    Owner.ControlWinterIsland = true;
                    break;
                case IslandTypesEnum.Mythic:
                    foreach (var unit in UnitsManager.allActiveUnits)
                    {
                        if (unit.PlayerIsOwner())
                        {
                            unit.currentStatusOnThisUnit.Clear();
                            unit.RPC_Heal(20);
                        }
                    }
                    break;
            }
        }

        private void CheckIfOwnerHaveOtherIslandsOfGivenType(IslandTypesEnum type)
        {
            if (data.Type == type)
            {
                var i = 0;
                foreach (var island in _worldManager.allIslands)
                {
                    if (island.Owner != Owner) continue;
                    
                    if (island.data.Type == type)
                    {
                        i++;
                    }
                }
                
                if (i <= 1)
                {
                    switch (type)
                    {
                        case IslandTypesEnum.Winter:
                            Owner.ControlWinterIsland = false;
                            break;
                    }
                }
            }
        }
    }
}