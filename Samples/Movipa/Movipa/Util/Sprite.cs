#region File Description
//-----------------------------------------------------------------------------
// Sprite.cs
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
    /// Manages Sprite information.
    /// Contains the main parameters used by SpriteBatch, as well as 
    /// a primitive draw method available for Draw events. 
    /// 
    /// スプライトの情報を管理します。
    /// SpriteBatchで使用される主なパラメータを持ち、
    /// Drawイベントで使用できる基本的な描画メソッドを有します。
    /// </summary>
    public class Sprite : Movipa.Util.GameComponentObject
    {
        #region Fields
        protected Texture2D texture;
        protected Vector2 position;
        protected Vector2 texturePosition;
        protected Vector2 size;
        protected Color color;
        protected float scale;
        protected Vector2 origin;
        protected float priority;
        protected float rotate;
        protected float direction;
        protected float speed;
        protected Sprite parent;
        protected List<Sprite> child;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the texture.
        /// 
        /// テクスチャを取得または設定します。
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }


        /// <summary>
        /// Obtains or sets the position.
        /// 
        /// ポジションを取得または設定します。
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }


        /// <summary>
        /// Obtains or sets the texture source coordinates.
        /// 
        /// テクスチャの転送元座標を取得または設定します。
        /// </summary>
        public Vector2 TexturePosition
        {
            get { return texturePosition; }
            set { texturePosition = value; }
        }


        /// <summary>
        /// Obtains or sets the size.
        /// 
        /// サイズを取得または設定します。
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }


        /// <summary>
        /// Obtains or sets the draw color.
        /// 
        /// 描画色を取得または設定します。
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }


        /// <summary>
        /// Obtains or sets the scale.
        /// 
        /// スケールを取得または設定します。
        /// </summary>
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }


        /// <summary>
        /// Obtains or sets the center coordinates.
        /// 
        /// 中心座標を取得または設定します。
        /// </summary>
        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }


        /// <summary>
        /// Obtains or sets the priority.
        /// 
        /// 優先度を取得または設定します。
        /// </summary>
        public float Priority
        {
            get { return priority; }
            set { priority = value; }
        }


        /// <summary>
        /// Obtains or sets the rotation angle.
        /// 
        /// 回転角度を取得または設定します。
        /// </summary>
        public float Rotate
        {
            get { return rotate; }
            set { rotate = value; }
        }


        /// <summary>
        /// Obtains or sets the movement direction.
        /// 
        /// 移動方向を取得または設定します。
        /// </summary>
        public float Direction
        {
            get { return direction; }
            set { direction = value; }
        }


        /// <summary>
        /// Obtains or sets the movement speed. 
        /// 
        /// 移動速度を取得または設定します。
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }


        /// <summary>
        /// Obtains or sets the parent sprite.
        /// 
        /// 親のスプライトを取得または設定します。
        /// </summary>
        public Sprite Parent
        {
            get { return parent; }
            set { parent = value; }
        }


        /// <summary>
        /// Obtains the child sprite.
        /// 
        /// 子のスプライトリストを取得します。
        /// </summary>
        public List<Sprite> Child
        {
            get { return child; }
        }


        /// <summary>
        /// Obtains the position.
        /// 
        /// ポジションを取得します。
        /// </summary>
        public Rectangle RectanglePosition
        {
            get
            {
                Rectangle value = new Rectangle();

                value.X = (int)Position.X;
                value.Y = (int)Position.Y;
                value.Width = (int)Size.X;
                value.Height = (int)Size.Y;

                return value;
            }
        }


        /// <summary>
        /// Obtains the texture source.
        /// 
        /// テクスチャの転送元を取得します。
        /// </summary>
        public Rectangle SourceRectangle
        {
            get
            {
                Rectangle value = new Rectangle();

                value.X = (int)TexturePosition.X;
                value.Y = (int)TexturePosition.Y;
                value.Width = (int)Size.X;
                value.Height = (int)Size.Y;

                return value;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public Sprite(Game game)
            : base(game)
        {
            // Initializes the member variables.
            // 
            // メンバ変数を初期化します。
            texture = null;
            position = Vector2.Zero;
            texturePosition = Vector2.Zero;
            size = Vector2.Zero;
            color = Color.White;
            scale = 1.0f;
            origin = Vector2.Zero;
            priority = 0.0f;
            rotate = 0.0f;
            direction = 0.0f;
            speed = 1.0f;
            parent = null;
            child = new List<Sprite>();
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs a primitive draw.
        /// This method needs to be invoked in advance since the 
        /// SpriteBatch Begin and End are not performed.
        ///
        /// 基本的な描画を行います。
        /// このメソッドではSpriteBatchのBegin/Endが行われないので
        /// 事前に呼び出して下さい。
        /// </summary>
        protected void Draw(SpriteBatch batch)
        {
            // Processing is not performed if there is no texture.
            // 
            // テクスチャが無い場合は処理を行わないようにします。
            if (Texture == null)
                return;

            // Performs drawing processing.
            // 
            // 描画を行います。
            batch.Draw(
                Texture,
                Position,
                SourceRectangle,
                Color,
                Rotate,
                Origin,
                Scale,
                SpriteEffects.None,
                Priority);
        }
        #endregion
   }
}