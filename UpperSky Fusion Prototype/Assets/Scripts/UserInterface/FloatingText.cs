using UnityEngine;
using Random = UnityEngine.Random;

namespace Custom_UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float timeAlive;
        [SerializeField] private Vector3 offset;
        [SerializeField] private Vector3 randomizePosition;
        
        private float _count;

        private void Start()
        {
            var localPosition = transform.localPosition;
            
            localPosition += offset;

            localPosition += new Vector3(
                Random.Range(-randomizePosition.x, randomizePosition.x),
                Random.Range(-randomizePosition.y, randomizePosition.y), 
                Random.Range(-randomizePosition.z, randomizePosition.z));
            
            transform.localPosition = localPosition;
        }

        private void Update()
        {
            var newY = transform.localPosition.y + Time.deltaTime * speed;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            
            _count += Time.deltaTime;
            
            if (_count >= timeAlive)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
