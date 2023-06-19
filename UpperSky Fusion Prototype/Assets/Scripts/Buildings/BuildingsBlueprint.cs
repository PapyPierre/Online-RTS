using Fusion;
using Network;
using UnityEngine;

namespace Buildings
{
    public class BuildingsBlueprint : MonoBehaviour
    {
        private BuildingsManager _buildingsManager;
        private NetworkManager _networkManager;
        
        private RaycastHit _hit;
        private Vector3 _movePoint;
        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private int buildingIndex;

        private void Start()
        {        
            _networkManager = NetworkManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
            _networkManager.thisPlayer.hasBlueprintInHand = true;
            _networkManager.thisPlayer.blueprintBuildingIndex = buildingIndex;
        }

        private void Update()
        {
            Ray ray = _networkManager.thisPlayer.myCam.ScreenPointToRay((Input.mousePosition));

            if (Physics.Raycast(ray, out _hit, 5000, terrainLayer))
            {
                transform.position = _hit.point;
            }

            _networkManager.thisPlayer.blueprintPos = transform.position;
            _networkManager.thisPlayer.blueprintRot = transform.rotation;

            if (Input.GetMouseButtonDown(0))
            {
            //   _buildingsManager.BuildBuilding(buildingIndex, transform.position, transform.rotation, _networkManager.thisPlayer.MyPlayerRef);
               Destroy(gameObject);
            }
        }
        
        
    }
}
