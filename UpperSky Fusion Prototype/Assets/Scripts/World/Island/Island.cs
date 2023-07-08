using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace World.Island
{
    public class Island : NetworkBehaviour, IStateAuthorityChanged
    {
        private WorldManager _worldManager;
        private GameManager _gameManager;
        private UnitsManager _unitsManager;

        [field: Header("Networked Properties")]
        [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        [Networked] public IslandTypesEnum Type { get; set; }
        [Networked] public int BuildingsCount { get; set; }
        
        [HideInInspector] public List<BaseBuilding> buildingOnThisIsland = new();

        [Header("Components")]
        [SerializeField] private GameObject graphObject;
        [SerializeField] private GameObject canvas;
        [SerializeField] private Image minimapIcon;
        [SerializeField] private MeshRenderer meshRenderer;
        public GameObject coloniseBtn;

        public override void Spawned()
        {
            _worldManager = WorldManager.Instance;
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;

            _worldManager.allIslands.Add(this);
            coloniseBtn.SetActive(false);
            
            GetComponent<FogAgent_Island>().Init(graphObject, canvas);
        }

        public void Init(Transform parent, PlayerController owner, IslandTypesEnum type)
        {
            transform.parent = parent;
            Owner = owner;
            Type = type;

            if (Owner is not null)
            {
                if (Owner == _gameManager.thisPlayer)
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
            
            changed.Behaviour.meshRenderer.material.color = changed.Behaviour.Owner.myColor;
            changed.Behaviour.minimapIcon.color = changed.Behaviour.Owner.myColor;
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
            Owner = _gameManager.thisPlayer;

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
