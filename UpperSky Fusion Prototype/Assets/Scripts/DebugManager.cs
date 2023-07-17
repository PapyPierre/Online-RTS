using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application = UnityEngine.Device.Application;

public class DebugManager : MonoBehaviour
{
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
           DebugMouseHoverObjectName();
        }
    }

    private void DebugMouseHoverObjectName()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
            
        if (Physics.Raycast(ray, out hit))
        { 
            Transform hitTransform = hit.transform;
            Debug.Log("Objet survolé par la souris : " + hitTransform.name); 
        }
    }
#endif
}
