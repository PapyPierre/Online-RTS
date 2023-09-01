using System;
using Element.Island;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class IslandProps : MonoBehaviour
    {
        private IslandGenerator _islandGenerator;
        private BaseIsland _myIsland;

        private Collider _myCollider;
        private MeshFilter _myMeshFilter;

        [ReadOnly] public bool isMoving; 

        private void Awake()
        {
            _myCollider = GetComponent<Collider>();
            _myCollider.enabled = false;
            _myMeshFilter = GetComponent<MeshFilter>();
        }

        public void Init(IslandGenerator islandGenerator, BaseIsland island)
        {
            _islandGenerator = islandGenerator;
            _myIsland = island;
            var rotation = transform.rotation;
            rotation = Quaternion.Euler(rotation.x, Random.Range(0f,179f), rotation.z);
            transform.rotation = rotation;
            _myCollider.enabled = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Props"))
            {
                _islandGenerator.MoveObjOnIsland(this, _myCollider, _myMeshFilter, _myIsland);
            }
        }
    }
}