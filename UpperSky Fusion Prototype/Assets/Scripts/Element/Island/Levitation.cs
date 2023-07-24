using UnityEngine;

namespace World.Island
{
    public class Levitation : MonoBehaviour
    {
        public float levitationHeight = 1f;
        public float levitationSpeed = 1f;

        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;
        }

        private void Update()
        {
            float newY = initialPosition.y + Mathf.Sin(Time.time * levitationSpeed) * levitationHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
