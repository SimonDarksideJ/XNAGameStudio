#region File Description
//-----------------------------------------------------------------------------
// PrimitiveDraw2D.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Movipa.Util
{
    /// <summary>
    /// Performs a primitive drawing in 2D.
    /// Provides rectangle, fill and line drawing functions as well as addition,
    /// alpha blend and other functions to change drawing mode.
    /// 
    /// 2Dに関する基本描画をします。
    /// 矩形、塗りつぶし、ラインの描画機能と、加算やアルファブレンドなどの
    /// 描画モードを変更する機能を提供します。
    /// </summary>
    public class PrimitiveDraw2D : PrimitiveRenderState, IDisposable
    {
        #region Fields
        protected GraphicsDevice graphicsDevice;
        protected BasicEffect basicEffect;
        protected VertexDeclaration vertexDeclaration;
        protected VertexPositionColor[] vertices;
        protected Matrix defaultScreenProjection;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the default screen projection matrix.
        /// 
        /// デフォルトのスクリーンプロジェクションマトリックスを取得します。
        /// </summary>
        public Matrix DefaultScreenProjection
        {
            get { return defaultScreenProjection; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public PrimitiveDraw2D(GraphicsDevice graphics)
        {
            // Sets the graphic device.
            // 
            // グラフィックスデバイスを設定します。
            graphicsDevice = graphics;

            // Creates BasicEffect.
            // 
            // BasicEffectを作成します。
            basicEffect = new BasicEffect(graphicsDevice, null);
            basicEffect.VertexColorEnabled = true;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.Identity;

            // Creates the screen coordinates projection.
            // 
            // スクリーン座標のプロジェクションを作成します。
            defaultScreenProjection = Matrix.CreateOrthographicOffCenter(
                0.0f,
                graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height,
                0.0f,
                0.0f,
                1.0f);

            // Sets the vertex buffer.
            // 
            // 頂点バッファを設定します。
            vertices = new VertexPositionColor[5];
            VertexElement[] elements = VertexPositionColor.VertexElements;
            vertexDeclaration = new VertexDeclaration(graphicsDevice, elements);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the lines.
        /// 
        /// 線を描画します。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="start">Start coordinate</param>
        ///  
        /// <param name="start">開始座標</param>

        /// <param name="end">End coordinate</param>
        ///  
        /// <param name="end">終了座標</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void DrawLine(Matrix? projection, Vector2 start, Vector2 end, Color color)
        {
            DrawLine(projection, start, end, color, color);
        }


        /// <summary>
        /// Draws the lines.
        /// 
        /// 線を描画します。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="start">Start coordinate</param>
        ///  
        /// <param name="start">開始座標</param>

        /// <param name="end">End coordinate</param>
        ///  
        /// <param name="end">終了座標</param>

        /// <param name="startColor">Start draw color</param>
        ///  
        /// <param name="startColor">開始描画色</param>

        /// <param name="endColor">End draw color</param>
        ///  
        /// <param name="endColor">終了描画色</param>
        public void DrawLine(
            Matrix? projection,
            Vector2 start,
            Vector2 end,
            Color startColor,
            Color endColor)
        {
            vertices[0].Position.X = start.X;
            vertices[0].Position.Y = start.Y;
            vertices[0].Position.Z = 0;
            vertices[0].Color = startColor;

            vertices[1].Position.X = end.X;
            vertices[1].Position.Y = end.Y;
            vertices[1].Position.Z = 0;
            vertices[1].Color = endColor;

            basicEffect.Projection = projection ?? defaultScreenProjection;
            basicEffect.Begin();
            {
                basicEffect.GraphicsDevice.VertexDeclaration = vertexDeclaration;
                basicEffect.CurrentTechnique.Passes[0].Begin();
                basicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList, vertices, 0, 1);
                basicEffect.CurrentTechnique.Passes[0].End();
            }
            basicEffect.End();
        }


        /// <summary>
        /// Draws the rectangle lines.
        /// 
        /// 矩形の線を描画します。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void DrawRect(Matrix? projection, Rectangle rect, Color color)
        {
            DrawRect(projection, rect, new Color[] { color, color, color, color });
        }


        /// <summary>
        /// Draws the rectangle lines.
        /// 
        /// 矩形の線を描画します。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void DrawRect(Matrix? projection, Vector4 rect, Color color)
        {
            DrawRect(projection, rect, new Color[] { color, color, color, color });
        }


        /// <summary>
        /// Draws the rectangle lines.
        /// 
        /// 矩形の線を描画します。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void DrawRect(Matrix? projection, Rectangle rect, Color[] color)
        {
            Vector4 vector = new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
            DrawRect(projection, vector, color);
        }


        /// <summary>
        /// Draws the rectangle lines.
        /// 
        /// 矩形の線を描画します。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>

        public void DrawRect(Matrix? projection, Vector4 rect, Color[] color)
        {
            vertices[0].Position.X = rect.X;
            vertices[0].Position.Y = rect.Y;
            vertices[0].Position.Z = 0;
            vertices[0].Color = color[0];

            vertices[1].Position.X = rect.X + rect.Z;
            vertices[1].Position.Y = rect.Y;
            vertices[1].Position.Z = 0;
            vertices[1].Color = color[1];

            vertices[2].Position.X = rect.X + rect.Z;
            vertices[2].Position.Y = rect.Y + rect.W;
            vertices[2].Position.Z = 0;
            vertices[2].Color = color[2];

            vertices[3].Position.X = rect.X;
            vertices[3].Position.Y = rect.Y + rect.W;
            vertices[3].Position.Z = 0;
            vertices[3].Color = color[3];

            vertices[4].Position.X = rect.X;
            vertices[4].Position.Y = rect.Y;
            vertices[4].Position.Z = 0;
            vertices[4].Color = color[0];

            basicEffect.Projection = projection ?? defaultScreenProjection;
            basicEffect.Begin();
            {
                basicEffect.GraphicsDevice.VertexDeclaration = vertexDeclaration;
                basicEffect.CurrentTechnique.Passes[0].Begin();
                basicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.LineStrip, vertices, 0, 4);
                basicEffect.CurrentTechnique.Passes[0].End();
            }
            basicEffect.End();

        }


        /// <summary>
        /// Fills the rectangle.
        /// 
        /// 矩形を塗りつぶします。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void FillRect(Matrix? projection, Rectangle rect, Color color)
        {
            FillRect(projection, rect, new Color[] { color, color, color, color });
        }


        /// <summary>
        /// Fills the rectangle
        /// 
        /// 矩形を塗りつぶします。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>

        public void FillRect(Matrix? projection, Vector4 rect, Color color)
        {
            FillRect(projection, rect, new Color[] { color, color, color, color });
        }


        /// <summary>
        /// Fills the rectangle
        /// 
        /// 矩形を塗りつぶします。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void FillRect(Matrix? projection, Rectangle rect, Color[] color)
        {
            Vector4 vector = new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
            FillRect(projection, vector, color);
        }


        /// <summary>
        /// Fills the rectangle
        /// 
        /// 矩形を塗りつぶします。
        /// </summary>
        /// <param name="projection">Projection</param>
        ///  
        /// <param name="projection">プロジェクション</param>

        /// <param name="rect">Rectangle to draw</param>
        ///  
        /// <param name="rect">描画する矩形</param>

        /// <param name="color">Draw color</param>
        ///  
        /// <param name="color">描画色</param>
        public void FillRect(Matrix? projection, Vector4 rect, Color[] color)
        {
            vertices[0].Position.X = rect.X;
            vertices[0].Position.Y = rect.Y;
            vertices[0].Position.Z = 0;
            vertices[0].Color = color[0];

            vertices[1].Position.X = rect.X + rect.Z;
            vertices[1].Position.Y = rect.Y;
            vertices[1].Position.Z = 0;
            vertices[1].Color = color[1];

            vertices[2].Position.X = rect.X + rect.Z;
            vertices[2].Position.Y = rect.Y + rect.W;
            vertices[2].Position.Z = 0;
            vertices[2].Color = color[2];

            vertices[3].Position.X = rect.X;
            vertices[3].Position.Y = rect.Y + rect.W;
            vertices[3].Position.Z = 0;
            vertices[3].Color = color[3];

            basicEffect.Projection = projection ?? defaultScreenProjection;
            basicEffect.Begin();
            {
                basicEffect.GraphicsDevice.VertexDeclaration = vertexDeclaration;
                basicEffect.CurrentTechnique.Passes[0].Begin();
                basicEffect.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleFan, vertices, 0, 2);
                basicEffect.CurrentTechnique.Passes[0].End();
            }
            basicEffect.End();
        }
        #endregion

        #region IDisposable Members

        private bool disposed = false;
        public bool Disposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                if (basicEffect != null)
                {
                    basicEffect.Dispose();
                    basicEffect = null;
                }
                if (vertexDeclaration != null)
                {
                    vertexDeclaration.Dispose();
                    vertexDeclaration = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}