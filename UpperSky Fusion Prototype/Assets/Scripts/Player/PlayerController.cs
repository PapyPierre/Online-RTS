using System.Collections.Generic;
using Element;
using Element.Entity;
using Element.Entity.Buildings;
using Element.Entity.Military_Units;
using Element.Island;
using Fusion;
using NaughtyAttributes;
using UnityEngine;
using UserInterface;
using World;

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
        public Camera myCam;

        [HideInInspector] public PlayerRessources ressources;
        
       [SerializeField, ReadOnly] public BaseElement mouseAboveThisElement;
        [HideInInspector] public bool isMajKeyPressed;
        public List<BaseElement> currentlySelectedElements;
        public BaseElement lastSelectedElement;

        public LayerMask unitMovementClickLayer;
        
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
            }
            else myCam.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!Object.HasInputAuthority || _gameManager.gameIsFinished) return;
            
            if (_gameManager.connectedPlayers[^1].IsReadyToPlay
                && !IsReadyToPlay && HasStateAuthority) MakesPlayerReady();

            isMajKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            if (Input.GetMouseButtonDown(0)) OnLeftButtonClick();
            
            if (Input.GetMouseButton(0)) OnLeftButtonHold();
            
            if (Input.GetMouseButtonUp(0)) OnLeftButtonUp();
                
            if (Input.GetMouseButtonDown(1)) OnRightButtonClick();
        }

        private void OnLeftButtonClick()
        {
            _rectangleSelection.OnLeftButtonDown_RectSelection();
            
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            
            if (!mouseAboveThisElement || isOverUI) return;
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

                if (Physics.Raycast(ray, out hit, 5000, unitMovementClickLayer, QueryTriggerInteraction.Collide))
                {
                    _uiManager.HideBuildMenu();
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
                    if (!isMajKeyPressed && !_rectangleSelection.dragSelection) UnselectAllElements();
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
                    if (building is FormationBuilding formationBuilding)
                    {
                        _uiManager.OpenFormationBuilding(formationBuilding);
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
            ressources.Init();

            IsReadyToPlay = true;
        }
        
        public void SpawnStartBuilding(HomeIsland homeIsland)
        {
            var islandPos = homeIsland.transform.position;
            transform.position = new Vector3(islandPos.x, 10, islandPos.z);
            transform.LookAt(Vector3.zero);
            
            var startBuilding = _buildingsManager.BuildBuilding(13, islandPos,
                Quaternion.identity, homeIsland, true);
            startBuilding.transform.parent = homeIsland.transform;
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

