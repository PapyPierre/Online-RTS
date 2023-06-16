using System;
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
        private WorldManager _worldManager;
        
        [SerializeField, Header("Inner Border")]  private float innerBorderThickness;
        [SerializeField] private Color innerBorderColor;
        
        [SerializeField, Header("Outer Border")] private float outerBorderThickness;
        [SerializeField] private Color outerBorderColor;

        private void Start()
        {
            _worldManager = WorldManager.Instance;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            Handles.color = innerBorderColor;
            Handles.DrawWireDisc(
                transform.position, 
                Vector3.up, 
                _worldManager.innerBorderRadius, 
                innerBorderThickness);
            
            Handles.color = outerBorderColor;
            Handles.DrawWireDisc(
                transform.position, 
                Vector3.up,
                _worldManager.outerBorderRadius, 
                outerBorderThickness);
        }
        #endif
        
    }
}
