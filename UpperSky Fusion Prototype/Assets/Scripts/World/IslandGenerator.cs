using System.Collections;
using System.Threading.Tasks;
using Element.Island;
using Fusion;
using Player;
using Ressources.AOSFogWar.Used_Scripts;
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
            int x = Random.Range(0, baseIslandsMesh.Length);
            var islandRot = Quaternion.Euler(0, Random.Range(0f,179f), 0);
            
            BaseIsland island = _gameManager.thisPlayer.Runner.Spawn(baseIslandsMesh[x], position, islandRot,
                owner != null ? owner.Object.StateAuthority : PlayerRef.None).GetComponent<BaseIsland>();
            
            island.SetUp(owner,  _worldManager.allIslandsData[(int) type]);
        }

        public async void GeneratePropsOnIsland(BaseIsland island, IslandData data)
        {
            // Rocks
            if (isGeneratingRocks) await GenerateObj(island, data.NumberOfRocks.x, data.NumberOfRocks.y, data.Rocks, data);

            // Trees
            if (isGeneratingTrees) await GenerateObj(island, data.NumberOfTrees.x, data.NumberOfTrees.y, data.Trees, data);
            
            // Trunk, Log, other medium stuff
            if (isGeneratingMediumStuff) await GenerateObj(island, data.NumberOfMediumStuff.x, data.NumberOfMediumStuff.y, data.MediumStuff, data);
            
            // Stones
            if (isGeneratingStones) await GenerateObj(island, data.NumberOfStones.x, data.NumberOfStones.y, data.Stones, data);
            
            // Plants include mushrooms and flowers
            if (isGeneratingPlants) await GenerateObj(island, data.NumberOfPlants.x, data.NumberOfPlants.y, data.Plants, data);
            
            // Grass
            if (data.SpawnGrass && isGeneratingGrass) await GenerateGrass(island, data.Grass[0], data);

            island.hasGeneratedProps = true;
            Debug.Log("A " + data.Type + " island have been generated at " + island.transform.position);
            
            island.FogOfWarInit();
        }

        private async Task GenerateObj(BaseIsland island, int numberOfObjX, int numberOfObjY,
            GameObject[] obj, IslandData data)
        {
            int r = Random.Range(numberOfObjX, numberOfObjY);
            
            for (int i = 0; i < r; i++)
            {              
                int x = Random.Range(0, obj.Length);
                GameObject selectedObj = obj[x];
                
               await SpawnObjOnIsland(selectedObj, island, data);
            }
        }

        private Task GenerateGrass(BaseIsland island, GameObject grassObj, IslandData data)
        {
            Texture2D texture = GenerateNoiseTexture();
            perlinNoiseRenderer.material.mainTexture = texture;
            
            int width = texture.width;
            int height = texture.height;
            
            BoxCollider islandCollider = island.GetComponent<BoxCollider>();
            float boxColWidth = islandCollider.bounds.size.x;
            float boxColHeight = islandCollider.bounds.size.z; // not proper "height", but "height" in top down view

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
                        
                        Vector3 islandPos = island.transform.position;

                        float x = normalizedU * boxColWidth + islandPos.x - boxColWidth/2;
                        float y = normalizedV * boxColHeight + islandPos.z - boxColHeight/2;
                        
                        Vector3 spawnPosition = new Vector3(x , islandPos.y, y);

                        GameObject newGrassObj = Instantiate(grassObj, spawnPosition, Quaternion.identity);
                        newGrassObj.transform.parent = island.graphObject.transform;

                        if (!Physics.Raycast(newGrassObj.transform.position + new Vector3(0,5,0), Vector3.down, 
                                50, terrainLayer, QueryTriggerInteraction.Ignore))
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
            
            return Task.CompletedTask;
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

        private async Task SpawnObjOnIsland(GameObject obj, BaseIsland island, IslandData data)
        {
            Vector3 objPos = FindPosOverIsland(island);

            GameObject newObj = Instantiate(obj, objPos, Quaternion.identity);
            newObj.transform.parent = island.graphObject.transform;
            
            var newObjRot = newObj.transform.rotation;
            newObjRot = Quaternion.Euler(newObjRot.x, Random.Range(0f,179f), newObjRot.z);
            newObj.transform.rotation = newObjRot;

            MeshRenderer meshRenderer = newObj.GetComponent<MeshRenderer>();
                    
            foreach (var mat in meshRenderer.materials)
            { 
                if (mat.shader.name == "Polytope Studio/PT_Vegetation_Foliage_Shader") 
                { 
                    mat.SetColor(ShaderTopColor, data.TopColor); 
                    mat.SetColor(ShaderGroundColor, data.GroundColor);
                }
            }

            MeshFilter newObjMeshFilter = newObj.GetComponent<MeshFilter>();

            if (!IsMeshOverlappingCompletely(newObjMeshFilter.mesh, newObj.transform.position))
            {
               await MoveObjOverIsland(newObj.transform, newObjMeshFilter, island);
            }
            else
            {
                var objNewPos = newObj.transform.position;
                objNewPos = new Vector3(objNewPos.x, island.transform.position.y, objNewPos.z);
                newObj.transform.position = objNewPos;
            }
        }

        private Vector3 FindPosOverIsland(BaseIsland island)
        {
            BoxCollider islandCollider = island.GetComponent<BoxCollider>();
            float boxColWidth = islandCollider.bounds.size.x;
            float boxColHeight = islandCollider.bounds.size.z; // not proper "height", but "height" in top down view
            
            Vector3 pos = island.graphObject.transform.position;

            Vector2 center = new Vector2(pos.x, pos.z);
            
            Vector2 obj2DPos = CustomHelper.GenerateRandomPosIn2DArea(center, boxColHeight, boxColWidth);
            
            return new Vector3(obj2DPos.x, 20, obj2DPos.y);
        }

        private Task MoveObjOverIsland(Transform obj, MeshFilter objMeshFilter, BaseIsland island)
        {
            int i = 0;
            
            while (true)
            {
                obj.transform.position = FindPosOverIsland(island);

                if (i > 500)
                {
                    Debug.LogError("Anti-Crash Stop");
                    break;
                }

                if (!IsMeshOverlappingCompletely(objMeshFilter.mesh, obj.transform.position))
                {
                    i++;
                    continue;
                }
                
                var objNewPos = obj.position;
                objNewPos = new Vector3(objNewPos.x, island.transform.position.y, objNewPos.z);
                obj.position = objNewPos;
                break;
            }
            
            return Task.CompletedTask;
        }

        private bool IsMeshOverlappingCompletely(Mesh mesh, Vector3 islandPos)
        {
            foreach (Vector3 vertex in mesh.vertices)
            {
                Vector3 worldVertex = islandPos + vertex;
                
                if (!Physics.Raycast(worldVertex, Vector3.down, 
                        50,  1 << 7, QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(worldVertex, Vector3.down * 50, Color.red, 100);
                    return false; 
                }
                
                Debug.DrawRay(worldVertex, Vector3.down * 50, Color.green, 100);
            }


            return true; 
        }
    }
}