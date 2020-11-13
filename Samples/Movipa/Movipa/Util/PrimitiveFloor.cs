#region File Description
//-----------------------------------------------------------------------------
// PrimitiveFloor.cs
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
    /// Draws the floor plane.
    /// The floor is initially centered on Vector 3(0, 0, 0).
    /// Use SetPositionOffset to shift the floor.
    /// 
    /// 平面の床を描画します。
    /// 初期状態ではVector3(0, 0, 0)を中心とし、床を作成します。
    /// 床を移動させるにはSetPositionOffsetを使用してください。
    /// </summary>
    public class PrimitiveFloor : PrimitiveRenderState, IDisposable
    {
        #region Fields
        private BasicEffect basicEffect;
        private VertexDeclaration vertexDeclaration;
        private VertexPositionColor[] vertices;
        private VertexBuffer vertexBuffer;
        private int totalSurface;
        private int totalVertices;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        /// <param name="width">Number of horizontal tiles</param>
        ///  s
        /// <param name="width">横のタイル枚数</param>

        /// <param name="height">Number of vertical tiles</param>
        ///  
        /// <param name="height">縦のタイル枚数</param>

        /// <param name="scale">Tile size</param>
        ///  
        /// <param name="scale">タイルのサイズ</param>

        /// <param name="color">Tile color</param>
        ///  
        /// <param name="color">タイルの色</param>
        public PrimitiveFloor(
            GraphicsDevice graphics, int width, int height, float scale, Color color)
        {
            // Creates the BasicEffect.
            // 
            // BasicEffectを作成します。
            basicEffect = new BasicEffect(graphics, null);

            // Sets the vertex buffer.
            // 
            // 頂点バッファを設定します。
            VertexElement[] elements = VertexPositionColor.VertexElements;
            vertexDeclaration = new VertexDeclaration(graphics, elements);

            // Creates the floor vertex.
            // 
            // 床の頂点を作成します。
            CreateFloor(graphics, width, height, scale, color);

            // Registers the fog settings.
            // 
            // フォグの設定を行います。
            SetFogMode(true, Vector3.Zero, 0.0f, 1000.0f);
        }


        /// <summary>
        /// Initializes the instance.
        /// Creates grey tiles.
        ///
        /// インスタンスを初期化します。
        /// グレーのタイルを作成します。
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        /// <param name="width">Number of horizontal tiles</param>
        ///  
        /// <param name="width">横のタイル枚数</param>

        /// <param name="height">Number of vertical tiles </param>
        ///  
        /// <param name="height">縦のタイル枚数</param>

        /// <param name="scale">Tile size</param>
        ///  
        /// <param name="scale">タイルのサイズ</param>

        public PrimitiveFloor(
            GraphicsDevice graphics, int width, int height, float scale)
            : this(graphics, width, height, scale, Color.Gray)
        {
        }


        /// <summary>
        /// Initializes the instance. 
        /// Creates tiles of side length 50 in grey.
        /// 
        /// インスタンスを初期化します。
        /// グレーで、一辺が50のタイルを作成します。
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        /// <param name="width">Number of horizontal tiles</param>
        ///  
        /// <param name="width">横のタイル枚数</param>

        /// <param name="height">Number of vertical tiles</param>
        ///  
        /// <param name="height">縦のタイル枚数</param>
        public PrimitiveFloor(GraphicsDevice graphics, int width, int height)
            : this(graphics, width, height, 50.0f, Color.Gray)
        {
        }


        /// <summary>
        /// Initializes the instance.
        /// Creates tiles of side length 50 in grey.
        /// 
        /// インスタンスを初期化します。
        /// グレーで、一辺が50のタイルを作成します。
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        /// <param name="tiles">Number of tiles</param>
        ///  
        /// <param name="tiles">タイル枚数</param>
        public PrimitiveFloor(GraphicsDevice graphics, Point tiles)
            : this(graphics, tiles.X, tiles.Y, 50.0f, Color.Gray)
        {
        }


        /// <summary>
        /// Initializes the instance.
        /// Creates horizontal and vertical sides of 25 tiles 
        /// with side length 50 in grey.
        /// 
        /// インスタンスを初期化します。
        /// グレーで、一辺が50のタイルを縦横25枚ずつ作成します。
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        public PrimitiveFloor(GraphicsDevice graphics)
            : this(graphics, 25, 25, 50.0f, Color.Gray)
        {
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the floor.
        /// 
        /// 床を描画します。
        /// </summary>
        public void Draw(Matrix projection, Matrix view)
        {
            // Registers the projection settings.
            // 
            // プロジェクションの設定をします。
            basicEffect.Projection = projection;

            // Registers the view settings.
            // 
            // ビューの設定をします。
            basicEffect.View = view;

           // Draws the floor.
           // 
           // 床を描画します。
            DrawFloor(basicEffect);
        }


        /// <summary>
        /// Draws the floor.
        /// 
        /// 床を描画します。
        /// </summary>
        private void DrawFloor(BasicEffect basicEffect)
        {
            GraphicsDevice graphics = basicEffect.GraphicsDevice;
            VertexStream stream = graphics.Vertices[0];

            // Sets the definitions of the vertex data to be drawn.
            // 
            // 描画する頂点データの定義を設定します。
            graphics.VertexDeclaration = vertexDeclaration;

            // Sets the vertex buffer.
            // 
            // 頂点バッファを設定します。
            stream.SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);

            // Enables the vertex color.
            // 
            // 頂点カラーを有効にします。
            basicEffect.VertexColorEnabled = true;

            // Begins using the effect.
            // 
            // エフェクトの使用を開始します。
            basicEffect.Begin();

            // Repeats drawing for the number of passes.
            // 
            // パスの数だけ繰り替えし描画します。
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                // Begins the pass.
                // 
                // パスを開始します。
                pass.Begin();

                // Draws with TriangleList.
                // 
                // TriangleList で描画します。
                basicEffect.GraphicsDevice.DrawPrimitives(
                    PrimitiveType.TriangleList, 0, totalSurface * 2);

                // Finishes the pass.
                // 
                // パスを終了します。
                pass.End();
            }

            // Finishes using the effect.
            // 
            // エフェクトの使用を終了します。
            basicEffect.End();

        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates floor vertex information.
        /// 
        /// 床の頂点情報を作成します。
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        /// <param name="width">Number of horizontal tiles</param>
        ///  
        /// <param name="width">横のタイル枚数</param>

        /// <param name="height">Number of vertical tiles</param>
        ///  
        /// <param name="height">縦のタイル枚数</param>

        /// <param name="scale">Tile size</param>
        ///  
        /// <param name="scale">タイルのサイズ</param>

        /// <param name="color">Tile color</param>
        ///  
        /// <param name="color">タイルの色</param>

        private void CreateFloor(
            GraphicsDevice graphics, int width, int height, float scale, Color color)
        {
            // Calculates the base position.
            // 
            // 基準点を計算します。
            Vector3 basePosition = new Vector3();
            basePosition.X = -((float)width / 2) * scale;
            basePosition.Z = -((float)height / 2) * scale;

            // Calculates the number of surfaces.
            // 
            // 面の数を計算します。
            totalSurface = (width * height);

            // Calculates the number of vertices.
            // 
            // 頂点の数を計算します。
            totalVertices = totalSurface * 6;

            // Sets the floor color.
            // 
            // 床の色を設定します。
            Color[] colors = new Color[]
            {
                color,
                new Color(color.ToVector3() * 0.8f),
            };

            // Sets the vertex position differential.
            // 
            // 頂点の位置の差分を設定します。
            Vector3[] offsets = new Vector3[] {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(scale, 0.0f, 0.0f),
                new Vector3(scale, 0.0f, scale),
                new Vector3(scale, 0.0f, scale),
                new Vector3(0.0f, 0.0f, scale),
                new Vector3(0.0f, 0.0f, 0.0f),
            };

            // Sets the vertex.
            // 
            // 頂点を設定します。
            vertices = new VertexPositionColor[totalVertices];
            int colorCount = 0;
            int vertexCount = 0;
            for (int x = 0; x < width; x++)
            {
                // Switches the tile color arrangement.
                // 
                // タイルの色を並びを入れ替えます。
                colorCount = ((x % 2) == 0) ? 0 : 1;

                for (int y = 0; y < height; y++)
                {
                    // Sets the vertex position.
                    // 
                    // 頂点の位置を設定します。
                    Vector3 position = basePosition;
                    position.X += x * scale;
                    position.Z += y * scale;

                    // Obtains the vertex color.
                    // 
                    // 頂点の色を取得します。
                    Color floorColor = colors[(colorCount++ % 2)];

                    // Sets the vertex.
                    // 
                    // 頂点を設定します。
                    foreach (Vector3 offset in offsets)
                    {
                        vertices[vertexCount++] = 
                            new VertexPositionColor(position + offset, floorColor);
                    }
                }
            }

            // Creates a vertex buffer for TriangleList.
            // 
            // TriangleList 用頂点バッファ作成します。
            int sizeInBytes = VertexPositionColor.SizeInBytes * totalVertices;
            vertexBuffer = new VertexBuffer(graphics, sizeInBytes, BufferUsage.None);

            // Writes vertex data to the vertex buffer.
            // 
            // 頂点データを頂点バッファに書き込みます。
            vertexBuffer.SetData<VertexPositionColor>(vertices);
        }


        /// <summary>
        /// Registers the fog settings.
        /// 
        /// フォグの設定をします。
        /// </summary>
        public void SetFogMode(bool fogEnabled)
        {
            if (fogEnabled)
            {
                // Sets the fog parameters.
                // 
                // フォグのパラメータを設定します。
                SetFogMode(true, Vector3.Zero, 0.0f, 1000.0f);
            }
            else
            {
                // Disables the fog settings.
                // 
                // フォグの設定を無効にするします。
                basicEffect.FogEnabled = fogEnabled;
            }
        }


        /// <summary>
        /// Registers the fog settings.
        /// 
        /// フォグの設定をします。
        /// </summary>
        /// <param name="fogEnabled">Enabled flag</param>
        ///  
        /// <param name="fogEnabled">有効フラグ</param>

        /// <param name="fogColor">Fog color</param>
        ///  
        /// <param name="fogColor">フォグの色</param>

        /// <param name="fogStart">Fog start position</param>
        ///  
        /// <param name="fogStart">フォグの開始位置</param>

        /// <param name="fogEnd">Fog end position</param>
        ///  
        /// <param name="fogEnd">フォグの終了位置</param>
        public void SetFogMode(
            bool fogEnabled, Vector3 fogColor, float fogStart, float fogEnd)
        {
            // Sets the fog parameters.
            // 
            // フォグのパラメータを設定します。
            basicEffect.FogEnabled = fogEnabled;
            basicEffect.FogColor = fogColor;
            basicEffect.FogStart = fogStart;
            basicEffect.FogEnd = fogEnd;
        }



        /// <summary>
        /// Shifts the floor position.
        /// 
        /// 床の位置を移動します。
        /// </summary>
        public void SetPositionOffset(Vector3 offset)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position += offset;
            }
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
                if (vertexBuffer != null)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}