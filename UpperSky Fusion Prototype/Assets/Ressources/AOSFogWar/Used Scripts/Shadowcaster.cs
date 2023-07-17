/*
 * ORIGINAL SCRIPT :
 * 
 * Created :    Winter 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    FogWar
 * Filename :   Shadowcaster.cs (non-static module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 *
 * HAVE BEEN MODIFIED BY PIERRE FERRARI
 */

/*
 * This part of the code utilizes the algorithm known as "Shadowcasting", the FOV algorithm of retro Roguelikes.
 * http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html
 * The subject is wonderfully explained in depth inside the above link.
 * https://www.albertford.com/shadowcasting
 * By the way, this script is based on the above version of the implementation. Be sure to check it out!
 */


using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AOSFogWar.Used_Scripts
{
    /// A non-static submodule that performs shadowcasting algorithm over a LevelData object.

    /// An instance of this module is instantiated for each construction of the csFogWar object.\n
    /// While the LevelData class defined within csFogWar class holds the information about obstacles,\n
    /// the FogField class defined within Shadowcaster class holds the information about player sight.
    
    public class Shadowcaster
    {
        /// A class that holds visibility data updated based on the FOV.
        /// 
        /// This class is only instantiated and managed by a single Shadowcaster object,\n
        /// and the object only lasts per session, unlike the serialized LevelData of csFogWar.\n
        /// This class also has the GetColors() method, which returns the actual texture data in a 1D Color array format. 
        public class FogField
        {
            public void AddColumn(LevelColumn levelColumn)
            {
                _levelRow.Add(levelColumn);
            }

            public void Reset()
            {
                foreach (LevelColumn levelColumn in _levelRow)
                {
                    levelColumn.Reset();
                }
            }

            public Color[] GetColors(float fogPlaneAlpha)
            {
                _colors ??= new Color[_levelRow.Count * _levelRow[0].Count()];

                for (int xIterator = 0; xIterator < _levelRow[0].Count(); xIterator++)
                {
                    for (int yIterator = 0; yIterator < _levelRow.Count; yIterator++)
                    {
                        int visibility = (int)_levelRow[yIterator][_levelRow[0].Count() - 1 - xIterator];

                        // The reason that the darker side is the revealed ones is to let users customize fog's color
                        _colors[_levelRow.Count() * (xIterator + 1) - (yIterator + 1)] =
                        new Color(
                            1 - visibility,
                            1 - visibility,
                            1 - visibility,
                            (1 - visibility) * fogPlaneAlpha);
                    }
                }

                return _colors;
            }

            // Indexer definition
            public LevelColumn this[int index] {
                get {
                    if (index >= 0 && index < _levelRow.Count)
                    {
                        return _levelRow[index];
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in x axis is out of range");

                        return null;
                    }
                }
                set {
                    if (index >= 0 && index < _levelRow.Count)
                    {
                        _levelRow[index] = value;
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in x axis is out of range");
                    }
                }
            }

            private List<LevelColumn> _levelRow = new ();

            // To be initialized with the dependant initialization function call
            private Color[] _colors;
        }

        public class LevelColumn
        {
            public LevelColumn(IEnumerable<ETileVisibility> visibilityTiles)
            {
                _levelColumn = new List<ETileVisibility>(visibilityTiles);
            }

            // This is separated from the LevelColumn class of csHomebrewOfWar due to the size of the level data file
            public enum ETileVisibility
            {
                Hidden,
                Revealed
            }

            public void Reset()
            {
                for (int i = 0; i < _levelColumn.Count; i++)
                {
                    _levelColumn[i] = ETileVisibility.Hidden;
                }
            }

            public int Count()
            {
                return _levelColumn.Count;
            }

            // Indexer definition
            public ETileVisibility this[int index] {
                get {
                    if (index >= 0 && index < _levelColumn.Count)
                    {
                        return _levelColumn[index];
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in y axis is out of range");

                        return ETileVisibility.Hidden;
                    }
                }
                set {
                    if (index >= 0 && index < _levelColumn.Count)
                    {
                        _levelColumn[index] = value;
                    }
                    else
                    {
                        Debug.LogErrorFormat("index given in y axis is out of range");
                    }
                }
            }

            private List<ETileVisibility> _levelColumn;
        }



        /// An iterator that transforms coordinates based on a single quadrant to all the others.
        /// 
        /// For an explanation in depth, refer to the link below.\n
        /// https://www.albertford.com/shadowcasting
        private class QuadrantIterator
        {
            public QuadrantIterator(FogOfWar fogOfWar)
            {
                this._fogOfWar = fogOfWar;
            }

            public enum ECardinal
            {
                East,
                North,
                West,
                South
            }

            public Vector2Int QuadrantToLevel(Vector2Int quadrantVector)
            {
                Vector2Int quadrantPoint;

                switch (Cardinal)
                {
                    default:
                    case ECardinal.East:
                        quadrantPoint = new Vector2Int(OriginPoint.x + quadrantVector.x, OriginPoint.y + quadrantVector.y);
                        break;
                    case ECardinal.North:
                        quadrantPoint = new Vector2Int(OriginPoint.x - quadrantVector.y, OriginPoint.y + quadrantVector.x);
                        break;
                    case ECardinal.West:
                        quadrantPoint =  new Vector2Int(OriginPoint.x - quadrantVector.x, OriginPoint.y - quadrantVector.y);
                        break;
                    case ECardinal.South:
                        quadrantPoint = new Vector2Int(OriginPoint.x + quadrantVector.y, OriginPoint.y - quadrantVector.x);
                        break;
                }

                return _fogOfWar.WorldToLevel(_fogOfWar.GetWorldVector(quadrantPoint));
            }

            public ECardinal Cardinal { get; set; } = ECardinal.East;

            public Vector2Int OriginPoint { get; set; }

            // To be initialized within the constructor, needed for reference
            private FogOfWar _fogOfWar;
        }



        private class ColumnIterator
        {
            public ColumnIterator(int depth, int maxDepth, float startSlope, float endSlope)
            {
                this.Depth = depth;
                this.MaxDepth = maxDepth;
                this.StartSlope = startSlope;
                this.EndSlope = endSlope;
            }

            public List<Vector2Int> GetTiles()
            {
                List<Vector2Int> quadrantPoints = new List<Vector2Int>();

                int minRow = Mathf.RoundToInt(Depth * StartSlope);
                int maxRow = Mathf.RoundToInt(Depth * EndSlope);

                for (int i = minRow; i < maxRow + 1; i++)
                {
                    quadrantPoints.Add(new Vector2Int(Depth, i));
                }

                if (EndSlope is 1)
                {
                    quadrantPoints.RemoveAt(quadrantPoints.Count - 1);
                }

                return quadrantPoints;
            }

            public bool IsProceedable()
            {
                return (Depth < MaxDepth);
            }

            public void ProceedIfPossible()
            {
                if (Depth < MaxDepth)
                {
                    Depth += 1;
                }
            }

            // In my implementaion, we consider the depth as the x axis of the eastern quadrant
            public int Depth { get; private set; }
            public int MaxDepth { get; private set; }

            // The variable startSlope is the 'lower' one, and the endSlope is the 'higher' one
            public float StartSlope { get; set; }
            public float EndSlope { get; set; }
        }



        // Parent manager module, passed with initialization
        private FogOfWar _fogOfWar;

        public FogField fogField { get; private set; } = new ();

        // We declare this here to prevent creating and destroying the same object over and over
        private QuadrantIterator _quadrantIterator;



        /// Initializes the shadowcaster module with properties from the dependant csFogWar object.
        public void Initialize(FogOfWar fogOfWar)
        {
            this._fogOfWar = fogOfWar;

            for (int xIterator = 0; xIterator < fogOfWar.levelData.levelDimensionX; xIterator++)
            {
                // Adding a new list for column (y axis) for each unit in row (x axis)
                fogField.AddColumn(new LevelColumn(
                    Enumerable.Repeat(LevelColumn.ETileVisibility.Hidden, fogOfWar.levelData.levelDimensionY)));
            }

            _quadrantIterator = new QuadrantIterator(fogOfWar);
        }



        /// Resets all tile's visibility info.
        public void ResetTileVisibility()
        {
            fogField.Reset();
        }



        /// Processes the level data with shadowcasting algorithm, and updates the FogField object accordingly
        public void ProcessLevelData(Vector2Int revealerPoint, int sightRange)
        {
            // Reveal the first tile where the revealer is at
            RevealTile(_fogOfWar.WorldToLevel(_fogOfWar.GetWorldVector(revealerPoint)));

            // Give the quadrant iterator the revealer's position
            _quadrantIterator.OriginPoint = revealerPoint;

            // We deal with 90 degrees each, anti-clockwise, starting from east to west
            foreach (int cardinal in System.Enum.GetValues(typeof(QuadrantIterator.ECardinal)))
            {
                _quadrantIterator.Cardinal = (QuadrantIterator.ECardinal)cardinal;

                // Here goes the BFS algorithm, we queue a new pass during each pass if needed, then start the new one
                Queue<ColumnIterator> columnIterators = new Queue<ColumnIterator>();

                // The first pass of the given quadrant, start from slope -1 to slope 1
                columnIterators.Enqueue(new ColumnIterator(1, sightRange, -1, 1));

                while (columnIterators.Count > 0)
                {
                    ColumnIterator columnIterator = columnIterators.Dequeue();

                    // Note that the given points may have negative y values instead of starting from zero
                    List<Vector2Int> quadrantPoints = columnIterator.GetTiles();

                    // This is to detect points where the obstacle tile and the empty tile are adjacent
                    Vector2Int lastQuadrantPoint = new Vector2Int();

                    // This is to skip the first pass where the lastQuadrantPoint variable is not assigned yet
                    bool firstStepFlag = true;

                    foreach (Vector2Int quadrantPoint in quadrantPoints)
                    {
                        if (IsTileObstacle(quadrantPoint) || IsTileVisible(columnIterator, quadrantPoint))
                        {
                            RevealTileIteratively(quadrantPoint, sightRange);
                        }

                        if (firstStepFlag == false)
                        {
                            if (IsTileObstacle(lastQuadrantPoint) && IsTileEmpty(quadrantPoint))
                            {
                                columnIterator.StartSlope = GetQuadrantSlope(quadrantPoint);
                            }

                            if (IsTileEmpty(lastQuadrantPoint) && IsTileObstacle(quadrantPoint))
                            {
                                if (columnIterator.IsProceedable() == false)
                                {
                                    continue;
                                }

                                ColumnIterator nextColumnIterator = new ColumnIterator(
                                    columnIterator.Depth,
                                    sightRange,
                                    columnIterator.StartSlope,
                                    GetQuadrantSlope(quadrantPoint));

                                nextColumnIterator.ProceedIfPossible();

                                columnIterators.Enqueue(nextColumnIterator);
                            }
                        }

                        lastQuadrantPoint = quadrantPoint;

                        firstStepFlag = false;
                    }

                    if (IsTileEmpty(lastQuadrantPoint) && columnIterator.IsProceedable())
                    {
                        columnIterator.ProceedIfPossible();

                        columnIterators.Enqueue(columnIterator);
                    }
                }
            }
        }



        private void RevealTileIteratively(Vector2Int quadrantPoint, int sightRange)
        {
            Vector2Int levelCoordinates = _quadrantIterator.QuadrantToLevel(quadrantPoint);

            if (_fogOfWar.CheckLevelGridRange(levelCoordinates) == false)
            {
                return;
            }

            if (quadrantPoint.magnitude > sightRange)
            {
                return;
            }

            fogField[levelCoordinates.x][levelCoordinates.y] = LevelColumn.ETileVisibility.Revealed;
        }



        private void RevealTile(Vector2Int levelCoordinates)
        {
            if (_fogOfWar.CheckLevelGridRange(levelCoordinates) == false)
            {
                return;
            }

            fogField[levelCoordinates.x][levelCoordinates.y] = LevelColumn.ETileVisibility.Revealed;
        }



        private bool IsTileEmpty(Vector2Int quadrantPoint)
        {
            Vector2Int levelCoordinates = _quadrantIterator.QuadrantToLevel(quadrantPoint);

            if (_fogOfWar.CheckLevelGridRange(levelCoordinates) == false)
            {
                return true;
            }

            return (_fogOfWar.levelData[levelCoordinates.x][levelCoordinates.y] == FogOfWar.LevelColumn.ETileState.Empty);
        }



        private bool IsTileObstacle(Vector2Int quadrantPoint)
        {
            Vector2Int levelCoordinates = _quadrantIterator.QuadrantToLevel(quadrantPoint);

            if (_fogOfWar.CheckLevelGridRange(levelCoordinates) == false)
            {
                return false;
            }

            return (_fogOfWar.levelData[levelCoordinates.x][levelCoordinates.y] == FogOfWar.LevelColumn.ETileState.Obstacle);
        }



        private bool IsTileVisible(ColumnIterator columnIterator, Vector2Int quadrantPoint)
        {
            return (quadrantPoint.y >= columnIterator.Depth * columnIterator.StartSlope) &&
                (quadrantPoint.y <= columnIterator.Depth * columnIterator.EndSlope);
        }



        private float GetQuadrantSlope(Vector2Int quadrantPoint)
        {
            // The reason that this is not simply y / x is that the wall is diamond-shaped, refer to the links at the top
            return (((quadrantPoint.y * 2) - 1) / ((float)quadrantPoint.x * 2));
        }
    }



}