using System.Collections;
using Custom_UI;
using Custom_UI.InGame_UI;
using UnityEngine;
using World.Island;

namespace Entity.Buildings
{
    public class BuildingsBlueprint : MonoBehaviour
    {
        private BuildingsManager _buildingsManager;
        private GameManager _gameManager;
        private UIManager _uiManager;

        private RaycastHit _hit;
        private bool _isBuildPositionFree = true;

        private Island _islandToBuildOn;
        private bool _isBuilding;
       [SerializeField] private ProductionBar productionBar;
   
        [SerializeField] private BuildingsManager.AllBuildingsEnum thisBuilding;
        [SerializeField] private Renderer[] renderers;

        private void Start()
        {        
            _gameManager = GameManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
            productionBar.gameObject.SetActive(false);
            _uiManager = UIManager.Instance;
        }

        private void Update()
        {
            if (!_isBuilding) MoveAndColorBlueprint();

            // Start construction
            if (Input.GetMouseButtonDown(0))
            {
                if (!_isBuildPositionFree)
                {
                    Debug.Log("can't build there");
                    return;
                }

                if (!_isBuilding) StartCoroutine(StartBuilding());
            }

            // Cancel construction
            if (Input.GetMouseButtonDown(1))
            {
                if (_isBuilding) return;
                CancelBlueprint();
            }
        }

        private void MoveAndColorBlueprint()
        {
            Ray ray = _gameManager.thisPlayer.myCam.ScreenPointToRay((Input.mousePosition));
            
            if (Physics.Raycast(ray, out _hit, 5000, _buildingsManager.terrainLayer))
            {
                transform.position = _hit.point;
            }
            
            if (_hit.collider is null) return;

            _islandToBuildOn = _hit.collider.GetComponentInParent<Island>();

            if (_islandToBuildOn.Owner != _gameManager.thisPlayer
                || _islandToBuildOn.BuildingsCount >= _buildingsManager.MaxBuildingsPerIslands)
            {
                _isBuildPositionFree = false;
            }
            else
            {
                _isBuildPositionFree = !Physics.Raycast(ray, out _hit, 5000, _buildingsManager.buildingLayer);
            }
            
            foreach (var renderer1 in renderers)
            {
                renderer1.material.color = _isBuildPositionFree ? 
                    _buildingsManager.BlueprintPossibleBuildColor : _buildingsManager.BlueprintOverlapColor;
            }
        }

        private IEnumerator StartBuilding()
        {
            _isBuilding = true;
            float buildingTime = _buildingsManager.allBuildingsDatas[(int) thisBuilding].ProductionTime;
            productionBar.gameObject.SetActive(true);
            productionBar.Init(buildingTime);
            _uiManager.ShowOrHideBuildMenu();
            _uiManager.HideInfobox();
            yield return new WaitForSeconds(buildingTime);
            _buildingsManager.BuildBuilding((int) thisBuilding, transform.position, transform.rotation, _islandToBuildOn);
            _isBuilding = false;
            Destroy(gameObject);
        }

        private void CancelBlueprint()
        {
            _buildingsManager.haveBlueprintInHand = false;
            Destroy(gameObject);
        }
    }
}
