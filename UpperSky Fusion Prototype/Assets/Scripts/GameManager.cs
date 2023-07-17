using System.Collections.Generic;
using Player;
using UnityEngine;
using World;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private WorldManager _worldManager;

    // Seiralized for debug
    public PlayerController thisPlayer;
    
    [SerializeField] private int expectedNumberOfPlayers;

    public List<PlayerController> connectedPlayers;
    private List<PlayerController> playersStillAlive = new();

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
     }

     public void ConnectPlayer(PlayerController player)
     {
         connectedPlayers.Add(player);
         playersStillAlive.Add(player);
         player.myId = connectedPlayers.Count;

         if (player.HasInputAuthority) 
         {
             thisPlayer = player;

             if (connectedPlayers.Count == expectedNumberOfPlayers)
             {
                 _worldManager.CallWorldGeneration(expectedNumberOfPlayers);
                 gameIsStarted = true;
             }
         }
     }
     
     public void KillPlayer(PlayerController defeatedPlayer)
     {
         defeatedPlayer.RPC_OutOfGame();
         playersStillAlive.Remove(defeatedPlayer);

         if (playersStillAlive.Count is 1) EndGame(playersStillAlive[0]);
     }
     
     private void EndGame(PlayerController winner)
     {
         gameIsFinished = true;
         Time.timeScale = 0;
         winner.RPC_Win();
     }
}
