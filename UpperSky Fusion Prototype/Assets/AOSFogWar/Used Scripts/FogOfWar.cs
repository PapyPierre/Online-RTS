/*
 * ORIGINAL SCRIPT :
 * 
 * Created :    Winter 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    FogWar
 * Filename :   csHomebrewFogWar.cs (non-static monobehaviour module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 *
 * HAVE BEEN MODIFIED BY PIERRE FERRARI
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AOSFogWar.Used_Scripts
{
    /// The non-static high-level monobehaviour interface of the AOS Fog of War module.

    /// This class holds serialized data for various configuration properties,\n
    /// and is resposible for scanning / saving / loading the LevelData object.\n
    /// The class handles the update frequency of the fog, plus some shader businesses.\n
    /// Various public interfaces related to FogRevealer's FOV are also available.
    public class FogOfWar : MonoBehaviour
    {
        public static FogOfWar Instance;
        
        /// A class for storing the base level data.
        /// 
        /// This class is later serialized into Json format.\n
        /// Empty spaces are stored as 0, while the obstacles are stored as 1.\n
        /// If a level is loaded instead of being scanned, 
        /// the level dimension properties of csFogWar will be replaced by the level data.
        [Serializable]
        public class LevelData
        {
            public void AddColumn(LevelColumn levelColumn)
            {
                levelRow.Add(levelColumn);
            }

            // Indexer definition
            public LevelColumn this[int index] {
                get {
                    if (index >= 0 && index < levelRow.Count)
                    {
                        return levelRow[index];
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in x axis is out of range");

                        return null;
                    }
                }
                set {
                    if (index >= 0 && index < levelRow.Count)
                    {
                        levelRow[index] = value;
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in x axis is out of range");
                    }
                }
            }

            // Adding private getter / setters are not allowed for serialization
            public int levelDimensionX;
            public int levelDimensionY;

            public float unitScale;

            public float scanSpacingPerUnit;

            [SerializeField]
            private List<LevelColumn> levelRow = new ();
        }

        [Serializable]
        public class LevelColumn
        {
            public LevelColumn(IEnumerable<ETileState> stateTiles)
            {
                levelColumn = new List<ETileState>(stateTiles);
            }

            // If I create a separate Tile class, it will impact the size of the save file (but enums will be saved as int)
            public enum ETileState
            {
                Empty,
                Obstacle
            }

            // Indexer definition
            public ETileState this[int index] {
                get {
                    if (index >= 0 && index < levelColumn.Count)
                    {
                        return levelColumn[index];
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in y axis is out of range");

                        return ETileState.Empty;
                    }
                }
                set {
                    if (index >= 0 && index < levelColumn.Count)
                    {
                        levelColumn[index] = value;
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in y axis is out of range");
                    }
                }
            }

            [SerializeField] private List<ETileState> levelColumn;
        }
        
        [Serializable]
        public class FogRevealer
        {
            public FogRevealer(Transform revealerTransform, int sightRange, bool updateOnlyOnMove)
            {
                this.revealerTransform = revealerTransform;
                this.sightRange = sightRange;
                this.updateOnlyOnMove = updateOnlyOnMove;
            }

            public Vector2Int GetCurrentLevelCoordinates(FogOfWar fogOfWar)
            {
                var position = revealerTransform.position;
                _currentLevelCoordinates = new Vector2Int(fogOfWar.GetUnitX(position.x), fogOfWar.GetUnitY(position.z));

                return _currentLevelCoordinates;
            }
            
            // To be assigned manually by the user
            [SerializeField] private Transform revealerTransform;
            // These are called expression-bodied properties btw, being stricter here because these are not pure data containers
            public Transform RevealerTransform => revealerTransform;

            [SerializeField] private int sightRange;
            public int SightRange => sightRange;

            [SerializeField] private bool updateOnlyOnMove;
            public bool UpdateOnlyOnMove => updateOnlyOnMove;

            private Vector2Int _currentLevelCoordinates;
            public Vector2Int CurrentLevelCoordinates {
                get {
                    lastSeenAt = _currentLevelCoordinates;

                    return _currentLevelCoordinates;
                }
            }

            [Header("Debug")]
            [SerializeField]
            private Vector2Int lastSeenAt = new Vector2Int(Int32.MaxValue, Int32.MaxValue);
            public Vector2Int LastSeenAt => lastSeenAt;
        }

        [BigHeader("Basic Properties")]
        public List<FogRevealer> fogRevealers;
        public List<FogRevealer> FogRevealers => fogRevealers;
        [SerializeField] private Transform levelMidPoint;
        public Transform LevelMidPoint => levelMidPoint;
        [SerializeField, Range(1, 30)] private float fogRefreshRate = 10;

        [BigHeader("Fog Properties")]
        [SerializeField, Range(0, 100)] private float fogPlaneHeight = 1;
        [SerializeField] private bool updateFogPlaneHeightInRealTime;
        [SerializeField] private Material fogPlaneMaterial;
        [SerializeField] private Color fogColor = new Color32(5, 15, 25, 255);
        [SerializeField, Range(0, 1)] private float fogPlaneAlpha = 1;
        [SerializeField, Range(0, 5)] private float fogLerpSpeed = 2.5f;

        [Header("Debug")]
        [SerializeField] private Texture2D fogPlaneTextureLerpTarget;
        [SerializeField] private Texture2D fogPlaneTextureLerpBuffer;
        
        [BigHeader("Level Data")]
        [SerializeField] private TextAsset levelDataToLoad;
        [SerializeField] private bool saveDataOnScan = true;
        [ShowIf("saveDataOnScan"), SerializeField] private string levelNameToSave = "Default";

        [BigHeader("Scan Properties")]
        [SerializeField, Range(1, 500)] private int levelDimensionX = 11;
        [SerializeField, Range(1, 500)] private int levelDimensionY = 11;
        [SerializeField] private float unitScale = 1; 
        public float UnitScale => unitScale;
        [SerializeField] private float scanSpacingPerUnit = 0.25f;
        [SerializeField] private float rayStartHeight = 5;
        [SerializeField] private float rayMaxDistance = 10;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private bool ignoreTriggers = true;

        [BigHeader("Debug Options")]
        [SerializeField] private bool drawGizmos;
        [SerializeField] private bool logOutOfRange;

        // External shadowcaster module
        public Shadowcaster Shadowcaster { get; private set; } = new ();

        public LevelData levelData { get; private set; } = new ();

        // The primitive plane which will act as a mesh for rendering the fog with
        private GameObject _fogPlane;
        private MeshRenderer _fogPlaneMeshRenderer;

        private float _fogRefreshRateTimer;
        private static readonly int FogPlaneMatColorNameID = Shader.PropertyToID("_Color");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private const string LevelScanDataPath = "/LevelData";

        // --- --- ---
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(name);
                return;
            }
        
            Instance = this;
        }

        private void Start()
        {
            CheckProperties();

            InitializeVariables();

            if (levelDataToLoad == null)
            {
                ScanLevel();

                if (saveDataOnScan)
                {
                    // Preprocessor definitions are used because the save function code will be stripped out on build
                    #if UNITY_EDITOR
                    SaveScanAsLevelData();
                    #endif
                }
            }
            else
            {
                LoadLevelData();
            }

            InitializeFog();

            // This part passes the needed references to the shadowcaster
            Shadowcaster.Initialize(this);

            // This is needed because we do not update the fog when there's no unit-scale movement of each fogRevealer
            ForceUpdateFog();
        }
        
        private void Update()
        {
            UpdateFog();
        }
        
        // --- --- ---

        private void CheckProperties()
        {
            foreach (FogRevealer fogRevealer in fogRevealers)
            {
                if (fogRevealer.RevealerTransform == null)
                {
                    Debug.LogErrorFormat("Please assign a Transform component to each Fog Revealer!");
                }
            }

            if (unitScale <= 0)
            {
                Debug.LogErrorFormat("Unit Scale must be bigger than 0!");
            }

            if (scanSpacingPerUnit <= 0)
            {
                Debug.LogErrorFormat("Scan Spacing Per Unit must be bigger than 0!");
            }

            if (levelMidPoint == null)
            {
                Debug.LogErrorFormat("Please assign the Level Mid Point property!");
            }

            if (fogPlaneMaterial == null)
            {
                Debug.LogErrorFormat("Please assign the \"FogPlane\" material to the Fog Plane Material property!");
            }
        }
        
        private void InitializeVariables()
        {
            // This is for faster development iteration purposes
            if (obstacleLayers.value == 0)
            {
                obstacleLayers = LayerMask.GetMask("Default");
            }

            // This is also for faster development iteration purposes
            if (levelNameToSave == String.Empty)
            {
                levelNameToSave = "Default";
            }
        }

        private void InitializeFog()
        {
            _fogPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _fogPlaneMeshRenderer = _fogPlane.GetComponent<MeshRenderer>();

            _fogPlane.name = "[RUNTIME] Fog_Plane";

            var position = levelMidPoint.position;
            _fogPlane.transform.position = new Vector3(position.x, position.y + fogPlaneHeight, position.z);

            _fogPlane.transform.localScale = new Vector3(
                (levelDimensionX * unitScale) / 10.0f,
                1,
                (levelDimensionY * unitScale) / 10.0f);

            fogPlaneTextureLerpTarget = new Texture2D(levelDimensionX, levelDimensionY);
            fogPlaneTextureLerpBuffer = new Texture2D(levelDimensionX, levelDimensionY);

            fogPlaneTextureLerpBuffer.wrapMode = TextureWrapMode.Clamp;

            fogPlaneTextureLerpBuffer.filterMode = FilterMode.Bilinear;

            _fogPlane.GetComponent<MeshRenderer>().material = new Material(fogPlaneMaterial);

            _fogPlane.GetComponent<MeshRenderer>().material.SetTexture(MainTex, fogPlaneTextureLerpBuffer);

            _fogPlane.GetComponent<MeshCollider>().enabled = false;
        }

        private void ForceUpdateFog()
        {
            UpdateFogField();

            Graphics.CopyTexture(fogPlaneTextureLerpTarget, fogPlaneTextureLerpBuffer);
        }

        private void UpdateFog()
        {
            if (updateFogPlaneHeightInRealTime)
            {
                var position = levelMidPoint.position;
                _fogPlane.transform.position = new Vector3(position.x, position.y + fogPlaneHeight, position.z);
            }
            
            _fogRefreshRateTimer += Time.deltaTime;

            if (_fogRefreshRateTimer < 1 / fogRefreshRate)
            {
                UpdateFogPlaneTextureBuffer();

                return;
            }
            else
            {
                // This is to cancel out minor excess values
                _fogRefreshRateTimer -= 1 / fogRefreshRate;
            }

            foreach (FogRevealer fogRevealer in fogRevealers)
            {
                if (fogRevealer.UpdateOnlyOnMove == false) break;

                Vector2Int currentLevelCoordinates = fogRevealer.GetCurrentLevelCoordinates(this);

                if (currentLevelCoordinates != fogRevealer.LastSeenAt)
                {
                    break;
                }

                if (fogRevealer == fogRevealers.Last())
                {
                    return;
                }
            }

            UpdateFogField();

            UpdateFogPlaneTextureBuffer();
        }
        
        private void UpdateFogField()
        {
            Shadowcaster.ResetTileVisibility();

            foreach (FogRevealer fogRevealer in fogRevealers)
            {
                fogRevealer.GetCurrentLevelCoordinates(this);

                Shadowcaster.ProcessLevelData(
                    fogRevealer.CurrentLevelCoordinates,
                    Mathf.RoundToInt(fogRevealer.SightRange / unitScale));
            }

            UpdateFogPlaneTextureTarget();
        }
        
        // Doing shader business on the script, if we pull this out as a shader pass, same operations must be repeated
        private void UpdateFogPlaneTextureBuffer()
        {
            for (int xIterator = 0; xIterator < levelDimensionX; xIterator++)
            {
                for (int yIterator = 0; yIterator < levelDimensionY; yIterator++)
                {
                    Color bufferPixel = fogPlaneTextureLerpBuffer.GetPixel(xIterator, yIterator);
                    Color targetPixel = fogPlaneTextureLerpTarget.GetPixel(xIterator, yIterator);

                    if (fogLerpSpeed > 0)
                    {
                        fogPlaneTextureLerpBuffer.SetPixel(xIterator, yIterator, Color.Lerp(
                            bufferPixel,
                            targetPixel,
                            fogLerpSpeed * Time.deltaTime));
                    }
                }
            }

            fogPlaneTextureLerpBuffer.Apply();
        }

        private void UpdateFogPlaneTextureTarget()
        {
            _fogPlaneMeshRenderer.material.SetColor(FogPlaneMatColorNameID, fogColor);

            fogPlaneTextureLerpTarget.SetPixels(Shadowcaster.fogField.GetColors(fogPlaneAlpha));

            fogPlaneTextureLerpTarget.Apply();
        }

        private void ScanLevel()
        {
            Debug.LogFormat("There is no level data file assigned, scanning level...");

            // These operations have no real computational meaning, but it will bring consistency to the data
            levelData.levelDimensionX = levelDimensionX;
            levelData.levelDimensionY = levelDimensionY;
            levelData.unitScale = unitScale;
            levelData.scanSpacingPerUnit = scanSpacingPerUnit;

            for (int xIterator = 0; xIterator < levelDimensionX; xIterator++)
            {
                // Adding a new list for column (y axis) for each unit in row (x axis)
                levelData.AddColumn(new LevelColumn(Enumerable.Repeat(LevelColumn.ETileState.Empty, levelDimensionY)));

                for (int yIterator = 0; yIterator < levelDimensionY; yIterator++)
                {
                    bool isObstacleHit = Physics.BoxCast(
                        new Vector3(
                            GetWorldX(xIterator),
                            levelMidPoint.position.y + rayStartHeight,
                            GetWorldY(yIterator)),
                        new Vector3(
                            (unitScale - scanSpacingPerUnit) / 2.0f,
                            unitScale / 2.0f,
                            (unitScale - scanSpacingPerUnit) / 2.0f),
                        Vector3.down,
                        Quaternion.identity,
                        rayMaxDistance,
                        obstacleLayers,
                        (QueryTriggerInteraction)(2 - Convert.ToInt32(ignoreTriggers)));

                    if (isObstacleHit)
                    {
                        levelData[xIterator][yIterator] = LevelColumn.ETileState.Obstacle;
                    }
                }
            }

            Debug.LogFormat("Successfully scanned level with a scale of {0} x {1}", levelDimensionX, levelDimensionY);
        }

        // We intend to use Application.dataPath only for accessing project files directory (only in unity editor)
        #if UNITY_EDITOR
        private void SaveScanAsLevelData()
        {
            string fullPath = Application.dataPath + LevelScanDataPath + "/" + levelNameToSave + ".json";

            if (Directory.Exists(Application.dataPath + LevelScanDataPath) == false)
            {
                Directory.CreateDirectory(Application.dataPath + LevelScanDataPath);

                Debug.LogFormat("level scan data folder at \"{0}\" is missing, creating...", LevelScanDataPath);
            }

            if (File.Exists(fullPath))
            {
                Debug.LogFormat("level scan data already exists, overwriting...");
            }

            string levelJson = JsonUtility.ToJson(levelData);

            File.WriteAllText(fullPath, levelJson);

            Debug.LogFormat("Successfully saved level scan data at \"{0}\"", fullPath);
        }
        #endif
        
        private void LoadLevelData()
        {
            Debug.LogFormat("Level scan data with a name of \"{0}\" is assigned, loading...", levelDataToLoad.name);

            // Exception check is indirectly performed through branching on the upper part of the code
            string levelJson = levelDataToLoad.ToString();

            levelData = JsonUtility.FromJson<LevelData>(levelJson);

            levelDimensionX = levelData.levelDimensionX;
            levelDimensionY = levelData.levelDimensionY;
            unitScale = levelData.unitScale;
            scanSpacingPerUnit = levelData.scanSpacingPerUnit;

            Debug.LogFormat("Successfully loaded level scan data with the name of \"{0}\"", levelDataToLoad.name);
        }
        
        /// Adds a new FogRevealer instance to the list and returns its index
        public int AddFogRevealer(FogRevealer fogRevealer)
        {
            fogRevealers.Add(fogRevealer);

            return fogRevealers.Count - 1;
        }

        /// Removes a FogRevealer instance from the list with index
        public void RemoveFogRevealer(int revealerIndex)
        {
            if (fogRevealers.Count > revealerIndex && revealerIndex > -1)
            {
                fogRevealers.RemoveAt(revealerIndex);
            }
            else
            {
                Debug.LogFormat("Given index of {0} exceeds the revealers' container range", revealerIndex);
            }
        }

        /*
        /// Replaces the FogRevealer list with the given one
        public void ReplaceFogRevealerList(List<FogRevealer> fogRevealers)
        {
            this.fogRevealers = fogRevealers;
        }
        */
        
        /// Checks if the given level coordinates are within level dimension range.
        public bool CheckLevelGridRange(Vector2Int levelCoordinates)
        {
            bool result =
                levelCoordinates.x >= 0 &&
                levelCoordinates.x < levelData.levelDimensionX &&
                levelCoordinates.y >= 0 &&
                levelCoordinates.y < levelData.levelDimensionY;

            if (result == false && logOutOfRange)
            {
                Debug.LogFormat("Level coordinates \"{0}\" is out of grid range", levelCoordinates);
            }

            return result;
        }

        /// Checks if the given world coordinates are within level dimension range.
        public bool CheckWorldGridRange(Vector3 worldCoordinates)
        {
            Vector2Int levelCoordinates = WorldToLevel(worldCoordinates);

            return CheckLevelGridRange(levelCoordinates);
        }

        /// Checks if the given pair of world coordinates and additionalRadius is visible by FogRevealers.
        public bool CheckVisibility(Vector3 worldCoordinates, int additionalRadius)
        {
            Vector2Int levelCoordinates = WorldToLevel(worldCoordinates);

            if (additionalRadius == 0)
            {
                return Shadowcaster.fogField[levelCoordinates.x][levelCoordinates.y] == 
                    Shadowcaster.LevelColumn.ETileVisibility.Revealed;
            }

            int scanResult = 0;

            for (int xIterator = -1; xIterator < additionalRadius + 1; xIterator++)
            {
                for (int yIterator = -1; yIterator < additionalRadius + 1; yIterator++)
                {
                    if (CheckLevelGridRange(new Vector2Int(
                        levelCoordinates.x + xIterator, 
                        levelCoordinates.y + yIterator)) == false)
                    {
                        scanResult = 0;

                        break;
                    }

                    scanResult += Convert.ToInt32(
                        Shadowcaster.fogField[levelCoordinates.x + xIterator][levelCoordinates.y + yIterator] == 
                        Shadowcaster.LevelColumn.ETileVisibility.Revealed);
                }
            }

            if (scanResult > 0)
            {
                return true;
            }

            return false;
        }
        
        /// Converts unit (divided by unitScale, then rounded) world coordinates to level coordinates.
        public Vector2Int WorldToLevel(Vector3 worldCoordinates)
        {
            Vector2Int unitWorldCoordinates = GetUnitVector(worldCoordinates);

            return new Vector2Int(
                unitWorldCoordinates.x + (levelDimensionX / 2),
                unitWorldCoordinates.y + (levelDimensionY / 2));
        }

        /// Converts level coordinates into world coordinates.
        public Vector3 GetWorldVector(Vector2Int worldCoordinates)
        {
            return new Vector3(
                GetWorldX(worldCoordinates.x + (levelDimensionX / 2)), 
                0, 
                GetWorldY(worldCoordinates.y + (levelDimensionY / 2)));
        }
        
        /// Converts "pure" world coordinates into unit world coordinates.
        public Vector2Int GetUnitVector(Vector3 worldCoordinates)
        {
            return new Vector2Int(GetUnitX(worldCoordinates.x), GetUnitY(worldCoordinates.z));
        }
        
        /// Converts level coordinate to corresponding unit world coordinates.
        public float GetWorldX(int xValue)
        {
            if (levelData.levelDimensionX % 2 == 0)
            {
                return (levelMidPoint.position.x - ((levelDimensionX / 2.0f) - xValue) * unitScale);
            }

            return (levelMidPoint.position.x - ((levelDimensionX / 2.0f) - (xValue + 0.5f)) * unitScale);
        }

        /// Converts world coordinate to unit world coordinates.
        public int GetUnitX(float xValue)
        {
            return Mathf.RoundToInt((xValue - levelMidPoint.position.x) / unitScale);
        }

        /// Converts level coordinate to corresponding unit world coordinates.
        public float GetWorldY(int yValue)
        {
            if (levelData.levelDimensionY % 2 == 0)
            {
                return (levelMidPoint.position.z - ((levelDimensionY / 2.0f) - yValue) * unitScale);
            }

            return (levelMidPoint.position.z - ((levelDimensionY / 2.0f) - (yValue + 0.5f)) * unitScale);
        }

        /// Converts world coordinate to unit world coordinates.
        public int GetUnitY(float yValue)
        {
            return Mathf.RoundToInt((yValue - levelMidPoint.position.z) / unitScale);
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (drawGizmos == false)
            {
                return;
            }

            Handles.color = Color.yellow;

            for (int xIterator = 0; xIterator < levelDimensionX; xIterator++)
            {
                for (int yIterator = 0; yIterator < levelDimensionY; yIterator++)
                {
                    if (levelData[xIterator][yIterator] == LevelColumn.ETileState.Obstacle)
                    {
                        if (Shadowcaster.fogField[xIterator][yIterator] == Shadowcaster.LevelColumn.ETileVisibility.Revealed)
                        {
                            Handles.color = Color.green;
                        }
                        else
                        {
                            Handles.color = Color.red;
                        }

                        Handles.DrawWireCube(
                            new Vector3(
                                GetWorldX(xIterator),
                                levelMidPoint.position.y,
                                GetWorldY(yIterator)),
                            new Vector3(
                                unitScale - scanSpacingPerUnit,
                                unitScale,
                                unitScale - scanSpacingPerUnit));
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;

                        Gizmos.DrawSphere(
                            new Vector3(
                                GetWorldX(xIterator),
                                levelMidPoint.position.y,
                                GetWorldY(yIterator)),
                            unitScale / 5.0f);
                    }

                    if (Shadowcaster.fogField[xIterator][yIterator] == Shadowcaster.LevelColumn.ETileVisibility.Revealed)
                    {
                        Gizmos.color = Color.green;

                        Gizmos.DrawSphere(
                            new Vector3(
                                GetWorldX(xIterator),
                                levelMidPoint.position.y,
                                GetWorldY(yIterator)),
                            unitScale / 3.0f);
                    }
                }
            }
        }
        #endif
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string BaseCondition => _mBaseCondition;

        private string _mBaseCondition;

        public ShowIfAttribute(string baseCondition)
        {
            _mBaseCondition = baseCondition;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BigHeaderAttribute : PropertyAttribute
    {
        public string Text {
            get { return _mText; }
        }

        private string _mText;

        public BigHeaderAttribute(string text)
        {
            _mText = text;
        }
    }
}