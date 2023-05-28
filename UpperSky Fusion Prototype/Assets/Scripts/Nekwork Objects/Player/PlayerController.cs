using Custom_UI;
using Fusion;
using NaughtyAttributes;
using Nekwork_Objects.Interactible;
using Network_Logic;
using UnityEngine;

namespace Nekwork_Objects.Player
{
    public class PlayerController : NetworkBehaviour
    {
        private UIManager _uiManager;
        
        [SerializeField, Required()] private NetworkPrefabRef buildingPrefab;
        [SerializeField, Required()] private NetworkPrefabRef unitPrefab;

        private Camera _cam;
        [Networked] private TickTimer Delay { get; set; }

        private bool _isConnected;

        public override void Spawned()
        {
            _cam = Camera.main;
            _uiManager = UIManager.instance;
            _uiManager.connectionInfoTMP.text = "Is connected";
            _isConnected = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!_isConnected) return;
         
            transform.position = _cam.transform.position;
        
            if (GetInput(out NetworkInputData data))
            {
                if (!Object.HasInputAuthority) return;
                
                if (data.number1Key != 0)
                {
                    Vector3 spawnPos = new Vector3(0, 2, 0);
                    RPC_SpawnInteractibleNetworkObjects(
                        unitPrefab, spawnPos, Quaternion.identity, Object.InputAuthority);
               
                    data.number1Key = 0;
                }
                
                if (Delay.ExpiredOrNotRunning(Runner))
                {
                    if (data.number2Key != 0)
                    {
                        Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                        BuildAtCursorPos();
                        data.number2Key = 0;
                    }
                }
            }
        }

        private RaycastHit hit;
        public void BuildAtCursorPos()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 50000))
            {
                if (!hit.collider.CompareTag("Building") &&  !hit.collider.CompareTag("Unit"))
                {
                    Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
                    RPC_SpawnInteractibleNetworkObjects(
                        buildingPrefab, spawnPos, Quaternion.identity, Object.InputAuthority);
                }
                else
                {
                    Debug.LogWarning("Can't build on another building");
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SpawnInteractibleNetworkObjects(
            NetworkPrefabRef prefab, 
            Vector3 position, 
            Quaternion rotation,
            PlayerRef owner,
            RpcInfo info = default)
        {
           Runner.Spawn(prefab, position, rotation, owner);
        }
    
        private void OnDrawGizmos()
        {
            Debug.DrawRay(_cam.transform.position, _cam.transform.forward, Color.cyan);
        }
    }
}

