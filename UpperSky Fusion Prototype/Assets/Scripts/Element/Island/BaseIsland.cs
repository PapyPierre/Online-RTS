using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Element.Entity.Buildings;
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

        [Header("Components")]
        public GameObject coloniseBtn;

        public override void Spawned()
        {
            base.Spawned();

            _worldManager = WorldManager.Instance;

            _worldManager.allIslands.Add(this);
            coloniseBtn.SetActive(false);

            
            if (transform.rotation.y != 0)
            {
               minimapIcon.transform.localRotation = Quaternion.Euler(0,0, 
                     minimapIcon.transform.localRotation.z + transform.rotation.y * 2);
            }
            
            GetComponent<FogAgentIsland>().Init(graphObject, canvas, null);
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
            coloniseBtn.SetActive(CheckForColonizerUnits());
        }

        private bool CheckForColonizerUnits()
        {
            if (UnitsManager.currentlySelectedUnits.Count is 0 || BuildingsCount > 0 || Owner is not null) return false;

            foreach (BaseUnit unit in UnitsManager.currentlySelectedUnits)
            {
                if (unit.isDead) continue;

                if (unit.isCurrentlyColonizer && unit.Owner != Owner)
                {
                    var distToUnit = Vector3.Distance(
                        CustomHelper.ReturnPosInTopDown(transform.position), 
                        CustomHelper.ReturnPosInTopDown(unit.transform.position));
                    
                    if (distToUnit <= UnitsManager.distUnitToIslandToColonise) return true;
                }
            }
            
            return false;
        }

        // Call from inspector
        public void CallForColonise()
        {
            if (Object.HasStateAuthority) Colonise();
            else Object.RequestStateAuthority();
        }
        
        public void StateAuthorityChanged()
        {
            Colonise();
        }

        private void Colonise()
        {
            Owner = GameManager.thisPlayer;

            foreach (BaseUnit unit in UnitsManager.currentlySelectedUnits)
            {
                if (unit.isCurrentlyColonizer) 
                {
                    unit.DestroyEntity();
                    break;
                }
            }
        }
    }
}
