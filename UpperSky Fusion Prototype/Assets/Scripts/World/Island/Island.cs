using System;
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
        
        [Networked] public PlayerController Owner { get; set; }
        
        [Networked(OnChanged = nameof(NetworkTypeChanged))] public IslandTypesEnum Type { get; set; }
        
        [SerializeField] private MeshRenderer meshRenderer;
        public GameObject coloniseBtn;

        private static List<Color> possibleIslandColors = new ();

        public int buildingsCount;
        public int localOrichalRessources;

        public override void Spawned()
        {
            _worldManager = WorldManager.Instance;
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;

            foreach (var islandTypesClass in _worldManager.islandTypes)
            {
                possibleIslandColors.Add(islandTypesClass.colorGradient.Evaluate(0));
            }
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
        
        private static void NetworkTypeChanged(Changed<Island> changed)
        {
            changed.Behaviour.meshRenderer.material.color = possibleIslandColors[(int) changed.Behaviour.Type];
        }

        // Call from inspector
        public void Colonise()
        {
            Object.RequestStateAuthority();
            Owner = _gameManager.thisPlayer;
            
            //TODO Color in player color

            foreach (BaseUnit unit in _unitsManager.currentlySelectedUnits)
            {
                if (unit.isColonizer)
                {
                    unit.DestroyEntity();
                    break;
                }
            }
        }
    }
}
