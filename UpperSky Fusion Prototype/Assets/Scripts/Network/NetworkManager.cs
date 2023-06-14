using System;
using System.Collections.Generic;
using Custom_UI;
using Fusion;
using Fusion.Sockets;
using Player;
using UnityEngine;
using World;

namespace Network
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkManager Instance;
        private UIManager _uiManager;

        [SerializeField] private NetworkPrefabRef playerPrefab;
        private NetworkRunner _runner;
        [HideInInspector] public List<PlayerClass> ConnectedPlayers = new ();

        private bool _mouseButton0;
        private bool _keypad1;
        private bool _keypad2;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }
        
        private void Start()
        {
            _uiManager = UIManager.Instance;
        }

        public async void StartGame(GameMode mode, string roomName)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        
            _uiManager.connectionInfoTMP.text = "Is connecting...";

            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs() 
                {
                    GameMode = mode,
                    SessionName = roomName,
                    Scene = 1,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                }
            );
            
            _uiManager.mainMenu.SetActive(false);
            _uiManager.ressourcesLayout.SetActive(true);
        }
    
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log(player + " joined");

            if (_runner.IsServer)
            {
                Vector3 spawnPosition = new  Vector3(0,30,-50);
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                
                PlayerClass newPlayer = new PlayerClass();
                newPlayer.Ref = player;
                newPlayer.NetworkObject = networkPlayerObject;
                newPlayer.Controller = networkPlayerObject.GetComponent<PlayerController>();
                ConnectedPlayers.Add(newPlayer
                );
                
                if (ConnectedPlayers.Count == 2)
                {
                    WorldGenerator.Instance.GenerateWorld(2);
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log(player + " left");

            foreach (var playerClass in ConnectedPlayers)
            {
                if (playerClass.Ref == player)
                {
                    playerClass.Disconnected = true;
                }
            }
        }
    
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mouseButton0 = true;
            }
            
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                _keypad1 = true;
            }
            
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                _keypad2 = true;
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

            if (_keypad1)
            {
                data.number1Key = 0b01;
            }
            _keypad1 = false;
            
            if (_keypad2)
            {
                data.number2Key = 0b01;
            }
            _keypad2 = false;

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
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public NetworkObject RPC_SpawnNetworkObject(NetworkPrefabRef prefab, Vector3 position, Quaternion rotation,
            PlayerRef owner, NetworkRunner runner, RpcInfo info = default)
        {
          return runner.Spawn(prefab, position, rotation, owner);
        }

     
    }
}
