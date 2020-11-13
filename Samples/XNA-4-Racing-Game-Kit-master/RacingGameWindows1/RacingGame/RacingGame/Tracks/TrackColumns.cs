#region File Description
//-----------------------------------------------------------------------------
// TrackColumns.cs
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
using Model = RacingGame.Graphics.Model;
using RacingGame.Landscapes;
using RacingGame.Shaders;
#endregion

namespace RacingGame.Tracks
{
    /// <summary>
    /// Track columns
    /// </summary>
    class TrackColumns : IDisposable
    {
        #region Constants
        #region Guardrail vertices
        /// <summary>
        /// Column vertices, just a simple circle!
        /// </summary>
        readonly TangentVertex[] BaseColumnVertices =
            new TangentVertex[]
            {
                // 0
                new TangentVertex(
                new Vector3(1, 0, 0), new Vector2(0.0f / 6.0f, 0.0f),
                new Vector3(1, 0, 0), new Vector3(0, 0, -1)),
                // 1
                new TangentVertex(
                new Vector3(0.5f, 0.866025f, 0), new Vector2(1.0f / 6.0f, 0.0f),
                new Vector3(0.5f, 0.866025f, 0), new Vector3(0, 0, -1)),
                // 2
                new TangentVertex(
                new Vector3(-0.5f, 0.866025f, 0), new Vector2(2.0f/ 6.0f, 0.0f),
                new Vector3(-0.5f, 0.866025f, 0), new Vector3(0, 0, -1)),
                // 3
                new TangentVertex(
                new Vector3(-1, 0, 0), new Vector2(3.0f / 6.0f, 0.0f),
                new Vector3(-1, 0, 0), new Vector3(0, 0, -1)),
                // 4
                new TangentVertex(
                new Vector3(-0.5f, -0.866025f, 0), new Vector2(4.0f / 6.0f, 0.0f),
                new Vector3(-0.5f, -0.866025f, 0), new Vector3(0, 0, -1)),
                // 5
                new TangentVertex(
                new Vector3(0.5f, -0.866025f, 0), new Vector2(5.0f / 6.0f, 0.0f),
                new Vector3(0.5f, -0.866025f, 0), new Vector3(0, 0, -1)),
                // 6 (duplicated first to wrap around tex coords)
                new TangentVertex(
                new Vector3(1, 0, 0), new Vector2(6.0f / 6.0f, 0.0f),
                new Vector3(1, 0, 0), new Vector3(0, 0, -1)),
            };
        #endregion

        /// <summary>
        /// Gap between the seperate piles.
        /// </summary>
        const float ColumnsDistance = 33.0f;

        /// <summary>
        /// Column rendering height above the landscape ground.
        /// </summary>
        const float ColumnGroundHeight = 1.0f;

        /// <summary>
        /// Minimium column height we need, else we skip generation!
        /// </summary>
        const float MinimumColumnHeight = 2.5f;

        /// <summary>
        /// Put columns a little bit on the inside of the road,
        /// we don't want them to come through the road.
        /// </summary>
        const float TopColumnSubHeight = 0.55f;
        #endregion

        #region Variables
        /// <summary>
        /// Column position list.
        /// </summary>
        List<Vector3> columnPositions = new List<Vector3>();
        /// <summary>
        /// Rail vertices.
        /// </summary>
        TangentVertex[] columnVertices = null;
        /// <summary>
        /// Vertex buffer of guard rail.
        /// </summary>
        VertexBuffer columnVb = null;
        /// <summary>
        /// Index buffer orf guard rail.
        /// </summary>
        IndexBuffer columnIb = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Create track columns
        /// </summary>
        /// <param name="points">Points</param>
        /// <param name="landscape">Landscape for getting the ground height</param>
        public TrackColumns(List<TrackVertex> points, Landscape landscape)
        {
            if (landscape == null)
                return;

            #region Find out column positions
            float lastColumnsDistance = ColumnsDistance;
            List<Matrix> columnPointSpacesTop = new List<Matrix>();
            List<Matrix> columnPointSpacesBottom = new List<Matrix>();
            for (int num = 0; num < points.Count; num++)
            {
                // Distance of the current position to the next position
                float distance = Vector3.Distance(
                    points[(num + 1) % points.Count].pos, points[num].pos);

                // Uniform calculation of the distance for the columns,
                // so it doesn't matter if there is a gap of 2 or 200 m
                // Have we reach or go over the ColumnsDistance?
                if (lastColumnsDistance - distance <= 0)
                {
                    // Catmull interpolation, instead the linear interpolation, for a
                    // better position calculation, especially in curves
                    Vector3 p1 = points[num - 1 < 0 ? points.Count - 1 : num - 1].pos;
                    Vector3 p2 = points[num].pos;
                    Vector3 p3 = points[(num + 1) % points.Count].pos;
                    Vector3 p4 = points[(num + 2) % points.Count].pos;

                    Vector3 holderPoint = Vector3.CatmullRom(p1, p2, p3, p4,
                        lastColumnsDistance / distance);

                    // Just find out how much this point is pointing up
                    float draft = Vector3.Dot(points[num].up, new Vector3(0, 0, 1));
                    // And don't add if height is too small!
                    float columnHeight = holderPoint.Z -
                        landscape.GetMapHeight(holderPoint.X, holderPoint.Y);

                    // Store the position for this holder
                    if (draft > 0.3f &&//< 0 MaxColumnGenerationAngel &&
                        columnHeight > MinimumColumnHeight)
                    {
                        columnPositions.Add(holderPoint);

                        // The unit vectors for our local point space
                        Vector3 right = points[num].right;
                        Vector3 dir = points[num].dir;
                        Vector3 up = points[num].up;

                        // Create the coordinate system for the current point by the 3
                        // unit vectors.
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

                        // Remember point space
                        columnPointSpacesTop.Add(pointSpace);

                        // Same for bottom, but don't use up vector (let it stay default)
                        pointSpace = Matrix.Identity;
                        Vector3 upVector = new Vector3(0, 0, 1);
                        // Rebuild right vector (to make it 90 degree to our up vector)
                        Vector3 rightVector = Vector3.Cross(dir, upVector);
                        pointSpace.M11 = rightVector.X;
                        pointSpace.M12 = rightVector.Y;
                        pointSpace.M13 = rightVector.Z;
                        pointSpace.M21 = dir.X;
                        pointSpace.M22 = dir.Y;
                        pointSpace.M23 = dir.Z;
                        columnPointSpacesBottom.Add(pointSpace);
                    }

                    // We have just set a pile, the next pile will be set after
                    // reaching the next holder gap.
                    lastColumnsDistance += ColumnsDistance;
                }

                // The distance we have to cover until the next position.
                // We subtract our current distance from the remaining gap distance,
                // which will then be checked in the next loop.
                lastColumnsDistance -= distance;
            }
            #endregion

            #region Generate vertex buffer
            columnVertices = new TangentVertex[
                columnPositions.Count * BaseColumnVertices.Length * 2];

            // Go through all columns
            for (int num = 0; num < columnPositions.Count; num++)
            {
                Vector3 pos = columnPositions[num];

                // Find out the current landscape height here
                Vector3 bottomPos = new Vector3(pos.X, pos.Y,
                    landscape.GetMapHeight(pos.X, pos.Y) +
                    ColumnGroundHeight);
                Vector3 topPos = new Vector3(pos.X, pos.Y,
                    pos.Z - TopColumnSubHeight);
                // Calculate top v tex coord for this column
                float topTexV =
                    Vector3.Distance(topPos, bottomPos) / (MathHelper.Pi * 2);

                // Use the BaseColumnVertices twice, once for the bottom and then for the
                // top part of our generated column.
                for (int topBottom = 0; topBottom < 2; topBottom++)
                {
                    // Go to all BaseColumnVertices
                    for (int i = 0; i < BaseColumnVertices.Length; i++)
                    {
                        int vertIndex = num * BaseColumnVertices.Length * 2 +
                            topBottom * BaseColumnVertices.Length + i;

                        // For the top positions, modify them them to fit directly
                        // on the bottom side of our road. Same for bottom, but don't
                        // modify the z value
                        Matrix transformMatrix = topBottom == 0 ?
                            columnPointSpacesBottom[num] : columnPointSpacesTop[num];

                        // We don't have to transform the vertices much, just adjust
                        // the z value and the v tex coord.
                        columnVertices[vertIndex] =
                            new TangentVertex(
                            (topBottom == 0 ? bottomPos : topPos) +
                            Vector3.Transform(BaseColumnVertices[i].pos,
                                transformMatrix),
                            BaseColumnVertices[i].U, topBottom == 0 ? 0 : topTexV,
                            Vector3.Transform(BaseColumnVertices[i].normal,
                                transformMatrix),
                            Vector3.Transform(-BaseColumnVertices[i].tangent,
                            transformMatrix));
                    }
                }

                // Also add the bottom position to the list of holders we want
                // to render later.
                if (landscape != null &&
                    // This is not really required, we can easily optimize this out.
                    BaseGame.HighDetail)
                    landscape.AddObjectToRender(
                        "RoadColumnSegment",
                        new Vector3(bottomPos.X, bottomPos.Y,
                        bottomPos.Z - ColumnGroundHeight));
            }

            // Create the vertex buffer from our vertices.
            // fix
            //columnVb = new VertexBuffer(
            //    BaseGame.Device,
            //    typeof(TangentVertex),
            //    columnVertices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            columnVb = new VertexBuffer(
                BaseGame.Device,
                typeof(TangentVertex),
                columnVertices.Length,
                BufferUsage.WriteOnly);
            columnVb.SetData(columnVertices);
            #endregion

            #region GenerateIndexBuffer
            // Count of quads (polygons) we have for each column
            int quadPolysPerColumn = BaseColumnVertices.Length - 1;
            int[] indices =
                new int[(2 * 3 * quadPolysPerColumn) * columnPositions.Count];
            // Current vertex index
            int vertexIndex = 0;
            // Helper variable, current index of the indices list
            int indicesIndex = 0;
            for (int num = 0; num < columnPositions.Count; num++)
            {
                // Set all quads of the column
                for (int j = 0; j < quadPolysPerColumn; j++)
                {
                    indicesIndex = 3 * 2 * (num * quadPolysPerColumn + j);

                    // 1. Polygon
                    indices[indicesIndex] = vertexIndex + j;
                    indices[indicesIndex + 1] =
                        vertexIndex + 1 + BaseColumnVertices.Length + j;
                    indices[indicesIndex + 2] = vertexIndex + 1 + j;

                    // 2. Polygon
                    indices[indicesIndex + 3] = indices[indicesIndex + 1];
                    indices[indicesIndex + 4] = indices[indicesIndex];
                    indices[indicesIndex + 5] =
                        vertexIndex + BaseColumnVertices.Length + j;
                }

                // Go to next column
                vertexIndex += BaseColumnVertices.Length * 2;
            }

            // Create the index buffer from our indices.
            // fix
            //columnIb = new IndexBuffer(
            //    BaseGame.Device,
            //    typeof(int),
            //    indices.Length,
            //    ResourceUsage.WriteOnly,
            //    ResourceManagementMode.Automatic);
            columnIb = new IndexBuffer(
                BaseGame.Device,
                typeof(int),
                indices.Length,
                BufferUsage.WriteOnly);
            columnIb.SetData(indices);
            #endregion
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            columnVb.Dispose();
            columnIb.Dispose();
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        public void Render(Material columnMaterial)
        {
            // We use tangent vertices for everything here

            // Restore the world matrix
            BaseGame.WorldMatrix = Matrix.Identity;

            // Render all columns
            ShaderEffect.normalMapping.Render(
                columnMaterial,
                "Specular20",
                new BaseGame.RenderHandler(RenderColumnVertices));

            /*now done in landscape object rendering
            // And also render all the holders
            foreach (Vector3 pos in columnHolderPositions)
            {
                holderModel.Render(pos);
            }
             */

            // Restore the world matrix
            BaseGame.WorldMatrix = Matrix.Identity;
        }

        /// <summary>
        /// Render column vertices
        /// </summary>
        private void RenderColumnVertices()
        {
            if (columnVertices == null)
                return;

            BaseGame.Device.SetVertexBuffer(columnVb);
            BaseGame.Device.Indices = columnIb;
            BaseGame.Device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0, 0, columnVertices.Length,
                0, (BaseColumnVertices.Length - 1) *
                columnPositions.Count * 2);
        }
        #endregion
    }
}
