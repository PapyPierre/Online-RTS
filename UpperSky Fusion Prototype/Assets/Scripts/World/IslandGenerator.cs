using System;
using Element.Island;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class IslandGenerator : MonoBehaviour
    {
        private WorldManager _worldManager;

        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private GameObject[] baseIslandsMesh;
        private GameObject _lastGeneratedIsland;
        private BoxCollider _islandCollider;
        private float _xBoxColSize;
        private float _zBoxColSize;

        public void GenerateIsland(Vector3 position, IslandTypesEnum type)
        {
            if (_worldManager == null)
            {
                _worldManager = GetComponent<WorldManager>();
            }

            if (_lastGeneratedIsland != null)
            {
                _lastGeneratedIsland.SetActive(false);
            }

            GenerateBaseMesh(position, type);

            Debug.Log("A " + type + " island have been generated at " + position);
        }

        private void GenerateBaseMesh(Vector3 position, IslandTypesEnum type)
        {
            int x = Random.Range(0, baseIslandsMesh.Length);
            _lastGeneratedIsland = Instantiate(baseIslandsMesh[x], position, Quaternion.identity);
            _islandCollider = _lastGeneratedIsland.GetComponent<BoxCollider>();
            _xBoxColSize = _islandCollider.size.x/2;
            _zBoxColSize = _islandCollider.size.z/2;

            _lastGeneratedIsland.GetComponent<BaseIsland>().ground.material.color
                = _worldManager.islandTypes[(int) type].data.GroundColor;

            var data = _worldManager.islandTypes[(int) type].data;
            
            // Rocks
            GenerateObj(position, type, data.NumberOfRocks.x, data.NumberOfRocks.y, data.Rocks);
            
            // Trees
            GenerateObj(position, type, data.NumberOfTrees.x, data.NumberOfTrees.y, data.Trees);

            // Trunk, Log, other medium stuff
            GenerateObj(position, type, data.NumberOfMediumStuff.x, data.NumberOfMediumStuff.y, data.MediumStuff);
            
            // Plants including mushrooms and bush
            GenerateObj(position, type, data.NumberOfPlants.x, data.NumberOfPlants.y, data.Plants);
            
            // Grass
            GenerateObj(position, type, data.NumberOfGrass.x, data.NumberOfGrass.y, data.Grass, true);
        }

        private void GenerateObj(Vector3 islandPos, IslandTypesEnum type, int numberOfObjX, int numberOfObjY,
            GameObject[] obj, bool isGrass = false)
        {
            int r = Random.Range(numberOfObjX, numberOfObjY);
            
            for (int i = 0; i < r; i++)
            {              
                int x = Random.Range(0, obj.Length);
                GameObject selectedObj = obj[x];
                
                SpawnObjOnIsland(selectedObj, islandPos, isGrass);
            }
        }

        private void SpawnObjOnIsland(GameObject obj, Vector3 islandPos, bool isGrass = false)
        {
            while (true)
            {
                Vector2 obj2DPos = CustomHelper.GenerateRandomPosIn2DArea(_zBoxColSize, _xBoxColSize);
                Vector3 objPos = new Vector3(obj2DPos.x, islandPos.y, obj2DPos.y);

                GameObject newObj = Instantiate(obj, objPos, Quaternion.identity, _lastGeneratedIsland.transform);
                
                if (!isGrass)
                {
                    newObj.GetComponent<IslandProps>().Init(this, islandPos);
                    
                    if (!IsMeshOverlappingCompletely(newObj.GetComponent<MeshFilter>().mesh, newObj.transform.position))
                    {
                        newObj.SetActive(false);
                        continue;
                    }

                    break;
                }
                else
                {
                    if (!Physics.Raycast(newObj.transform.position + new Vector3(0,5,0), Vector3.down, 
                            Mathf.Infinity, terrainLayer, QueryTriggerInteraction.Ignore))
                    {
                        newObj.SetActive(false);
                        continue;
                    }

                    break;
                }
            }
        }
        
        public void MoveObjOnIsland(GameObject obj, Vector3 islandPos)
        {
            Vector2 obj2DPos = CustomHelper.GenerateRandomPosIn2DArea(_zBoxColSize, _xBoxColSize);
            obj.transform.position = new Vector3(obj2DPos.x, islandPos.y, obj2DPos.y);
                
            if (!IsMeshOverlappingCompletely(obj.GetComponent<MeshFilter>().mesh, obj.transform.position))
            { 
                MoveObjOnIsland(obj, islandPos);
            }
        }

        private bool IsMeshOverlappingCompletely(Mesh mesh, Vector3 position)
        {
            foreach (Vector3 vertex in mesh.vertices)
            {
                Vector3 worldVertex = position + vertex;
                
                if (!Physics.Raycast(worldVertex + new Vector3(0,5,0), Vector3.down, 
                        Mathf.Infinity, terrainLayer, QueryTriggerInteraction.Ignore))
                {
                    return false; 
                }
               
            }

            return true; 
        }

        private void OnDrawGizmos()
        {
            if (_lastGeneratedIsland == null) return;
           
            BoxCollider islandCollider = _lastGeneratedIsland.GetComponent<BoxCollider>();
            float xBoxColSize = islandCollider.size.x/2;
            float zBoxColSize = islandCollider.size.z/2;
            Vector3 center = islandCollider.center;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center  + new Vector3(xBoxColSize, 0, -zBoxColSize), center + new Vector3(xBoxColSize, 0, zBoxColSize));
            Gizmos.DrawLine(center + new Vector3(xBoxColSize, 0, zBoxColSize), center + new Vector3(-xBoxColSize, 0, zBoxColSize));
            Gizmos.DrawLine(center + new Vector3(-xBoxColSize, 0, zBoxColSize), center + new Vector3(-xBoxColSize, 0, -zBoxColSize));
            Gizmos.DrawLine(center + new Vector3(-xBoxColSize, 0, -zBoxColSize), center + new Vector3(xBoxColSize, 0, -zBoxColSize));
        }
    }
}