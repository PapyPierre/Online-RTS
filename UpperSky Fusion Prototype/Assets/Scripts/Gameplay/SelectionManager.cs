using System.Collections.Generic;
using Fusion;
using NaughtyAttributes;
using Nekwork_Objects.Interactible.Buildings;
using Nekwork_Objects.Interactible.Military_Units;
using UnityEngine;

namespace Gameplay
{
   public class SelectionManager : NetworkBehaviour
   {
      public static SelectionManager instance;

      #region Current Selection Infos
      [Foldout("Current Selection Infos"), ReadOnly] public BaseUnit mouseAboveThisUnit;
      [Foldout("Current Selection Infos"), ReadOnly] public List<BaseUnit> currentlySelectedUnits;
      [Foldout("Current Selection Infos"), ReadOnly] public BaseBuilding mouseAboveThisBuilding;
      #endregion

      private bool _isMajKeyPressed;
    
      private void Awake()
      {
         if (instance != null)
         {
            Debug.LogError(name);
            return;
         }

         instance = this;
      }

      private void Update()
      {
         _isMajKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
       
         if (Input.GetMouseButtonDown(0)) OnLeftButtonClick();
       
         if (Input.GetMouseButtonDown(1)) OnRightButtonClick();
      }

      public void OnLeftButtonClick()
      {
         if (currentlySelectedUnits.Count is 0) // Si aucune unité n'est sélectionné 
         {
            if (!mouseAboveThisUnit) // si la souris ne hover pas une unité
            {
               UnSelectAll();
            }
            else if (mouseAboveThisUnit.Object.InputAuthority == Runner.LocalPlayer)
            {
               SelectUnit(mouseAboveThisUnit);
            }
         }
         else // Si au moins une unité est selectionné
         {
            if (!mouseAboveThisUnit) // Si la souris ne hover pas une unité
            {
               if (!_isMajKeyPressed) UnSelectAll();
            }
            else if (mouseAboveThisUnit) // Si la souris hover une unité
            {
               if (_isMajKeyPressed && mouseAboveThisUnit.Object.InputAuthority == Runner.LocalPlayer)
               {
                  SelectUnit(mouseAboveThisUnit);
               }
               else
               {
                  UnSelectAll();
      
                  if (mouseAboveThisUnit.Object.InputAuthority == Runner.LocalPlayer)
                  {
                     SelectUnit(mouseAboveThisUnit);
                  }
               }
            }
            else if (mouseAboveThisBuilding)
            {
               //TODO Si le bâtiments à un menu, l'ouvrir
            }
         }
      }
      
      public void OnRightButtonClick()
      { 
         //TODO Si y'a des unités allié selectionné et qu'on clique dans du vide, déplacé les untiés à cette position
         //TODO Si des unités allié sont selectionné et qu'on clique sur une untié/batiment ennemie, les unités selectionné attaque l'unité cliquer 
         //TODO Sinon, ne rien faire
      
         if (currentlySelectedUnits.Count > 0) // Si au moins une unité est sélectionné 
         {
            if (mouseAboveThisUnit)
            {
               //TODO Si l'unité qui est hover est ennemie, l'attaquer
            }
            else if (mouseAboveThisBuilding)
            {
               //TODO Si le bâtiments qui est hover est ennemie, l'attaquer
            }
         } 
      }
      
      private void SelectUnit(BaseUnit unit)
      {
         currentlySelectedUnits.Add(unit); 
         unit.SetActiveSelectionCircle(true);
      }
      
      private void UnSelectAll()
      {
         foreach (var unit in currentlySelectedUnits)
         {
            unit.SetActiveSelectionCircle(false);
         }
      
         currentlySelectedUnits.Clear();
      }
   }
}
