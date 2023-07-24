using System.Collections.Generic;
using Element.Entity.Buildings;
using Element.Island;
using Entity.Military_Units;
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

        #region Ownership
        [field: SerializeField, Header("Ownership")]
        [Networked(OnChanged = nameof(OwnerChanged))] public PlayerController Owner { get; set; }
        [SerializeField] private  List<MeshRenderer> meshToColor;
        #endregion
        
        [SerializeField, Space] protected GameObject graphObject;
        [SerializeField, Space] protected GameObject canvas;
        [SerializeField, Space] protected Image minimapIcon;

        private static void OwnerChanged(Changed<BaseElement> changed)
        {
            if (changed.Behaviour.Owner == null) return;

            foreach (var meshRenderer in changed.Behaviour.meshToColor)
            {
                meshRenderer.material.color *= changed.Behaviour.Owner.myColor;
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
        }
        
        #region Selection
        protected virtual void OnMouseEnter()
        {
            switch (this)
            { 
                case BaseIsland island: UIManager.ShowInGameInfoBox(island.Data, island.Owner, island.BuildingsCount);
                    break;
                case BaseUnit unit: UIManager.ShowInGameInfoBox(unit.Data, unit.Owner);
                    break;
                case BaseBuilding building: UIManager.ShowInGameInfoBox(building.Data, building.Owner);
                    break;
            }
        }
        
        protected virtual void OnMouseExit()
        {
            UIManager.HideInGameInfoBox();
        }
        
        
        #endregion
    }
}
