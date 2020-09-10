#region File Description
//-----------------------------------------------------------------------------
// Cat.cs
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Flocking
{
    /// <summary>
    /// Represents a dangerous object that the flocks flees from
    /// </summary>
    class Cat : Animal
    {
        #region Fields
        protected Vector2 center;
        #endregion

        #region Initialization
        /// <summary>
        /// Sets up the Cat's move speed and puts it in the center of the screen
        /// </summary>
        /// <param name="tex">Texture to use</param>
        /// <param name="screenSize">Size of the sample screen</param>
        public Cat(Texture2D tex, int screenWidth, int screenHeight)
            : base(tex, screenWidth, screenHeight)
        {
            if (tex != null)
            {
                texture = tex;
                textureCenter = new Vector2(texture.Width / 2, texture.Height / 2);
            }
            center.X = screenWidth / 2;
            center.Y = screenHeight / 2;
            location = center;
            moveSpeed = 500.0f;
            animaltype = AnimalType.Cat;
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Move the cat, keeping in inside screen boundry
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (gameTime != null)
            {
                if (direction.Length() > .01f)
                {
                    float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    location.X += direction.X * moveSpeed * elapsedTime;

                    if (location.X < 0.0f)
                    {
                        location.X = 0.0f;
                    }
                    else if (location.X > boundryWidth)
                    {
                        location.X = boundryWidth;
                    }

                    location.Y += direction.Y * moveSpeed * elapsedTime;
                    if (location.Y < 0.0f)
                    {
                        location.Y = 0.0f;
                    }
                    else if (location.Y > boundryHeight)
                    {
                        location.Y = boundryHeight;
                    }
                }
            }
        }
        /// <summary>
        /// Draw the cat
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, location, null, color,
                0f, textureCenter, 1.0f, SpriteEffects.None, 0.0f);
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Poll the input state for movement
        /// </summary>
        /// <param name="input"></param>
        public void HandleInput(InputState input)
        {
            direction.X = input.MoveCatX;
            direction.Y = input.MoveCatY;
        }
        #endregion

    }
}
