using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;

namespace World.Island
{
    public class Island : NetworkBehaviour, IStateAuthorityChanged
    {
        private WorldManager _worldManager;
        private GameManager _gameManager;
        private UnitsManager _unitsManager;
        
        [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        
        [Networked] public IslandTypesEnum Type { get; set; }
        
        [SerializeField] private GameObject graphObject;
        [SerializeField] private GameObject canvas;

        [SerializeField] private MeshRenderer meshRenderer;
        public GameObject coloniseBtn;
        private bool _colonise;

        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();
        [Networked] public int BuildingsCount { get; set; }

        public override void Spawned()
        {
            _worldManager = WorldManager.Instance;
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;

            _worldManager.allIslands.Add(this);
            coloniseBtn.SetActive(false);

            if (Owner is not null)
            {
                if (Owner == _gameManager.thisPlayer)
                {
                    Object.RequestStateAuthority();
                }
            }
            
            GetComponent<FogAgent_Island>().Init(graphObject, canvas);
        }

        public void Init(Transform parent, PlayerController owner, IslandTypesEnum type)
        {
            transform.parent = parent;
            Owner = owner;
            Type = type;
        }

        private void FixedUpdate() // Fixed for optimisation
        {
            coloniseBtn.SetActive(CheckForColonizerUnits());
        }

        private bool CheckForColonizerUnits()
        {
            if (_unitsManager.currentlySelectedUnits.Count is 0 || BuildingsCount > 0) return false;

            foreach (BaseUnit unit in _unitsManager.currentlySelectedUnits)
            {
                if (unit.isColonizer && unit.Owner != Owner)
                {
                    var distToUnit = Vector3.Distance(
                        CustomHelper.ReturnPosInTopDown(transform.position), 
                        CustomHelper.ReturnPosInTopDown(unit.transform.position));
                    
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
        public void CallForColonise()
        {
            _colonise = true;

            if (Object.HasStateAuthority) Colonise();
            else Object.RequestStateAuthority();
        }
        
        public void StateAuthorityChanged()
        {
            Colonise();
        }

        private void Colonise()
        {
            if (_colonise)
            { 
                Owner = _gameManager.thisPlayer;

                foreach (BaseUnit unit in _unitsManager.currentlySelectedUnits)
                {
                    if (unit.isColonizer)
                    {
                        unit.DestroyEntity();
                        break;
                    }
                }

                _colonise = false;
            }
        }
    }
}
