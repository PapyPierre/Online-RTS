using Fusion;
using TMPro;
using UnityEngine;

public class BuildingManager : NetworkBehaviour
{
   public static BuildingManager Instance;
   public TextMeshProUGUI tmp;

   public override void Spawned()
   {
      if (Instance == null)
      {
         Instance = this;
   //      tmp.text = "instance set";
      }
      else
      {
         Debug.LogError(name + "have already an instance in the scene");
   //      tmp.text = "error";
      }
   }

   public void BuildAtGivenPos(NetworkPrefabRef networkPrefab, Vector3 position, Quaternion rotation)
   {
      Debug.Log("try read function ");
      Debug.Log(Object.HasInputAuthority);
      Debug.Log(Object.HasStateAuthority);

      Runner.Spawn(networkPrefab, position, rotation);
      
      Debug.Log("ok");
   }
}
