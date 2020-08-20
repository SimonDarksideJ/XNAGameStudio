#region File Description
//-----------------------------------------------------------------------------
// GameBillboard.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Render;
using RobotGameData.Helper;
using RobotGameData.Resource;
using RobotGameData.Collision;
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.GameObject
{
    #region BillboardObject

    public class BillboardObject : INamed
    {
        [Flags]
        public enum UpdateTypes
        {
            None = 0x00000000,
            Position = 0x00000001,
            Texturecoord = 0x00000002,
            Color = 0x00000004,
            Enable = 0x0000008,
            Axis = 0x00000010,
        }

        String name = String.Empty;
        uint updateFlag = 0;
        bool enable = true;
        Vector3 start = Vector3.Zero;
        Vector3 end = Vector3.Zero;
        float height = 1.0f;
        Color color = new Color(255, 255, 255, 255);
        Vector2[] uv = new Vector2[2]
                {
                    Vector2.Zero,
                    Vector2.One,
                };

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        public Vector3 Start
        {
            get { return start; }
            set { start = value; }
        }

        public Vector3 End
        {
            get { return end; }
            set { end = value; }
        }

        public float Size
        {
            get { return height; }
            set { height = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public void AddUpdateType(UpdateTypes billboardUpdate)
        {
            updateFlag |= (uint)billboardUpdate;
        }

        public void RemoveUpdateType(UpdateTypes billboardUpdate)
        {
            updateFlag &= ~(uint)billboardUpdate;
        }

        public Vector2 MinUV
        {
            get { return this.uv[0]; }
            set { this.uv[0] = value; }
        }

        public Vector2 MaxUV
        {
            get { return this.uv[1]; }
            set { this.uv[1] = value; }
        }
    }

    #endregion

    /// <summary>
    /// this sprite always looks at the view matrix in the 3D world.
    /// It has a begin point and an end point in the 3D world.
    /// </summary>
    public class GameBillboard : GameMesh
    {
        #region Fields

        const int vertexStride = 4;
        const int indexStride = 6;

        List<BillboardObject> billboardList = new List<BillboardObject>();

        RenderingSpace renderingSpace = RenderingSpace.World;
        Matrix lastViewMatrix;
        int objectCount = 0;

        bool needToUpdate = false;
        bool alwaysUpdate = false;

        #endregion

        #region Properties

        public int ObjectCount
        {
            get { return objectCount; }
        }

        public RenderingSpace RenderingSpace
        {
            get { return renderingSpace; }
        }

        public static int VertexStride
        {
            get { return vertexStride; }
        }

        public static int IndexStride
        {
            get { return indexStride; }
        }

        #endregion

        /// <summary>
        /// updates the vertex data and draws
        /// It always looks at the view matrix at the center between 
        /// the begin point and the end point of the billboard.
        /// </summary>
        /// <param name="renderTracer">render information</param>
        protected override void OnDraw(RenderTracer renderTracer)
        {
            int count = 0;

            for (int i = 0; i < objectCount; i++)
            {
                if (billboardList[i].Enable == false) continue;
                count++;
            }

            if (count == 0) return;

            PrimitiveCount = count * 2;
            UpdateVertexCount = objectCount * vertexStride;

            //  needs to update?
            if (renderTracer.View != lastViewMatrix)
            {
                needToUpdate = true;
                this.lastViewMatrix = renderTracer.View;
            }

            if (alwaysUpdate || needToUpdate)
            {
                int vertexOffset = 0;
                int indexOffset = 0;

                // calculates inverse view matrix.
                Matrix billboardMatrix = this.TransformedMatrix * renderTracer.View;
                billboardMatrix = Helper3D.Transpose(billboardMatrix);

                // gets inverse view direction.
                Vector3 invertViewAt = billboardMatrix.Forward;
                invertViewAt.Normalize();

                for (int i = 0; i < objectCount; i++)
                {
                    BillboardObject obj = billboardList[i];

                    if (obj.Enable == false) continue;

                    Vector3 vec = Vector3.Zero;
                    Vector3 dir = Vector3.Zero;

                    dir = obj.End - obj.Start;
                    dir.Normalize();
                    Vector3.Cross(ref dir, ref invertViewAt, out vec);
                    vec.Normalize();

                    //  updates vertex positions.
                    SetBufferPosition(ref vertexData, obj, vertexOffset, vec);

                    //  updates texture coordinates.
                    SetBufferTextureCoord(ref vertexData, obj, vertexOffset, 
                                          renderingSpace);

                    //  updates vertex colors.
                    SetBufferColor(ref vertexData, obj, vertexOffset);

                    indexData[indexOffset + 0] = (short)(vertexOffset + 0);
                    indexData[indexOffset + 1] = (short)(vertexOffset + 2);
                    indexData[indexOffset + 2] = (short)(vertexOffset + 1);
                    indexData[indexOffset + 3] = (short)(vertexOffset + 1);
                    indexData[indexOffset + 4] = (short)(vertexOffset + 2);
                    indexData[indexOffset + 5] = (short)(vertexOffset + 3);

                    vertexOffset += vertexStride;
                    indexOffset += indexStride;                    
                }

                if (userPrimitive == false)
                {
                    //  binds the vertex buffer.
                    BindVertexBuffer();

                    //  binds the index buffer.
                    BindIndexBuffer();
                }

                if (needToUpdate)
                    needToUpdate = false;
            }

            // draws mesh
            base.OnDraw(renderTracer);   
        }

        /// <summary>
        /// create billboard objects using the texture.
        /// </summary>
        /// <param name="count">billboard object count</param>
        /// <param name="fileName">texture file name</param>
        /// <param name="renderingSpace">3D render space</param>
        /// <param name="alwaysUpdate"></param>
        public void Create(int count, string fileName, RenderingSpace renderingSpace, 
                            bool alwaysUpdate)
        {
            //  load a texture.
            GameResourceTexture2D resource = 
                                FrameworkCore.ResourceManager.LoadTexture(fileName);

            Create(count, resource.Texture2D, renderingSpace, alwaysUpdate);
        }

        /// <summary>
        /// create billboard objects using the texture.
        /// </summary>
        /// <param name="count">billboard object count</param>
        /// <param name="texture">texture resource</param>
        /// <param name="renderingSpace">3D render space</param>
        /// <param name="alwaysUpdate"></param>
        public void Create(int count, Texture2D texture, RenderingSpace renderingSpace, 
                            bool alwaysUpdate)
        {
            this.objectCount = count;
            this.renderingSpace = renderingSpace;
            this.alwaysUpdate = alwaysUpdate;

            // create billboard objects.
            for (int i = 0; i < count; i++)
            {
                BillboardObject obj = new BillboardObject();
                obj.AddUpdateType(BillboardObject.UpdateTypes.Enable);

                billboardList.Add(obj);
            }

            base.Create(count * vertexStride, count * indexStride, texture);
        }

        protected override void UnloadContent()
        {
            billboardList.Clear();

            base.UnloadContent();
        }
        
        /// <summary>
        /// enables/disables all billboard objects.
        /// </summary>
        public void SetUpdateType(bool billboardUpdate)
        {
            for (int i = 0; i < billboardList.Count; i++)
                SetUpdateType(i, billboardUpdate);
        }

        /// <summary>
        /// enables/disables an individual billboard object.
        /// </summary>
        public void SetUpdateType(int index, bool billboardUpdate)
        {
            if (billboardList.Count <= index || 0 > index)
                throw new ArgumentException("Invalid index.");

            if (billboardList[index].Enable != billboardUpdate)
            {
                billboardList[index].Enable = billboardUpdate;
                needToUpdate = true;
            }
        }

        /// <summary>
        /// configures the begin position of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="position">begin position of the billboard object</param>
        public void SetStart(int index, Vector3 position)
        {
            if (billboardList.Count <= index || 0 > index)
                throw new ArgumentException("Invalid index.");

            billboardList[index].Start = position;
            billboardList[index].AddUpdateType(
                BillboardObject.UpdateTypes.Position);
            needToUpdate = true;
        }

        /// <summary>
        /// configures the begin position of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="x">x-component of begin position of the billboard object</param>
        /// <param name="y">y-component of begin position of the billboard object</param>
        /// <param name="z">z-component of begin position of the billboard object</param>
        public void SetStart(int index, float x, float y, float z)
        {
            SetStart(index, x, y, z);
        }

        /// <summary>
        /// configures the end position of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="position">end position of the billboard object</param>
        public void SetEnd(int index, Vector3 position)
        {
            if (billboardList.Count <= index || 0 > index)
                throw new ArgumentException("Invalid index.");

            billboardList[index].End = position;
            billboardList[index].AddUpdateType(
                BillboardObject.UpdateTypes.Position);
            needToUpdate = true;
        }

        /// <summary>
        /// configures the end position of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="x">x-component of end position of the billboard object</param>
        /// <param name="y">y-component of end position of the billboard object</param>
        /// <param name="z">z-component of end position of the billboard object</param>
        public void SetEnd(int index, float x, float y, float z)
        {
            SetEnd(index, x, y, z);
        }

        /// <summary>
        /// configures the size of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="height">the height of billboard object</param>
        public void SetSize(int index, float height)
        {
            if (billboardList.Count <= index || 0 > index)
                throw new ArgumentException("Invalid index.");

            billboardList[index].Size = height;

            billboardList[index].AddUpdateType(
                BillboardObject.UpdateTypes.Position);
            needToUpdate = true;
        }

        /// <summary>
        /// configures the texture coordinates of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="min">texture coordinates of minimum</param>
        /// <param name="max">texture coordinates of maximum</param>
        public void SetTextureCoord(int index, Vector2 min, Vector2 max)
        {
            if (billboardList.Count <= index || 0 > index)
                throw new ArgumentException("Invalid index.");

            billboardList[index].MinUV = min;
            billboardList[index].MaxUV = max;

            billboardList[index].AddUpdateType(
                BillboardObject.UpdateTypes.Texturecoord);
            needToUpdate = true;
        }

        /// <summary>
        /// configures the texture coordinates of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="u1">texture "u1" coordinate</param>
        /// <param name="v1">texture "v1" coordinate</param>
        /// <param name="u2">texture "u2" coordinate</param>
        /// <param name="v2">texture "v2" coordinate</param>
        public void SetTextureCoord(int index, float u1, float v1, float u2, float v2)
        {
            SetTextureCoord(index, new Vector2(u1, v1), new Vector2(u2, v2));
        }

        /// <summary>
        /// configures the vertex color of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="color">color</param>
        public void SetColor(int index, Color color)
        {
            if (billboardList.Count <= index || 0 > index)
                throw new ArgumentException("Invalid index.");

            billboardList[index].Color = color;
            billboardList[index].AddUpdateType(
                BillboardObject.UpdateTypes.Color);

            needToUpdate = true;
        }

        /// <summary>
        /// configures the vertex color of each billboard object.
        /// </summary>
        /// <param name="index">an index of billboard object</param>
        /// <param name="r">a red component of color</param>
        /// <param name="g">a green component of color</param>
        /// <param name="b">a blue component of color</param>
        /// <param name="a">an alpha component of color</param>
        public void SetColor(int index, byte r, byte g, byte b, byte a)
        {
            SetColor(index, new Color(r, g, b, a));
        }

        /// <summary>
        /// configures a position vector to the vertex component data 
        /// using the billboard object.
        /// </summary>
        /// <param name="vertexData">target vertex component data</param>
        /// <param name="obj">source billboard object</param>
        /// <param name="startIndex">start index of the vertex component data</param>
        /// <param name="ay">position vector</param>
        private static void SetBufferPosition(
            ref VertexPositionColorTexture[] vertexData, BillboardObject obj, 
            int startIndex, Vector3 ay)
        {
            Vector3 by = Vector3.Zero;
            float cy = obj.Size * 0.5f;
            by = ay * cy;

            //   [0]           [1]
            //   +-------------+
            //   |             |
            //   +c            |
            //   |             |
            //   +-------------+
            //   [2]           [3]

            // 0 2 1 - 1 2 3

            vertexData[startIndex + 0].Position = obj.Start - by;
            vertexData[startIndex + 1].Position = obj.End - by;
            vertexData[startIndex + 2].Position = obj.Start + by;
            vertexData[startIndex + 3].Position = obj.End + by;
        }

        /// <summary>
        /// configures texture coordinates to the vertex component data 
        /// using the billboard object.
        /// </summary>
        /// <param name="vertexData">target vertex component data</param>
        /// <param name="obj">source billboard object</param>
        /// <param name="startIndex">start index of the vertex component data</param>
        /// <param name="renderingSpace">3D render space</param>
        private static void SetBufferTextureCoord(
            ref VertexPositionColorTexture[] vertexData, BillboardObject obj, 
            int startIndex, RenderingSpace renderingSpace)
        {
            float u1 = 0.0f, v1 = 0.0f, u2 = 0.0f, v2 = 0.0f;

            u1 = obj.MinUV.X;
            v1 = obj.MinUV.Y;
            u2 = obj.MaxUV.X;
            v2 = obj.MaxUV.Y;

            if (renderingSpace == RenderingSpace.Screen)
            {
                float swap = v1;
                v1 = v2;
                v2 = swap;
            }

            vertexData[startIndex + 0].TextureCoordinate = new Vector2(u1, v2);
            vertexData[startIndex + 1].TextureCoordinate = new Vector2(u1, v1);
            vertexData[startIndex + 2].TextureCoordinate = new Vector2(u2, v2);
            vertexData[startIndex + 3].TextureCoordinate = new Vector2(u2, v1);
        }

        /// <summary>
        /// configures vertex color to the vertex component data 
        /// using the billboard object.
        /// </summary>
        /// <param name="vertexData">target vertex component data</param>
        /// <param name="obj">source billboard object</param>
        /// <param name="startIndex">start index of the vertex component data</param>
        private static void SetBufferColor(ref VertexPositionColorTexture[] vertexData, 
                                    BillboardObject obj, int startIndex)
        {
            for (int i = 0; i < 4; i++)
                vertexData[startIndex + i].Color = obj.Color;
        }
    }
}
