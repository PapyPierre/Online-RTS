using Custom_UI;
using Entity;
using Entity.Units;
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
        
        [SerializeField, Required()] private NetworkPrefabRef buildingPrefab;
        [SerializeField, Required()] private NetworkPrefabRef unitPrefab;

        [Header("Cameras")]
        public Camera myCam;

        [SerializeField, Required()] private GameObject minimapIndicator;
        
        [Header("Network")]
        public bool isConnected;
        [Networked] public PlayerRef MyPlayerRef {get; set; }
        [Networked] public int PlayerId {get;  set; }
        [Networked] private TickTimer Delay { get; set; }

        private void Start()
        {
            _selectionManager = SelectionManager.Instance;
            _unitsManager = UnitsManager.Instance;
        }

        public override void Spawned()
        {
            _networkManager = NetworkManager.Instance;
            _uiManager = UIManager.Instance;
            _uiManager.connectionInfoTMP.text = "Is connected";
            isConnected = true;

            if (Object.HasInputAuthority) minimapIndicator.SetActive(true);
            else myCam.gameObject.SetActive(false);
            
            transform.Rotate(Vector3.up, 180);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1) && _selectionManager.currentlySelectedUnits.Count > 0)
            {
                foreach (var unit in _selectionManager.currentlySelectedUnits)
                {
                    if (unit.Object.InputAuthority.IsValid)
                    {
                        Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray, out hit, 50000))
                        {
                            if (!hit.collider.CompareTag("Building") &&  !hit.collider.CompareTag("Unit"))
                            {
                                unit.targetPosToMoveTo = new Vector3(hit.point.x, 
                                    _unitsManager.flyingHeightOfUnits , hit.point.z);
                            }
                            else
                            {
                                Debug.LogWarning("cant move uniths there");
                            }
                        }
                    }
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!isConnected) return;
         
           // transform.position = cam.transform.position;
        
            if (GetInput(out NetworkInputData data))
            {
                if (!Object.HasInputAuthority) return;
                
                if (data.number1Key != 0)
                {
                    Vector3 spawnPos = new Vector3(0, UnitsManager.Instance.flyingHeightOfUnits -1, 0);
                    _networkManager.RPC_SpawnNetworkObject(
                        unitPrefab, spawnPos, Quaternion.identity, Object.InputAuthority, Runner);
               
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
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 50000))
            {
                if (!hit.collider.CompareTag("Building") &&  !hit.collider.CompareTag("Unit"))
                {
                    Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
                     _networkManager.RPC_SpawnNetworkObject(
                        buildingPrefab, spawnPos, Quaternion.identity, Object.InputAuthority, Runner);
                }
                else
                {
                    Debug.LogWarning("Can't build on another building");
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            Debug.DrawRay(myCam.transform.position, myCam.transform.forward, Color.cyan);
        }
    }
}

