using System.Collections;
using System.Collections.Generic;
using Custom_UI.InGame_UI;
using Element.Island;
using Entity.Buildings;
using UnityEngine;
using UserInterface;

namespace Element.Entity.Buildings
{
    public class BuildingsBlueprint : MonoBehaviour
    {
        private BuildingsManager _buildingsManager;
        private GameManager _gameManager;
        private UIManager _uiManager;

        private RaycastHit _hit;
        private BaseIsland _islandToBuildOn;
        
        // Used to inform the player why this position is unvalid, 0 means a valid position
        private int _unvalidPosIndex;
        
        private bool _isBuilding;
       [SerializeField] private ProductionBar productionBar;
   
        [SerializeField] private BuildingsManager.AllBuildingsEnum thisBuilding;
        [SerializeField] private Renderer[] renderers;


        private void Start()
        {        
            _gameManager = GameManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
            _uiManager = UIManager.Instance;
            
            productionBar.gameObject.SetActive(false);
        }

        private void Update()
        { 
            if (_isBuilding) return;
            Ray ray = _gameManager.thisPlayer.myCam.ScreenPointToRay(Input.mousePosition);

            MoveBlueprint(ray);

            // Security to avoid errors when player is above sea of clouds
            if (_islandToBuildOn is null) return;
          
            ColorBlueprint(ray);

            // Start construction
            if (Input.GetMouseButtonDown(0))
            {
                if (_unvalidPosIndex > 0)
                {
                    switch (_unvalidPosIndex)
                    {
                        case 1: 
                            _uiManager.PopFloatingText(transform,"You doesn't own this island !", Color.red);
                            break;
                        
                        case 2:
                            _uiManager.PopFloatingText(transform,"Too much buildings on this island !", Color.red);
                            break;
                        
                        case 3:
                            _uiManager.PopFloatingText(transform,"You can't build on another building !", Color.red);
                            break;
                    }
                    
                    return;
                }

                StartCoroutine(BuildBuilding());
            }

            // Cancel construction
            if (Input.GetMouseButtonDown(1)) CancelBlueprint();
        }

        private void MoveBlueprint(Ray ray)
        {
            // Doesn't hit when above sea of clouds, return to avoid errors
            if (!Physics.Raycast(ray, out _hit, 5000, _buildingsManager.terrainLayer, QueryTriggerInteraction.Ignore)) return;

            if (_hit.point.y > -0.5f)
            {
                transform.position = _hit.point;
            }
                
            _islandToBuildOn = _hit.collider.GetComponentInParent<BaseIsland>();
        }
        
        // Color the blueprint depending on the validity of his current position
        private void ColorBlueprint(Ray ray)
        {
            foreach (Renderer r in renderers)
            {
                r.material.color = CheckIfPosIsValid(ray) ? 
                    _buildingsManager.BlueprintValidPosColor : _buildingsManager.BlueprintUnvalidPosColor;
            }
        }

        private bool CheckIfPosIsValid(Ray ray)
        {
            // Check if player own the island
            if (_islandToBuildOn.Owner != _gameManager.thisPlayer)
            {
                _unvalidPosIndex = 1;
                return false;
            }
            
            // Check if the island doesn't have too much buildings on it yet
            if (_islandToBuildOn.BuildingsCount >= _islandToBuildOn.data.MaxBuildingsOnThisIsland)
            {
                _unvalidPosIndex = 2;
                return false;
            }

            // Finally check if the raycast doesn't hit a building (to avoid overlaping buildings)

            if (Physics.Raycast(ray, out _hit, 5000, _buildingsManager.buildingLayer))
            {
                _unvalidPosIndex = 3;
                return false;
            }
           
            // 0 means the position is valid
            _unvalidPosIndex = 0;
            return true;
        }
        
        private IEnumerator BuildBuilding()
        {
            _isBuilding = true;

            BuildingData thisBuildingData = _buildingsManager.allBuildingsDatas[(int) thisBuilding];
            
            _uiManager.DisplayOnBuilding(thisBuildingData);
            
            _buildingsManager.PayForBuilding((int) thisBuilding);

            float buildingTime = thisBuildingData.ProductionTime;
            productionBar.gameObject.SetActive(true);
            productionBar.Init(buildingTime);
            
            _uiManager.HideBuildMenu();
            _uiManager.HideProdInfobox();
            
            yield return new WaitForSeconds(buildingTime);
            
            _buildingsManager.FinishBuilding((int) thisBuilding, transform.position, transform.rotation, _islandToBuildOn);
            _isBuilding = false;
            
            gameObject.SetActive(false);
        }

        private void CancelBlueprint()
        {
            _buildingsManager.haveBlueprintInHand = false;
            gameObject.SetActive(false);
        }
    }
}
