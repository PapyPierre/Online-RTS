using System.Collections.Generic;
using Custom_UI;
using Player;
using UnityEngine;
using World;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private WorldManager _worldManager;
    private UIManager _uiManager;

    // Seiralized for debug
    public PlayerController thisPlayer;

    public int expectedNumberOfPlayers;

    public List<PlayerController> connectedPlayers;
    private List<PlayerController> _playersStillAlive = new();

    private int _readyPlayersIndex;

    public bool gameIsStarted;
    public bool gameIsFinished;

    public enum EntityType
     {
         Unit,
         Building,
         Both
     }
     
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
         _worldManager = WorldManager.Instance;
         _uiManager = UIManager.Instance;
     }

     public void ConnectPlayer(PlayerController player)
     {
         connectedPlayers.Add(player);
         _uiManager.UpdateLoadingText(false);
         
         _playersStillAlive.Add(player);
         player.myId = connectedPlayers.Count;

         if (player.HasInputAuthority) 
         {
             thisPlayer = player;

             if (connectedPlayers.Count == expectedNumberOfPlayers)
             {
                 _worldManager.CallWorldGeneration(expectedNumberOfPlayers);

                 foreach (var playerController in connectedPlayers)
                 {
                     playerController.RPC_DisplayLoadingText();
                 } 
             }
         }
     }

     public void StartGame()
     {
         foreach (var player in connectedPlayers)
         {
             player.RPC_StartToPlay();
         }
     }
     
     public void DefeatPlayer(PlayerController defeatedPlayer)
     {
         defeatedPlayer.RPC_OutOfGame();
         _playersStillAlive.Remove(defeatedPlayer);

         if (_playersStillAlive.Count is 1) EndGame(_playersStillAlive[0]);
     }
     
     private void EndGame(PlayerController winner)
     {
         gameIsFinished = true;
         Time.timeScale = 0;
         winner.RPC_Win();
     }
}
