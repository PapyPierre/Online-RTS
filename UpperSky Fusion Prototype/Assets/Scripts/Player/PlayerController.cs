using System.Collections.Generic;
using Custom_UI;
using Element;
using Element.Entity;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Island;
using Entity;
using Entity.Buildings;
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
        
       [SerializeField, ReadOnly] public BaseElement mouseAboveThisElement;
        private bool _isMajKeyPressed;
        public List<BaseElement> currentlySelectedElements;
        public BaseElement lastSelectedElement;

        [SerializeField, Required()] private GameObject minimapIndicator;

        public LayerMask LayerMask;
        
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
            
            if (!mouseAboveThisElement) return;
            if (mouseAboveThisElement.Owner != this) return;

            SelectElement(mouseAboveThisElement);
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
            if (_unitsManager.currentlySelectedUnits.Count > 0) // Si au moins une unité est sélectionné 
            {
                if (mouseAboveThisElement)
                {
                    //Si l'unité qui est hover est ennemie, l'attaquer
                    if (mouseAboveThisElement.Owner != this && mouseAboveThisElement is BaseEntity entity)  
                    {
                        foreach (var unit in _unitsManager.currentlySelectedUnits)
                        {
                            if (unit.Data.CanShoot)
                            {
                                unit.SetTarget(entity);
                            }
                        }
                        
                        _unitsManager.OrderSelectedUnitsToMoveTo(mouseAboveThisElement.transform.position);
                        return;
                    }
                }

                foreach (var unit in _unitsManager.currentlySelectedUnits)
                {
                    unit.ResetTarget();
                }
                    
                Ray ray = myCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 5000, LayerMask, QueryTriggerInteraction.Collide))
                {
                    _unitsManager.OrderSelectedUnitsToMoveTo(hit.point);
                }
            }
            else
            {
                _uiManager.HideOpenedUI();
                UnselectAllElements();
            }
        }

        public void SelectElement(BaseElement element)
        {
            switch (element)
            {
                case BaseUnit unit :
                    _uiManager.HideOpenedUI();
                    if (!_isMajKeyPressed) UnselectAllElements();
                    _unitsManager.SelectUnit(unit);
                    lastSelectedElement = element;
                    currentlySelectedElements.Add(element);
                    _uiManager.CloseFormationBuilding();
                    _uiManager.ShowSelectionInfoBox(unit, unit.Data, unit.Owner);
                    break;
                
                case BaseBuilding building :
                    UnselectAllElements();
                    lastSelectedElement = element;
                    currentlySelectedElements.Add(element);
                    _uiManager.HideBuildMenu();
                    _uiManager.ShowSelectionInfoBox(building, building.Data, building.Owner);
                    if (building.Data.IsFormationBuilding)
                    {
                        _uiManager.OpenFormationBuilding(building);
                    }
                    else _uiManager.CloseFormationBuilding();
                    break;
                
                case BaseIsland island :
                    UnselectAllElements();
                    lastSelectedElement = element;
                    currentlySelectedElements.Add(element);
                    _uiManager.CloseFormationBuilding();
                    _uiManager.ShowSelectionInfoBox(island, island.Data, island.Owner);
                    break;
            }
            
            element.SetActiveSelectionCircle(true);
        }

        public void UnselectAllElements()
        {
            _unitsManager.UnSelectAllUnits();

            foreach (var element in currentlySelectedElements)
            {
                element.SetActiveSelectionCircle(false);
            }

            lastSelectedElement = null;
            currentlySelectedElements.Clear();
            _uiManager.HideSelectionInfoBox();
        }

        public void MakesPlayerReady()
        {
            var islandPos = MyStartingIsland.transform.position;
            transform.position = new Vector3(islandPos.x, 10, islandPos.z);
            transform.LookAt(Vector3.zero);
                    
            SpawnStartBuilding(MyStartingIsland);
            ressources.Init();

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

