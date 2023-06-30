using Custom_UI;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using UnityEngine;
using World;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        private UIManager _uiManager;
        private UnitsManager _unitsManager;
        private GameManager _gameManager;
        private WorldManager _worldManager;

        [Networked] private bool HasBeenTpToStartingIsland{ get; set; }

        public int playerId; // = à index dans ConnectedPlayers + 1 
        
        [HideInInspector] public PlayerRessources ressources;
        
        [HideInInspector] public BaseUnit mouseAboveThisUnit;
        private bool _isMajKeyPressed;

        [Header("Cameras")]
        public Camera myCam;

        [SerializeField, Required()] private GameObject minimapIndicator;

        public override void Spawned()
        {
            _uiManager = UIManager.Instance;
            _unitsManager = UnitsManager.Instance;
            _gameManager = GameManager.Instance;
            _worldManager = WorldManager.Instance;
            
            ressources = GetComponent<PlayerRessources>();
            
            _gameManager.ConnectPlayer(this);

            if (Object.HasInputAuthority)
            {        
                _uiManager.connectionInfoTMP.text = 
                    "Player " + playerId + " connected in " + Runner.GameMode + " Mode";
                
                minimapIndicator.SetActive(true);
            }
            else myCam.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!Object.HasInputAuthority) return;
            
            if (_gameManager.connectedPlayers[^1].HasBeenTpToStartingIsland
                && !HasBeenTpToStartingIsland && HasStateAuthority) TeleportToStartingIsland();

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

            if (_unitsManager.currentlySelectedUnits.Count > 0) // Si au moins une unité est sélectionné 
            {
                if (mouseAboveThisUnit)
                {
                    //Si l'unité qui est hover est ennemie, l'attaquer
                    if (mouseAboveThisUnit.Owner != this)
                    {
                        foreach (var unit in  _unitsManager.currentlySelectedUnits)
                        {
                            unit.targetedUnit = mouseAboveThisUnit;
                        }
                        
                        _unitsManager.OrderToMoveUnitsTo(_unitsManager.currentlySelectedUnits,
                            mouseAboveThisUnit.transform.position);
                    }
                }
                else
                {
                    foreach (var unit in  _unitsManager.currentlySelectedUnits)
                    {
                        unit.targetedUnit = null;
                    }
                    
                    Ray ray = myCam.ScreenPointToRay((Input.mousePosition));
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 5000))
                    {
                        _unitsManager.OrderToMoveUnitsTo(_unitsManager.currentlySelectedUnits, hit.point);
                    }
                }
            } 
        }

        public void TeleportToStartingIsland()
        {
            foreach (var island in _worldManager.allIslands)
            {
                if (island.Owner == this)
                {
                    var islandPos = island.transform.position;
                    transform.position = new Vector3(islandPos.x, 10, islandPos.z);
                    transform.LookAt(Vector3.zero);
                    HasBeenTpToStartingIsland = true;
                    break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            var camTransform = myCam.transform;
            Debug.DrawRay(camTransform.position, camTransform.forward, Color.cyan);
        }
    }
}

