using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Entity.Military_Units.Units_Specifics;
using Entity.Military_Units;
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

        [field: SerializeField, Expandable] public IslandData Data { get; private set; }

        [field: HideInInspector] [Networked] public int BuildingsCount { get; set; }
        
        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();

        public override void Spawned()
        {
            base.Spawned();

            _worldManager = WorldManager.Instance;

            _worldManager.allIslands.Add(this);

            if (transform.rotation.y != 0)
            {
               minimapIcon.transform.localRotation = Quaternion.Euler(0,0, 
                     minimapIcon.transform.localRotation.z + transform.rotation.y * 2);
            }
            
            GetComponent<FogAgentIsland>().Init(graphObject, canvas, minimapIcon.gameObject);
        }

        public void Init(Transform parent, PlayerController owner)
        {
            transform.parent = parent;
            Owner = owner;

            if (Owner is not null)
            {
                if (Owner == GameManager.thisPlayer)
                {
                    Object.RequestStateAuthority();
                }
            }
        }

        // Fixed for optimisation
        private void FixedUpdate()
        {
            CheckForColonizerUnits();
        }

        private void CheckForColonizerUnits()
        {
            if (UnitsManager.currentlySelectedUnits.Count is 0 || BuildingsCount > 0 || Owner == GameManager.thisPlayer) return;

            foreach (BaseUnit unit in UnitsManager.currentlySelectedUnits)
            {
                if (unit.isDead) continue;

                if (unit.Data.SkillData.Skill is UnitsManager.UnitSkillsEnum.Colonisation && unit.Owner != Owner)
                {
                    var distToUnit = Vector3.Distance(
                        CustomHelper.ReturnPosInTopDown(transform.position), 
                        CustomHelper.ReturnPosInTopDown(unit.transform.position));

                    if (distToUnit <= UnitsManager.distUnitToIslandToColonise)
                    {
                        unit.isSkillReady = true;
                        unit.GetComponent<Darwin>().targetIslandToColonise = this;
                        UIManager.ShowInGameInfoBox(unit, unit.Data, unit.Owner);
                    }
                }
            }
        }

        // Call from coloniser
        public void CallForColonise()
        {
            if (Object.HasStateAuthority) UpdateOwner();
            else Object.RequestStateAuthority();
        }
        
        public void StateAuthorityChanged() => UpdateOwner();
        
        private void UpdateOwner() => Owner = GameManager.thisPlayer;
    }
}