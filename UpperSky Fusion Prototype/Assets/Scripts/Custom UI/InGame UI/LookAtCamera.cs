using UnityEngine;

namespace Custom_UI.InGame_UI
{
    public class LookAtCamera : MonoBehaviour
    {
        private Transform _camTransform;

        private void Start()
        {
            if (Camera.main != null) _camTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + _camTransform.forward);
        }
    }
}