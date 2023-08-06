using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using World;

// Ce script est responsable de l'affichage des informations sur la minimap

namespace UserInterface.MiniMap
{
    public class MiniMap : MonoBehaviour
    {
        private GameManager _gameManager;
        private WorldManager _worldManager;
        private Camera _minimapCam;
        private Camera _playerCam;

        [SerializeField] private LineRenderer lineRenderer;
        private readonly Vector3[] _screenCornerPos = new Vector3[4];
        [SerializeField] private LayerMask fovDisplayRaycastHitLayer;

        [SerializeField] private MinimapHoverDetector minimapHoverDetector;
        
        [SerializeField, Header("Inner Border")]  private float innerBorderThickness;
        [SerializeField] private Color innerBorderColor;
        
        [SerializeField, Header("Outer Border")] private float outerBorderThickness;
        [SerializeField] private Color outerBorderColor;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _worldManager = WorldManager.Instance;
            _minimapCam = GetComponent<Camera>();
        }
        
        private void Update()
        {
            if (!_gameManager.gameIsStarted) return;

            if (_playerCam == null) _playerCam = _gameManager.thisPlayer.myCam;
            
            _screenCornerPos[0] = new Vector3(0, Screen.height, _playerCam.nearClipPlane);
            _screenCornerPos[1] = new Vector3(0, 0, _playerCam.nearClipPlane);
            _screenCornerPos[2] = new Vector3(Screen.width, 0, _playerCam.nearClipPlane);
            _screenCornerPos[3] = new Vector3(Screen.width, Screen.height, _playerCam.nearClipPlane);

            for (int i = 0; i < 4; i++)
            {
                Ray ray = _playerCam.ScreenPointToRay(_screenCornerPos[i]);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 5000, fovDisplayRaycastHitLayer, QueryTriggerInteraction.Collide))
                {
                    lineRenderer.SetPosition(i, hit.point);
                }
            }
            
            if (minimapHoverDetector.IsMouseOverMinimap())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = _minimapCam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        _gameManager.thisPlayer.transform.position = hit.point;
                    }
                }
            }
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
