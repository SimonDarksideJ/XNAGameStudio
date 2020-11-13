#region File Description
//-----------------------------------------------------------------------------
// Track.cs
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
using System.Text;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Landscapes;
using RacingGame.Shaders;
using Model = RacingGame.Graphics.Model;
using RacingGame.GameLogic;
#endregion

namespace RacingGame.Tracks
{
    /// <summary>
    /// Track for our road in the game. Generated with help of the TrackLine
    /// class, which is loaded by a couple of spline points we get exported
    /// from 3ds max.
    /// </summary>
    public class Track : TrackLine, IDisposable
    {
        #region Constants
        /// <summary>
        /// Factor for streching the width of the road back texture, smaller
        /// values will strech the texture more. 1.0f means we use the same as
        /// the road, which is defined in    TrackVertex.RoadWidthScale.
        /// </summary>
        const float RoadBackHullTextureWidthFactor = 1.0f;

        /// <summary>
        /// Factor for streching the width of the road tunnel texture, smaller
        /// values will strech the texture more. 1.0f means we use the same as
        /// the road, which is defined in    TrackVertex.RoadWidthScale.
        /// </summary>
        const float RoadTunnelTextureWidthFactor = 0.25f;

        /// <summary>
        /// Factor for texels we use from the roadBack texture for the sides.
        /// Most of the texture is used for the back side, but the sides
        /// are also at the top and bottom of the texture, use this factor
        /// to find out how much it is in the texture.
        /// </summary>
        const float RoadBackSideTextureHeight = 0.135f;

        /// <summary>
        /// Same as RoadBackSideTextureHeight, but we have more space for
        /// the sides in the tunnel texture!
        /// </summary>
        const float RoadTunnelSideTextureHeight = 0.235f;

        /// <summary>
        /// Palm and latern gap for the autogeneration.
        /// </summary>
        const float PalmAndLaternGap = 20.0f;

        /// <summary>
        /// Put a checkpoint every 500m
        /// </summary>
        const float CheckpointGap = 500.0f;

        /// <summary>
        /// Gap for signs, don't put them closer than this together.
        /// </summary>
        const float SignGap = 24;
        #endregion

        #region Variables
        /// <summary>
        /// Road material for the top of the road.
        /// </summary>
        Material roadMaterial = new Material(
            "Road", "RoadNormal");
        /// <summary>
        /// Road back material, for the other side of the road and the sides.
        /// </summary>
        Material roadBackMaterial = new Material(
            "RoadBack", "RoadBackNormal");

        /// <summary>
        /// Road tunnel material, used whereever we got tunnels.
        /// </summary>
        Material roadTunnelMaterial = new Material(
            // Use mainly ambient color (tunnel uses lightmaps)
            new Color(182, 182, 182),
            new Color(80, 80, 80),
            new Color(64, 64, 64),
            "RoadTunnel", "RoadTunnelNormal", "", "");

        /// <summary>
        /// Road cement material, used for the columns the road is staying on.
        /// </summary>
        Material roadCementMaterial = new Material(
            "RoadCement", "RoadCementNormal");

        /// <summary>
        /// Guard rail material, used for the left and right guard rails.
        /// It is also used for the guard rail 
        /// </summary>
        Material guardRailMaterial = new Material(
            new Color(72, 72, 72),
            new Color(182, 182, 182),
            new Color(225, 225, 225),
            "Leitplanke", "LeitplankeNormal", "", "");

        /// <summary>
        /// Vertices for the road itself.
        /// </summary>
        TangentVertex[] roadVertices = null;
        /// <summary>
        /// Vertex buffer for the road.
        /// </summary>
        VertexBuffer roadVb = null;
        /// <summary>
        /// Index buffer for the road.
        /// </summary>
        IndexBuffer roadIb = null;

        /// <summary>
        /// Vertices for the road back (hull, bottom side, sides).
        /// </summary>
        TangentVertex[] roadBackVertices = null;
        /// <summary>
        /// Vertex buffer for the road back (has different texture coordinates)
        /// </summary>
        VertexBuffer roadBackVb = null;
        /// <summary>
        /// Index buffer for the road back.
        /// </summary>
        IndexBuffer roadBackIb = null;

        /// <summary>
        /// Vertices for the road tunnels (sides and top).
        /// </summary>
        TangentVertex[] roadTunnelVertices = null;
        /// <summary>
        /// Remember road tunnel indices because determinating the count is
        /// not so easy (we can have multiple tunnels in here).
        /// </summary>
        int[] roadTunnelIndices = null;
        /// <summary>
        /// Vertex buffer for the road back (has different texture coordinates)
        /// </summary>
        VertexBuffer roadTunnelVb = null;
        /// <summary>
        /// Index buffer for the road back.
        /// </summary>
        IndexBuffer roadTunnelIb = null;

        /// <summary>
        /// Left and right guard rails.
        /// Renders the autogenerated rails and the helper piles to backup the
        /// gruard rail (leitplanke_pfahl.x model file, which is generated
        /// seperately).
        /// </summary>
        GuardRail leftRail = null,
            rightRail = null;

        /// <summary>
        /// Track columns, used for the columns the road is staying on.
        /// Renders autogenerated column objects and a helper segment for the
        /// ground (TrackColumnSegment.x model file, which is generated
        /// seperately).
        /// </summary>
        TrackColumns columns = null;

        /// <summary>
        /// Remember checkpoint segment positions for easier checkpoint checking.
        /// </summary>
        List<int> checkpointSegmentPositions = new List<int>();
        #endregion

        #region Properties
        /// <summary>
        /// Start position
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 StartPosition
        {
            get
            {
                return points[0].pos;
            }
        }

        /// <summary>
        /// Start direction
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 StartDirection
        {
            get
            {
                return points[0].dir;
            }
        }

        /// <summary>
        /// Start up vector
        /// </summary>
        /// <returns>Vector 3</returns>
        public Vector3 StartUpVector
        {
            get
            {
                return points[0].up;
            }
        }

        /// <summary>
        /// Length
        /// </summary>
        /// <returns>Float</returns>
        public float Length
        {
            get
            {
                return points.Count * 100.0f / (float)NumberOfIterationsPer100Meters;
            }
        }

        /// <summary>
        /// Number of segments
        /// </summary>
        /// <returns>int</returns>
        public int NumberOfSegments
        {
            get
            {
                return points.Count;
            }
        }

        /// <summary>
        /// Remember checkpoint segment positions for easier checkpoint checking.
        /// </summary>
        public List<int> CheckpointSegmentPositions
        {
            get
            {
                return checkpointSegmentPositions;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create track
        /// </summary>
        /// <param name="setTrackName">Track name to load</param>
        /// <param name="landscape">Landscape to check if we are above it</param>
        public Track(string setTrackName, Landscape landscape)
            : base(TrackData.Load(setTrackName), landscape)
        {
            GenerateVerticesAndObjects(landscape);
        }

        #region Reload
        /// <summary>
        /// Reload
        /// </summary>
        /// <param name="setTrackName">Track name</param>
        /// <param name="landscape">Landscape</param>
        public void Reload(string setTrackName, Landscape landscape)
        {
            // Do we need to load the base track again?
            // Always reload! Else we might mess up the checkpoints, etc.
            base.Load(TrackData.Load(setTrackName), landscape);

            GenerateVerticesAndObjects(landscape);
        }
        #endregion

        #region GenerateVerticesAndObjects
        /// <summary>
        /// Generate vertices and objects
        /// </summary>
        private void GenerateVerticesAndObjects(Landscape landscape)
        {
            #region Generate the road vertices
            // Each road segment gets 5 points:
            // left, left middle, middle, right middle, right.
            // The reason for this is that we would bad triangle errors if the
            // road gets wider and wider. This happens because we need to render
            // quad, but we can only render triangles, which often have different
            // orientations, which makes the road very bumpy. This still happens
            // with 8 polygons instead of 2, but it is much better this way.
            // Another trick is not to do so much iterations in TrackLine, which
            // causes this problem. Better to have a not so round track, but at
            // least the road up/down itself is smooth.
            // The last point is duplicated (see TrackLine) because we have 2 sets
            // of texture coordinates for it (begin block, end block).
            // So for the index buffer we only use points.Count-1 blocks.
            roadVertices = new TangentVertex[points.Count * 5];

            // Current texture coordinate for the roadway (in direction of movement)
            for (int num = 0; num < points.Count; num++)
            {
                // Get vertices with help of the properties in the TrackVertex class.
                // For the road itself we only need vertices for the left and right
                // side, which are vertex number 0 and 1.
                roadVertices[num * 5 + 0] = points[num].RightTangentVertex;
                roadVertices[num * 5 + 1] = points[num].MiddleRightTangentVertex;
                roadVertices[num * 5 + 2] = points[num].MiddleTangentVertex;
                roadVertices[num * 5 + 3] = points[num].MiddleLeftTangentVertex;
                roadVertices[num * 5 + 4] = points[num].LeftTangentVertex;
            }

            // fix
            //roadVb = new VertexBuffer(
            //    BaseGame.Device,
            //    typeof(TangentVertex),
            //    roadVertices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            roadVb = new VertexBuffer(
                BaseGame.Device,
                typeof(TangentVertex),
                roadVertices.Length,
                BufferUsage.WriteOnly);
            roadVb.SetData(roadVertices);

            // Also calculate all indices, we have 8 polygons for each segment with
            // 3 vertices each. We got 1 segment less than points because the
            // last point is duplicated (different tex coords).
            int[] indices = new int[(points.Count - 1) * 8 * 3];
            int vertexIndex = 0;
            for (int num = 0; num < points.Count - 1; num++)
            {
                // We only use 3 vertices (and the next 3 vertices),
                // but we have to construct all 24 indices for our 4 polygons.
                for (int sideNum = 0; sideNum < 4; sideNum++)
                {
                    // Each side needs 2 polygons.

                    // 1. Polygon
                    indices[num * 24 + 6 * sideNum + 0] =
                        vertexIndex + sideNum;
                    indices[num * 24 + 6 * sideNum + 1] =
                        vertexIndex + 5 + 1 + sideNum;
                    indices[num * 24 + 6 * sideNum + 2] =
                        vertexIndex + 5 + sideNum;

                    // 2. Polygon
                    indices[num * 24 + 6 * sideNum + 3] =
                        vertexIndex + 5 + 1 + sideNum;
                    indices[num * 24 + 6 * sideNum + 4] =
                        vertexIndex + sideNum;
                    indices[num * 24 + 6 * sideNum + 5] =
                        vertexIndex + 1 + sideNum;
                }

                // Go to the next 5 vertices
                vertexIndex += 5;
            }

            // Set road back index buffer
            // fix
            //roadIb = new IndexBuffer(
            //    BaseGame.Device,
            //    typeof(int),
            //    indices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            roadIb = new IndexBuffer(
                BaseGame.Device,
                typeof(int),
                indices.Length,
                BufferUsage.WriteOnly);
            roadIb.SetData(indices);
            #endregion

            #region Generate the road back vertices
            // We need 4 vertices per cross-section edge of the road back hull
            roadBackVertices = new TangentVertex[points.Count * 4];
            for (int num = 0; num < points.Count; num++)
            {
                // Left side of the road
                roadBackVertices[num * 4 + 0] =
                    points[num].LeftTangentVertex;
                roadBackVertices[num * 4 + 0].uv = new Vector2(
                    roadBackVertices[num * 4 + 0].U * RoadBackHullTextureWidthFactor,
                    0.0f);

                // Left lower side of the road
                roadBackVertices[num * 4 + 1] =
                    points[num].BottomLeftSideTangentVertex;
                roadBackVertices[num * 4 + 1].uv = new Vector2(
                    roadBackVertices[num * 4 + 0].U * RoadBackHullTextureWidthFactor,
                    RoadBackSideTextureHeight);

                // Right lower side of the road
                roadBackVertices[num * 4 + 2] =
                    points[num].BottomRightSideTangentVertex;
                roadBackVertices[num * 4 + 2].uv = new Vector2(
                    roadBackVertices[num * 4 + 0].U * RoadBackHullTextureWidthFactor,
                    1.0f - RoadBackSideTextureHeight);

                // Right side of the road
                roadBackVertices[num * 4 + 3] =
                    points[num].RightTangentVertex;
                roadBackVertices[num * 4 + 3].uv = new Vector2(
                    roadBackVertices[num * 4 + 3].U * RoadBackHullTextureWidthFactor,
                    1.0f);
            }

            // Set road back vertex buffer
            // fix
            //roadBackVb = new VertexBuffer(
            //    BaseGame.Device,
            //    typeof(TangentVertex),
            //    roadBackVertices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            roadBackVb = new VertexBuffer(
                BaseGame.Device,
                typeof(TangentVertex),
                roadBackVertices.Length,
                BufferUsage.WriteOnly);
            roadBackVb.SetData(roadBackVertices);

            // Also calculate all indices, we have 6 polygons for each segment with
            // 3 vertices each. We got 1 segment less than points because the
            // last point is duplicated (different tex coords).
            int[] backIndices = new int[(points.Count - 1) * 6 * 3];
            vertexIndex = 0;
            for (int num = 0; num < points.Count - 1; num++)
            {
                // We only use 4 vertices (and the next 4 vertices),
                // but we have to construct all 18 indices for our 6 polygons.
                for (int sideNum = 0; sideNum < 3; sideNum++)
                {
                    // Each side needs 2 polygons.

                    // 1. Polygon
                    backIndices[num * 18 + 6 * sideNum + 0] =
                        vertexIndex + sideNum;
                    backIndices[num * 18 + 6 * sideNum + 1] =
                        vertexIndex + 5 + sideNum;
                    backIndices[num * 18 + 6 * sideNum + 2] =
                        vertexIndex + 4 + sideNum;

                    // 2. Polygon
                    backIndices[num * 18 + 6 * sideNum + 3] =
                        vertexIndex + 5 + sideNum;
                    backIndices[num * 18 + 6 * sideNum + 4] =
                        vertexIndex + sideNum;
                    backIndices[num * 18 + 6 * sideNum + 5] =
                        vertexIndex + 1 + sideNum;
                }

                // Go to the next 4 vertices
                vertexIndex += 4;
            }

            // Set road back index buffer
            // fix
            //roadBackIb = new IndexBuffer(
            //    BaseGame.Device,
            //    typeof(int),
            //    backIndices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            roadBackIb = new IndexBuffer(
                BaseGame.Device,
                typeof(int),
                backIndices.Length,
                BufferUsage.WriteOnly);
            roadBackIb.SetData(backIndices);
            #endregion

            #region Generate the road tunnel vertices
            // Only generate tunnels for the parts were we want to have tunnels for.
            int totalTunnelLength = 0;
            foreach (RoadHelperPosition tunnelPos in helperPositions)
                if (tunnelPos.type == TrackData.RoadHelper.HelperType.Tunnel)
                    totalTunnelLength += 1 + (tunnelPos.endNum - tunnelPos.startNum);

            // Lets use 4 vertices per segment, we could improve that later
            // by adding more vertices for a round tunnel.
            roadTunnelVertices = new TangentVertex[totalTunnelLength * 4];
            vertexIndex = 0;
            foreach (RoadHelperPosition tunnelPos in helperPositions)
                if (tunnelPos.type == TrackData.RoadHelper.HelperType.Tunnel)
                    for (int num = tunnelPos.startNum; num <= tunnelPos.endNum; num++)
                    {
                        // Left side of the road
                        roadTunnelVertices[vertexIndex + 0] =
                            points[num].LeftTangentVertex;
                        roadTunnelVertices[vertexIndex + 0].uv = new Vector2(
                            roadTunnelVertices[vertexIndex + 0].U
                            * RoadTunnelTextureWidthFactor, 0.0f);

                        // Left top side of the road
                        roadTunnelVertices[vertexIndex + 1] =
                            points[num].TunnelTopLeftSideTangentVertex;
                        roadTunnelVertices[vertexIndex + 1].uv = new Vector2(
                            roadTunnelVertices[vertexIndex + 1].U *
                            RoadTunnelTextureWidthFactor, RoadTunnelSideTextureHeight);

                        // Right top side of the road
                        roadTunnelVertices[vertexIndex + 2] =
                            points[num].TunnelTopRightSideTangentVertex;
                        roadTunnelVertices[vertexIndex + 2].uv = new Vector2(
                            roadTunnelVertices[vertexIndex + 2].U *
                            RoadTunnelTextureWidthFactor,
                            1.0f - RoadTunnelSideTextureHeight);

                        // Right side of the road
                        roadTunnelVertices[vertexIndex + 3] =
                            points[num].RightTangentVertex;
                        roadTunnelVertices[vertexIndex + 3].uv = new Vector2(
                            roadTunnelVertices[vertexIndex + 3].U *
                            RoadTunnelTextureWidthFactor, 1.0f);

                        // Adjust normals for the 2 lower points
                        roadTunnelVertices[vertexIndex + 0].normal *= -1;
                        roadTunnelVertices[vertexIndex + 3].normal *= -1;
                        roadTunnelVertices[vertexIndex + 0].tangent *= -1;
                        roadTunnelVertices[vertexIndex + 3].tangent *= -1;

                        vertexIndex += 4;
                    }

            // Set road back vertex buffer
            if (roadTunnelVertices.Length > 0)
            {
                // fix
                //roadTunnelVb = new VertexBuffer(
                //    BaseGame.Device,
                //    typeof(TangentVertex),
                //    roadTunnelVertices.Length,
                //    ResourceUsage.WriteOnly,
                //    ResourceManagementMode.Automatic);
                roadTunnelVb = new VertexBuffer(
                   BaseGame.Device,
                   typeof(TangentVertex),
                   roadTunnelVertices.Length,
                   BufferUsage.WriteOnly);
                roadTunnelVb.SetData(roadTunnelVertices);

                // Also calculate all indices, we have 6 polygons for each segment with
                // 3 vertices each. We got 1 segment less than points because the
                // last point is duplicated (different tex coords).
                int totalIndices = 0;
                foreach (RoadHelperPosition tunnelPos in helperPositions)
                    if (tunnelPos.type == TrackData.RoadHelper.HelperType.Tunnel)
                        totalIndices += (tunnelPos.endNum - tunnelPos.startNum);
                roadTunnelIndices = new int[totalIndices * 6 * 3];
                vertexIndex = 0;
                int tunnelIndex = 0;
                foreach (RoadHelperPosition tunnelPos in helperPositions)
                    if (tunnelPos.type == TrackData.RoadHelper.HelperType.Tunnel)
                    {
                        for (int num = tunnelPos.startNum; num < tunnelPos.endNum; num++)
                        {
                            // We only use 4 vertices (and the next 4 vertices),
                            // but we have to construct all 18 indices for our 6 polygons.
                            for (int sideNum = 0; sideNum < 3; sideNum++)
                            {
                                // Each side needs 2 polygons.
                                // Note: This polygons are rendered with culling off because
                                // we want to see the inside and outside of the tunnel.

                                // 1. Polygon
                                roadTunnelIndices[tunnelIndex + 0] =
                                    vertexIndex + sideNum;
                                roadTunnelIndices[tunnelIndex + 2] =
                                    vertexIndex + 4 + sideNum;
                                roadTunnelIndices[tunnelIndex + 1] =
                                    vertexIndex + 5 + sideNum;

                                // 2. Polygon
                                roadTunnelIndices[tunnelIndex + 3] =
                                    vertexIndex + 5 + sideNum;
                                roadTunnelIndices[tunnelIndex + 5] =
                                    vertexIndex + 1 + sideNum;
                                roadTunnelIndices[tunnelIndex + 4] =
                                    vertexIndex + sideNum;

                                tunnelIndex += 6;
                            }

                            // Go to the next 4 vertices
                            vertexIndex += 4;
                        }

                        // Skip 4 vertices till the next tunnel
                        vertexIndex += 4;
                    }

                // Set road back index buffer
                // fix
                //roadTunnelIb = new IndexBuffer(
                //    BaseGame.Device,
                //    typeof(int),
                //    roadTunnelIndices.Length,
                //    ResourceUsage.WriteOnly,
                //    ResourceManagementMode.Automatic);
                roadTunnelIb = new IndexBuffer(
                    BaseGame.Device,
                    typeof(int),
                    roadTunnelIndices.Length,
                    BufferUsage.WriteOnly);
                roadTunnelIb.SetData(roadTunnelIndices);
            }
            #endregion

            #region Generate guard rails
            leftRail = new GuardRail(points, GuardRail.Modes.Left, landscape);
            rightRail = new GuardRail(points, GuardRail.Modes.Right, landscape);
            #endregion

            #region Generate columns
            columns = new TrackColumns(points, landscape);
            #endregion

            GenerateObjectsForTrack(landscape);
        }
        #endregion

        #region GenerateObjectsForTrack
        private void GenerateObjectsForTrack(Landscape landscape)
        {
            #region Generate palms and laterns for the road
            // Auto generate palms and laterns at the side of our road.
            float lastGap = 0;//PalmAndLaternGap;
            int generatedNum = 0;
            for (int num = 0; num < points.Count; num++)
            {
                bool palms = false;
                bool laterns = false;

                // Check if there are any palms or laterns here
                foreach (RoadHelperPosition helper in helperPositions)
                    if (num >= helper.startNum &&
                        num <= helper.endNum)
                    {
                        if (helper.type == TrackData.RoadHelper.HelperType.Palms)
                            palms = true;
                        else if (helper.type == TrackData.RoadHelper.HelperType.Laterns)
                            laterns = true;
                    }

                // No palms or laterns here?
                if (palms == false &&
                    laterns == false)
                    // Then skip
                    continue;

                // Distance of the current position to the next position
                float distance = Vector3.Distance(
                    points[(num + 1) % points.Count].pos, points[num].pos);

                // Have we reach or go over the holder gap ?
                if (lastGap - distance <= 0)
                {
                    // The unit vectors for our local point space
                    Vector3 right = points[num].right;
                    Vector3 dir = points[num].dir;
                    Vector3 up = points[num].up;

                    // Find out if this is a looping
                    bool upsideDown = up.Z < +0.05f;
                    bool movingUp = dir.Z > 0.65f;
                    bool movingDown = dir.Z < -0.65f;
                    if (upsideDown || movingUp || movingDown)
                        // Skip generation here!
                        continue;

                    // Create the coordinate system for the current point by the 3 unit
                    // vectors.
                    Matrix pointSpace = Matrix.Identity;
                    pointSpace.Right = right;
                    pointSpace.Up = dir;
                    pointSpace.Forward = -up;

                    // Catmull interpolation, instead the linear interpolation, for a
                    // better position calculation, especially in curves.
                    Vector3 p1 = points[num - 1 < 0 ? points.Count - 1 : num - 1].pos;
                    Vector3 p2 = points[num].pos;
                    Vector3 p3 = points[(num + 1) % points.Count].pos;
                    Vector3 p4 = points[(num + 2) % points.Count].pos;

                    Vector3 objPoint = Vector3.CatmullRom(p1, p2, p3, p4,
                        lastGap / distance);

                    generatedNum++;

                    // Store the calculated render matrix for this holder pile object
                    if (landscape != null)
                    {
                        if (palms)
                        {
                            // Check height,
                            // skip palm generation if height is more than 11m
                            if (objPoint.Z -
                                landscape.GetMapHeight(objPoint.X, objPoint.Y) < 11)
                            {
                                int randomNum = RandomHelper.GetRandomInt(4);
                                // Less propability for small palm
                                if (randomNum == 3)
                                    randomNum = RandomHelper.GetRandomInt(4);
                                landscape.AddObjectToRender(
                                    // Random palms
                                    randomNum == 0 ? "AlphaPalm" :
                                    randomNum == 1 ? "AlphaPalm2" :
                                    randomNum == 2 ? "AlphaPalm3" : "AlphaPalmSmall",
                                    // Scale them up a little
                                    Matrix.CreateScale(1.25f) *
                                    // Randomly rotate palms
                                    Matrix.CreateRotationZ(
                                    RandomHelper.GetRandomFloat(0, MathHelper.Pi * 2)) *
                                    // Put left/right
                                    Matrix.CreateTranslation(right *
                                        (generatedNum % 2 == 0 ? 0.6f : -0.6f) *
                                        points[num].roadWidth * TrackVertex.RoadWidthScale)
                                    // Put below the landscape to make it stick
                                    // to the ground
                                    * Matrix.CreateTranslation(new Vector3(0, 0, -50)) *
                                    // And finally we calculate to correct position where
                                    // the palm reaches exactly the holder gap
                                    Matrix.CreateTranslation(objPoint),
                                    // Enable this for shadow map generation
                                    true);
                            }
                        }
                        else
                        {
                            landscape.AddObjectToRender(
                                // Random palms or laterns?
                                "Laterne",
                                // Rotate laterns fixed left/right
                                Matrix.CreateRotationZ(
                                generatedNum % 2 == 0 ? MathHelper.Pi : 0.0f) *
                                // Put left/right
                                Matrix.CreateTranslation(new Vector3(
                                    (generatedNum % 2 == 0 ? 0.5f : -0.5f) *
                                    points[num].roadWidth *
                                    TrackVertex.RoadWidthScale - 0.35f, 0, -0.2f)) *
                                // the ordinary transformation to the
                                // current point space
                                    pointSpace *
                                // At last we calculate to correct position where the
                                // latern reaches exactly the holder gap
                                    Matrix.CreateTranslation(objPoint),
                                // Enable this for shadow map generation
                                    true);
                        }
                    }

                    // We have just set a pile, the next pile will be set after
                    // reaching the next holder gap.
                    lastGap += PalmAndLaternGap;
                }

                // The distance we have to cover until the next position.
                // We subtract our current distance from the remaining gap distance,
                // which will then be checked in the next loop.
                lastGap -= distance;
            }
            #endregion

            #region Generate signs and checkpoints
            // Add the goal and start light models always!
            if (landscape != null)
            {
                Vector3 startRight = points[0].right;
                Vector3 startDir = points[0].dir;
                Vector3 startUp = points[0].up;
                Matrix startPointSpace = Matrix.Identity;
                startPointSpace.Right = startRight;
                startPointSpace.Up = startDir;
                startPointSpace.Forward = -startUp;
                landscape.AddObjectToRender(
                    // Use RacingGame banners
                    "Banner6",
                    // Scale them up to fit on the road
                    Matrix.CreateScale(points[0].roadWidth) *
                    // Scale up 1.1, but compensate 1.2f done in landscape.AddObject
                    Matrix.CreateScale(1.051f) *
                    Matrix.CreateTranslation(new Vector3(0, -5.1f, 0)) *
                    // the ordinary transformation to the current point space
                    startPointSpace *
                    // Add the correct position where the goal is
                    Matrix.CreateTranslation(points[0].pos),
                    // Enable this for shadow map generation
                    true);
                landscape.AddObjectToRender(
                    // All 3 modes are handled and updated in BasePlayer class
                    "StartLight3",
                    Matrix.CreateScale(1.1f) *
                    // Put startlight 6 meters away, and on the right road side!
                    Matrix.CreateTranslation(new Vector3(
                    points[0].roadWidth * TrackVertex.RoadWidthScale * 0.50f - 0.3f,
                    6, -0.2f)) *
                    // the ordinary transformation to the current point space
                    startPointSpace *
                    // Add the correct position where the goal is
                    Matrix.CreateTranslation(points[0].pos),
                    // Enable this for shadow map generation
                    true);
            }

            // Make sure we don't reuse any of the old checkpoint positions.
            checkpointSegmentPositions.Clear();

            // Auto generate checkpoints every 500 meters.
            lastGap = CheckpointGap;
            float signGap = SignGap;
            // Don't add another one near the end!
            for (int num = 0; num < points.Count - 24; num++)
            {
                // Distance of the current position to the next position
                float distance = Vector3.Distance(
                    points[(num + 1) % points.Count].pos, points[num].pos);

                // The unit vectors for our local point space
                Vector3 right = points[num].right;
                Vector3 dir = points[num].dir;
                Vector3 up = points[num].up;

                // Find out if this is a looping
                bool upsideDown = up.Z < +0.05f;
                bool movingUp = dir.Z > 0.65f;
                bool movingDown = dir.Z < -0.65f;
                if (upsideDown || movingUp || movingDown)
                    // Skip generation here!
                    continue;

                // Create the coordinate system for the current point by the 3 unit
                // vectors.
                Matrix pointSpace = Matrix.Identity;
                pointSpace.Right = right;
                pointSpace.Up = dir;
                pointSpace.Forward = -up;

                // Catmull interpolation, instead the linear interpolation, for a
                // better position calculation, especially in curves.
                Vector3 p1 = points[num - 1 < 0 ? points.Count - 1 : num - 1].pos;
                Vector3 p2 = points[num].pos;
                Vector3 p3 = points[(num + 1) % points.Count].pos;
                Vector3 p4 = points[(num + 2) % points.Count].pos;

                // Have we reach or go over the holder gap ?
                if (lastGap - distance <= 0 &&
                    landscape != null)
                {
                    Vector3 objPoint = Vector3.CatmullRom(p1, p2, p3, p4,
                        lastGap / distance);

                    // Store the calculated render matrix for this holder pile object
                    int randomNum = RandomHelper.GetRandomInt(6);
                    landscape.AddObjectToRender(
                        // Random banners
                        randomNum == 0 ? "Banner" :
                        randomNum == 1 ? "Banner2" :
                        randomNum == 2 ? "Banner3" :
                        randomNum == 3 ? "Banner4" :
                        randomNum == 4 ? "Banner5" : "Banner6",
                        // Scale them up to fit on the road
                        Matrix.CreateScale(points[num].roadWidth) *
                        Matrix.CreateTranslation(new Vector3(0, 0, -0.1f)) *
                        // the ordinary transformation to the current point space
                        pointSpace *
                        // And finally we calculate to correct position where the palm
                        // reaches exactly the holder gap
                        Matrix.CreateTranslation(objPoint),
                        // Enable this for shadow map generation
                        true);

                    // Remember this segment for easier checking later.
                    checkpointSegmentPositions.Add(num);

                    // We have just set a pile, the next pile will be set after
                    // reaching the next holder gap.
                    lastGap += CheckpointGap;
                }
                else if (signGap - distance <= 0 &&
                    num >= 25 &&
                    landscape != null)
                {
                    Vector3 objPoint = Vector3.CatmullRom(p1, p2, p3, p4,
                        signGap / distance);

                    // Find out how curvy this point is by going back 25 points.
                    Vector3 backPos = points[(num - 25) % points.Count].pos;
                    // Calculate virtualBackPos as if the road were straight
                    bool loopingAhead = points[(num + 60) % points.Count].up.Z < 0.15f;
                    // Calc angle
                    Vector3 angleVec = Vector3.Normalize(backPos - points[num].pos);
                    float roadAngle = Vector3Helper.GetAngleBetweenVectors(
                        angleVec,
                        Vector3.Normalize(-points[num].dir));
                    // If road goes to the left, use negative angle value!
                    if (Vector3.Distance(points[num].right, angleVec) <
                        Vector3.Distance(-points[num].right, angleVec))
                        roadAngle = -roadAngle;

                    // Now compare, if the backPos is more than 12 meters down,
                    // add a warning sign.
                    if (loopingAhead)//backPos.Z > virtualBackPos.Z + 20)
                    {
                        landscape.AddObjectToRender(
                            "SignWarning",
                            //Matrix.CreateRotationZ(-MathHelper.Pi/2.0f) *
                            // Put it on the right side
                            Matrix.CreateTranslation(new Vector3(
                            points[num].roadWidth *
                            TrackVertex.RoadWidthScale * 0.5f - 0.1f, 0, -0.25f)) *
                            // the ordinary transformation to the current point space
                            pointSpace *
                            // And finally we calculate to correct position where the obj
                            // reaches exactly the holder gap
                            Matrix.CreateTranslation(objPoint),
                            // Enable this for shadow map generation
                            true);
                    }
                    // Else check if the angle less than -24 degrees (pi/7.5)
                    else if (roadAngle < -MathHelper.Pi / 7.5f)
                    {
                        // Show right road sign
                        landscape.AddObjectToRender(
                            "SignCurveRight",
                            Matrix.CreateRotationZ(MathHelper.Pi / 2.0f) *
                            // Put it on the left side
                            Matrix.CreateTranslation(new Vector3(
                            -points[num].roadWidth * TrackVertex.RoadWidthScale
                            * 0.5f - 0.15f, 0, -0.25f)) *
                            // the ordinary transformation to the current point space
                            pointSpace *
                            // And finally we calculate to correct position where the obj
                            // reaches exactly the holder gap
                            Matrix.CreateTranslation(objPoint),
                            // Enable this for shadow map generation
                            true);
                    }
                    // Same for other side
                    else if (roadAngle > MathHelper.Pi / 7.5f)
                    {
                        // Show right road sign
                        landscape.AddObjectToRender(
                            "SignCurveLeft",
                            Matrix.CreateRotationZ(-MathHelper.Pi / 2.0f) *
                            // Put it on the right side
                            Matrix.CreateTranslation(new Vector3(
                            points[num].roadWidth * TrackVertex.RoadWidthScale
                            * 0.5f - 0.15f, 0, -0.25f)) *
                            // the ordinary transformation to the current point space
                            pointSpace *
                            // And finally we calculate to correct position where the obj
                            // reaches exactly the holder gap
                            Matrix.CreateTranslation(objPoint),
                            // Enable this for shadow map generation
                            true);
                    }
                    // Also generate banner signs if roadAngle is at least 18 degrees
                    else if (roadAngle < -MathHelper.Pi / 10.0f ||
                        roadAngle > MathHelper.Pi / 10.0f ||
                        // Randomly generate sign
                        RandomHelper.GetRandomInt(9) == 4)
                    {
                        // Also mix in random curve signs, this is still a curve
                        int rndValue = RandomHelper.GetRandomInt(3);
                        // Randomize again if not that curvy here
                        if (rndValue == 0 &&
                            Math.Abs(roadAngle) < MathHelper.Pi / 24)
                            rndValue = RandomHelper.GetRandomInt(3);
                        else if (Math.Abs(roadAngle) < MathHelper.Pi / 20 &&
                            RandomHelper.GetRandomInt(2) == 1)
                            roadAngle *= -1;

                        // Show right road sign
                        landscape.AddObjectToRender(
                            rndValue == 0 ?
                            (roadAngle > 0 ? "SignCurveLeft" : "SignCurveRight") :
                            (rndValue == 1 ? "Sign" : "Sign2"),
                            Matrix.CreateRotationZ(
                            (roadAngle > 0 ? -1 : 1) * MathHelper.Pi / 2.0f) *
                            // Put it on the left side
                            Matrix.CreateTranslation(new Vector3(
                            (roadAngle > 0 ? 1 : -1) *
                            points[num].roadWidth * TrackVertex.RoadWidthScale * 0.5f -
                            (rndValue == 0 ? 0.15f : 0.005f),
                            0, -0.25f)) *
                            // the ordinary transformation to the current point space
                            pointSpace *
                            // And finally we calculate to correct position where the obj
                            // reaches exactly the holder gap
                            Matrix.CreateTranslation(objPoint),
                            // Enable this for shadow map generation
                            true);
                    }

                    // We have just set a sign (or not), check for next sign after gap.
                    signGap += SignGap;
                }

                // The distance we have to cover until the next position.
                // We subtract our current distance from the remaining gap distance,
                // which will then be checked in the next loop.
                lastGap -= distance;
                signGap -= distance;
            }
            #endregion

            #region Add random landscape objects to fill our level up
            // Randomly generate, but don't collide with existing objects
            // or the track!
            for (int num = 0; num < points.Count; num += 2)
            {
                if (landscape != null)
                {
                    // Get landscape height here
                    float landscapeHeight =
                        landscape.GetMapHeight(points[num].pos.X, points[num].pos.Y);

                    // Skip object generation at great heights!
                    if (points[num].pos.Z - landscapeHeight > 60.0f)
                        continue;
                }

                // The unit vectors for our local point space
                Vector3 right = points[num].right;
                Vector3 dir = points[num].dir;
                Vector3 up = points[num].up;

                // Find out if this is a looping
                bool upsideDown = up.Z < +0.05f;
                bool movingUp = dir.Z > 0.65f;
                bool movingDown = dir.Z < -0.65f;
                if (upsideDown || movingUp || movingDown)
                    // Skip generation here!
                    continue;

                // Show twice as many objects in high details mode
                int randomMaxPropability;
                if (BaseGame.HighDetail)
                    randomMaxPropability = 5;
                else
                    randomMaxPropability = 10;

                // Generate stuff in 20% of the cases
                if (RandomHelper.GetRandomInt(randomMaxPropability) == 0 &&
                    landscape != null)
                {
                    // Get random name
                    int randomObjNum = RandomHelper.GetRandomInt(
                        landscape.autoGenerationNames.Length);

                    // If above 6, generate again
                    if (randomObjNum >= 6)
                        randomObjNum = RandomHelper.GetRandomInt(
                            landscape.autoGenerationNames.Length);

                    // Don't generate so many casinos
                    if (randomObjNum == landscape.autoGenerationNames.Length - 1 &&
                        RandomHelper.GetRandomInt(3) < 2)
                        randomObjNum = RandomHelper.GetRandomInt(
                            landscape.autoGenerationNames.Length);

                    // Ok, generate
                    float distance = RandomHelper.GetRandomFloat(26, 88);
                    // For casinos make sure the object is far enough away.
                    if (randomObjNum == landscape.autoGenerationNames.Length - 1)
                        distance += 20;
                    bool side = RandomHelper.GetRandomInt(2) == 0;
                    float rotation = RandomHelper.GetRandomFloat(0, MathHelper.Pi * 2);
                    landscape.AddObjectToRender(
                        landscape.autoGenerationNames[randomObjNum],
                        rotation,
                        points[num].pos,
                        points[num].right, distance * (side ? 1 : -1));
                }
            }
            #endregion
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
                roadMaterial.Dispose();
                roadBackMaterial.Dispose();
                roadTunnelMaterial.Dispose();
                roadCementMaterial.Dispose();
                guardRailMaterial.Dispose();
                roadVb.Dispose();
                roadIb.Dispose();
                roadBackVb.Dispose();
                roadBackIb.Dispose();
                roadTunnelVb.Dispose();
                roadTunnelIb.Dispose();
                leftRail.Dispose();
                rightRail.Dispose();
                columns.Dispose();
            }
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        public void Render()
        {
            // We use tangent vertices for everything here

            // Restore the world matrix
            BaseGame.WorldMatrix = Matrix.Identity;

            // Make sure Anisotropic filtering is enabled for the road
            BaseGame.Device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            //BaseGame.Device.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
            //BaseGame.Device.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;

            //BaseGame.Device.SamplerStates[0].MipFilter = TextureFilter.Linear;
            //BaseGame.Device.SamplerStates[0].MaxAnisotropy = 8; // 8 is enough, isn't it?

            // Render the road itself
            ShaderEffect.normalMapping.Render(
                roadMaterial,
                // Use antrisopic filtering only if we have a fast GPU
                BaseGame.HighDetail ?
                "SpecularRoad20" :
                "Specular20",
                new BaseGame.RenderHandler(RenderRoadVertices));

            // Render the road back hull
            ShaderEffect.normalMapping.Render(
                roadBackMaterial,
                // Use antrisopic filtering only if we have a fast GPU
                BaseGame.HighDetail ?
                "SpecularRoad20" :
                "Specular20",
                new BaseGame.RenderHandler(RenderRoadBackVertices));

            // Render all tunnels (culling off)
            if (roadTunnelVb != null)
            {
                ShaderEffect.normalMapping.Render(
                    roadTunnelMaterial,
                    "Diffuse20",
                    new BaseGame.RenderHandler(RenderRoadTunnelVertices));
            }

            // Render all guard rails
            leftRail.Render(guardRailMaterial);
            rightRail.Render(guardRailMaterial);

            // Render all columns
            columns.Render(roadCementMaterial);
        }

        /// <summary>
        /// Render road vertices
        /// </summary>
        private void RenderRoadVertices()
        {
            BaseGame.Device.SetVertexBuffer(roadVb);
            BaseGame.Device.Indices = roadIb;
            BaseGame.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, points.Count * 5,
                0, (points.Count - 1) * 8);
        }

        /// <summary>
        /// Render road back vertices
        /// </summary>
        private void RenderRoadBackVertices()
        {
            BaseGame.Device.SetVertexBuffer(roadBackVb);
            BaseGame.Device.Indices = roadBackIb;
            BaseGame.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, points.Count * 4,
                0, (points.Count - 1) * 6);
        }

        /// <summary>
        /// Render road tunnel vertices
        /// </summary>
        private void RenderRoadTunnelVertices()
        {
            if (roadTunnelVb == null)
                return;

            // Disable culling (render tunnel from both sides)
            BaseGame.Device.RasterizerState = RasterizerState.CullNone;

            // Render vertices
            BaseGame.Device.SetVertexBuffer(roadTunnelVb);
            BaseGame.Device.Indices = roadTunnelIb;
            BaseGame.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, roadTunnelVertices.Length,
                0, roadTunnelIndices.Length / 3);

            // Restore culling (default is always counter clockwise)
            BaseGame.Device.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        #endregion

        #region Generate and use shadow for the track
        /// <summary>
        /// Generate shadow
        /// </summary>
        public void GenerateShadow()
        {
            // Generate shadows for the road and the tunnel, ignore the road back
            ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(
                Matrix.Identity);
            // We use tangent vertices for everything here

            // Disable culling (render road and tunnel from both sides,
            // this gives correct shadows to loopings, tunnels and overlappings)
            BaseGame.Device.RasterizerState = RasterizerState.CullNone;

            // Render road and tunnels
            RenderRoadVertices();
            RenderRoadTunnelVertices();

            // Generate shadows for both rails
            leftRail.GenerateShadow();
            rightRail.GenerateShadow();

            // And for all columns
            //not required, we don't see near columns anyway:
            //columns.GenerateShadow();
        }

        /// <summary>
        /// Use shadow
        /// </summary>
        public void UseShadow()
        {
            // Receive shadow on the landscape, just render it out.
            ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
                Matrix.Identity);
            RenderRoadVertices();
            // Tunnel shadows are kinda important ^^
            RenderRoadTunnelVertices();

            // Guard rails do not need to receive shadow often, but it will look
            // very wrong if the car does not throw shadows at the guard rails.
            // For that reason lets include them.
            leftRail.UseShadow();
            rightRail.UseShadow();

            // And for all columns
            //not required, we don't see near columns anyway:
            //columns.UseShadow();
        }
        #endregion

        #region Get track position matrix
        /// <summary>
        /// Get track position matrix, put in a value between 0 and 1 and
        /// you get a position on the track (0=start, 1=end).
        /// </summary>
        /// <param name="trackPositionPercent">Track position percent</param>
        /// <param name="roadWidth">Road width</param>
        /// <param name="nextRoadWidth">Next road width</param>
        /// <returns>Matrix</returns>
        public Matrix GetTrackPositionMatrix(float trackPositionPercent,
            out float roadWidth, out float nextRoadWidth)
        {
            // Make sure we are between 0 and 1
            while (trackPositionPercent < 0)
                trackPositionPercent += 1;
            while (trackPositionPercent > 1)
                trackPositionPercent -= 1;
            int num = ((int)(trackPositionPercent * points.Count)) % points.Count;

            // Get position with catmul rom spline
            TrackVertex p1 = points[num - 1 < 0 ? points.Count - 1 : num - 1];
            TrackVertex p2 = points[num];
            TrackVertex p3 = points[(num + 1) % points.Count];
            TrackVertex p4 = points[(num + 2) % points.Count];

            float eachPointPercent = 1.0f / (float)points.Count;
            float pointPercent =
                (trackPositionPercent - num * eachPointPercent) / eachPointPercent;
            //dunno why this is bumpy
            //Log.Write("pointPercent="+pointPercent);

            Vector3 interpolatedPos = Vector3.CatmullRom(
                p1.pos, p2.pos, p3.pos, p4.pos, pointPercent);
            Vector3 interpolatedDir = Vector3.CatmullRom(
                p1.dir, p2.dir, p3.dir, p4.dir, pointPercent);
            Vector3 interpolatedRight = Vector3.CatmullRom(
                p1.right, p2.right, p3.right, p4.right, pointPercent);
            Vector3 interpolatedUp = Vector3.CatmullRom(
                p1.up, p2.up, p3.up, p4.up, pointPercent);

            // Build matrix with interpolated values and return it
            Matrix mat = Matrix.Identity;
            mat.Right = interpolatedRight;
            mat.Up = interpolatedUp;
            mat.Forward = interpolatedDir;
            mat.Translation = interpolatedPos;

            roadWidth = MathHelper.Lerp(p2.roadWidth, p3.roadWidth, pointPercent) *
                TrackVertex.RoadWidthScale;
            nextRoadWidth = p4.roadWidth * TrackVertex.RoadWidthScale;
            return mat;// *
            //Matrix.CreateTranslation(interpolatedPos);
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
            // Make sure we are between 0 and 1
            if (trackSegmentPercent < 0)
                trackSegmentPercent = 0;
            if (trackSegmentPercent > 1)
                trackSegmentPercent = 1;
            float pointPercent = trackSegmentPercent;
            int num = trackSegmentNum % points.Count;

            // Get position with catmul rom spline
            TrackVertex p1 = points[num - 1 < 0 ? points.Count - 1 : num - 1];
            TrackVertex p2 = points[num];
            TrackVertex p3 = points[(num + 1) % points.Count];
            TrackVertex p4 = points[(num + 2) % points.Count];

            Vector3 interpolatedPos = Vector3.CatmullRom(
                p1.pos, p2.pos, p3.pos, p4.pos, pointPercent);
            Vector3 interpolatedDir = Vector3.CatmullRom(
                p1.dir, p2.dir, p3.dir, p4.dir, pointPercent);
            Vector3 interpolatedRight = Vector3.CatmullRom(
                p1.right, p2.right, p3.right, p4.right, pointPercent);
            Vector3 interpolatedUp = Vector3.CatmullRom(
                p1.up, p2.up, p3.up, p4.up, pointPercent);

            // Build matrix with interpolated values and return it
            Matrix mat = Matrix.Identity;
            mat.Right = interpolatedRight;
            mat.Up = interpolatedUp;
            mat.Forward = interpolatedDir;
            mat.Translation = interpolatedPos;

            roadWidth = MathHelper.Lerp(p2.roadWidth, p3.roadWidth, pointPercent) *
                TrackVertex.RoadWidthScale;
            nextRoadWidth = //p4.roadWidth * TrackVertex.RoadWidthScale;
                MathHelper.Lerp(p3.roadWidth, p4.roadWidth, pointPercent) *
                TrackVertex.RoadWidthScale;
            return mat;
        }
        #endregion

        #region UpdateCarTrackPosition
        /// <summary>
        /// Update car track position
        /// </summary>
        /// <param name="carPos">Car position</param>
        /// <param name="trackSegmentNumber">Track segment number</param>
        /// <param name="trackPositionPercent">Track position percent</param>
        public void UpdateCarTrackPosition(
            Vector3 carPos,
            ref int trackSegmentNumber, ref float trackSegmentPercent)
        {
            // Make sure trackSegmentNumber is valid, its also easier working with
            // num instead of the long name trackSegmentNumber.
            int num = trackSegmentNumber;

            // Check until car is between trackSegmentNumber and the next segemnt
            bool gotCarInThisSegment = false;
            float thisPointDist = 0;
            float nextPointDist = 1;
            int maxNumberOfIterations = 100;
            do
            {
                TrackVertex thisPoint = points[num];
                TrackVertex nextPoint = points[(num + 1) % points.Count];

                // First check if car is behind trackSegmentNumber
                thisPointDist = Vector3Helper.SignedDistanceToPlane(carPos,
                    thisPoint.pos, -thisPoint.dir);
                nextPointDist = Vector3Helper.SignedDistanceToPlane(carPos,
                    nextPoint.pos, nextPoint.dir);
                if (thisPointDist < 0)
                    num--;
                // Then check if we still are inside this segment
                else if (nextPointDist < 0)
                    num++;
                else
                    // Ok, we got it.
                    gotCarInThisSegment = true;

                if (num < 0)
                    num = points.Count - 1;
                if (num >= points.Count)
                    num = 0;

                // Get outa here if we are above the max. iterations!
                if (maxNumberOfIterations-- < 0)
                    return;
            } while (gotCarInThisSegment == false);

            trackSegmentNumber = num;
            // Btw: Is this a tunnel? Then disable lens flares!
            // Check every 10 frames to save a little performance.
            if (BaseGame.TotalFrames % 10 == 0)
                disableLensFlareInTunnel = IsTunnel(num);

            // Also calculate our track segment position
            float segmentLength = thisPointDist + nextPointDist;
            if (segmentLength == 0)
                trackSegmentPercent = 0;
            else
                trackSegmentPercent = thisPointDist / segmentLength;
        }

        /// <summary>
        /// Disable lens flare if we are inside a tunnel, looks wrong
        /// otherwise because we can't do occlusion querying in XNA yet.
        /// </summary>
        internal static bool disableLensFlareInTunnel = false;
        /// <summary>
        /// Is tunnel
        /// </summary>
        /// <param name="trackSegment">Track segment</param>
        /// <returns>Bool</returns>
        public bool IsTunnel(int trackSegment)
        {
            // Check all tunnels
            for (int num = 0; num < helperPositions.Count; num++)
            {
                RoadHelperPosition tunnelPos = helperPositions[num];
                if (tunnelPos.type == TrackData.RoadHelper.HelperType.Tunnel &&
                    trackSegment >= tunnelPos.startNum &&
                    trackSegment <= tunnelPos.endNum)
                    return true;
            }

            // No tunnel found here
            return false;
        }
        #endregion
    }
}
