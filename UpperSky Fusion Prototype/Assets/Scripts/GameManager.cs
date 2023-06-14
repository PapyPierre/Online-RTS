using UnityEngine;

public class GameManager : MonoBehaviour
{
  [SerializeField] private GameObject[] dontDestroyOnLoad;

  private void Start()
  {
    foreach (var go in dontDestroyOnLoad)
    {
      DontDestroyOnLoad(go);
    }
  }
}
