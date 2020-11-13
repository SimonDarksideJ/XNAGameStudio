#region File Description
//-----------------------------------------------------------------------------
// TrackLine.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Landscapes;
#endregion

namespace RacingGame.Tracks
{
    /// <summary>
    /// Track line
    /// </summary>
    public class TrackLine
    {
        #region Constants
        /// <summary>
        /// Number of iterations we will produce from the input points to
        /// get our line positions. All data is generated with help of
        /// CattmullRom splines.
        /// </summary>
        protected const int NumberOfIterationsPer100Meters = 40;
        /// <summary>
        /// Curve factor, 1.0 will make all curves the same as the rotations are.
        /// Basically we just have to drive ahead. Reduce this value to make
        /// the track harder!
        /// </summary>
        const float CurveFactor = 0.25f;
        /// <summary>
        /// Correct the road to be up each step. This is important to
        /// make our road face always up except where we have no choice (loopings).
        /// </summary>
        const float UpFactorCorrector = 0.6f;

        /// <summary>
        /// Factor for streching the road texture, smaller values will the texture
        /// strech more.
        /// </summary>
        const float RoadTextureStrechFactor = 0.125f;

        /// <summary>
        /// Number of values we put into our second pass up vector smoothing
        /// alsorithm below.
        /// </summary>
        const int NumberOfUpSmoothValues = 10;

        /// <summary>
        /// Minimium distance of the road track to the landscape.
        /// </summary>
        const float MinimumLandscapeDistance = 2.0f;

        /// <summary>
        /// Looping points for generating smooth loops.
        /// See TrackLine constructor for details.
        /// </summary>
        static readonly Vector3[] LoopingPoints =
            new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0.353553f, 0.146447f),
                new Vector3(0, 0.5f, 0.5f),
                new Vector3(0, 0.353553f, 1.0f-0.146447f),
                new Vector3(0, 0, 1.0f),
                new Vector3(0, -0.353553f, 1.0f-0.146447f),
                new Vector3(0, -0.5f, 0.5f),
                new Vector3(0, -0.353553f, 0.146447f),
                new Vector3(0, 0, 0),
            };
        #endregion

        #region Variables
        /// <summary>
        /// Points of this track (middle line), generated from the input points
        /// in the constructor using <see="NumberOfIterationsForInputPoints" />.
        /// <para></para>
        /// Each point has also the following 3 vectors:
        /// Right vector: Makes it easier to create the road vertices. Can also be
        /// calculated by just crossing up and dir vectors! See Unit Test below.
        /// Up vector: This are the calculated up vectors after the second pass.
        /// We also work the CurveFactor and UpFactorCorrector in here!
        /// Dir vector: Not really used, can be ignored externally.
        /// But useful for generation and checking distances.
        /// </summary>
        protected List<TrackVertex> points = new List<TrackVertex>();

        /// <summary>
        /// Road helper position
        /// </summary>
        public class RoadHelperPosition
        {
            /// <summary>
            /// Type
            /// </summary>
            public TrackData.RoadHelper.HelperType type;
            /// <summary>
            /// Start and end road segment num for our tunnel/palms/laterns.
            /// </summary>
            public int startNum, endNum;

            /// <summary>
            /// Create road helper position
            /// </summary>
            /// <param name="setStartNum">Set start number</param>
            /// <param name="setEndNum">Set end number</param>
            public RoadHelperPosition(TrackData.RoadHelper.HelperType setType,
                int setStartNum, int setEndNum)
            {
                type = setType;
                startNum = setStartNum;
                endNum = setEndNum;
            }
        }

        /// <summary>
        /// Remember tunnel positions, used in the Track class for the
        /// tunnel generation. Usually we don't have much tunnels.
        /// </summary>
        protected List<RoadHelperPosition> helperPositions =
            new List<RoadHelperPosition>();
        #endregion

        #region Constructors
        /// <summary>
        /// Create track line
        /// </summary>
        /// <param name="inputPoints">Input Points, will form a closed curve
        /// </param>
        public TrackLine(Vector3[] inputPoints,
            List<TrackData.WidthHelper> widthHelpers,
            List<TrackData.RoadHelper> roadHelpers,
            List<TrackData.NeutralObject> neutralObjects,
            Landscape landscape)
        {
            Load(inputPoints, widthHelpers, roadHelpers, neutralObjects, landscape);
        }

#if DEBUG
        /// <summary>
        /// Create track line.
        /// This constructor is only used for unit tests.
        /// </summary>
        /// <param name="inputPoints">Input points</param>
        public TrackLine(Vector3[] inputPoints)
            : this(inputPoints,
            new List<TrackData.WidthHelper>(),
            new List<TrackData.RoadHelper>(),
            new List<TrackData.NeutralObject>(),
            null)
        {
        }
#endif

        /// <summary>
        /// Create track line
        /// </summary>
        /// <param name="inputPointsFromTrack">
        /// Input points from track</param>
        public TrackLine(TrackData inputPointsFromTrack,
            Landscape landscape)
            : this(inputPointsFromTrack.TrackPoints.ToArray(),
            inputPointsFromTrack.WidthHelpers,
            inputPointsFromTrack.RoadHelpers,
            inputPointsFromTrack.NeutralsObjects,
            landscape)
        {
        }
        #endregion

        #region Load
        /// <summary>
        /// Load
        /// </summary>
        /// <param name="inputPoints">Input points</param>
        /// <param name="widthHelpers">Width helpers</param>
        /// <param name="roadHelpers">Road helpers</param>
        /// <param name="neutralObjects">Neutral objects</param>
        /// <param name="landscape">Landscape</param>
        protected void Load(Vector3[] inputPoints,
            List<TrackData.WidthHelper> widthHelpers,
            List<TrackData.RoadHelper> roadHelpers,
            List<TrackData.NeutralObject> neutralObjects,
            Landscape landscape)
        {
            #region Kill all previously loaded data
            points.Clear();
            helperPositions.Clear();

            // Kill all loaded objects
            if (landscape != null)
                landscape.KillAllLoadedObjects();
            #endregion

            #region Make sure we got valid data
            if (inputPoints == null ||
                inputPoints.Length < 3)
                throw new ArgumentException("inputPoints is invalid, we need at " +
                    "least 3 valid input points to generate a TrackLine.");
            #endregion

            #region Check if all points are ABOVE the landscape
            if (landscape != null)
            {
                // Go through all spline points
                for (int num = 0; num < inputPoints.Length; num++)
                {
                    // Get landscape height here
                    float landscapeHeight = landscape.GetMapHeight(
                        inputPoints[num].X, inputPoints[num].Y) +
                        // add little to fix ground errors
                        MinimumLandscapeDistance * 2.25f;

                    // And make sure we are always above it!
                    if (inputPoints[num].Z < landscapeHeight)
                        inputPoints[num].Z = landscapeHeight;
                }

                // Second pass, check 24 interpolation points between all inputPoints
                for (int num = 0; num < inputPoints.Length; num++)
                    for (int iter = 1; iter < 25; iter++)
                    {
                        float iterPercent = iter / 25.0f;

                        float iterHeight = inputPoints[num].Z * (1 - iterPercent) +
                            inputPoints[(num + 1) % inputPoints.Length].Z * iterPercent;

                        // Check 2x2 points (in all directions) to make sure
                        // we don't go through the landscape at the sides
                        for (int x = 0; x < 2; x++)
                            for (int y = 0; y < 2; y++)
                            {
                                // Also get height at middle to next pos
                                float landscapeHeight = landscape.GetMapHeight(
                                    -5.0f + 10.0f * x +
                                    inputPoints[num].X * (1 - iterPercent) +
                                    inputPoints[(num + 1) % inputPoints.Length].X
                                    * iterPercent, -5.0f + 10.0f * y +
                                    inputPoints[num].Y * (1 - iterPercent) +
                                    inputPoints[(num + 1) % inputPoints.Length].Y
                                    * iterPercent) +
                                    // add little to fix ground errors
                                    MinimumLandscapeDistance * 1.6f;

                                // Increase both positions if this point
                                // is under the landscape
                                if (iterHeight < landscapeHeight)
                                {
                                    float increaseHeight = landscapeHeight - iterHeight;
                                    inputPoints[num].Z += increaseHeight;
                                    inputPoints[(num + 1) % inputPoints.Length].Z +=
                                        increaseHeight;
                                }
                            }
                    }
            }
            #endregion

            #region Search for any loopings indicated by 2 points above each other
            // Go through all spline points (ignore first and last 3, this
            // makes it easier to remove points and add new ones).
            for (int num = 1; num < inputPoints.Length - 3; num++)
            {
                // X/Y distance has to be 4 times smaller than Z distance
                Vector3 distVec = inputPoints[num + 1] - inputPoints[num];
                float xyDist = (float)Math.Sqrt(
                    distVec.X * distVec.X + distVec.Y * distVec.Y);
                float zDist = Math.Abs(distVec.Z);
                // Also check if next point is down again.
                Vector3 distVec2 = inputPoints[num + 2] - inputPoints[num + 1];
                if (zDist / 2 > xyDist &&
                    Math.Abs(distVec.Z + distVec2.Z) < zDist / 2)
                {
                    // Find out which direction we are going
                    Vector3 dir = inputPoints[num] - inputPoints[num - 1];
                    dir.Normalize();
                    Vector3 upVec = new Vector3(0, 0, 1);
                    Vector3 rightVec = Vector3.Cross(dir, upVec);
                    // Matrix build helper matrix to rotate our looping points
                    Matrix rotMatrix = new Matrix(
                        rightVec.X, rightVec.Y, rightVec.Z, 0,
                        dir.X, dir.Y, dir.Z, 0,
                        upVec.X, upVec.Y, upVec.Z, 0,
                        0, 0, 0, 1);

                    // Ok do a looping with zDist as height.
                    // Start with the current point, loop around and end with the
                    // point after the looping. We will remove the current and the
                    // next 2 points, but add 9 new points instead for our smooth loop.
                    // See LoopingPoints for the looping itself.
                    Vector3 startLoopPos = inputPoints[num];
                    Vector3 endLoopPos = inputPoints[num + 2];

                    // Insert 7 new points (9 new points, but we reuse
                    // start, middle and end points which are num, num+1 and num+2,
                    // plus an additional point after the looping to keep the road
                    // straight!)
                    Vector3[] remInputPoints = (Vector3[])inputPoints.Clone();
                    inputPoints = new Vector3[inputPoints.Length + 7];
                    // Copy everything over
                    for (int copyNum = 0; copyNum < remInputPoints.Length; copyNum++)
                        if (copyNum < num)
                            inputPoints[copyNum] = remInputPoints[copyNum];
                        else
                            inputPoints[copyNum + 7] = remInputPoints[copyNum];

                    // Ok, now we can add our loop
                    for (int loopNum = 0; loopNum < LoopingPoints.Length; loopNum++)
                    {
                        // Interpolate between start and end pos to land at the end pos!
                        float loopPercent = loopNum / (float)(LoopingPoints.Length - 1);
                        inputPoints[num + loopNum] =
                            startLoopPos * (1 - loopPercent) +
                            endLoopPos * loopPercent +
                            zDist * Vector3.Transform(LoopingPoints[loopNum], rotMatrix);
                    }

                    // Add extra point to keep the road straight
                    Vector3 newRoadDir =
                        inputPoints[num + 10] - inputPoints[num + 8];
                    // Don't go more than zDist * 2 units away!
                    if (newRoadDir.Length() > zDist * 2)
                    {
                        newRoadDir.Normalize();
                        newRoadDir = newRoadDir * zDist;
                        inputPoints[num + 9] = inputPoints[num + 8] + newRoadDir;
                    }
                    else
                        // Just add an interpolation point
                        inputPoints[num + 9] =
                            (inputPoints[num + 8] + inputPoints[num + 10]) / 2.0f;

                    // Advance 10 points until we check for the next loop
                    num += 10;

                    // That's it, good work everyone ^^
                }
            }
            #endregion

            #region Generate all points with help of catmull rom splines
            // Generate all points with help of catmull rom splines
            for (int num = 0; num < inputPoints.Length; num++)
            {
                // Get the 4 required points for the catmull rom spline
                Vector3 p1 = inputPoints[num - 1 < 0 ? inputPoints.Length - 1 : num - 1];
                Vector3 p2 = inputPoints[num];
                Vector3 p3 = inputPoints[(num + 1) % inputPoints.Length];
                Vector3 p4 = inputPoints[(num + 2) % inputPoints.Length];

                // Calculate number of iterations we use here based
                // on the distance of the 2 points we generate new points from.
                float distance = Vector3.Distance(p2, p3);
                int numberOfIterations =
                    (int)(NumberOfIterationsPer100Meters * (distance / 100.0f));
                if (numberOfIterations <= 0)
                    numberOfIterations = 1;

                for (int iter = 0; iter < numberOfIterations; iter++)
                {
                    TrackVertex newVertex = new TrackVertex(
                        Vector3.CatmullRom(p1, p2, p3, p4,
                        iter / (float)numberOfIterations));

                    points.Add(newVertex);
                }
            }
            #endregion

            #region Generate up vectors, very important for our road building
            // Pre up vectors are used to first generate all optimal up vectors
            // for the track, but this is not useful for driving because we need
            // the road to point up always except for loopings.
            List<Vector3> preUpVectors = new List<Vector3>();

            // Now generate all up vectors, first pass does optimal up vectors.
            Vector3 defaultUpVec = new Vector3(0, 0, 1);
            Vector3 lastUpVec = defaultUpVec;
            for (int num = 0; num < points.Count; num++)
            {
                // Get direction we are driving in at this point,
                // interpolate with help of last and next points.
                Vector3 dir = points[(num + 1) % points.Count].pos -
                    points[num - 1 < 0 ? points.Count - 1 : num - 1].pos;
                dir.Normalize();

                // Now calculate the optimal up vector for this point
                Vector3 middlePoint = (points[(num + 1) % points.Count].pos +
                    points[num - 1 < 0 ? points.Count - 1 : num - 1].pos) / 2.0f;
                Vector3 optimalUpVector = middlePoint - points[num].pos;
                if (optimalUpVector.Length() < 0.0001f)
                    optimalUpVector = lastUpVec;
                optimalUpVector.Normalize();

                // Store the optimalUpVectors in the preUpVectors list
                preUpVectors.Add(optimalUpVector);

                // Also save dir vector
                points[num].dir = dir;

                // And remember the last upVec in case the road is going straight ahead
                lastUpVec = optimalUpVector;
            }

            // Interpolate the first up vector for a smoother road at the start pos
            preUpVectors[0] = preUpVectors[preUpVectors.Count - 1] + preUpVectors[1];
            preUpVectors[0].Normalize();
            #endregion

            #region Interpolate the up vectors and also add the dir and right vectors
            // Second pass, interpolated precalced values and apply our logic :)
            //preUpVectors[0] =
            lastUpVec = Vector3.Lerp(defaultUpVec, preUpVectors[0],
                1.5f * CurveFactor * UpFactorCorrector);
            //lastUpVec = preUpVectors[0];
            Vector3 lastUpVecUnmodified = lastUpVec;// defaultUpVec;
            for (int num = 0; num < points.Count; num++)
            {
                // Grab dir vector (could be calculated here too)
                Vector3 dir = points[num].dir;

                // First of all interpolate the preUpVectors
                Vector3 upVec =
                    //single input: preUpVectors[num];
                    Vector3.Zero;
                for (int smoothNum = -NumberOfUpSmoothValues / 2;
                    smoothNum <= NumberOfUpSmoothValues / 2; smoothNum++)
                    upVec +=
                        preUpVectors[(num + points.Count + smoothNum) % points.Count];
                upVec.Normalize();

                // Find out if this road piece is upside down and if we are
                // moving up or down. This is VERY important for catching loopings.
                bool upsideDown = upVec.Z < -0.25f &&
                    lastUpVecUnmodified.Z < -0.05f;
                bool movingUp = dir.Z > 0.75f;
                bool movingDown = dir.Z < -0.75f;

                // Mix in the last vector to make curves weaker
                upVec = Vector3.Lerp(lastUpVec, upVec, CurveFactor);
                upVec.Normalize();
                // Store the last value to check for loopings.
                lastUpVecUnmodified = upVec;

                // Don't mix in default up if we head up or are upside down!
                // Its very useful to know if we move up or down to fix the
                // problematic areas at loopings by pointing stuff correct right away.
                if (movingUp)
                    lastUpVec = Vector3.Lerp(upVec, -defaultUpVec, UpFactorCorrector);
                else if (movingDown)
                    lastUpVec = Vector3.Lerp(upVec, defaultUpVec, UpFactorCorrector);
                else if (upsideDown)
                    lastUpVec = Vector3.Lerp(upVec, -defaultUpVec, UpFactorCorrector);
                else
                    lastUpVec = Vector3.Lerp(upVec, defaultUpVec, UpFactorCorrector);

                // If we are very close to the ground, make the road point up more!
                if (//upsideDown == false &&
                    landscape != null)
                {
                    // Get landscape height here
                    float landscapeHeight = landscape.GetMapHeight(
                        points[num].pos.X, points[num].pos.Y);

                    // If point is close to the landscape, let everything point up more
                    if (points[num].pos.Z - landscapeHeight <
                        MinimumLandscapeDistance * 4)
                        lastUpVec = Vector3.Lerp(upVec, defaultUpVec,
                            1.75f * UpFactorCorrector);
                }

                // And finally calculate rightVectors with just a cross product.
                // Used to render the track later.
                Vector3 rightVec = Vector3.Cross(dir, upVec);
                rightVec.Normalize();
                points[num].right = rightVec;

                // Recalculate up vector with help of right and dir.
                // This makes the up vector to always point up 90 degrees.
                upVec = Vector3.Cross(rightVec, dir);
                upVec.Normalize();
                points[num].up = upVec;
            }
            #endregion

            #region Smooth up vectors!
            lastUpVec = points[0].up;
            for (int num = 0; num < points.Count; num++)
                preUpVectors[num] = points[num].up;
            for (int num = 0; num < points.Count; num++)
            {
                // Interpolate up vectors again
                Vector3 upVec = Vector3.Zero;
                for (int smoothNum = -NumberOfUpSmoothValues;
                    smoothNum <= NumberOfUpSmoothValues; smoothNum++)
                {
                    upVec +=
                        preUpVectors[(num + points.Count + smoothNum) % points.Count];
                }
                upVec.Normalize();
                points[num].up = upVec;

                // Also rebuild right vector
                Vector3 dir = points[num].dir;
                points[num].right = Vector3.Cross(dir, upVec);
            }
            #endregion

            AdjustRoadWidths(widthHelpers);

            GenerateUTextureCoordinates();

            GenerateTunnelsAndLandscapeObjects(
                roadHelpers, neutralObjects, landscape);
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="trackData">track</param>
        /// <param name="landscape">Landscape</param>
        protected void Load(TrackData trackData, Landscape landscape)
        {
            Load(trackData.TrackPoints.ToArray(),
                trackData.WidthHelpers,
                trackData.RoadHelpers,
                trackData.NeutralsObjects,
                landscape);
        }
        #endregion

        #region AdjustRoadWidths
        private void AdjustRoadWidths(
            List<TrackData.WidthHelper> widthHelpers)
        {
            #region Go through all width helpers and adjust the road width
            // Go through all width helpers along the road,
            // everything close enough (<25) is interessting for us.
            float currentWidth = TrackVertex.DefaultRoadWidth;
            float widthInfluence = currentWidth;
            for (int num = 0; num < points.Count; num++)
            {
                Vector3 pos = points[num].pos;
                foreach (TrackData.WidthHelper widthHelper in widthHelpers)
                {
                    float dist = Vector3.Distance(widthHelper.pos, pos);
                    if (dist < 25.0f)
                    {
                        float influence = (1 - (dist / 25.0f));
                        widthInfluence =
                            (1 - influence) * widthInfluence +
                            influence * widthHelper.scale;
                    }
                }

                // Use 90% the old width and 10% the new width.
                currentWidth =
                    currentWidth * 0.9f +
                    widthInfluence * 0.1f;

                // At the end of the road, mix in the staring road width (loop)
                if (num > points.Count - 7)
                {
                    float influence =
                        num == (points.Count - 1) ? 0.75f :
                        num == (points.Count - 2) ? 0.5f :
                        num == (points.Count - 2) ? 0.25f : 0.175f;
                    currentWidth =
                        influence * points[0].roadWidth +
                        (1 - influence) * currentWidth;
                }

                if (currentWidth < TrackVertex.MinRoadWidth)
                    currentWidth = TrackVertex.MinRoadWidth;
                if (currentWidth > TrackVertex.MaxRoadWidth)
                    currentWidth = TrackVertex.MaxRoadWidth;
                points[num].roadWidth = currentWidth;
            }
            #endregion
        }
        #endregion

        #region GenerateUTextureCoordinates
        private void GenerateUTextureCoordinates()
        {
            #region Generate u texture coordinates
            float currentRoadUTexValue = 0.0f;
            for (int num = 0; num < points.Count; num++)
            {
                // Assign u texture coordinate
                points[num].uv.X = currentRoadUTexValue;

                // Uniform calculation of the texture coordinates for the roadway,
                // so it doesn't matter if there is a gap of 2 or 200 m
                currentRoadUTexValue += RoadTextureStrechFactor *
                    (points[(num + 1) % points.Count].pos -
                    points[num % points.Count].pos).Length();
            }

            // Now we got a problem, for the polygons between the last and the first
            // points (last road block) we might have very different texture
            // coordinates, which may look very wrong. To fix this we generate a new
            // point by just duplicating the first point and applying another set of
            // texture coordinates!
            points.Add(new TrackVertex(
                points[0].pos,
                points[0].right,
                points[0].up,
                points[0].dir,
                new Vector2(currentRoadUTexValue, 0),
                points[0].roadWidth));
            #endregion
        }
        #endregion

        #region GenerateTunnelsAndLandscapeObjects
        private void GenerateTunnelsAndLandscapeObjects(
            List<TrackData.RoadHelper> roadHelpers,
            List<TrackData.NeutralObject> neutralObjects,
            Landscape landscape)
        {
            #region Find out where the tunnels are
            // Go through all tunnel helpers along the road,
            // everything close enough (<25) is interessting for us.
            int helperStartedNum = -1;
            TrackData.RoadHelper.HelperType remType =
                TrackData.RoadHelper.HelperType.Reset;
            for (int num = 0; num < points.Count; num++)
            {
                Vector3 pos = points[num].pos;
                foreach (TrackData.RoadHelper roadHelper in roadHelpers)
                {
                    float dist = Vector3.Distance(roadHelper.pos, pos);
                    if (dist < 25.0f)
                    {
                        if (helperStartedNum >= 0)
                        {
                            helperPositions.Add(new RoadHelperPosition(
                                remType, helperStartedNum, num));
                            // Reset?
                            if (roadHelper.type == TrackData.RoadHelper.HelperType.Reset)
                                helperStartedNum = -1;
                            else
                            {
                                // Start new part
                                helperStartedNum = num;
                                remType = roadHelper.type;
                            }
                        }
                        else
                        {
                            helperStartedNum = num;
                            remType = roadHelper.type;
                        }

                        // Remove this roadHelper (don't use it again)!
                        roadHelpers.Remove(roadHelper);
                        break;
                    }
                }
            }

            // Still a helper open? Then close it close before the end!
            if (helperStartedNum > 0)
                helperPositions.Add(new RoadHelperPosition(
                    remType, helperStartedNum, points.Count - 3));
            #endregion

            #region Copy over neutral objects for landscape rendering
            if (landscape != null)
            {
                for (int num = 0; num < neutralObjects.Count; num++)
                {
                    TrackData.NeutralObject obj = neutralObjects[num];
                    landscape.AddObjectToRender(obj.modelName, obj.matrix, false);
                }
            }
            #endregion
        }
        #endregion
    }
}
