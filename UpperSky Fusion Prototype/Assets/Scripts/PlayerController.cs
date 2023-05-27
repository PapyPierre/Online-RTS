using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float speed;

    [SerializeField] private NetworkPrefabRef buildingPrefab;
    [SerializeField] private Camera cam;
    private NetworkCharacterControllerPrototype _cc;
    [Networked] private TickTimer Delay { get; set; }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {  
            data.MoveDir.Normalize();
            _cc.Move(speed * data.MoveDir * Runner.DeltaTime);
            
            if (Delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.Buttons & NetworkInputData._mousebutton1) != 0)
                {
                    Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    BuildAtCursorPos();
                    data.Buttons = 0;
                }
            }
        }
    }

    private RaycastHit hit;
    public void BuildAtCursorPos()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit, 50000))
        {
            Runner.Spawn(buildingPrefab, hit.point, Quaternion.identity, Object.InputAuthority);
        }
        
    }
}