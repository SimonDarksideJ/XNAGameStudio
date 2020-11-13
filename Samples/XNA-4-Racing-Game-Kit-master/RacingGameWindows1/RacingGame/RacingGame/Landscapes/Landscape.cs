#region File Description
//-----------------------------------------------------------------------------
// Landscape.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Shaders;
using RacingGame.Tracks;
using Model = RacingGame.Graphics.Model;
using RacingGame.GameLogic;
using RacingGame.GameScreens;
using RacingGame.Sounds;
using System.Threading;
#endregion

namespace RacingGame.Landscapes
{
    /// <summary>
    /// Landscape
    /// </summary>
    public class Landscape : IDisposable
    {
        #region Constants
        /// <summary>
        /// Grid width and height
        /// </summary>
        const int GridWidth = 257,
            GridHeight = 257;

        const float MapWidthFactor = 10,
            MapHeightFactor = 10,
            MapZScale = 300.0f;
        #endregion

        #region Objects to render on this landscape
        /// <summary>
        /// Landscape object
        /// </summary>
        public class LandscapeObject
        {
            /// <summary>
            /// Model
            /// </summary>
            Model model;
            /// <summary>
            /// Matrix
            /// </summary>
            Matrix matrix;
            /// <summary>
            /// Is banner, sign or building?
            /// Shadows are only generated for these objects, not received.
            /// </summary>
            bool isBanner = false;

            /// <summary>
            /// Change model
            /// </summary>
            /// <param name="setNewModel">Set new model</param>
            public void ChangeModel(Model setNewModel)
            {
                model = setNewModel;
            }

            /// <summary>
            /// Is big building
            /// </summary>
            /// <returns>Bool</returns>
            public bool IsBigBuilding
            {
                get
                {
                    return model.Name.ToLower().Contains("hotel") ||
                           model.Name.ToLower().Contains("building");
                }
            }

            /// <summary>
            /// Is banner
            /// </summary>
            public bool IsBanner
            {
                get
                {
                    return isBanner;
                }
            }

            /// <summary>
            /// Position
            /// </summary>
            /// <returns>Vector 3</returns>
            public Vector3 Position
            {
                get
                {
                    return matrix.Translation;
                }
            }

            /// <summary>
            /// Size
            /// </summary>
            /// <returns>Float</returns>
            public float Size
            {
                get
                {
                    return model.Size;
                }
            }

            /// <summary>
            /// Create landscape object
            /// </summary>
            /// <param name="setModel">Set model</param>
            /// <param name="setMatrix">Set matrix</param>
            public LandscapeObject(Model setModel, Matrix setMatrix)
            {
                if (setModel == null)
                    throw new ArgumentNullException("setModel");

                model = setModel;
                matrix = setMatrix;

                // Also include signs no reason to receive shadows for them!
                // Faster and looks better!
                isBanner = model.Name.ToLower().Contains("banner")
                    || model.Name.ToLower().Contains("sign");
            }

            /// <summary>
            /// Render
            /// </summary>
            public void Render()
            {
                model.Render(matrix);
            }

            /// <summary>
            /// Generate shadows
            /// </summary>
            public void GenerateShadows()
            {
                model.GenerateShadow(matrix);
            }

            /// <summary>
            /// Use shadows
            /// </summary>
            public void UseShadows()
            {
                model.UseShadow(matrix);
            }
        }

        /// <summary>
        /// List of landscape objects.
        /// </summary>
        List<LandscapeObject> landscapeObjects = new List<LandscapeObject>();

        /// <summary>
        /// Extra list for objects that are near the track, all the objects
        /// in this list are also in the landscapeObjects list. Usually this
        /// list is a lot smaller and it is used for the shadow mapping
        /// generation in GenerateShadow and UseShadow methods below.
        /// </summary>
        List<LandscapeObject> nearTrackObjects = new List<LandscapeObject>();

        /// <summary>
        /// Remember start light object because we will exchange it
        /// as the time goes down.
        /// </summary>
        LandscapeObject startLightObject = null;

        /// <summary>
        /// Replace start light object, 0=red, 1=yellow, 2=green.
        /// </summary>
        /// <param name="number">Number</param>
        public void ReplaceStartLightObject(int number)
        {
            // Make sure we only use 0-2
            if (number < 0 || number >= 3)
                number = 0;

            if (startLightObject != null)
            {
                if (number == 2)
                    Sound.Play(Sound.Sounds.Bleep);
                else
                    Sound.Play(Sound.Sounds.Beep);

                startLightObject.ChangeModel(landscapeModels[number]);
            }
        }

        /// <summary>
        /// Kill all loaded objects
        /// </summary>
        public void KillAllLoadedObjects()
        {
            landscapeObjects.Clear();
            nearTrackObjects.Clear();
            startLightObject = null;
        }

        /// <summary>
        /// All landscape models are preloaded and then used in AddObjectToRender.
        /// </summary>
        Model[] landscapeModels = new Model[]
            {
                new Model("StartLight"),
                new Model("StartLight2"),
                new Model("StartLight3"),
                new Model("Blockade"),
                new Model("Blockade2"),
                new Model("Hydrant"),
                new Model("Kaktus"),
                new Model("Kaktus2"),
                new Model("KaktusBenny"),
                new Model("KaktusSeg"),
                new Model("AlphaDeadTree"),
                new Model("AlphaPalm"),
                new Model("AlphaPalm2"),
                new Model("AlphaPalm3"),
                new Model("AlphaPalmSmall"),
                new Model("Laterne"),
                new Model("Laterne2Sides"),
                new Model("Trashcan"),
                new Model("Roadsign"),
                new Model("Roadsign2"),
                new Model("Goal"),
                new Model("Building"),
                new Model("Building2"),
                new Model("Building3"),
                new Model("Building4"),
                new Model("Building5"),
                new Model("OilPump"),
                new Model("OilTanks"),
                new Model("RoadColumnSegment"),
                new Model("Windmill"),
                new Model("Ruin"),
                new Model("RuinHouse"),
                new Model("SandCastle"),
                new Model("Banner"),
                new Model("Banner2"),
                new Model("Banner3"),
                new Model("Banner4"),
                new Model("Banner5"),
                new Model("Banner6"),
                new Model("Sign"),
                new Model("Sign2"),
                new Model("SignWarning"),
                new Model("SignCurveLeft"),
                new Model("SignCurveRight"),
                new Model("SharpRock"),
                new Model("SharpRock2"),
                new Model("Stone4"),
                new Model("Stone5"),
                new Model("AlphaTrain"),
                new Model("GuardRailHolder"),
                new Model("Hotel01"),
                new Model("Hotel02"),
                new Model("Casino01"),
            };

        /// <summary>
        /// Combos, which are used in the level file and for the automatic
        /// object generation below. Very useful. Each combo contains between
        /// 5 and 15 landscape model objects.
        /// </summary>
        TrackCombiModels[] combos = new TrackCombiModels[]
            {
                new TrackCombiModels("CombiPalms"),
                new TrackCombiModels("CombiPalms2"),
                new TrackCombiModels("CombiRuins"),
                new TrackCombiModels("CombiRuins2"),
                new TrackCombiModels("CombiStones"),
                new TrackCombiModels("CombiStones2"),
                new TrackCombiModels("CombiOilTanks"),
                new TrackCombiModels("CombiSandCastle"),
                new TrackCombiModels("CombiBuildings"),
                new TrackCombiModels("CombiHotels"),
            };

        /// <summary>
        /// Names for autogenerating stuff near the road to fill the level up.
        /// First 6 entries are used with more propability (fit better).
        /// </summary>
        internal string[] autoGenerationNames = new string[]
            {
                "CombiPalms",
                "CombiPalms2",
                "CombiRuins",
                "CombiRuins2",
                "CombiStones",
                "CombiStones2",
                //causes to much trouble and overlappings: "CombiOilTanks",
                "Kaktus",
                "Kaktus2",
                "KaktusBenny",
                "KaktusSeg",
                "AlphaDeadTree",
                "AlphaPalm",
                "AlphaPalm2",
                "AlphaPalm3",
                "AlphaPalmSmall",
                "Laterne2Sides",
                "Trashcan",
                "OilPump",
                "OilTanks",
                "RoadColumnSegment",
                "Windmill",
                "Ruin",
                "RuinHouse",
                "Sign",
                "Sign2",
                "SharpRock",
                "SharpRock2",
                "Stone4",
                "Stone5",
                "Casino01",
            };

        /// <summary>
        /// Add object to render
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <param name="renderMatrix">Render matrix</param>
        /// <param name="isNearTrack">Is near track</param>
        public void AddObjectToRender(string modelName, Matrix renderMatrix,
            bool isNearTrackForShadowGeneration)
        {
            // Fix wrong model names
            if (modelName == "OilWell")
                modelName = "OilPump";
            else if (modelName == "PalmSmall")
                modelName = "AlphaPalmSmall";
            else if (modelName == "AlphaPalm4")
                modelName = "AlphaPalmSmall";
            else if (modelName == "Palm")
                modelName = "AlphaPalm";
            else if (modelName == "Casino")
                modelName = "Casino01";
            else if (modelName == "Combi")
                modelName = "CombiPalms";

            // Always include windmills and buildings for shadow generation
            if (modelName.ToLower() == "windmill" ||
                modelName.ToLower().Contains("hotel") ||
                modelName.ToLower().Contains("building") ||
                modelName.ToLower().Contains("casino01"))
                isNearTrackForShadowGeneration = true;

            // Search for combos
            for (int num = 0; num < combos.Length; num++)
            {
                TrackCombiModels combi = combos[num];
                //slower: if (StringHelper.Compare(combi.Name, modelName))
                if (combi.Name == modelName)
                {
                    // Add all combi objects (calls this method for each model)
                    combi.AddAllModels(this, renderMatrix);
                    // Thats it.
                    return;
                }
            }

            Model foundModel = null;
            // Search model by name!
            for (int num = 0; num < landscapeModels.Length; num++)
            {
                Model model = landscapeModels[num];
                //slower: if (StringHelper.Compare(model.Name, modelName))
                if (model.Name == modelName)
                {
                    foundModel = model;
                    break;
                }
            }

            // Only add if we found the model
            if (foundModel != null)
            {
                // Fix z position to be always ABOVE the landscape
                Vector3 modelPos = renderMatrix.Translation;

                // Get landscape height here
                float landscapeHeight = GetMapHeight(modelPos.X, modelPos.Y);
                // And make sure we are always above it!
                if (modelPos.Z < landscapeHeight)
                {
                    modelPos.Z = landscapeHeight;
                    // Fix render matrix
                    renderMatrix.Translation = modelPos;
                }

                // Check if another object is nearby, then skip this one!
                // Don't skip signs or banners!
                if (modelName.StartsWith("Banner") == false &&
                    modelName.StartsWith("Sign") == false &&
                    modelName.StartsWith("StartLight") == false)
                {
                    for (int num = 0; num < landscapeObjects.Count; num++)
                        if (Vector3.DistanceSquared(
                            landscapeObjects[num].Position, modelPos) <
                            foundModel.Size * foundModel.Size / 4)
                        {
                            // Don't add
                            return;
                        }
                }

                LandscapeObject newObject =
                    new LandscapeObject(foundModel,
                    // Scale all objects up a little (else world is not filled enough)
                    Matrix.CreateScale(1.2f) *
                    renderMatrix);

                // Add
                landscapeObjects.Add(newObject);

                // Add again to the nearTrackObjects list if near the track
                if (isNearTrackForShadowGeneration)
                    nearTrackObjects.Add(newObject);

                if (modelName.StartsWith("StartLight"))
                    startLightObject = newObject;
            }
#if DEBUG
            else if (modelName.Contains("Track") == false)
                // Add warning in log file
                Log.Write("Landscape model "+modelName+" is not supported and "+
                    "can't be added for rendering!");
#endif
        }

        /// <summary>
        /// Add object to render
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="trackPos">Track position</param>
        /// <param name="trackRight">Track right</param>
        /// <param name="distance">Distance</param>
        public void AddObjectToRender(string modelName,
            float rotation, Vector3 trackPos, Vector3 trackRight,
            float distance)
        {
            // Find out size
            float objSize = 1;

            // Search for combos
            for (int num = 0; num < combos.Length; num++)
            {
                TrackCombiModels combi = combos[num];
                //slower: if (StringHelper.Compare(combi.Name, modelName))
                if (combi.Name == modelName)
                {
                    objSize = combi.Size;
                    break;
                }
            }

            // Search model by name!
            for (int num = 0; num < landscapeModels.Length; num++)
            {
                Model model = landscapeModels[num];
                //slower: if (StringHelper.Compare(model.Name, modelName))
                if (model.Name == modelName)
                {
                    objSize = model.Size;
                    break;
                }
            }

            // Make sure it is away from the road.
            if (distance > 0 &&
                distance - 10 < objSize)
                distance += objSize;
            if (distance < 0 &&
                distance + 10 > -objSize)
                distance -= objSize;

            AddObjectToRender(modelName,
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateTranslation(
                trackPos + trackRight * distance + new Vector3(0, 0, -100)), false);
        }

        /// <summary>
        /// Add object to render
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <param name="renderPos">Render position</param>
        public void AddObjectToRender(string modelName, Vector3 renderPos)
        {
            AddObjectToRender(modelName, Matrix.CreateTranslation(renderPos), false);
        }
        #endregion

        #region Variables
        /// <summary>
        /// Currently loaded level
        /// </summary>
        RacingGameManager.Level level = RacingGameManager.Level.Beginner;

        /// <summary>
        /// Vertices
        /// </summary>
        TangentVertex[] vertices = new TangentVertex[GridWidth * GridHeight];
        /// <summary>
        /// Matrix
        /// </summary>
        Material mat = new Material(
            //new Color(62, 62, 62), // ambient
            //new Color(240, 240, 240), // diffuse
            //new Color(24, 24, 24), // specular
            new Color(88, 88, 88), // ambient (bright day)
            new Color(234, 234, 234), // diffuse (also bright)
            new Color(33, 33, 33), // specular (unused anyway)
            "Landscape",
            "LandscapeNormal",
            "",
            "LandscapeDetail");

        /// <summary>
        /// City material for displaying an extra material whereever the ground
        /// is flat. This makes the ground look much better at such locations,
        /// especially where the city is at.
        /// </summary>
        Material cityMat = new Material(
            new Color(32, 32, 32),
            new Color(200, 200, 200),
            new Color(128, 128, 128),
            "CityGround",
            "CityGroundNormal", "", "");

        /// <summary>
        /// City material
        /// </summary>
        /// <returns>Material</returns>
        public Material CityMaterial
        {
            get
            {
                return cityMat;
            }
        }

        /// <summary>
        /// City planes we render additionally to the landscape.
        /// Each city plane is just 2 triangles and the cityMat material, very
        /// fast and easy stuff.
        /// </summary>
        PlaneRenderer cityPlane = null;

        /// <summary>
        /// Vertex buffer for our landscape
        /// </summary>
        VertexBuffer vertexBuffer;
        /// <summary>
        /// Index buffer for our landscape
        /// </summary>
        IndexBuffer indexBuffer;

        /// <summary>
        /// Map heights
        /// </summary>
        float[,] mapHeights = null;

        /// <summary>
        /// Track for our landscape, can be TrackBeginner, TrackAdvanced and
        /// TrackExpert, which will be selected in the menu.
        /// </summary>
        Track track = null;

        /// <summary>
        /// Best replay for the best lap time showing the player driving.
        /// And a new replay which is recorded in case we archive a better
        /// time this time when we drive :)
        /// </summary>
        Replay bestReplay = null,
            newReplay = null;

        /// <summary>
        /// Compare checkpoint time to the bestReplay times.
        /// </summary>
        /// <param name="checkpointNum">Checkpoint num</param>
        /// <returns>Time in milliseconds we improved</returns>
        public int CompareCheckpointTime(int checkpointNum)
        {
            // Invalid data?
            if (bestReplay == null ||
                checkpointNum >= bestReplay.CheckpointTimes.Count)
                // Then we can't return anything
                return 0;

            // Else just return difference
            float differenceMs =
                RacingGameManager.Player.GameTimeMilliseconds -
                bestReplay.CheckpointTimes[checkpointNum] * 1000.0f;

            return (int)differenceMs;
        }

        /// <summary>
        /// Start new lap, checks if the newReplay is good and
        /// can be stored as best replay :)
        /// </summary>
        public void StartNewLap()
        {
            float thisLapTime =
                RacingGameManager.Player.GameTimeMilliseconds / 1000.0f;

            // Upload new highscore (as we currently are in game,
            // no bonus or anything will be added, this score is low!)
            Highscores.SubmitHighscore((int)level,
                (int)RacingGameManager.Player.GameTimeMilliseconds);

            RacingGameManager.Player.AddLapTime(thisLapTime);

            if (thisLapTime < bestReplay.LapTime)
            {
                // Add final checkpoint
                RacingGameManager.Landscape.NewReplay.CheckpointTimes.Add(
                    thisLapTime);

                // Record lap time
                newReplay.LapTime = thisLapTime;

                // Save this replay to load it everytime in the future
                // Do this on a worker thread to prevent the game from skipping frames
                ThreadPool.QueueUserWorkItem(new WaitCallback(SaveReplay),
                                            (Replay)newReplay.Clone());

                // Set it as the current best replay
                bestReplay = newReplay;
            }

            // And start a new replay for this round
            newReplay = new Replay((int)level, true, track);
        }

        /// <summary>
        /// Callback used for saving a replay from a worker thread
        /// </summary>
        /// <param name="replay">Replay to be saved</param>
        private void SaveReplay(object replay)
        {
            ((Replay)replay).Save();
        }

        /// <summary>
        /// New replay
        /// </summary>
        public Replay NewReplay
        {
            get
            {
                return newReplay;
            }
        }

        /// <summary>
        /// Remember a list of brack tracks, which will be generated if we brake.
        /// </summary>
        List<TangentVertex> brakeTracksVertices = new List<TangentVertex>();

        /// <summary>
        /// Little helper to avoid creating a new array each frame for rendering
        /// </summary>
        TangentVertex[] brakeTracksVerticesArray = null;
        #endregion

        #region Properties
        /// <summary>
        /// Current track name
        /// </summary>
        /// <returns>String</returns>
        public string CurrentTrackName
        {
            get
            {
                return level.ToString();
            }
        }

        /// <summary>
        /// Track length
        /// </summary>
        /// <returns>Float</returns>
        public float TrackLength
        {
            get
            {
                return track.Length;
            }
        }

        /// <summary>
        /// Remember checkpoint segment positions for easier checkpoint checking.
        /// </summary>
        public List<int> CheckpointSegmentPositions
        {
            get
            {
                return track.CheckpointSegmentPositions;
            }
        }

        /// <summary>
        /// Best replay for the best lap time showing the player driving.
        /// </summary>
        public Replay BestReplay
        {
            get
            {
                return bestReplay;
            }
        }
        #endregion

        #region Get map height
        /// <summary>
        /// Get map height at a specific point, int based and not as percise as
        /// the float version, which interpolates between our grid points.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Float</returns>
        public float GetMapHeight(int x, int y)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x >= GridWidth)
                x = GridWidth - 1;
            if (y >= GridHeight)
                y = GridHeight - 1;

            return mapHeights[x, y];
        }

        /// <summary>
        /// This functions keeps sure we keep in 0-max range,
        /// simple modulate (%) will do this only correctly for positiv values!
        /// </summary>
        private static int ModulateValueInRange(float val, int max)
        {
            if (val < 0.0f)
                return (max - 1) - ((int)(-val) % max);
            else
                return (int)val % max;
        }

        /// <summary>
        /// Get map height at a specific point
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Float</returns>
        public float GetMapHeight(float x, float y)
        {
            // Rescale to our current dimensions
            x /= MapWidthFactor;
            y /= MapHeightFactor;

            // Interpolate the current position
            int
                // size-1 is because we need +1 for interpolating
                ix = ModulateValueInRange(x, GridWidth - 1),
                iy = ModulateValueInRange(y, GridHeight - 1);

            // Get the position ON the current tile (0.0-1.0)!!!
            float
                fX = x - ((float)((int)x)),
                fY = y - ((float)((int)y));

            int ix2 = (ix + 1) % (GridWidth - 1);
            int iy2 = (iy + 1) % (GridHeight - 1);

            if (fX + fY < 1) // opt. version
            {
                // we are on triangle 1 !!
                //     ------- (f_tile_width-mx)/f_tile_width
                //  0__v___1
                //  |     /
                //  |    /
                //  |---/--- (f_tile_height-my)/f_tile_height
                //  |  /
                //  | /
                //  3/
                return
                    mapHeights[ix, iy] + // 0
                    fX * (mapHeights[ix2, iy] - mapHeights[ix, iy]) + // 1
                    fY * (mapHeights[ix, iy2] - mapHeights[ix, iy]); // 3
            }
            // we are on triangle 1 !!
            // calc height (as above, but a bit more difficult for triangle 1)
            //        1
            //       /|
            //      / |
            //     /  |  my/f_tile_height (fX)
            //    /   |
            //   /    |
            //  3_____2
            //     ^---  mx/f_tile_width  (fY)
            return
                mapHeights[ix2, iy2] + // 2
                (1.0f - fY) * (mapHeights[ix2, iy] - mapHeights[ix2, iy2]) +    // 1
                (1.0f - fX) * (mapHeights[ix, iy2] - mapHeights[ix2, iy2]); // 3
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create landscape.
        /// This constructor should only be called
        /// from the RacingGame main class!
        /// </summary>
        /// <param name="setLevel">Level we want to load</param>
        internal Landscape(RacingGameManager.Level setLevel)
        {
            #region Load map height data
            Stream file = TitleContainer.OpenStream(
                "Content\\LandscapeHeights.data");
            byte[] heights = new byte[GridWidth * GridHeight];
            file.Read(heights, 0, GridWidth * GridHeight);
            file.Close();

            mapHeights = new float[GridWidth, GridHeight];
            #endregion

            #region Build tangent vertices
            // Build our tangent vertices
            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                {
                    // Step 1: Calculate position
                    int index = x + y * GridWidth;
                    Vector3 pos = CalcLandscapePos(x, y, heights);//texData);
                    mapHeights[x, y] = pos.Z;
                    vertices[index].pos = pos;

                    //if (x == 0)
                    //    Log.Write("vertices " + y + ": " + pos);

                    // Step 2: Calculate all edge vectors (for normals and tangents)
                    // This involves quite complicated optimizations and mathematics,
                    // hard to explain with just a comment. Read my book :D
                    Vector3 edge1 = pos - CalcLandscapePos(x, y + 1, heights);
                    Vector3 edge2 = pos - CalcLandscapePos(x + 1, y, heights);
                    Vector3 edge3 = pos - CalcLandscapePos(x - 1, y + 1, heights);
                    Vector3 edge4 = pos - CalcLandscapePos(x + 1, y + 1, heights);
                    Vector3 edge5 = pos - CalcLandscapePos(x - 1, y - 1, heights);

                    // Step 3: Calculate normal based on the edges (interpolate
                    // from 3 cross products we build from our edges).
                    vertices[index].normal = Vector3.Normalize(
                        Vector3.Cross(edge2, edge1) +
                        Vector3.Cross(edge4, edge3) +
                        Vector3.Cross(edge3, edge5));

                    // Step 4: Set tangent data, just use edge1
                    vertices[index].tangent = Vector3.Normalize(edge1);

                    // Step 5: Set texture coordinates, use full 0.0f to 1.0f range!
                    vertices[index].uv = new Vector2(
                        //x / (float)(GridWidth - 1),
                        //y / (float)(GridHeight - 1));
                        y / (float)(GridHeight - 1),
                        x / (float)(GridWidth - 1));
                }
            #endregion

            #region Smooth normals
            // Smooth all normals, first copy them over, then smooth everything
            Vector3[,] normalsForSmoothing = new Vector3[GridWidth, GridHeight];
            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                {
                    int index = x + y * GridWidth;
                    normalsForSmoothing[x, y] = vertices[index].normal;
                }

            // Time to smooth to normals we just saved
            for (int x = 1; x < GridWidth - 1; x++)
                for (int y = 1; y < GridHeight - 1; y++)
                {
                    int index = x + y * GridWidth;

                    // Smooth 3x3 normals, but still use old normal to 40% (5 of 13)
                    Vector3 normal = vertices[index].normal * 4;
                    for (int xAdd = -1; xAdd <= 1; xAdd++)
                        for (int yAdd = -1; yAdd <= 1; yAdd++)
                            normal += normalsForSmoothing[x + xAdd, y + yAdd];
                    vertices[index].normal = Vector3.Normalize(normal);

                    // Also recalculate tangent to let it stay 90 degrees on the normal
                    Vector3 helperVector = Vector3.Cross(
                        vertices[index].normal,
                        vertices[index].tangent);
                    vertices[index].tangent = Vector3.Cross(
                        helperVector,
                        vertices[index].normal);
                }
            #endregion

            #region Set vertex buffer
            // Set vertex buffer
            // fix
            //vertexBuffer = new VertexBuffer(
            //    BaseGame.Device,
            //    typeof(TangentVertex),
            //    vertices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            //vertexBuffer.SetData(vertices);
            vertexBuffer = new VertexBuffer(
                BaseGame.Device,
                typeof(TangentVertex),
                vertices.Length, 
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            #endregion

            #region Calc index buffer
            // Calc index buffer (Note: have to use uint, ushort is not sufficiant
            // in our case because we have MANY vertices ^^)
            uint[] indices = new uint[(GridWidth - 1) * (GridHeight - 1) * 6];
            int currentIndex = 0;
            for (int x = 0; x < GridWidth - 1; x++)
                for (int y = 0; y < GridHeight - 1; y++)
                {
                    // Set landscape data (Note: Right handed)
                    indices[currentIndex + 0] = (uint)(x * GridHeight + y);
                    indices[currentIndex + 2] =
                        (uint)((x + 1) * GridHeight + (y + 1));
                    indices[currentIndex + 1] = (uint)((x + 1) * GridHeight + y);
                    indices[currentIndex + 3] =
                        (uint)((x + 1) * GridHeight + (y + 1));
                    indices[currentIndex + 5] = (uint)(x * GridHeight + y);
                    indices[currentIndex + 4] = (uint)(x * GridHeight + (y + 1));

                    // Add indices
                    currentIndex += 6;
                }
            #endregion

            #region Set index buffer
            // fix
            //indexBuffer = new IndexBuffer(
            //    BaseGame.Device,
            //    typeof(uint),
            //    (GridWidth - 1) * (GridHeight - 1) * 6,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);

            indexBuffer = new IndexBuffer(
                BaseGame.Device,
                typeof(uint),
                (GridWidth - 1) * (GridHeight - 1) * 6,
                BufferUsage.WriteOnly);

            indexBuffer.SetData(indices);
            #endregion

            #region Load track (and replay inside ReloadLevel method)
            // Load track based on the level selection and set car pos with
            // help of the ReloadLevel method.
            ReloadLevel(setLevel);
            #endregion

            #region Add city planes
            // Just set one giant plane for the whole city!
            foreach (LandscapeObject obj in landscapeObjects)
                if (obj.IsBigBuilding)
                {
                    cityPlane = new PlaneRenderer(
                        obj.Position,
                        new Plane(new Vector3(0, 0, 1), 0.1f),
                        cityMat, Math.Min(obj.Position.X, obj.Position.Y));
                    break;
                }
            #endregion
        }

        #region Reload level
        /// <summary>
        /// Reload level
        /// </summary>
        /// <param name="setLevel">Level</param>
        internal void ReloadLevel(RacingGameManager.Level setLevel)
        {
            level = setLevel;

            // Load track based on the level selection, do this after
            // we got all the height data because the track might be adjusted.
            if (track == null)
                track = new Track("Track" + level.ToString(), this);
            else
                track.Reload("Track" + level.ToString(), this);

            // Load replay for this track to show best player
            bestReplay = new Replay((int)level, false, track);
            newReplay = new Replay((int)level, true, track);

            // Kill brake tracks
            brakeTracksVertices.Clear();
            brakeTracksVerticesArray = null;

            // Set car at start pos
            SetCarToStartPosition();

            // Begin game with red start light
            startLightObject.ChangeModel(landscapeModels[0]);
        }
        #endregion

        #region Calc landscape position
        /// <summary>
        /// Calc landscape position
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Vector 3</returns>
        private static Vector3 CalcLandscapePos(int x, int y, byte[] heights)
        {
            // Make sure we stay on the valid map data
            int mapX = x < 0 ? 0 : x >= GridWidth ? GridWidth - 1 : x;
            int mapY = y < 0 ? 0 : y >= GridHeight ? GridHeight - 1 : y;

            // Get height
            float heightPercent = heights[mapX + mapY * GridWidth] / 255.0f;

            // Build landscape position vector
            return new Vector3(
                x * MapWidthFactor,
                y * MapHeightFactor,
                heightPercent * MapZScale);
        }
        #endregion
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int num = 0; num < landscapeModels.Length; num++)
                    landscapeModels[num].Dispose();

                mat.Dispose();
                cityMat.Dispose();
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
                track.Dispose();
            }
        }
        #endregion

        #region Set car to start pos
        /// <summary>
        /// Set car to start pos
        /// </summary>
        public void SetCarToStartPosition()
        {
            RacingGameManager.Player.SetCarPosition(
                track.StartPosition, track.StartDirection, track.StartUpVector);
            // Camera is set in zooming in method of the Player class.
        }
        #endregion

        #region Render
        /// <summary>
        /// Render landscape (just at the origin)
        /// </summary>
        public void Render()
        {
            // Make sure z buffer is on
            BaseGame.Device.DepthStencilState = DepthStencilState.Default;

            BaseGame.WorldMatrix = Matrix.Identity;

            // Render landscape (pretty easy with all the data we got here)
            ShaderEffect.landscapeNormalMapping.Render(
                mat, "DiffuseWithDetail20",
                new BaseGame.RenderHandler(RenderLandscapeVertices));

            cityPlane.Render();

            // Render track
            track.Render();

            // Render all landscape objects
            for (int num = 0; num < landscapeObjects.Count; num++)
            {
                landscapeObjects[num].Render();
            }

            // Render all brake tracks
            RenderBrakeTracks();
        }

        #region RenderLandscapeVertices
        /// <summary>
        /// Render landscape vertices
        /// </summary>
        private void RenderLandscapeVertices()
        {
            BaseGame.Device.SetVertexBuffer(vertexBuffer);
            BaseGame.Device.Indices = indexBuffer;
            BaseGame.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, GridWidth * GridHeight,
                0, (GridWidth - 1) * (GridHeight - 1) * 2);
        }
        #endregion
        #endregion

        #region Generate and use shadow for the landscape
        /// <summary>
        /// Generate shadow
        /// </summary>
        public void GenerateShadow()
        {
            // Don't generate shadow for the landscape, it only receives shadow!

            // Just generate shadows for the road.
            track.GenerateShadow();

            // Render shadow all landscape objects that near our road
            for (int num = 0; num < nearTrackObjects.Count; num++)
            {
                nearTrackObjects[num].GenerateShadows();
            }
        }

        /// <summary>
        /// Use shadow
        /// </summary>
        public void UseShadow()
        {
            // Receive shadow on the landscape, just render it out.
            ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(Matrix.Identity);

            // Render shadows for palms and other objects near the road.
            RenderLandscapeVertices();

            // Also receive shadows for all landscape objects that near our road.
            // This is not really required (still looks good without it), but
            // sometimes objects may have lookthrough-shadows or windmills
            // are usually a problem. This fixes this or makes it at least less
            // noticable.
            if (BaseGame.HighDetail)
            {
                for (int num = 0; num < nearTrackObjects.Count; num++)
                    // Don't receive shadows on signs, looks weird.
                    if (nearTrackObjects[num].IsBanner == false)
                    {
                        nearTrackObjects[num].UseShadows();
                    }
            }

            // And the track receives shadow too
            track.UseShadow();
        }
        #endregion

        #region GetTrackPositionMatrix and UpdateCarTrackPosition
        /// <summary>
        /// Get track position matrix, used for the game background and unit tests.
        /// </summary>
        /// <param name="carTrackPos">Car track position</param>
        /// <param name="roadWidth">Road width</param>
        /// <param name="nextRoadWidth">Next road width</param>
        /// <returns>Matrix</returns>
        public Matrix GetTrackPositionMatrix(float carTrackPos,
            out float roadWidth, out float nextRoadWidth)
        {
            return track.GetTrackPositionMatrix(carTrackPos,
                out roadWidth, out nextRoadWidth);
        }

        /// <summary>
        /// Get track position matrix
        /// </summary>
        /// <param name="trackSegmentNum">Track segment number</param>
        /// <param name="trackSegmentPercent">Track segment percent</param>
        /// <param name="roadWidth">Road width</param>
        /// <param name="nextRoadWidth">Next road width</param>
        /// <returns>Matrix</returns>
        public Matrix GetTrackPositionMatrix(
            int trackSegmentNum, float trackSegmentPercent,
            out float roadWidth, out float nextRoadWidth)
        {
            return track.GetTrackPositionMatrix(
                trackSegmentNum, trackSegmentPercent,
                out roadWidth, out nextRoadWidth);
        }

        /// <summary>
        /// Update car track position
        /// </summary>
        /// <param name="carPos">Car position</param>
        /// <param name="trackSegmentNumber">Track segment number</param>
        /// <param name="trackPositionPercent">Track position percent</param>
        public void UpdateCarTrackPosition(
            Vector3 carPos,
            ref int trackSegmentNumber, ref float trackPositionPercent)
        {
            track.UpdateCarTrackPosition(carPos,
                ref trackSegmentNumber, ref trackPositionPercent);
        }
        #endregion

        #region Add and render brake tracks
        /// <summary>
        /// Helper to skip track generation if it is near the last generated pos.
        /// </summary>
        Vector3 lastAddedTrackPos = new Vector3(-1000, -1000, -1000);
        /// <summary>
        /// Render a maximum of 140 brake tracks.
        /// </summary>
        const int MaxBrakeTrackVertices = 6 * 140;

        /// <summary>
        /// The amount to raise the break tracks decal off the road surface
        /// </summary>
        const float RaiseBreakTracksAmount = 0.2f;

        /// <summary>
        /// Add brake track
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="dir">Dir vector</param>
        /// <param name="right">Right vector</param>
        public void AddBrakeTrack(CarPhysics car)
        {
            Vector3 position = car.CarPosition + car.CarDirection * 1.25f;

            // Just skip if we setting to a similar location again.
            // This check is much faster and accurate for tracks on top of each
            // other than the foreach loop below, which is only useful to
            // put multiple tracks correctly behind each other!
            if (Vector3.DistanceSquared(position, lastAddedTrackPos) < 0.024f ||
                // Limit number of tracks to keep rendering fast.
                brakeTracksVertices.Count > MaxBrakeTrackVertices)
                return;

            lastAddedTrackPos = position;

            const float width = 2.4f; // car is 2.6m width, we use 2.4m for tires
            const float length = 4.5f; // Length of break tracks
            float maxDist =
                (float)Math.Sqrt(width * width + length * length) / 2 - 0.35f;

            // Check if there is any track already set here or nearby?
            for (int num = 0; num < brakeTracksVertices.Count; num++)
                if (Vector3.DistanceSquared(brakeTracksVertices[num].pos, position) <
                    maxDist * maxDist)
                    // Then skip this brake track, don't put that much stuff on
                    // top of each other.
                    return;

            // Move position a little bit up (above the road)
            position += Vector3.Normalize(car.CarUpVector) * RaiseBreakTracksAmount;

            // Just add 6 new vertices to render (2 triangles)
            TangentVertex[] newVertices = new TangentVertex[]
            {
                // First triangle
                new TangentVertex(
                    position -car.CarRight*width/2 -car.CarDirection*length/2, 0, 0,
                    car.CarUpVector, car.CarRight),
                new TangentVertex(
                    position -car.CarRight*width/2 +car.CarDirection*length/2, 0, 5,
                    car.CarUpVector, car.CarRight),
                new TangentVertex(
                    position +car.CarRight*width/2 +car.CarDirection*length/2, 1, 5,
                    car.CarUpVector, car.CarRight),
                // Second triangle
                new TangentVertex(
                    position -car.CarRight*width/2 -car.CarDirection*length/2, 0, 0,
                    car.CarUpVector, car.CarRight),
                new TangentVertex(
                    position +car.CarRight*width/2 +car.CarDirection*length/2, 1, 5,
                    car.CarUpVector, car.CarRight),
                new TangentVertex(
                    position +car.CarRight*width/2 -car.CarDirection*length/2, 1, 0,
                    car.CarUpVector, car.CarRight),
            };

            brakeTracksVertices.AddRange(newVertices);
            brakeTracksVerticesArray = brakeTracksVertices.ToArray();
        }

        /// <summary>
        /// Render brake tracks
        /// </summary>
        public void RenderBrakeTracks()
        {
            // Nothing to render?
            if (brakeTracksVerticesArray == null)
                return;

            BaseGame.SetAlphaBlendingEnabled(true);
            BaseGame.WorldMatrix = Matrix.Identity;
            ShaderEffect.lighting.Render(
                RacingGameManager.BrakeTrackMaterial,
                "Diffuse20",
                delegate
                {
                    // Draw the vertices
                    BaseGame.Device.DrawUserPrimitives(
                        PrimitiveType.TriangleList,
                        brakeTracksVerticesArray, 0, brakeTracksVerticesArray.Length / 3);
                });
        }
        #endregion
    }
}
