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
        private float _boxColWidth;
        private float _boxColHeight;

        [SerializeField, Header("Grass Perlin Noise")] private Renderer perlinNoiseRenderer;
        [SerializeField] private int perlinNoiseHeight;
        [SerializeField] private int perlinNoiseWidth;
        [SerializeField] private float perlinNoiseScale;
        [SerializeField] private float perlinNoiseSpawnReduction;
        private float _xOffset;
        private float _yOffset;
        private static readonly int ShaderTopColor = Shader.PropertyToID("_TopColor");
        private static readonly int ShaderGroundColor = Shader.PropertyToID("_GroundColor");

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

            _boxColWidth = _islandCollider.bounds.size.x;
            _boxColHeight = _islandCollider.bounds.size.z; // not proper "height", but "height" in top down view
            
            _lastGeneratedIsland.GetComponent<BaseIsland>().ground.material.color
                = _worldManager.islandTypes[(int) type].data.GroundColor;

            IslandTypeData data = _worldManager.islandTypes[(int) type].data;
            
            // Rocks
            GenerateObj(position, data.NumberOfRocks.x, data.NumberOfRocks.y, data.Rocks, data);
            
            // Trees
            GenerateObj(position, data.NumberOfTrees.x, data.NumberOfTrees.y, data.Trees, data);

            // Trunk, Log, other medium stuff
            GenerateObj(position, data.NumberOfMediumStuff.x, data.NumberOfMediumStuff.y, data.MediumStuff, data);
            
            // Plants including mushrooms and bush
            GenerateObj(position, data.NumberOfPlants.x, data.NumberOfPlants.y, data.Plants, data);
            
            // Grass
            if (data.SpawnGrass) GenerateGrass(position, data.Grass[0], data);
        }

        private void GenerateObj(Vector3 islandPos, int numberOfObjX, int numberOfObjY,
            GameObject[] obj, IslandTypeData data)
        {
            int r = Random.Range(numberOfObjX, numberOfObjY);
            
            for (int i = 0; i < r; i++)
            {              
                int x = Random.Range(0, obj.Length);
                GameObject selectedObj = obj[x];
                
                SpawnObjOnIsland(selectedObj, islandPos, data);
            }
        }

        private void GenerateGrass(Vector3 islandPos, GameObject grassObj, IslandTypeData data)
        {
            Texture2D texture = GenerateNoiseTexture();
            perlinNoiseRenderer.material.mainTexture = texture;
            
            int width = texture.width;
            int height = texture.height;

            for (int u = 0; u < width; u++)
            {
                for (int v = 0; v < height; v++)
                {
                    float perlinValue = texture.GetPixel(u, v).grayscale;

                    float r = Random.Range(0f, perlinNoiseSpawnReduction);
                    
                    if (Mathf.Clamp01(perlinValue) >= r)
                    {
                        float normalizedU = u / (float) width;
                        float normalizedV = v / (float) height;

                        float x = normalizedU * _boxColWidth + islandPos.x - _boxColWidth/2;
                        float y = normalizedV * _boxColHeight + islandPos.z - _boxColHeight/2;
                        
                        Vector3 spawnPosition = new Vector3(x , islandPos.y, y);

                        GameObject newGrassObj =
                            Instantiate(grassObj, spawnPosition, Quaternion.identity, _lastGeneratedIsland.transform);
                        
                        if (!Physics.Raycast(newGrassObj.transform.position + new Vector3(0,5,0), Vector3.down, 
                                Mathf.Infinity, terrainLayer, QueryTriggerInteraction.Ignore))
                        {
                            newGrassObj.SetActive(false);
                        }
                        else
                        {
                            // Change grass color according to island type
                            MeshRenderer[] grassRenderers = newGrassObj.GetComponentsInChildren<MeshRenderer>();

                            foreach (var renderer in grassRenderers)
                            {
                                renderer.material.SetColor(ShaderTopColor, data.TopColor);
                                renderer.material.SetColor(ShaderGroundColor, data.GroundColor);
                            }
                        }
                    }
                }
            }
        }

        private Texture2D GenerateNoiseTexture()
        {
            Texture2D texture = new Texture2D(perlinNoiseWidth, perlinNoiseHeight);
            
            _xOffset = Random.Range(0, 9999f);
            _yOffset = Random.Range(0, 9999f);
            
            for (int x = 0; x < perlinNoiseWidth; x++)
            {
                for (int y = 0; y < perlinNoiseHeight; y++)
                {
                    Color color = CalculateNoiseColor(x, y);
                    texture.SetPixel(x,y,color);
                }
            }
            
            texture.Apply();
            
            return texture;
        }

        private Color CalculateNoiseColor(int x, int y)
        {
            float xCoord = (float) x / perlinNoiseWidth * perlinNoiseScale + _xOffset;
            float yCoord = (float) y / perlinNoiseHeight * perlinNoiseScale + _yOffset;
            
            float perlinNoiseValue = Mathf.Clamp01(Mathf.PerlinNoise(xCoord, yCoord));

            return new Color(perlinNoiseValue, perlinNoiseValue, perlinNoiseValue);
        }

        private void SpawnObjOnIsland(GameObject obj, Vector3 islandPos, IslandTypeData data)
        {
            while (true)
            {
                Vector2 obj2DPos = CustomHelper.GenerateRandomPosIn2DArea(new Vector2(islandPos.x, islandPos.z), _boxColHeight, _boxColWidth);
                Vector3 objPos = new Vector3(obj2DPos.x, islandPos.y, obj2DPos.y);

                GameObject newObj = Instantiate(obj, objPos, Quaternion.identity, _lastGeneratedIsland.transform);
                newObj.GetComponent<IslandProps>().Init(this, islandPos);
                    
                if (!IsMeshOverlappingCompletely(newObj.GetComponent<MeshFilter>().mesh, newObj.transform.position))
                {
                    newObj.SetActive(false); 
                    continue;
                }
                else
                {
                    
                    MeshRenderer renderer = newObj.GetComponent<MeshRenderer>();
                    
                    foreach (var mat in renderer.materials)
                    {
                        if (mat.shader.name == "Polytope Studio/PT_Vegetation_Foliage_Shader")
                        {
                            mat.SetColor(ShaderTopColor, data.TopColor);
                            mat.SetColor(ShaderGroundColor, data.GroundColor);
                        }
                    }
                }

                break;
            }
        }
        
        public void MoveObjOnIsland(GameObject obj, Vector3 islandPos)
        {
            Vector2 obj2DPos = CustomHelper.GenerateRandomPosIn2DArea(new Vector2(islandPos.x, islandPos.z), _boxColHeight, _boxColWidth);
            obj.transform.position = new Vector3(obj2DPos.x, islandPos.y, obj2DPos.y);
                
            if (!IsMeshOverlappingCompletely(obj.GetComponent<MeshFilter>().mesh, obj.transform.position))
            { 
                MoveObjOnIsland(obj, islandPos);
            }
        }

        private bool IsMeshOverlappingCompletely(Mesh mesh, Vector3 islandPos)
        {
            foreach (Vector3 vertex in mesh.vertices)
            {
                Vector3 worldVertex = islandPos + vertex;
                
                if (!Physics.Raycast(worldVertex + new Vector3(0,5,0), Vector3.down, 
                        Mathf.Infinity, terrainLayer, QueryTriggerInteraction.Ignore))
                {
                    return false; 
                }
               
            }

            return true; 
        }
    }
}