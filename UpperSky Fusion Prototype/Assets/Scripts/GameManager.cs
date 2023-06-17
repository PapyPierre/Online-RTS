using UnityEngine;

public class GameManager : MonoBehaviour
{
  [SerializeField] private GameObject[] dontDestroyOnLoad;

  public enum EntityType
  {
    Unit,
    Building,
    Both
  }
  
    private void Start()
    {
     foreach (var go in dontDestroyOnLoad)
      {
       DontDestroyOnLoad(go);
     }
  } 
}
