#region File Description
//-----------------------------------------------------------------------------
// GuardRail.cs
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
using RacingGame.Shaders;
using Model = RacingGame.Graphics.Model;
using RacingGame.Landscapes;
#endregion

namespace RacingGame.Tracks
{
    /// <summary>
    /// Guard rail
    /// </summary>
    class GuardRail : IDisposable
    {
        #region Constants
        #region Guardrail vertices
        /// <summary>
        /// Guard rail vertices, determinted with help of the unit test below!
        /// </summary>
        readonly TangentVertex[] GuardRailVertices =
            new TangentVertex[]
            {
                // 0
                new TangentVertex(
                new Vector3(10, 0, -105), new Vector2(0.0f, 1 - 0.442877f),
                new Vector3(-0.382683f, 0, -0.923880f), new Vector3(0, -1, 0)),
                // 1
                new TangentVertex(
                new Vector3(20, 0, -105), new Vector2(0.0f, 1 - 0.432881f),
                new Vector3(0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 2
                new TangentVertex(
                new Vector3(-10, 0, -75), new Vector2(0.0f, 1 - 0.402893f),
                new Vector3(0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 3
                new TangentVertex(
                new Vector3(-10, 0, -45), new Vector2(0.0f, 1 - 0.372905f),
                new Vector3(0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 4
                new TangentVertex(
                new Vector3(20, 0, -15), new Vector2(0.0f, 1 - 0.342917f),
                new Vector3(0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 5
                new TangentVertex(
                new Vector3(20, 0, 15), new Vector2(0.0f, 1 - 0.312929f),
                new Vector3(0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 6
                new TangentVertex(
                new Vector3(-10, 0, 45), new Vector2(0.0f, 1 - 0.282941f),
                new Vector3(0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 7
                new TangentVertex(
                new Vector3(-10, 0, 75), new Vector2(0.0f, 1 - 0.252953f),
                new Vector3(0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 8
                new TangentVertex(
                new Vector3(20, 0, 105), new Vector2(0.0f, 1 - 0.222965f),
                new Vector3(0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 9
                new TangentVertex(
                new Vector3(10, 0, 105), new Vector2(0.0f, 1 - 0.212969f),
                new Vector3(-0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 10
                new TangentVertex(
                new Vector3(-20, 0, 75), new Vector2(0.0f, 1 - 0.182981f),
                new Vector3(-0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 11
                new TangentVertex(
                new Vector3(-20, 0, 45), new Vector2(0.0f, 1 - 0.152993f),
                new Vector3(-0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 12
                new TangentVertex(
                new Vector3(10, 0, 15), new Vector2(0.0f, 1 - 0.123005f),
                new Vector3(-0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 13
                new TangentVertex(
                new Vector3(10, 0, -15), new Vector2(0.0f, 1 - 0.093017f),
                new Vector3(-0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 14
                new TangentVertex(
                new Vector3(-20, 0, -45), new Vector2(0.0f, 1 - 0.063029f),
                new Vector3(-0.923880f, 0, 0.382683f), new Vector3(0, -1, 0)),
                // 15
                new TangentVertex(
                new Vector3(-20, 0, -75), new Vector2(0.0f, 1 - 0.033041f),
                new Vector3(-0.923880f, 0, -0.382683f), new Vector3(0, -1, 0)),
                // 16 (duplicated first to wrap around tex coords)
                new TangentVertex(
                new Vector3(10, 0, -105), new Vector2(0.0f, 1 - 0.003053f),
                new Vector3(-0.382683f, 0, -0.923880f), new Vector3(0, -1, 0)),
            };
        #endregion

        /// <summary>
        /// Downscale factor for the vertices.
        /// </summary>
        const float CorrectionScale = 0.0019f;

        /// <summary>
        /// Gap between the seperate piles.
        /// </summary>
        const float HolderGap = 15.0f;

        /// <summary>
        /// Guard rail rendering height (little bit above the ground).
        /// </summary>
        const float GuardRailHeight = 1.35f * 1.5f * 0.425f;

        /// <summary>
        /// Put guardrails a little bit on the inside of the road.
        /// </summary>
        public const float InsideRoadDistance = 0.5f;

        /// <summary>
        /// Correction vector to put holder pile at the correct position.
        /// </summary>
        static readonly Vector3 HolderPileCorrectionVector =
            new Vector3(0.225f, 0, 0);
        #endregion

        #region Modes enum
        /// <summary>
        /// We got 2 modes for a guard rail, left and right, depending
        /// on with side of the road we want to have it.
        /// </summary>
        public enum Modes
        {
            Left,
            Right,
        }
        #endregion

        #region Variables
        /// <summary>
        /// Rail points for the whole guard rail.
        /// </summary>
        TrackVertex[] railPoints = null;
        /// <summary>
        /// Rail vertices.
        /// </summary>
        TangentVertex[] railVertices = null;
        /// <summary>
        /// Vertex buffer of guard rail.
        /// </summary>
        VertexBuffer railVb = null;
        /// <summary>
        /// Index buffer orf guard rail.
        /// </summary>
        IndexBuffer railIb = null;

        /*now done in track class!
        /// <summary>
        /// Matrix list for each of the holder pile objects we have to render.
        /// </summary>
        List<Matrix> holderPileMatrices = new List<Matrix>();
         */
        #endregion

        #region Constructor
        /// <summary>
        /// Create guard rail
        /// </summary>
        /// <param name="points">Points of the road itself</param>
        /// <param name="mode">Mode, left or right</param>
        /// <param name="landscape">Landscape</param>
        public GuardRail(List<TrackVertex> points, Modes mode,
            Landscape landscape)
        {
            #region Generate guardrail points
            // First generate a list of points at the side of the road where
            // we are going to generate all the guard rail vertices.
            // Note: We use only half as much points as points provides!
            railPoints = new TrackVertex[points.Count / 2 + 1];
            for (int num = 0; num < railPoints.Length; num++)
            {
                // Make sure we have a closed line, we might have to skip the
                // last points entry because we devided through 2.
                int pointNum = num * 2;
                if (pointNum >= points.Count - 1)
                    pointNum = points.Count - 1;

                // Just copy the points over and manipulate the position and the
                // right vector depending on which side of the guard rail we are
                // going to generate here.
                if (mode == Modes.Left)
                {
                    railPoints[num] = points[pointNum].LeftTrackVertex;
                    // Invert the direction and right vector for the left side
                    // This makes everything point in the other road direction!
                    railPoints[num].right = -railPoints[num].right;
                    railPoints[num].dir = -railPoints[num].dir;
                    // Move it a little inside the road
                    railPoints[num].pos -=
                        railPoints[num].right * InsideRoadDistance;
                }
                else
                {
                    railPoints[num] = points[pointNum].RightTrackVertex;
                    // Move it a little inside the road
                    railPoints[num].pos -=
                        railPoints[num].right * InsideRoadDistance;
                }
            }
            #endregion

            #region Generate vertex buffer
            railVertices =
                new TangentVertex[railPoints.Length * GuardRailVertices.Length];

            // Current texture coordinate for the guardrail in our current direction.
            float uTexValue = 0.5f;
            float lastHolderGap = 0;//HolderGap;
            for (int num = 0; num < railPoints.Length; num++)
            {
                // The unit vectors for our local point space
                Vector3 right = railPoints[num].right;
                Vector3 dir = railPoints[num].dir;
                Vector3 up = railPoints[num].up;

                // Create the coordinate system for the current point by the 3 unit
                // vectors.
                Matrix pointSpace = Matrix.Identity;
                pointSpace.M11 = right.X;
                pointSpace.M12 = right.Y;
                pointSpace.M13 = right.Z;
                pointSpace.M21 = dir.X;
                pointSpace.M22 = dir.Y;
                pointSpace.M23 = dir.Z;
                pointSpace.M31 = up.X;
                pointSpace.M32 = up.Y;
                pointSpace.M33 = up.Z;

                Vector3 localPos = railPoints[num].pos;
                // Adjust the position for the guardrail, put it up a little.
                localPos += up * GuardRailHeight;

                // Set the beginning- or ending point for the guardrail around the
                // current spline position
                for (int i = 0; i < GuardRailVertices.Length; i++)
                {
                    // Transform each of our guardrail points by our local point space
                    // and translate it to our current position.
                    Vector3 pos = Vector3.Transform(
                        GuardRailVertices[i].pos * CorrectionScale,
                        pointSpace * Matrix.CreateTranslation(localPos));
                    // Also transform our normal and tangent data
                    Vector3 normal = Vector3.TransformNormal(
                        // Left side needs inverted normals (side is inverted)
                        (mode == Modes.Left ? -1 : 1) *
                        GuardRailVertices[i].normal,
                        pointSpace);
                    Vector3 tangent = Vector3.TransformNormal(
                        -GuardRailVertices[i].tangent,
                        //GuardRailVertices[i].normal,
                        //new Vector3(0, 0, -1),
                        pointSpace);

                    // Store vertex
                    railVertices[num * GuardRailVertices.Length + i] =
                        new TangentVertex(pos,
                        uTexValue, GuardRailVertices[i].V,
                        normal, tangent);
                }

                // Distance of the current position to the next position
                float distance = Vector3.Distance(
                    railPoints[(num + 1) % railPoints.Length].pos, railPoints[num].pos);

                // Uniform calculation of the texture coordinates for the guardrail,
                // so it doesn't matter if there is a gap of 2 or 200 m
                // -> through "1 / HolderGap" we guarantee that the drilling
                //        (from the texture) is always set in the front of the pile,
                //        no matter which holder gap is set
                // Note: Only display a holder for every 3 texture loops.
                uTexValue += (1 / HolderGap) * distance * 2.0f;

                // Have we reach or go over the holder gap ?
                if (lastHolderGap - distance <= 0)
                {
                    // Catmull interpolation, instead the linear interpolation, for a
                    // better position calculation, especially in curves
                    Vector3 p1 =
                        railPoints[num - 1 < 0 ? railPoints.Length - 1 : num - 1].pos;
                    Vector3 p2 = railPoints[num].pos;
                    Vector3 p3 = railPoints[(num + 1) % railPoints.Length].pos;
                    Vector3 p4 = railPoints[(num + 2) % railPoints.Length].pos;

                    Vector3 holderPoint = Vector3.CatmullRom(p1, p2, p3, p4,
                        lastHolderGap / distance);

                    // Store the calculated render matrix for this holder pile object
                    if (landscape != null &&
                        // Completely ignore all guard rails for low detail
                        // to save performance (few thousand objects per track less)
                        BaseGame.HighDetail)
                        landscape.AddObjectToRender(
                            "GuardRailHolder",
                            // Fix scaling a little
                            //Matrix.CreateScale(0.9f) *
                            Matrix.CreateScale(1.125f) *
                            // First the translation to get the pile to the back of the
                            // guardrail and not in the middle
                            Matrix.CreateTranslation(HolderPileCorrectionVector) *
                            // the ordinary transformation to the current point space
                            pointSpace *
                            // at least we calculate to correct position where the pile
                            // reaches exactly the holder gap
                            Matrix.CreateTranslation(holderPoint),
                            // Optimize performance: Set this to false,
                            // but then we won't have shadows for the holder piles.
                            false);//true);

                    // We have just set a pile, the next pile will be set after
                    // reaching the next holder gap.
                    lastHolderGap += HolderGap;
                }

                // The distance we have to cover until the next position.
                // We subtract our current distance from the remaining gap distance,
                // which will then be checked in the next loop.
                lastHolderGap -= distance;
            }

            // Create the vertex buffer from our vertices.
            //railVb = new VertexBuffer(
            //    BaseGame.Device,
            //    typeof(TangentVertex),
            //    railVertices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            railVb = new VertexBuffer(
                BaseGame.Device,
                typeof(TangentVertex),
                railVertices.Length,
                BufferUsage.WriteOnly);
            railVb.SetData(railVertices);
            #endregion

            #region GenerateIndexBuffer
            // Count of quads (polygons) which creates the current guardrail segment
            int quadPolysPerStrip = GuardRailVertices.Length - 1;
            int[] indices =
                new int[(2 * 3 * quadPolysPerStrip) * (railPoints.Length - 1)];
            // Current vertex index
            int vertexIndex = 0;
            // Helper variable, current index of the indices list
            int indicesIndex = 0;
            for (int num = 0; num < railPoints.Length - 1; num++)
            {
                // Set all quads of the guardrail
                for (int j = 0; j < quadPolysPerStrip; j++)
                {
                    indicesIndex = 3 * 2 * (num * quadPolysPerStrip + j);

                    // 1. Polygon
                    indices[indicesIndex] = vertexIndex + j;
                    indices[indicesIndex + 1] = vertexIndex + 1 + j;
                    indices[indicesIndex + 2] =
                        vertexIndex + 1 + GuardRailVertices.Length + j;

                    // 2. Polygon
                    indices[indicesIndex + 3] = indices[indicesIndex + 2];
                    indices[indicesIndex + 4] =
                        vertexIndex + GuardRailVertices.Length + j;
                    indices[indicesIndex + 5] = indices[indicesIndex];
                }

                vertexIndex += GuardRailVertices.Length;
            }

            // Create the index buffer from our indices.
            //railIb = new IndexBuffer(
            //    BaseGame.Device,
            //    typeof(int),
            //    indices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            railIb = new IndexBuffer(
                BaseGame.Device,
                typeof(int),
                indices.Length,
                BufferUsage.WriteOnly);
            railIb.SetData(indices);
            #endregion
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            railVb.Dispose();
            railIb.Dispose();
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        public void Render(Material guardRailMaterial)
        {
            // We use tangent vertices for everything here

            // Restore the world matrix
            BaseGame.WorldMatrix = Matrix.Identity;

            // Render the complete guardrail
            ShaderEffect.normalMapping.Render(
                guardRailMaterial,
                "Specular20",
                new BaseGame.RenderHandler(RenderGuardRailVertices));

            /*done in track class
            // And also render all the holder piles
            foreach (Matrix mat in holderPileMatrices)
            {
                holderModel.Render(mat);
            }
             */

            // Restore the world matrix
            BaseGame.WorldMatrix = Matrix.Identity;
        }

        /// <summary>
        /// Render guard rail vertices
        /// </summary>
        private void RenderGuardRailVertices()
        {
            BaseGame.Device.SetVertexBuffer(railVb);
            BaseGame.Device.Indices = railIb;
            BaseGame.Device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0, 0, railVertices.Length,
                0, (GuardRailVertices.Length - 1) * (railPoints.Length - 1) * 2);
        }
        #endregion

        #region Generate and use shadow for the guard rails
        /// <summary>
        /// Generate shadow
        /// </summary>
        public void GenerateShadow()
        {
            // Just render out the guard rails (world matrix is already set)
            RenderGuardRailVertices();
        }

        /// <summary>
        /// Use shadow
        /// </summary>
        public void UseShadow()
        {
            // Receive shadow on the guard rails
            RenderGuardRailVertices();
        }
        #endregion
    }
}
