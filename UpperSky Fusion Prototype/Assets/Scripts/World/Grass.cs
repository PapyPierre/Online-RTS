using UnityEngine;

namespace World
{
    public class Grass : MonoBehaviour
    {
        private void Start()
        {
            return;
            for (int i = 0; i < Random.Range(0, 1); i++)
            {
                var r = Random.Range(-2f, 2f);
                
                switch (Random.Range(1, 3))
                {
                    case 1: 
                        Instantiate(gameObject, transform.position + new Vector3(r,0, r), Quaternion.identity);
                        break;
                    case 2:
                        Instantiate(gameObject, transform.position + new Vector3(r,0, 0), Quaternion.identity);
                        break;
                    case 3:
                        Instantiate(gameObject, transform.position + new Vector3(0,0, r), Quaternion.identity);
                        break;
                }
            }
        }
    }
}
