#region File Description
//-----------------------------------------------------------------------------
// GameMesh.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Render;
using RobotGameData.Resource;
using RobotGameData.Helper;
#endregion

namespace RobotGameData.GameObject
{
    public enum RenderingSpace
    {
        /// <summary>
        /// 3D world space
        /// </summary>
        World = 0,

        /// <summary>
        /// 2D screen space
        /// </summary>
        Screen,
    }

    /// <summary>
    /// This mesh is drawn on the 3D world.
    /// It contains a vertex buffer and an index buffer.
    /// </summary>
    public class GameMesh : GameSceneNode
    {
        #region Fields

        //  If it's true, will be disabled VertexBuffer and IndexBuffer
        public bool userPrimitive = true;   

        public VertexPositionColorTexture[] vertexData = null;
        public short[] indexData = null;
        public VertexBuffer vertexBuffer = null;
        public IndexBuffer indexBuffer = null;        

        Texture2D textureResource = null;
        int updateVertexCount = 0;
        int primitiveCount = 0;

        bool alphaTestEnable = false;
        bool alphaBlendEnable = false;
        CompareFunction alphaFunction = CompareFunction.Always;
        Blend sourceBlend = Blend.One;
        Blend destinationBlend = Blend.Zero;
        BlendFunction blendFunction = BlendFunction.Add;
        int referenceAlpha = 0;
        bool depthBufferEnable = true;
        bool depthBufferWriteEnable = true;
        CompareFunction depthBufferFunction = CompareFunction.LessEqual;
        CullMode cullMode = CullMode.CullCounterClockwiseFace;

        static VertexDeclaration vertexDeclaration = null;
        static BasicEffect basicEffect = null;

        #endregion

        #region Properties

        public int UpdateVertexCount
        {
            get { return updateVertexCount; }
            set { updateVertexCount = value; }
        }

        public int PrimitiveCount
        {
            get { return this.primitiveCount; }
            set { this.primitiveCount = value; }
        }

        public bool AlphaTestEnable
        {
            get { return alphaTestEnable; }
            set { alphaTestEnable = value; }
        }

        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        public int ReferenceAlpha
        {
            get { return referenceAlpha; }
            set { referenceAlpha = value; }
        }        

        public CompareFunction AlphaFunction
        {
            get { return alphaFunction; }
            set { alphaFunction = value; }
        }

        public bool DepthBufferEnable
        {
            get { return depthBufferEnable; }
            set { depthBufferEnable = value; }
        }

        public bool DepthBufferWriteEnable
        {
            get { return depthBufferWriteEnable; }
            set { depthBufferWriteEnable = value; }
        }

        public CompareFunction DepthBufferFunction
        {
            get { return depthBufferFunction; }
            set { depthBufferFunction = value; }
        }

        public Blend SourceBlend
        {
            get { return sourceBlend; }
            set { sourceBlend = value; }
        }

        public Blend DestinationBlend
        {
            get { return destinationBlend; }
            set { destinationBlend = value; }
        }

        public BlendFunction BlendFunction
        {
            get { return blendFunction; }
            set { blendFunction = value; }
        }

        public CullMode CullMode
        {
            get { return cullMode; }
            set { cullMode = value; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.Dispose();
                this.vertexBuffer = null;
            }

            if (this.indexBuffer != null)
            {
                this.indexBuffer.Dispose();
                this.indexBuffer = null;
            }

            if (vertexDeclaration != null)
            {
                vertexDeclaration.Dispose();
                vertexDeclaration = null;
            }

            if (basicEffect != null)
            {
                basicEffect.Dispose();
                basicEffect = null;
            }            

            base.Dispose(disposing);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// the mesh is drawn by using the vertexData.
        /// When the userPrimitive member is set to true, 
        /// it is drawn without using the vertex buffer and the index buffer.
        /// </summary>
        /// <param name="renderTracer"></param>
        protected override void OnDraw(RenderTracer renderTracer)
        {
            GraphicsDevice device = renderTracer.Device;
            RenderState renderState = device.RenderState;

            basicEffect.Texture = this.textureResource;
            basicEffect.World = this.TransformedMatrix;
            basicEffect.View = renderTracer.View;
            basicEffect.Projection = renderTracer.Projection;
            basicEffect.LightingEnabled = false;

            device.VertexDeclaration = vertexDeclaration;
            device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            device.SamplerStates[0].MinFilter = TextureFilter.Linear;
            device.SamplerStates[0].MagFilter = TextureFilter.Linear;
            device.SamplerStates[0].MipFilter = TextureFilter.Point;

            renderState.AlphaTestEnable = alphaTestEnable;
            renderState.AlphaBlendEnable = alphaBlendEnable;
            renderState.AlphaFunction = alphaFunction;

            renderState.SourceBlend = sourceBlend;
            renderState.DestinationBlend = destinationBlend;
            renderState.BlendFunction = blendFunction;

            renderState.ReferenceAlpha = referenceAlpha;
            renderState.DepthBufferEnable = depthBufferEnable;
            renderState.DepthBufferWriteEnable = depthBufferWriteEnable;
            renderState.DepthBufferFunction = depthBufferFunction;
            renderState.CullMode = cullMode;

            basicEffect.Begin();

            for (int i = 0; i < basicEffect.CurrentTechnique.Passes.Count; i++)
            {
                EffectPass pass = basicEffect.CurrentTechnique.Passes[i];

                pass.Begin();

                if (userPrimitive )
                {
                    //  Use index?
                    if (indexData != null)
                    {
                        // only use vertex and index data
                        device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(
                                                    PrimitiveType.TriangleList,
                                                    vertexData,
                                                    0,
                                                    vertexData.Length,
                                                    indexData,
                                                    0,
                                                    this.primitiveCount);
                    }
                    else
                    {
                        device.DrawUserPrimitives<VertexPositionColorTexture>(
                                                    PrimitiveType.TriangleList,
                                                    vertexData,
                                                    0,
                                                    this.primitiveCount);

                    }                    
                }
                else
                {
                    //  Use vertex buffer
                    device.Vertices[0].SetSource(vertexBuffer,
                                                 0,
                                                 VertexPositionColorTexture.SizeInBytes);

                    //  Use index?
                    if (indexBuffer != null)
                    {
                        //  Use index buffer
                        device.Indices = indexBuffer;

                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     0,
                                                     0,
                                                     updateVertexCount,
                                                     0,
                                                     this.primitiveCount);
                    }
                    else
                    {
                        device.DrawPrimitives(PrimitiveType.TriangleList,
                                             0,
                                             this.primitiveCount);
                    }
                }

                pass.End();
            }

            basicEffect.End();

            device.RenderState.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// create members for creating mesh data.
        /// </summary>
        /// <param name="vertexCount">vertex count</param>
        /// <param name="indexCount">index count</param>
        /// <param name="fileName">texture file name</param>
        public void Create(int vertexCount, int indexCount, string fileName)
        {
            //  Load texture
            GameResourceTexture2D resource = 
                            FrameworkCore.ResourceManager.LoadTexture(fileName);

            Create(vertexCount, indexCount, resource.Texture2D);
        }

        /// <summary>
        /// create members for creating mesh data.
        /// </summary>
        /// <param name="vertexCount">vertex count</param>
        /// <param name="indexCount">index count</param>
        /// <param name="texture">texture resource</param>
        public void Create(int vertexCount, int indexCount, Texture2D texture)
        {
            GraphicsDevice device = FrameworkCore.Game.GraphicsDevice;

            this.updateVertexCount = vertexCount;

            if( indexCount > 0)
                this.primitiveCount = vertexCount / 2;
            else
                this.primitiveCount = vertexCount / 3;

            this.textureResource = texture;

            if (basicEffect == null)
            {
                basicEffect = new BasicEffect(device, null);
                basicEffect.LightingEnabled = false;
                basicEffect.VertexColorEnabled = true;
                basicEffect.TextureEnabled = true;
            }

            if (vertexDeclaration == null)
            {
                vertexDeclaration = new VertexDeclaration(device,
                                        VertexPositionColorTexture.VertexElements);
            }

            //  Create vertexBuffer
            if( vertexCount > 0)
            {
                vertexData = new VertexPositionColorTexture[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    vertexData[i].Color.PackedValue = 0xFFFFFFFF;
                }

                if (userPrimitive == false)
                {
                    vertexBuffer = new DynamicVertexBuffer(device,
                                    VertexPositionColorTexture.SizeInBytes * vertexCount,
                                    BufferUsage.WriteOnly);

                    vertexBuffer.SetData(vertexData);
                }
                
            }

            //  Create indexBuffer
            if( indexCount > 0)
            {
                indexData = new short[indexCount];

                if (userPrimitive == false)
                {
                    indexBuffer = new DynamicIndexBuffer(device,
                                            sizeof(short) * indexCount,
                                            BufferUsage.WriteOnly,
                                            IndexElementSize.SixteenBits);

                    indexBuffer.SetData(indexData);
                }
            }
        }

        /// <summary>
        /// configures a position to the vertex component data.
        /// </summary>
        /// <param name="index">an index of the vertex component data</param>
        /// <param name="position">a position vector</param>
        public void SetPositionData(int index, Vector3 position)
        {
            vertexData[index].Position = position;
        }

        /// <summary>
        /// configure positions to the vertex component data.
        /// </summary>
        /// <param name="position">array of position vector</param>
        public void SetPositionData(Vector3[] position)
        {
            for(int i = 0; i < position.Length; i++)
            {
                SetPositionData(i, position[i]);
            }
        }

        /// <summary>
        /// configures color to the vertex component data.
        /// </summary>
        /// <param name="index">an index of the vertex component data</param>
        /// <param name="color">packed color value</param>
        public void SetColorData(int index, uint color)
        {
            vertexData[index].Color.PackedValue = color;
        }

        /// <summary>
        /// configures color to the vertex component data.
        /// </summary>
        /// <param name="index">an index of the vertex component data</param>
        /// <param name="color">color</param>
        public void SetColorData(int index, Color color)
        {
            vertexData[index].Color = color;
        }

        /// <summary>
        /// configure colors to the vertex component data.
        /// </summary>
        /// <param name="color">array of packed color value</param>
        public void SetColorData(uint[] color)
        {
            for (int i = 0; i < color.Length; i++)
            {
                SetColorData(i, color[i]);
            }
        }

        /// <summary>
        /// configure colors to the vertex component data.
        /// </summary>
        /// <param name="color">array of color</param>
        public void SetColorData(Color[] color)
        {
            for (int i = 0; i < color.Length; i++)
            {
                SetColorData(i, color[i]);
            }
        }

        /// <summary>
        /// configures texture coordinates to the vertex component data.
        /// </summary>
        /// <param name="index">an index of the vertex component data</param>
        /// <param name="texturecoord">texture coordinates</param>
        public void SetTextureCoordData(int index, Vector2 texturecoord)
        {
            vertexData[index].TextureCoordinate = texturecoord;
        }

        /// <summary>
        /// configures texture coordinates to the vertex component data.
        /// </summary>
        /// <param name="texturecoord">array of texture coordinates</param>
        public void SetTextureCoordData(Vector2[] texturecoord)
        {
            for (int i = 0; i < texturecoord.Length; i++)
            {
                SetTextureCoordData(i, texturecoord[i]);
            }
        }

        /// <summary>
        /// configures vertex index to the vertex component data.
        /// </summary>
        /// <param name="index">an index of the vertex component data</param>
        /// <param name="val">index value</param>
        public void SetIndexData(int index, short val)
        {
            indexData[index] = val;
        }

        /// <summary>
        /// configures vertex indicies to the vertex component data.
        /// </summary>
        /// <param name="val">array of value</param>
        public void SetIndexData(short[] val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                SetIndexData(i, val[i]);
            }
        }

        /// <summary>
        /// binds the vertex component data to the vertex buffer.
        /// </summary>
        public void BindVertexBuffer()
        {
            //  Set vertex buffer
            vertexBuffer.SetData<VertexPositionColorTexture>(vertexData);
        }

        /// <summary>
        /// binds the index values to the index buffer.
        /// </summary>
        public void BindIndexBuffer()
        {
            //  Set index buffer
            indexBuffer.SetData<short>(indexData);
        }
    }
}