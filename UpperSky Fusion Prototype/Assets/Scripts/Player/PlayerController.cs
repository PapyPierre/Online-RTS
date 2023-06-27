using Custom_UI;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using Network;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        private UIManager _uiManager;
        private NetworkManager _networkManager;
        private UnitsManager _unitsManager;
        
        [HideInInspector] public PlayerRessources ressources;
        
        [HideInInspector] public BaseUnit mouseAboveThisUnit;
        private bool _isMajKeyPressed;

        [Header("Cameras")]
        public Camera myCam;

        [SerializeField, Required()] private GameObject minimapIndicator;
        
        [Networked] public PlayerRef MyPlayerRef {get; set; }
        [Networked] private TickTimer Delay { get; set; }

        public override void Spawned()
        {
            _networkManager = NetworkManager.Instance;
            _uiManager = UIManager.Instance;
            _unitsManager = UnitsManager.Instance;

            ressources = GetComponent<PlayerRessources>();

            _uiManager.connectionInfoTMP.text = "Is connected - " + Runner.GameMode;

            if (Object.HasInputAuthority)
            {
                _networkManager.thisPlayer = this;
                minimapIndicator.SetActive(true);
            }
            else
            {
                myCam.gameObject.SetActive(false);
            }
            
            transform.Rotate(Vector3.up, 180);
        }

        private void Update()
        {
            if (_networkManager.thisPlayer != this) return;
            
            _isMajKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            if (Input.GetMouseButtonDown(0)) OnLeftButtonClick();
       
            if (Input.GetMouseButtonDown(1)) OnRightButtonClick();
        } 
        
        private void OnLeftButtonClick()
        {
         if (_unitsManager.currentlySelectedUnits.Count is 0) // Si aucune unité n'est sélectionné 
         {
             if (mouseAboveThisUnit != null)
             {
                 if (mouseAboveThisUnit.Owner == this)
                 {
                     _unitsManager.SelectUnit(mouseAboveThisUnit);
                 }
             }
         }
         else // Si au moins une unité est selectionné
         {
             if (mouseAboveThisUnit)  // Si la souris hover une unité
            {
               if (_isMajKeyPressed && mouseAboveThisUnit.Owner == this)
               {
                   _unitsManager.SelectUnit(mouseAboveThisUnit);
               }
               else
               {
                   _unitsManager.UnSelectAllUnits();
      
                  if (mouseAboveThisUnit.Owner == this)
                  {
                      _unitsManager.SelectUnit(mouseAboveThisUnit);
                  }
               }
            }
         }
        }

        private void OnRightButtonClick()
        { 
            _uiManager.HideOpenedUI();
            
            //TODO Si y'a des unités allié selectionné et qu'on clique dans du vide, déplacé les untiés à cette position
            //TODO Si des unités allié sont selectionné et qu'on clique sur une untié/batiment ennemie, les unités selectionné attaque l'unité cliquer 
            //TODO Sinon, ne rien faire
      
            if (_unitsManager.currentlySelectedUnits.Count > 0) // Si au moins une unité est sélectionné 
            {
                if (mouseAboveThisUnit)
                {
                    //TODO Si l'unité qui est hover est ennemie, l'attaquer
                }
                else
                {
                    Ray ray = _networkManager.thisPlayer.myCam.ScreenPointToRay((Input.mousePosition));
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 5000))
                    {
                        _unitsManager.OrderToMoveUnitsTo(_unitsManager.currentlySelectedUnits, hit.point);
                    }
                }
            } 
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SpawnNetworkObj(NetworkPrefabRef prefab, Vector3 position, Quaternion rotation, 
            RpcInfo info = default)
        {
            Runner.Spawn(prefab, position, rotation, Object.InputAuthority);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_MoveNetworkObj(NetworkObject obj, Vector3 newPosition, Quaternion newRotation, 
            RpcInfo info = default)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position,newPosition, 0.5f);
            obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation, newRotation, 0.5f);
        }
        
        private void OnDrawGizmos()
        {
            Debug.DrawRay(myCam.transform.position, myCam.transform.forward, Color.cyan);
        }
    }
}

