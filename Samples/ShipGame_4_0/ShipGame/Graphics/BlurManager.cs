#region File Description
//-----------------------------------------------------------------------------
// BlurManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace ShipGame
{
    // supported render techniques
    public enum BlurTechnique
    {
        Color = 0,              // plain color
        ColorTexture,           // plain texture mapping
        BlurHorizontal,         // horizontal blur
        BlurVertical,           // vertical blur
        BlurHorizontalSplit,    // horizontal split screen blur
    }

    public class BlurManager : IDisposable
    {
        // blur effect
        Effect blurEffect;

        // screen quad vertex declaration and buffer
        VertexDeclaration vertexDeclaration;        
        VertexBuffer vertexBuffer;

        // render target resolution
        int sizeX;
        int sizeY;
        
        // normalized pixel size (1.0/size)
        Vector2 pixelSize;

        // 2D ortho view projection matrix
        Matrix viewProjection;

        // parameters
        EffectParameter paramWorldViewProjection;  // world * view * proj matrix
        EffectParameter paramColorMap;             // color texture
        EffectParameter paramColor;                // color 
        EffectParameter paramPixelSize;            // pixel size

        /// <summary>
        /// Create a new blur manager
        /// </summary>
        public BlurManager(GraphicsDevice gd, Effect effect, int sizex, int sizey)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }
            if (effect == null)
            {
                throw new ArgumentNullException("effect");
            }

            blurEffect = effect;    // save effect
            sizeX = sizey;      // save horizontal buffer size
            sizeY = sizex;      // save verical buffer size

            // get effect parameters
            paramWorldViewProjection = blurEffect.Parameters["g_WorldViewProj"];
            paramColorMap = blurEffect.Parameters["g_ColorMap"];
            paramColor = blurEffect.Parameters["g_Color"];
            paramPixelSize = blurEffect.Parameters["g_PixelSize"];

            pixelSize = new Vector2(1.0f / sizeX, 1.0f / sizeY);
            viewProjection = Matrix.CreateOrthographicOffCenter(0,sizeX,0,sizeY,-1,1);

            // create vertex buffer
            vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTexture), 
                        6, BufferUsage.WriteOnly);
            
            // create vertex declaration
            vertexDeclaration =
                new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

            // create vertex data
            SetVertexData();
        }

        /// <summary>
        /// Set vertex data with textureCube vertex normals (used for cubemap blur option only)
        /// </summary>
        public void SetVertexData()
        {
            VertexPositionTexture[] data = new VertexPositionTexture[6];

            data[0] = new VertexPositionTexture(
                        new Vector3(0, 0, 0), new Vector2(0, 1));
            data[1] = new VertexPositionTexture(
                        new Vector3(sizeX, sizeY, 0), new Vector2(1, 0));
            data[2] = new VertexPositionTexture(
                        new Vector3(sizeX, 0, 0), new Vector2(1, 1));
            data[3] = new VertexPositionTexture(
                        new Vector3(0, 0, 0), new Vector2(0, 1));
            data[4] = new VertexPositionTexture(
                        new Vector3(0, sizeY, 0), new Vector2(0, 0));
            data[5] = new VertexPositionTexture(
                        new Vector3(sizeX, sizeY, 0), new Vector2(1, 0));
            
            vertexBuffer.SetData<VertexPositionTexture>(data);

            data = null;
        }

        /// <summary>
        /// Render a screen aligned quad used to process 
        /// the horizontal and vertical blur operations
        /// </summary>
        public void RenderScreenQuad(
            GraphicsDevice gd, BlurTechnique technique, 
            Texture2D texture, Vector4 color)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            gd.SetVertexBuffer(vertexBuffer);
            
            blurEffect.CurrentTechnique = blurEffect.Techniques[(int)technique];

            paramWorldViewProjection.SetValue(viewProjection);
            paramPixelSize.SetValue(pixelSize);
            paramColorMap.SetValue(texture);
            paramColor.SetValue(color);
            
            blurEffect.CurrentTechnique.Passes[0].Apply();
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            gd.SetVertexBuffer(null);
        }

        /// <summary>
        /// Render a screen aligned quad used to process 
        /// the horizontal and vertical blur operations
        /// </summary>
        public void RenderScreenQuad(GraphicsDevice gd, BlurTechnique technique,
            Texture2D texture, Vector4 color, float scale)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            gd.SetVertexBuffer(vertexBuffer);

            blurEffect.CurrentTechnique = blurEffect.Techniques[(int)technique];

            Matrix m = Matrix.CreateTranslation(-sizeX / 2, -sizeY / 2, 0) *
                     Matrix.CreateScale(scale, scale, 1) *
                     Matrix.CreateTranslation(sizeX / 2, sizeY / 2, 0);

            paramWorldViewProjection.SetValue(m * viewProjection);
            paramPixelSize.SetValue(pixelSize);
            paramColorMap.SetValue(texture);
            paramColor.SetValue(color);

            blurEffect.CurrentTechnique.Passes[0].Apply();
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            gd.SetVertexBuffer(null);
        }

        #region IDisposable Members
        
        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (vertexBuffer != null)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }
                if (vertexDeclaration != null)
                {
                    vertexDeclaration.Dispose();
                    vertexDeclaration = null;
                }
            }
        }

        #endregion
    }
}
