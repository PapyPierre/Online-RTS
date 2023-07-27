using System;
using Element.Entity.Military_Units;
using Entity.Military_Units;
using UnityEngine;

namespace Player
{
    public class RectangleSelection : MonoBehaviour
    {
        public static RectangleSelection Instance;
        private GameManager _gameManager;
        private UnitsManager _unitsManager;

        [SerializeField] private Color rectangleColor;
        [SerializeField] private Color borderColor;

        private bool _dragSelection;
        
        private RaycastHit hitRectangle;
        
        private MeshCollider selectionBox;
        private Mesh selectionMesh;

        private Vector3 p1;
        private Vector3 p2;
        
        //the corners of our 2d selection box
        private Vector2[] corners;

        //the vertices of our meshcollider
        private Vector3[] verts;
        private Vector3[] vecs;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _unitsManager = UnitsManager.Instance;
        }

        public void OnLeftButtonDown_RectSelection()
        {
            p1 = Input.mousePosition;
        }
        
        public void OnLeftButtonHold_RectSelection()
        {
            if((p1 - Input.mousePosition).magnitude > 20)
            {
                _dragSelection = true;
            }
        }
        
        public void OnLeftButtonUp_RectSelection(Camera cam)
        {
            if (_dragSelection) // Selection en rectangle
            {
                verts = new Vector3[4];
                vecs = new Vector3[4]; 
                int i = 0; 
                p2 = Input.mousePosition;
                
                corners = GetBoundingBox(p1, p2);

                foreach (Vector2 corner in corners) 
                {
                    Ray ray = cam.ScreenPointToRay(corner);
                    
                    if (Physics.Raycast(ray, out hitRectangle, 50000.0f, (1 << 4)))
                    { 
                        verts[i] = new Vector3(hitRectangle.point.x, hitRectangle.point.y, hitRectangle.point.z); 
                        vecs[i] = ray.origin - hitRectangle.point; 
                        // Debug.DrawLine(cam.ScreenToWorldPoint(corner), hitRectangle.point, Color.red, 1.0f);
                    }
                    i++;
                }

                //generate the mesh
                selectionMesh = GenerateSelectionMesh(verts,vecs);
                
                //GetComponent<MeshFilter>().mesh = selectionMesh; // Pour débug
                
                selectionBox = gameObject.AddComponent<MeshCollider>(); 
                selectionBox.sharedMesh = selectionMesh;
                selectionBox.convex = true;
                selectionBox.isTrigger = true;
                if (!Input.GetKey(KeyCode.LeftShift)) UnitsManager.Instance.UnSelectAllUnits();

                Destroy(selectionBox, 0.05f);
                if (_unitsManager.currentlySelectedUnits.Count < 1)
                {
                    _gameManager.thisPlayer.UnselectAllElements();
                }
            }

            _dragSelection = false;
        }
        
        Vector2[] GetBoundingBox(Vector2 p1,Vector2 p2)
        {
            // Min and Max to get 2 corners of rectangle regardless of drag direction.
            var bottomLeft = Vector3.Min(p1, p2);
            var topRight = Vector3.Max(p1, p2);

            // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
            Vector2[] corners =
            {
                new Vector2(bottomLeft.x, topRight.y),
                new Vector2(topRight.x, topRight.y),
                new Vector2(bottomLeft.x, bottomLeft.y),
                new Vector2(topRight.x, bottomLeft.y)
            };
            return corners;
        }

        //generate a mesh from the 4 bottom points
        private Mesh GenerateSelectionMesh(Vector3[] corners, Vector3[] vecs)
        {
            Vector3[] verts = new Vector3[8];
            int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 }; //map the tris of our cube

            for(int i = 0; i < 4; i++)
            {
                verts[i] = corners[i];
            }

            for(int j = 4; j < 8; j++)
            {
                verts[j] = corners[j - 4] + vecs[j - 4];
            }

            Mesh selectionMesh = new Mesh();
            selectionMesh.vertices = verts;
            selectionMesh.triangles = tris;

            return selectionMesh;
        }
        
        private void OnGUI()
        {
            if(_dragSelection)
            {
                var rect = Utils.GetScreenRect(p1, Input.mousePosition);
                Utils.DrawScreenRect(rect, rectangleColor);
                Utils.DrawScreenRectBorder(rect, 2,borderColor);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Unit"))
            {
                BaseUnit unit = other.gameObject.GetComponent<BaseUnit>();
                
                if (unit.Owner == _gameManager.thisPlayer)
                {
                    _gameManager.thisPlayer.SelectElement(unit);
                }
            }
        }
    }
}
