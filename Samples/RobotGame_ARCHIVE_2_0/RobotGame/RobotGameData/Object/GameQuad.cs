#region File Description
//-----------------------------------------------------------------------------
// GameQuad.cs
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
using Microsoft.Xna.Framework.Graphics;
using RobotGameData;
using RobotGameData.Render;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// Since this sprite contains its own up vector and normal vector, 
    /// it does not get affected by view matrix.
    /// </summary>
    public class GameQuad : GameSceneNode
    {
        #region Fields

        Vector3 upperLeft;
        Vector3 lowerLeft;
        Vector3 upperRight;
        Vector3 lowerRight;
        Vector3 normal;
        Vector3 planeUp;
        Vector3 left;

        VertexPositionNormalTexture[] vertices;
        int[] indices;
        VertexDeclaration vertexDecl;
        BasicEffect effect;
        IGraphicsDeviceService graphics;

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return effect.World; }
            set { effect.World = value; }
        }

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return effect.View; }
            set { effect.View = value; }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return effect.Projection; }
            set { effect.Projection = value; }
        }

        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return effect.Texture; }
            set 
            { 
                effect.Texture = value;

                if (effect.Texture == null)
                    effect.TextureEnabled = false;
                else
                    effect.TextureEnabled = true;
            }
        }

        /// <summary>
        /// Gets or sets the lighting enable.
        /// </summary>
        public bool LightingEnabled
        {
            get { return effect.LightingEnabled; }
            set { effect.LightingEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the vertex color enable.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return effect.VertexColorEnabled; }
            set { effect.VertexColorEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the alpha of the mesh.
        /// </summary>
        public float Alpha
        {
            get { return effect.Alpha; }
            set { effect.Alpha = value; }
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="origin">origin position</param>
        /// <param name="normal">normal vector</param>
        /// <param name="up">up vector</param>
        /// <param name="width">width size</param>
        /// <param name="height">height size</param>
        public GameQuad(Vector3 origin, Vector3 normal, Vector3 up, 
            float width, float height)
        {
            this.graphics = 
                (IGraphicsDeviceService)FrameworkCore.Game.Services.GetService(
                    typeof(IGraphicsDeviceService));

            this.effect = new BasicEffect(graphics.GraphicsDevice, null);

            this.vertices = new VertexPositionNormalTexture[4];
            this.indices = new int[6];
            this.normal = normal;
            this.planeUp = up;

            // Calculate the quad corners
            this.left = Vector3.Cross(normal, this.planeUp);
            Vector3 uppercenter = (this.planeUp * height / 2) + origin;
            this.upperLeft = uppercenter + (this.left * width / 2);
            this.upperRight = uppercenter - (this.left * width / 2);
            this.lowerLeft = this.upperLeft - (this.planeUp * height);
            this.lowerRight = this.upperRight - (this.planeUp * height);

            vertexDecl = new VertexDeclaration(graphics.GraphicsDevice,
            VertexPositionNormalTexture.VertexElements);

            FillVertices();
        }

        /// <summary>
        /// fills vertex data using members.
        /// </summary>
        private void FillVertices()
        {
            // Fill in texture coordinates to display full texture
            // on quad
            Vector2 textureUpperLeft = new Vector2( 0.0f, 0.0f );
            Vector2 textureUpperRight = new Vector2( 1.0f, 0.0f );
            Vector2 textureLowerLeft = new Vector2( 0.0f, 1.0f );
            Vector2 textureLowerRight = new Vector2( 1.0f, 1.0f );

            // Provide a normal for each vertex
            for (int i = 0; i < this.vertices.Length; i++)
            {
                this.vertices[i].Normal = this.normal;
            }

            // Set the position and texture coordinate for each
            // vertex
            this.vertices[0].Position = this.lowerLeft;
            this.vertices[0].TextureCoordinate = textureLowerLeft;
            this.vertices[1].Position = this.upperLeft;
            this.vertices[1].TextureCoordinate = textureUpperLeft;
            this.vertices[2].Position = this.lowerRight;
            this.vertices[2].TextureCoordinate = textureLowerRight;
            this.vertices[3].Position = this.upperRight;
            this.vertices[3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            this.indices[0] = 0;
            this.indices[1] = 1;
            this.indices[2] = 2;
            this.indices[3] = 2;
            this.indices[4] = 1;
            this.indices[5] = 3;
        }

        protected override void  Dispose(bool disposing)
        {
            if (vertexDecl != null)
            {
                vertexDecl.Dispose();
                vertexDecl = null;
            }

            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }

              base.Dispose(disposing);
        }

        /// <summary>
        /// draws a 3D quad.
        /// </summary>
        /// <param name="renderTracer"></param>
        protected override void OnDraw(RenderTracer renderTracer)
        {
            World = this.TransformedMatrix;
            View = renderTracer.View;
            Projection = renderTracer.Projection;

            renderTracer.Device.VertexDeclaration = vertexDecl;
          
            renderTracer.Device.RenderState.AlphaTestEnable = alphaTestEnable;
            renderTracer.Device.RenderState.AlphaBlendEnable = alphaBlendEnable;
            renderTracer.Device.RenderState.AlphaFunction = alphaFunction;

            renderTracer.Device.RenderState.SourceBlend = sourceBlend;
            renderTracer.Device.RenderState.DestinationBlend = destinationBlend;
            renderTracer.Device.RenderState.BlendFunction = blendFunction;

            renderTracer.Device.RenderState.ReferenceAlpha = referenceAlpha;
            renderTracer.Device.RenderState.DepthBufferEnable = depthBufferEnable;
            renderTracer.Device.RenderState.DepthBufferWriteEnable = 
                depthBufferWriteEnable;
            renderTracer.Device.RenderState.DepthBufferFunction = depthBufferFunction;
            renderTracer.Device.RenderState.CullMode = cullMode;

            GraphicsDevice device = renderTracer.Device;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList, this.vertices, 0, 4, this.indices, 0, 2);

                pass.End();
            }
            effect.End();
        }
    }
}
