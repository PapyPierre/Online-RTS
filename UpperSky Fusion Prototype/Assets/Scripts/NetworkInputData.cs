using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte _mousebutton1 = 0b1;
    public byte Buttons;
    
    public Vector3 MoveDir;

}
