using Fusion;
using Network;
using UnityEngine;

namespace Buildings
{
    public class BuildingsBlueprint : MonoBehaviour
    {
        private BuildingsManager _buildingsManager;
        private NetworkManager _networkManager;

        private Camera _cam;
        private PlayerRef _playerRef;
        private RaycastHit _hit;
        private Vector3 _movePoint;
        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private NetworkPrefabRef buildingPrefab;

        private void Start()
        {        
            _networkManager = NetworkManager.Instance;
            _buildingsManager = BuildingsManager.Instance;
            
            _cam = _networkManager.thisPlayer.myCam;
            _playerRef = _networkManager.thisPlayer.MyPlayerRef;
        }

        private void Update()
        {
            Ray ray = _cam.ScreenPointToRay((Input.mousePosition));

            if (Physics.Raycast(ray, out _hit, 5000, terrainLayer))
            {
                transform.position = _hit.point;
            }

            if (Input.GetMouseButtonDown(0))
            {
               _buildingsManager.BuildBuilding(buildingPrefab, transform.position, transform.rotation, _playerRef);
               Destroy(gameObject);
            }
        }
    }
}
