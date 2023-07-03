using System.Collections.Generic;
using Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;

namespace World.Island
{
    public class Island : NetworkBehaviour
    {
        private WorldManager _worldManager;
        private GameManager _gameManager;
        private UnitsManager _unitsManager;
        
        [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        
        [Networked] public IslandTypesEnum Type { get; set; }
        
        [SerializeField] private MeshRenderer meshRenderer;
        public GameObject coloniseBtn;

        public int buildingsCount;
        public int localOrichalRessources;

        public override void Spawned()
        {
            _worldManager = WorldManager.Instance;
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;
            
            _worldManager.allIslands.Add(this);
            coloniseBtn.SetActive(false);
        }

        private void FixedUpdate() // Fixed for optimisation
        {
            coloniseBtn.SetActive(CheckForColonizerUnits());
        }

        private bool CheckForColonizerUnits()
        {
            if (_unitsManager.currentlySelectedUnits.Count is 0 || buildingsCount > 0) return false;

            foreach (BaseUnit unit in _unitsManager.currentlySelectedUnits)
            {
                if (unit.isColonizer && unit.Owner != Owner)
                {
                    var correctedPos = new Vector3(transform.position.x, 0, transform.position.z);
                    var unitCorrectedPos = new Vector3(unit.transform.position.x, 0, unit.transform.position.z);

                    var distToUnit = Vector3.Distance(correctedPos, unitCorrectedPos);
                    
                    if (distToUnit <= _unitsManager.distUnitToIslandToColonise)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        private static void OwnerChanged(Changed<Island> changed)
        {
            if (changed.Behaviour.Owner == null) return;

            for (var i = 0; i < GameManager.Instance.connectedPlayers.Count; i++)
            {
                PlayerController player = GameManager.Instance.connectedPlayers[i];
                if (player == changed.Behaviour.Owner)
                {
                    changed.Behaviour.meshRenderer.material.color = WorldManager.Instance.playersColors[i];
                    return;
                }
            }
        }

        // Call from inspector
        public void Colonise()
        {
            RPC_ModifyOwner(_gameManager.thisPlayer);

            foreach (BaseUnit unit in _unitsManager.currentlySelectedUnits)
            {
                if (unit.isColonizer)
                {
                    unit.DestroyEntity();
                    break;
                }
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ModifyOwner(PlayerController newOwner)
        {
            // The code inside here will run on the client which owns this object (has state and input authority).
            Owner = newOwner;
        }
    }
}
