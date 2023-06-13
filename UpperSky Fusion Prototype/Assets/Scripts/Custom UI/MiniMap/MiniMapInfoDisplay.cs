using NaughtyAttributes;
using UnityEngine;
using World;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Ce script est responsable de l'affichage des informations sur la minimap

namespace Custom_UI.MiniMap
{
    public class MiniMapInfoDisplay : MonoBehaviour
    {
        [SerializeField, Required()] private WorldGenerator worldGenerator;
        
        [SerializeField, Header("Inner Border")]  private float innerBorderThickness;
        [SerializeField] private Color innerBorderColor;
        
        [SerializeField, Header("Outer Border")] private float outerBorderThickness;
        [SerializeField] private Color outerBorderColor;

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = innerBorderColor;
            Handles.DrawWireDisc(
                transform.position, 
                Vector3.up, 
                worldGenerator.innerBorderRadius, 
                innerBorderThickness);
            
            Handles.color = outerBorderColor;
            Handles.DrawWireDisc(
                transform.position, 
                Vector3.up,
                worldGenerator.outerBorderRadius, 
                outerBorderThickness);
        }
        #endif
        
    }
}
