using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef buildingPrefab;
    private Camera cam;
    [Networked] private TickTimer Delay { get; set; }

    public override void Spawned()
    {
        cam = Camera.main;
    }

    public override void FixedUpdateNetwork()
    {
        transform.position = cam.transform.position;
        
        if (GetInput(out NetworkInputData data))
        {
            if (Delay.ExpiredOrNotRunning(Runner) && Object.HasInputAuthority)
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
            Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z);
            RPC_NetworkSpawn(buildingPrefab, spawnPos, Quaternion.identity);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_NetworkSpawn(NetworkPrefabRef prefab, Vector3 position, Quaternion rotation, RpcInfo info = default)
    {
        Runner.Spawn(prefab, position, rotation, Object.InputAuthority);
    }
    
    private void OnDrawGizmos()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.cyan);
    }
}

