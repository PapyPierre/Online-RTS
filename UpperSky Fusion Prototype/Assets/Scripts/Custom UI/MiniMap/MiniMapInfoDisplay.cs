using System;
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
        private WorldGenerator _worldGenerator;
        
        [SerializeField, Header("Inner Border")]  private float innerBorderThickness;
        [SerializeField] private Color innerBorderColor;
        
        [SerializeField, Header("Outer Border")] private float outerBorderThickness;
        [SerializeField] private Color outerBorderColor;

        private void Start()
        {
            _worldGenerator = WorldGenerator.instance;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            Handles.color = innerBorderColor;
            Handles.DrawWireDisc(
                transform.position, 
                Vector3.up, 
                _worldGenerator.innerBorderRadius, 
                innerBorderThickness);
            
            Handles.color = outerBorderColor;
            Handles.DrawWireDisc(
                transform.position, 
                Vector3.up,
                _worldGenerator.outerBorderRadius, 
                outerBorderThickness);
        }
        #endif
        
    }
}
