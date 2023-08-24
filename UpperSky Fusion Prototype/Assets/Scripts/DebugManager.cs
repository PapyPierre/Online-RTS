using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    public bool showUnitsStateDebugText;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError(name);
            return;
        }
        
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
           DebugMouseHoverObjectName();
        }
    }

    private void DebugMouseHoverObjectName()
    {
        if (!Application.isEditor) return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
            
        if (Physics.Raycast(ray, out hit))
        { 
            Transform hitTransform = hit.transform;
            Debug.Log("Objet survolé par la souris : " + hitTransform.name); 
        }
    }
}
