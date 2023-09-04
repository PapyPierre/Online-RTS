using System;
using System.Collections.Generic;
using Fusion;
using Player;
using UnityEngine;
using UserInterface;
using World;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private WorldManager _worldManager;
    private UIManager _uiManager;
    
    public PlayerController thisPlayer;

    public int expectedNumberOfPlayers;

    public List<PlayerController> connectedPlayers;
    private List<PlayerController> _playersStillAlive = new();

    public bool gameIsStarted;
    public bool gameIsFinished;

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

     private void Update()
     {
         CheckToStartTheGame();
     }

     private void CheckToStartTheGame()
     {
         if (gameIsStarted || gameIsFinished || connectedPlayers.Count < expectedNumberOfPlayers) return;

         foreach (var player in connectedPlayers)
         {
             if (!player.IsReadyToPlay)
             {
                 return;
             }
         }
         
         StartGame();
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
             
             Debug.Log("This player is : Player " +  player.myId);

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

     private void StartGame()
     {
         foreach (var player in connectedPlayers)
         {
             player.RPC_OnGameStart();
         }
     }
     
     public void DefeatPlayer(PlayerController defeatedPlayer)
     {
         defeatedPlayer.RPC_OutOfGame();
         _playersStillAlive.Remove(defeatedPlayer);

         if (_playersStillAlive.Count is 1) EndGame(_playersStillAlive[0]);
     }
     
     public void EndGame(PlayerController winner)
     {
         gameIsFinished = true;
         Time.timeScale = 0;
         _playersStillAlive.Remove(winner);

         foreach (var player in _playersStillAlive)
         {
             player.RPC_OutOfGame();
         }
         
         winner.RPC_Win();
     }
}
