using Buildings;
using Custom_UI;
using Fusion;
using Military_Units;
using NaughtyAttributes;
using Network;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        private UIManager _uiManager;
        private SelectionManager _selectionManager;
        private UnitsManager _unitsManager;
        private NetworkManager _networkManager;
        private BuildingsManager _buildingsManager;

        [HideInInspector] public PlayerRessources ressources;

        [Header("Cameras")]
        public Camera myCam;

        [SerializeField, Required()] private GameObject minimapIndicator;
        
        [Networked] public PlayerRef MyPlayerRef {get; set; }
        [Networked] private TickTimer Delay { get; set; }

        public override void Spawned()
        {
            _selectionManager = SelectionManager.Instance;
            _unitsManager = UnitsManager.Instance;
            _networkManager = NetworkManager.Instance;
            _uiManager = UIManager.Instance;
            _buildingsManager = BuildingsManager.Instance;

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

        private RaycastHit _hit;
        public void BuildAtCursorPos()
        {
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out _hit, 50000))
            {
                if (!_hit.collider.CompareTag("Building") &&  !_hit.collider.CompareTag("Unit"))
                {
                    Vector3 spawnPos = new Vector3(_hit.point.x, _hit.point.y + 0.5f, _hit.point.z);
                }
                else
                {
                    Debug.LogWarning("Can't build on another building");
                }
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SpawnNetworkObj(NetworkPrefabRef prefab, Vector3 position, Quaternion rotation,
            RpcInfo info = default)
        {
            Runner.Spawn(prefab, position, rotation, Object.InputAuthority);
        }
        
        private void OnDrawGizmos()
        {
            Debug.DrawRay(myCam.transform.position, myCam.transform.forward, Color.cyan);
        }
    }
}

