#region File Description
//-----------------------------------------------------------------------------
// GameText.cs
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
using RobotGameData.GameInterface;
using RobotGameData.Render;
using RobotGameData.Text;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// displays text on 2D screen.
    /// Since it can attach directly to the 2D scene node, 
    /// it will be affected by the 2D scene node.
    /// </summary>
    public class GameText : GameSceneNode
    {
        #region Fields

        TextItem textItem = new TextItem();

        #endregion

        #region Properties

        public SpriteFont Font
        {
            get { return this.textItem.Font; }
            set { this.textItem.Font = value; }
        }

        public string Text
        {
            get { return this.textItem.Text; }
            set { this.textItem.Text = value; }
        }

        public int PosX
        {
            get { return this.textItem.PosX; }
            set { this.textItem.PosX = value; }
        }

        public int PosY
        {
            get { return this.textItem.PosY; }
            set { this.textItem.PosY = value; }
        }

        public Color Color
        {
            get { return this.textItem.Color; }
            set { this.textItem.Color = value; }
        }

        public byte Alpha
        {
            get { return this.textItem.Color.A; }
            set
            {
                this.textItem.Color = new Color(
                        this.textItem.Color.R,
                        this.textItem.Color.G,
                        this.textItem.Color.B,
                        value);
            }
        }

        public float Rotation
        {
            get { return this.textItem.Rotation; }
            set { this.textItem.Rotation = value; }
        }

        public float Scale
        {
            get { return this.textItem.Scale; }
            set { this.textItem.Scale = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="font">sprite font</param>
        /// <param name="text">message</param>
        /// <param name="x">position x</param>
        /// <param name="y">position y</param>
        /// <param name="color">text color</param>
        public GameText(SpriteFont font, string text, int x, int y, Color color)
            : base()
        {
            Create(font, text, x, y, color);
        }

        /// <summary>
        /// draws string on screens by using the information stored in TextItem.
        /// Internally, the sprite batch’s Begin() function and the End() function 
        /// are not called.
        /// </summary>
        /// <param name="renderTracer"></param>
        protected override void OnDraw(RenderTracer renderTracer)
        {
            renderTracer.SpriteBatch.DrawString(this.textItem.Font, 
                                                this.textItem.Text,
                                                new Vector2(this.textItem.PosX, 
                                                    this.textItem.PosY), 
                                                this.textItem.Color,
                                                this.textItem.Rotation,
                                                Vector2.Zero,
                                                this.textItem.Scale,
                                                SpriteEffects.None,
                                                1.0f);
        }

        /// <summary>
        /// creates a text message.
        /// text information is stored as TextItem structure.
        /// </summary>
        /// <param name="font">sprite font</param>
        /// <param name="text">string message</param>
        /// <param name="x">2D screen x-position (pixel)</param>
        /// <param name="y">2D screen y-position (pixel)</param>
        /// <param name="color">text color</param>
        public void Create(SpriteFont font, string text, int x, int y, Color color)
        {
            this.textItem.Font = font;
            this.textItem.Text = text;
            this.textItem.PosX = x;
            this.textItem.PosY = y;
            this.textItem.Color = color;
        }
    }
}
