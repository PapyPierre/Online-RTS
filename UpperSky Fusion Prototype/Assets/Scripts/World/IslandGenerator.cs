using Element.Island;
using Fusion;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class IslandGenerator : MonoBehaviour
    {
        private WorldManager _worldManager;
        private GameManager _gameManager;

        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private NetworkPrefabRef[] baseIslandsMesh;

        [SerializeField, Header("Generation Check")] private bool isGeneratingRocks;
        [SerializeField] private bool isGeneratingStones;
        [SerializeField] private bool isGeneratingTrees;
        [SerializeField] private bool isGeneratingMediumStuff;
        [SerializeField] private bool isGeneratingPlants;
        [SerializeField] private bool isGeneratingGrass;


        [SerializeField, Header("Grass Perlin Noise")] private Renderer perlinNoiseRenderer;
        [SerializeField] private int perlinNoiseHeight;
        [SerializeField] private int perlinNoiseWidth;
        [SerializeField] private float perlinNoiseScale;
        [SerializeField] private float perlinNoiseSpawnReduction;
        private float _xOffset;
        private float _yOffset;
        private static readonly int ShaderTopColor = Shader.PropertyToID("_TopColor");
        private static readonly int ShaderGroundColor = Shader.PropertyToID("_GroundColor");
        
        private void Start()
        {
            _worldManager = GetComponent<WorldManager>();
            _gameManager = GameManager.Instance;
        }
        
        public void GenerateIsland(Vector3 position, IslandTypesEnum type, PlayerController owner)
        {
            GenerateBaseMesh(position, type, owner);

            Debug.Log("A " + type + " island have been generated at " + position);
        }

        private void GenerateBaseMesh(Vector3 position, IslandTypesEnum type, PlayerController owner)
        {
            int x = Random.Range(0, baseIslandsMesh.Length);
            var islandRot = Quaternion.Euler(0, Random.Range(0f,179f), 0);
            
            BaseIsland generatedIsland  = _gameManager.thisPlayer.Runner.Spawn(baseIslandsMesh[x], position, islandRot,
                    owner != null ? owner.Object.StateAuthority : PlayerRef.None).GetComponent<BaseIsland>();
            
            generatedIsland.data = _worldManager.islandTypes[(int) type].data;

            generatedIsland.transform.parent = _worldManager.worldGenerator.worldCenter;
            generatedIsland.Init(owner, generatedIsland.data);

            generatedIsland.ground.material.color =  generatedIsland.data.GroundColor;
            generatedIsland.rockMesh.material.color =  generatedIsland.data.RockColor;

            GeneratePropsOnIsland(generatedIsland, generatedIsland.data);
        }

        private void GeneratePropsOnIsland(BaseIsland island, IslandData data)
        {
            // Rocks
            if (isGeneratingRocks) GenerateObj(island, data.NumberOfRocks.x, data.NumberOfRocks.y, data.Rocks, data);

            // Stones
            if (isGeneratingStones) GenerateObj(island, data.NumberOfStones.x, data.NumberOfStones.y, data.Stones, data);
            
            // Trees
            if (isGeneratingTrees) GenerateObj(island, data.NumberOfTrees.x, data.NumberOfTrees.y, data.Trees, data);

            // Trunk, Log, other medium stuff
            if (isGeneratingMediumStuff) GenerateObj(island, data.NumberOfMediumStuff.x, data.NumberOfMediumStuff.y, data.MediumStuff, data);
            
            // Plants including mushrooms,bush and flowers
            if (isGeneratingPlants) GenerateObj(island, data.NumberOfPlants.x, data.NumberOfPlants.y, data.Plants, data);
            
            // Grass
            if (data.SpawnGrass && isGeneratingGrass) GenerateGrass(island, data.Grass[0], data);
        }

        private void GenerateObj(BaseIsland island, int numberOfObjX, int numberOfObjY,
            GameObject[] obj, IslandData data)
        {
            int r = Random.Range(numberOfObjX, numberOfObjY);
            
            for (int i = 0; i < r; i++)
            {              
                int x = Random.Range(0, obj.Length);
                GameObject selectedObj = obj[x];
                
                SpawnObjOnIsland(selectedObj, island, data);
            }
        }

        private void GenerateGrass(BaseIsland island, GameObject grassObj, IslandData data)
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
                        
                        BoxCollider islandCollider = island.GetComponent<BoxCollider>();
                        float boxColWidth = islandCollider.bounds.size.x;
                        float boxColHeight = islandCollider.bounds.size.z; // not proper "height", but "height" in top down view

                        Vector3 islandPos = island.transform.position;

                        float x = normalizedU * boxColWidth + islandPos.x - boxColWidth/2;
                        float y = normalizedV * boxColHeight + islandPos.z - boxColHeight/2;
                        
                        Vector3 spawnPosition = new Vector3(x , islandPos.y, y);

                        GameObject newGrassObj =
                            Instantiate(grassObj, spawnPosition, Quaternion.identity,  island.transform);
                        
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

        private void SpawnObjOnIsland(GameObject obj, BaseIsland island, IslandData data)
        {
            while (true)
            {
                Vector3 objPos = FindPosOnIsland(island);

                NetworkObject newObj = _gameManager.thisPlayer.Runner.Spawn(obj, objPos, Quaternion.identity, PlayerRef.None);
                
                newObj.GetComponent<IslandProps>().Init(this, island);
                newObj.transform.parent = island.graphObject.transform;

                
                if (!IsMeshOverlappingCompletely(newObj.GetComponent<MeshFilter>().mesh, newObj.transform.position))
                {
                    newObj.gameObject.SetActive(false); 
                    continue;
                }
                else
                {
                    MeshRenderer meshRenderer = newObj.GetComponent<MeshRenderer>();
                    
                    foreach (var mat in meshRenderer.materials)
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

        private Vector3 FindPosOnIsland(BaseIsland island)
        {
            BoxCollider islandCollider = island.GetComponent<BoxCollider>();
            float boxColWidth = islandCollider.bounds.size.x;
            float boxColHeight = islandCollider.bounds.size.z; // not proper "height", but "height" in top down view
            
            Vector3 islandPos = island.transform.position;

            Vector2 center = new Vector2(islandPos.x, islandPos.z);
            
            Vector2 obj2DPos = CustomHelper.GenerateRandomPosIn2DArea(center, boxColHeight, boxColWidth);
            
            return new Vector3(obj2DPos.x, islandPos.y, obj2DPos.y);
        }
        
        public void MoveObjOnIsland(GameObject obj, BaseIsland island)
        {
            obj.transform.position = FindPosOnIsland(island);

            // To reset collision detection 
            Collider objCollider = obj.GetComponent<Collider>();
            objCollider.enabled = false;
            objCollider.enabled = true;
            
            if (!IsMeshOverlappingCompletely(obj.GetComponent<MeshFilter>().mesh, obj.transform.position))
            { 
                MoveObjOnIsland(obj, island);
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