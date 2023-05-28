using System;
using System.Collections.Generic;
using CustomUI;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network_Logic
{
    public class NetworkBrain : MonoBehaviour, INetworkRunnerCallbacks
    {
        private UIManager _uiManager;

        [SerializeField] private NetworkPrefabRef _playerPrefab;
        private NetworkRunner _runner;
        private  Dictionary<PlayerRef, NetworkObject> _connectedPlayers = new ();

        private bool _mouseButton0;

        private void Start()
        {
            _uiManager = UIManager.instance;
        }

        async void StartGame(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        
            _uiManager.connectionInfoTMP.text = "Is connecting...";

            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs() 
                {
                    GameMode = mode,
                    SessionName = "TestRoom",
                    Scene = SceneManager.GetActiveScene().buildIndex,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                }
            );
        }
    
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log(player + " joined");

            if (_runner.IsServer)
            {
                Vector3 spawnPosition = new  Vector3(0,30,-50);
                NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                _connectedPlayers.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log(player + " left");
        
            if (_connectedPlayers.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _connectedPlayers.Remove(player);
            }
        }
    
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mouseButton0 = true;
            }
        }
    
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            if (_mouseButton0)
            {
                data.mouseLeftButton = 0b01;
            }
            _mouseButton0 = false;

            input.Set(data);
        }
    
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0,0,200,40), "Host"))
                {
                    StartGame(GameMode.Host);
                }
                if (GUI.Button(new Rect(0,40,200,40), "Join"))
                {
                    StartGame(GameMode.Client);
                }
            }
        }
    }
}
