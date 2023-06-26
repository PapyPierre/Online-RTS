using System.Collections.Generic;
using Entity.Buildings;
using Entity.Military_Units;
using NaughtyAttributes;
using Network;
using UnityEngine;

namespace Player
{
   public class SelectionManager : MonoBehaviour
   {
      public static SelectionManager Instance;
      private NetworkManager _networkManager;

      #region Current Selection Infos
      [Foldout("Current Selection Infos"), ReadOnly] public BaseUnit mouseAboveThisUnit;
      [Foldout("Current Selection Infos"), ReadOnly] public List<BaseUnit> currentlySelectedUnits;
      #endregion

      private bool _isMajKeyPressed;
      
      private void Awake()
      {
         if (Instance != null)
         {
            Debug.LogError(name);
            return;
         }

         Instance = this; 
      }

      private void Start()
      {
         _networkManager = NetworkManager.Instance;
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
            else if (mouseAboveThisUnit.Object.InputAuthority == _networkManager.myRunner.LocalPlayer)
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
               if (_isMajKeyPressed && mouseAboveThisUnit.Object.InputAuthority == _networkManager.myRunner.LocalPlayer)
               {
                  SelectUnit(mouseAboveThisUnit);
               }
               else
               {
                  UnSelectAll();
      
                  if (mouseAboveThisUnit.Object.InputAuthority == _networkManager.myRunner.LocalPlayer)
                  {
                     SelectUnit(mouseAboveThisUnit);
                  }
               }
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
            else
            {
               foreach (var unit in currentlySelectedUnits)
               {        
                  Ray ray = _networkManager.thisPlayer.myCam.ScreenPointToRay((Input.mousePosition));
                  RaycastHit hit;

                  if (Physics.Raycast(ray, out hit, 5000))
                  {
                     unit.MoveTo(hit.point);
                  }
               }
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
