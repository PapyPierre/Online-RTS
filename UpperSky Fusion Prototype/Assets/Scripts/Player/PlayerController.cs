using CustomUI;
using Fusion;
using Network_Logic;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        private UIManager _uiManager;
        
        [SerializeField] private NetworkPrefabRef buildingPrefab;
        
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
                if (Delay.ExpiredOrNotRunning(Runner) && Object.HasInputAuthority)
                {
                    if (data.mouseLeftButton != 0)
                    {
                        Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                        BuildAtCursorPos();
                        data.mouseLeftButton = 0;
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
                if (!hit.collider.CompareTag("Building"))
                {
                    Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
                    RPC_NetworkSpawn(buildingPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("Can't build on another building");
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_NetworkSpawn(NetworkPrefabRef prefab, Vector3 position, Quaternion rotation, RpcInfo info = default)
        {
            Runner.Spawn(prefab, position, rotation, Object.InputAuthority);
        }
    
        private void OnDrawGizmos()
        {
            Debug.DrawRay(_cam.transform.position, _cam.transform.forward, Color.cyan);
        }
    }
}

