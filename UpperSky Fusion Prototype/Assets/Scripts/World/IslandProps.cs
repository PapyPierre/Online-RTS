using System;
using Element.Island;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class IslandProps : MonoBehaviour
    {
        private IslandGenerator _islandGenerator;
        private Vector3 _myIslandPos;
        
        public void Init(IslandGenerator islandGenerator, Vector3 pos)
        {
            _islandGenerator = islandGenerator;
            _myIslandPos = pos;
            var rotation = transform.rotation;
            rotation = Quaternion.Euler(rotation.x, Random.Range(0f,179f), rotation.z);
            transform.rotation = rotation;
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Props"))
            {
                _islandGenerator.MoveObjOnIsland(gameObject, _myIslandPos);
            }
        }
    }
}