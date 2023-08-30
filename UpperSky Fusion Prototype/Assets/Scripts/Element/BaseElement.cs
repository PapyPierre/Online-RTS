using System.Collections.Generic;
using AOSFogWar.Used_Scripts;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Island;
using Fusion;
using Player;
using UnityEngine;
using UnityEngine.UI;
using UserInterface;

namespace Element
{
    /// <summary>
    /// Elements include Islands, Buildings and Units
    /// </summary>

    public class BaseElement : NetworkBehaviour
    {
        protected UnitsManager UnitsManager;
        protected GameManager GameManager;
        protected UIManager UIManager;
        protected FogOfWar FogOfWar;

        protected int FogRevealerIndex;

        #region Ownership
        [field: SerializeField, Header("Ownership")]
        [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        [SerializeField] private  List<MeshRenderer> meshToColor;
        private Color _baseColor;
        #endregion

        [SerializeField, Header("Graph")] protected GameObject selectionCircle;
        public GameObject graphObject;
        
        [SerializeField, Header("UI")] protected GameObject canvas;
        
        [SerializeField, Header("Minimap")] protected Image minimapIcon;
        [SerializeField] public Transform minimapCanvas;

        private static void OwnerChanged(Changed<BaseElement> changed)
        {
            if (changed.Behaviour.Owner == null) return;

            foreach (var meshRenderer in changed.Behaviour.meshToColor)
            {
                meshRenderer.material.color = changed.Behaviour._baseColor * changed.Behaviour.Owner.myColor;
            }

            if (changed.Behaviour.minimapIcon != null)
            {
                changed.Behaviour.minimapIcon.color = changed.Behaviour.Owner.myColor;
            }
        }
        
        public override void Spawned()
        {
            GameManager = GameManager.Instance;
            UnitsManager = UnitsManager.Instance;
            UIManager = UIManager.Instance;
            FogOfWar = FogOfWar.Instance;

            if (meshToColor.Count > 0) _baseColor = meshToColor[0].material.color;
        }

        public virtual void Init(PlayerController owner, ElementData data)
        {
            Owner = owner;
            
            if (PlayerIsOwner())
            {
                if (!HasStateAuthority) Object.RequestStateAuthority();
                var fogRevealer = new FogOfWar.FogRevealer(transform, data.SightRange, true);
                FogRevealerIndex = FogOfWar.AddFogRevealer(fogRevealer);
            }
        }
        
        public bool PlayerIsOwner()
        {
            return Owner == GameManager.thisPlayer;
        }
        
        protected bool MouseAboveThisElement()
        {
            return GameManager.thisPlayer.mouseAboveThisElement == this;
        }
        
        #region Selection
        protected virtual void OnMouseEnter()
        {
            GameManager.thisPlayer.mouseAboveThisElement = this;
            if (PlayerIsOwner()) SetActiveSelectionCircle(true);
        }
        
        protected virtual void OnMouseExit()
        {
            GameManager.thisPlayer.mouseAboveThisElement = null;
            
            if (PlayerIsOwner() && !GameManager.thisPlayer.currentlySelectedElements.Contains(this))
            {
                SetActiveSelectionCircle(false);
            }
        }
        
        public void SetActiveSelectionCircle(bool value)
        {
            if (selectionCircle != null) selectionCircle.SetActive(value);
        }
        #endregion
    }
}