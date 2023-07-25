using Custom_UI;
using Element.Entity;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Island;
using Entity;
using Entity.Buildings;
using Entity.Military_Units;
using Fusion;
using NaughtyAttributes;
using UnityEngine;
using UserInterface;
using World;
using World.Island;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        private UIManager _uiManager;
        private UnitsManager _unitsManager;
        private GameManager _gameManager;
        private WorldManager _worldManager;
        private RectangleSelection _rectangleSelection;
        private BuildingsManager _buildingsManager;
        
        [Networked] public bool IsReadyToPlay{ get; set; }
        
        [Networked] public bool IsOutOfGame { get; set; }

        public int myId; // = à index dans ConnectedPlayers + 1 
        public Color myColor;
        [Networked] public BaseIsland MyStartingIsland { get; set; }
        public Camera myCam;

        [HideInInspector] public PlayerRessources ressources;
        
       [SerializeField, ReadOnly] public BaseEntity mouseAboveThisEntity;
        private bool _isMajKeyPressed;
        

        [SerializeField, Required()] private GameObject minimapIndicator;

        public override void Spawned()
        {
            _uiManager = UIManager.Instance;
            _unitsManager = UnitsManager.Instance;
            _gameManager = GameManager.Instance;
            _worldManager = WorldManager.Instance;
            _rectangleSelection = RectangleSelection.Instance;
            _buildingsManager = BuildingsManager.Instance;
                
            ressources = GetComponent<PlayerRessources>();
            
            _gameManager.ConnectPlayer(this);
            myColor = _worldManager.playersColors[myId -1];

            if (Object.HasInputAuthority)
            {        
                _uiManager.connectionInfoTMP.text = 
                    "Player " + myId + " connected in " + Runner.GameMode + " Mode";
                
                minimapIndicator.SetActive(true);
            }
            else myCam.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!Object.HasInputAuthority || _gameManager.gameIsFinished) return;
            
            if (_gameManager.connectedPlayers[^1].IsReadyToPlay
                && !IsReadyToPlay && HasStateAuthority) MakesPlayerReady();

            _isMajKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            if (Input.GetMouseButtonDown(0)) OnLeftButtonClick();
            
            if (Input.GetMouseButton(0)) OnLeftButtonHold();
            
            if (Input.GetMouseButtonUp(0)) OnLeftButtonUp();
                
            if (Input.GetMouseButtonDown(1)) OnRightButtonClick();
        } 
        
        private void OnLeftButtonClick()
        {
            _rectangleSelection.OnLeftButtonDown_RectSelection();

            if (_unitsManager.currentlySelectedUnits.Count is 0) // Si aucune unité n'est sélectionné 
            {
                 if (mouseAboveThisEntity)
                 {
                     if (mouseAboveThisEntity is BaseUnit unit)
                     {
                         if (mouseAboveThisEntity.Owner == this)
                         {
                             _unitsManager.SelectUnit(unit);
                         }
                     }
                 }
            }
            else // Si au moins une unité est selectionné
            {
                if (mouseAboveThisEntity)  // Si la souris hover une unité
                {
                    if (mouseAboveThisEntity is BaseUnit unit)
                    {
                        if (_isMajKeyPressed && mouseAboveThisEntity.Owner == this)
                        {
                            _unitsManager.SelectUnit(unit);
                        }
                        else
                        {
                            _unitsManager.UnSelectAllUnits();
      
                            if (mouseAboveThisEntity.Owner == this)
                            {
                                _unitsManager.SelectUnit(unit);
                            }
                        }
                    }
                }
            }
        }

        private void OnLeftButtonHold()
        {
            _rectangleSelection.OnLeftButtonHold_RectSelection();
        }

        private void OnLeftButtonUp()
        {
            _rectangleSelection.OnLeftButtonUp_RectSelection(myCam);
        }

        private void OnRightButtonClick()
        { 
            _uiManager.HideOpenedUI();
            _uiManager.HideInGameInfoBox();

            if (_unitsManager.currentlySelectedUnits.Count > 0) // Si au moins une unité est sélectionné 
            {
                if (mouseAboveThisEntity)
                {
                    //Si l'unité qui est hover est ennemie, l'attaquer
                    if (mouseAboveThisEntity.Owner != this)
                    {
                        foreach (var unit in  _unitsManager.currentlySelectedUnits)
                        {
                            unit.SetTarget(mouseAboveThisEntity);
                        }
                        
                        _unitsManager.OrderSelectedUnitsToMoveTo(mouseAboveThisEntity.transform.position);
                    }
                }
                else
                {
                    foreach (var unit in  _unitsManager.currentlySelectedUnits)
                    {
                        unit.ResetTarget();
                    }
                    
                    Ray ray = myCam.ScreenPointToRay((Input.mousePosition));
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 5000))
                    {
                        _unitsManager.OrderSelectedUnitsToMoveTo(hit.point);
                    }
                }
            } 
        }

        public void MakesPlayerReady()
        {
            var islandPos = MyStartingIsland.transform.position;
            transform.position = new Vector3(islandPos.x, 10, islandPos.z);
            transform.LookAt(Vector3.zero);
                    
            SpawnStartBuilding(MyStartingIsland);
                    
            IsReadyToPlay = true;
        }
        
        private void SpawnStartBuilding(BaseIsland startingIsland)
        {
            var startBuilding = _buildingsManager.BuildBuilding(13, startingIsland.transform.position,
                Quaternion.identity, startingIsland, true);
            startBuilding.transform.parent = startingIsland.transform;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SetStartingIsland(BaseIsland island)
        {
            // The code inside here will run on the client which owns this object (has state and input authority).

            MyStartingIsland = island;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_DisplayLoadingText()
        {
            // The code inside here will run on the client which owns this object (has state and input authority).
            
            _uiManager.UpdateLoadingText(true);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_StartToPlay()
        {
            // The code inside here will run on the client which owns this object (has state and input authority).

            _uiManager.loadingScreen.SetActive(false);
            _uiManager.menuCamera.SetActive(false);
            _uiManager.playerRessources = GetComponent<PlayerRessources>();
            _gameManager.gameIsStarted = true;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_OutOfGame()
        {
            // The code inside here will run on the client which owns this object (has state and input authority).
            IsOutOfGame = true;
            
            _uiManager.ShowLoseText();
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_Win()
        {
            // The code inside here will run on the client which owns this object (has state and input authority).

            _uiManager.ShowWinText();
        }

        private void OnDrawGizmos()
        {
            var camTransform = myCam.transform;
            Debug.DrawRay(camTransform.position, camTransform.forward, Color.cyan);
        }
    }
}

