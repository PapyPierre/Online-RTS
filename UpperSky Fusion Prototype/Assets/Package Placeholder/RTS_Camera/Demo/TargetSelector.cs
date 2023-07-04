using UnityEngine;

namespace Package_Placeholder.RTS_Camera.Demo
{
    [RequireComponent(typeof(RTS_Cam.RTS_Camera))]
    public class TargetSelector : MonoBehaviour 
    {
        private RTS_Cam.RTS_Camera _cam;
        private Camera _camera;
        public string targetsTag;

        private void Start()
        {
            _cam = gameObject.GetComponent<RTS_Cam.RTS_Camera>();
            _camera = gameObject.GetComponent<Camera>();
        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.CompareTag(targetsTag))
                        _cam.SetTarget(hit.transform);
                    else
                        _cam.ResetTarget();
                }
            }
        }
    }
}
