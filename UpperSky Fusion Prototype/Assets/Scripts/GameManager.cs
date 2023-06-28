using System.Collections.Generic;
using Player;
using UnityEngine;
using World;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private WorldManager _worldManager;

    [HideInInspector] public PlayerController thisPlayer;
    [SerializeField] private int expectedNumberOfPlayers;

    public List<PlayerController> connectedPlayers;

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
            Destroy(gameObject);
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
         player.playerId = connectedPlayers.Count;

         if (player.HasInputAuthority) 
         {
             thisPlayer = player;

             if (connectedPlayers.Count == expectedNumberOfPlayers)
             {
                 _worldManager.CallWorldGeneration(expectedNumberOfPlayers);
             }
         }
     }
}
