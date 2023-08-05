using Fusion;
using UnityEngine;

namespace Player
{
    public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
    {
        public GameObject playerPrefab;
        public NetworkDebugStart nds;

        public void SpawnPlayers(string roomName)
        {
            nds.DefaultRoomName = roomName;
            nds.StartSharedClient();
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (player == Runner.LocalPlayer)
            { 
                Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            }
        }
    }
}