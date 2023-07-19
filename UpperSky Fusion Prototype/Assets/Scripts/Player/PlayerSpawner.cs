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
                Runner.Spawn(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
            }
        }
    }
}