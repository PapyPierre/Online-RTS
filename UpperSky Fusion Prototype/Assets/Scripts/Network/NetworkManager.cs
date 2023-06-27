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
        [HideInInspector] public NetworkRunner myRunner;
        
        [HideInInspector] public List<PlayerController> connectedPlayers = new();
        [HideInInspector] public PlayerController thisPlayer;

        private bool _mouseButton0;

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
            connectedPlayers = new List<PlayerController>();
        }

        public async void StartGame(GameMode mode, string roomName)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            myRunner = gameObject.AddComponent<NetworkRunner>();
            myRunner.ProvideInput = true;
        
            _uiManager.connectionInfoTMP.text = "Is connecting...";

            // Start or join (depends on gamemode) a session with a specific name
            await myRunner.StartGame(new StartGameArgs() 
                {
                    GameMode = mode,
                    SessionName = roomName,
                    Scene = 1,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                }
            );
            
            SetActiveInGameUI();
        }

        private void SetActiveInGameUI()
        { 
            _uiManager.mainMenu.SetActive(false);
            _uiManager.inGameUI.SetActive(true);
        }
    
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log(player + " joined");

            if (runner.IsServer)
            {
                Vector3 spawnPosition = new  Vector3(0,30,-50);
                NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

                PlayerController playerController = playerObject.GetComponent<PlayerController>();
                playerController.MyPlayerRef = player;
                connectedPlayers.Add(playerController);

                if (connectedPlayers.Count == 2)
                {
                    WorldManager.Instance.CallWorldGeneration(2);
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log(player + " left");
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
    }
}
