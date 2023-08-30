using Element.Island;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class IslandProps : MonoBehaviour
    {
        private IslandGenerator _islandGenerator;
        private BaseIsland _myIsland;
        
        public void Init(IslandGenerator islandGenerator, BaseIsland island)
        {
            _islandGenerator = islandGenerator;
            _myIsland = island;
            var rotation = transform.rotation;
            rotation = Quaternion.Euler(rotation.x, Random.Range(0f,179f), rotation.z);
            transform.rotation = rotation;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Props"))
            {
                _islandGenerator.MoveObjOnIsland(gameObject, _myIsland);
            }
        }
    }
}