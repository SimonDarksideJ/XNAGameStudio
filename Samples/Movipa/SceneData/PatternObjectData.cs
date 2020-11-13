#region File Description
//-----------------------------------------------------------------------------
// PatternObjectData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SceneDataLibrary
{
    /// <summary>
    /// Pattern data
    /// This data class is for a rectangle that has conversion parameters and textures.
    /// In Layout, pattern objects correspond to this pattern data.
    /// The data managed in this class contains:
    /// textures to be used, cutting range, display position (within a pattern group), 
    /// scale, angle, center position, color, flip information, etc.
    ///
    /// パターンデータ
    /// テクスチャを伴い、変換用のパラメータをもった矩形のデータクラスです。
    /// Layoutではパターンオブジェクトが相当します。
    /// 保持するデータとして、使用するテクスチャ、切り取り範囲、
    /// （パターングループ内での）表示位置、スケール、角度、中心位置、色、反転情報
    /// などがあります。
    /// </summary>
    public class PatternObjectData
    {
        #region Fields
        private String textureName = String.Empty;//Texture name //テクスチャ名
        private Texture2D texture = null;//Texture substance //テクスチャの実体
        private Rectangle patternRect = new Rectangle();//Pattern rectangle
        private bool flipH = false;//Horizontal flip flag //水平反転フラグ
        private bool flipV = false;//Vertical flip flag //垂直反転フラグ
        private DrawData drawData = new DrawData();//Conversion information //変換情報
        //Temporary conversion information to play a sequence
        //
        //シーケンス再生用の一時的な変換情報
        private DrawData interpolationDrawData = new DrawData();
        #endregion

        #region Properties
        /// <summary>
        /// Obtains and sets the texture name.
        /// The setting is called from SceneDataReader on initialization of scenes.
        ///
        /// テクスチャ名の設定取得を行います。
        /// 設定はシーンの初期化時にSceneDataReaderから呼び出されます。
        /// </summary>
        public String TextureName
        {
            get
            {
                return textureName;
            }
            set
            {
                textureName = value;
            }
        }

        /// <summary>
        /// Obtains and sets the pattern rectangle.
        /// The setting is called from SceneDataReader on initialization of scenes.
        ///
        /// パターン矩形の設定取得を行います。
        /// 設定はシーンの初期化時にSceneDataReaderから呼び出されます。
        /// </summary>
        public Rectangle Rect
        {
            get
            {
                return patternRect;
            }
            set
            {
                patternRect = value;
            }
        }

        /// <summary>
        /// Obtains and sets the horizontal flip flag for patterns.
        /// The setting is called from SceneDataReader on initialization of scenes.
        ///
        /// パターンの水平反転フラグの設定取得を行います。
        /// 設定はシーンの初期化時にSceneDataReaderから呼び出されます。
        /// </summary>
        public bool FlipH
        {
            get
            {
                return flipH;
            }
            set
            {
                flipH = value;
            }
        }

        /// <summary>
        /// Obtains and sets the vertical flip flag for patterns.
        /// The setting is called from SceneDataReader on initialization of scenes.
        ///
        /// パターンの垂直反転フラグの設定取得を行います。
        /// 設定はシーンの初期化時にSceneDataReaderから呼び出されます。
        /// </summary>
        public bool FlipV
        {
            get
            {
                return flipV;
            }
            set
            {
                flipV = value;
            }
        }

        /// <summary>
        /// Obtains and sets the conversion parameters for drawing.
        /// The setting is called from SceneDataReader on initialization of scenes.
        ///
        /// 描画用変換パラメータの設定取得を行います。
        /// 設定はシーンの初期化時にSceneDataReaderから呼び出されます。
        /// </summary>
        public DrawData Data
        {
            get
            {
                return drawData;
            }
            set
            {
                drawData = value;
            }
        }

        /// <summary>
        /// Obtains the drawing position.
        ///
        /// 描画位置を取得します。
        /// </summary>
        public Point Position
        {
            get
            {
                return drawData.Position;
            }
        }

        /// <summary>
        /// Obtains the drawing color.
        ///
        /// 描画色を取得します。
        /// </summary>
        public Color Color
        {
            get
            {
                return drawData.Color;
            }
        }

        /// <summary>
        /// Obtains the drawing conversion scale.
        ///
        /// 描画変換スケールを取得します。
        /// </summary>
        public Vector2 Scale
        {
            get
            {
                return drawData.Scale;
            }
        }

        /// <summary>
        /// Obtains the center position for drawing conversion.
        ///
        /// 描画変換中心を取得します。
        /// </summary>
        public Point Center
        {
            get
            {
                return drawData.Center;
            }
        }

        /// <summary>
        /// Obtains the rotation value for drawing conversion.
        ///
        /// 描画変換回転量を取得します。
        /// </summary>
        public float RotateZ
        {
            get
            {
                return drawData.RotateZ;
            }
        }

        /// <summary>
        /// Obtains and sets the texture.
        /// The setting is called from SceneDataReader on initialization of scenes.
        ///
        /// テクスチャの設定取得を行います。
        /// 設定はシーンの初期化時にSceneDataReaderから呼び出されます。
        /// </summary>
        private Texture2D Texture
        {
            get{
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        /// <summary>
        /// The temporary drawing conversion information 
        /// specified during sequence play.  Based on this information, 
        /// display items synchronized with the sequence can be positioned.
        /// </summary>
        [ContentSerializerIgnore]
        public DrawData InterpolationDrawData
        {
            get { return interpolationDrawData; }
            set { interpolationDrawData = value; }
        }

        #endregion

        /// <summary>
        /// Performs initialization.
        /// Loads the XNA graphic textures through ContentManager.
        ///
        /// 初期化を行います。
        /// ContentManagerを通して、
        /// XNAグラフィックのテクスチャを読み込みます。
        /// </summary>
        /// <param name="content">
        /// ContentManager
        /// 
        /// コンテントマネージャー
        /// </param>
        public void Init(ContentManager content)
        {
            if (!String.IsNullOrEmpty(TextureName))
            {
                Texture = content.Load<Texture2D>(TextureName);
            }
        }

        /// <summary>
        /// Performs drawing.
        /// baseDrawData contains information for entire sequence conversion, 
        /// and sequenceDrawData contains conversion information interpolated for 
        /// sequence display (including conversion information for pattern objects
        /// themselves). 
        /// 
        /// 描画を行います。
        /// baseDrawDataは、シーケンス全体の変換を、
        /// sequenceDrawDataは、シーケンス表示のために動きを補完された
        /// 変換情報が入っています(これはパターンオブジェクト自体の変換情報を
        /// 含んでいます)。
        /// </summary>
        /// <param name="sb">
        /// SpriteBatch
        /// 
        /// スプライトバッチ
        /// </param>
        /// <param name="sequenceDrawData">
        /// Conversion information for sequence
        /// 
        /// シーケンス用変換情報
        /// </param>
        /// <param name="baseDrawData">
        /// Basic conversion information for drawing
        /// 
        /// 描画用基本変換情報
        /// </param>
        public void Draw(SpriteBatch sb, DrawData sequenceDrawData, 
                                            DrawData baseDrawData)
        {
            // If no texture is specified, returns.
            // 
            // テクスチャが指定されていなければ抜ける
            if ((Texture == null) || Texture.IsDisposed)
            {
                return;
            }

            Vector2 position = new Vector2();

            //Creates a matrix and colors for drawing 
            //from the interpolated conversion information.
            //This matrix is temporarily used to determine the display position.
            //
            //補完された変換情報から
            //描画のためのマトリクスと色を作成します
            //マトリクスは、表示位置を求めるための一時的なものです。
            float rotateZ = sequenceDrawData.RotateZ;
            Vector2 vectorScale = sequenceDrawData.Scale;
            Color color = sequenceDrawData.Color;

            Matrix matrix = Matrix.CreateTranslation(
                sequenceDrawData.Position.X + sequenceDrawData.Center.X,
                sequenceDrawData.Position.Y + sequenceDrawData.Center.Y,
                0.0f
                );

            //If the basic conversion information is valid, 
            //creates the matrix and colors.
            //
            //基本変換情報が有効なら、
            //マトリクスと色を作成します。
            if (null != baseDrawData)
            {
                rotateZ += baseDrawData.RotateZ;
                vectorScale *= baseDrawData.Scale;
                color = new Color(  (byte)(color.R * baseDrawData.Color.R / 0xFF), 
                                    (byte)(color.G * baseDrawData.Color.G / 0xFF), 
                                    (byte)(color.B * baseDrawData.Color.B / 0xFF),
                                    (byte)(color.A * baseDrawData.Color.A / 0xFF));

                position = new Vector2(baseDrawData.Position.X, 
                                        baseDrawData.Position.Y);

                matrix *= 
                    Matrix.CreateScale(new Vector3(baseDrawData.Scale.X, 
                                        baseDrawData.Scale.Y, 1.0f)) *
                                        Matrix.CreateRotationZ(baseDrawData.RotateZ);
            }

            //Determines the final display position.
            //
            //最終の表示位置を求めます。
            position += new Vector2(matrix.Translation.X, matrix.Translation.Y);

            SpriteEffects effects = SpriteEffects.None;

            //If the sprite needs to be flipped, apply the flip information to it.
            //
            //反転がある場合、適用します。
            if (flipH)
                effects |= SpriteEffects.FlipHorizontally;
            if (flipV)
                effects |= SpriteEffects.FlipVertically;

            //Drawing
            //
            //描画
            sb.Draw(Texture, position, Rect, color, 
                    MathHelper.ToRadians(rotateZ),
                    new Vector2(sequenceDrawData.Center.X, sequenceDrawData.Center.Y),
                    vectorScale,
                    effects,
                    1.0f);
        }
    }
}
