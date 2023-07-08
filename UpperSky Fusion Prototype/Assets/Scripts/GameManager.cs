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

    public bool gameIsStarted;

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
     
   
}
