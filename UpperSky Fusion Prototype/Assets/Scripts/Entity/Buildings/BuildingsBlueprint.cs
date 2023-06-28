using UnityEngine;
using World.Island;

namespace Entity.Buildings
{
    public class BuildingsBlueprint : MonoBehaviour
    {
        private BuildingsManager _buildingsManager;
        private GameManager _gameManager;

        private RaycastHit _hit;
        private bool _isBuildPositionFree = true;
   
        [SerializeField] private BuildingsManager.AllBuildingsEnum thisBuilding;
        [SerializeField] private Renderer[] renderers;

        private void Start()
        {        
            _gameManager = GameManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
        }

        private void Update()
        {
            Ray ray = _gameManager.thisPlayer.myCam.ScreenPointToRay((Input.mousePosition));
            
            if (Physics.Raycast(ray, out _hit, 5000, _buildingsManager.terrainLayer))
            {
                transform.position = _hit.point;
            }
            
            if (_hit.collider is null) return;

            Island island = _hit.collider.GetComponentInParent<Island>();

            if (island.Owner != _gameManager.thisPlayer)
            {
                _isBuildPositionFree = false;
            }
            else
            {
                _isBuildPositionFree = !Physics.Raycast(ray, out _hit, 5000, _buildingsManager.buildingLayer);
            }
            
            foreach (var renderer in renderers)
            {
                renderer.material.color = _isBuildPositionFree ? 
                    _buildingsManager.BlueprintPossibleBuildColor : _buildingsManager.BlueprintOverlapColor;
            }

            // Start construction
            if (Input.GetMouseButtonDown(0))
            {
                if (!_isBuildPositionFree)
                {
                    Debug.Log("can't build there");
                    return;
                }

                island.buildingsCount++;
                _buildingsManager.BuildBuilding((int) thisBuilding, transform.position, transform.rotation);
                Destroy(gameObject);
            }

            // Cancel construction
            if (Input.GetMouseButtonDown(1))
            {
                _buildingsManager.haveBlueprintInHand = false;
                Destroy(gameObject);
            }
        }
    }
}
