#region File Description
//-----------------------------------------------------------------------------
// Animal.cs
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace Flocking
{
    public enum AnimalType
    {
        // no type
        Generic,
        // flies around and reacts
        Bird,
        // controled by teh thumbstick, birds flee from it
        Cat
    }
    /// <summary>
    /// base class for moveable, drawable critters onscreen
    /// </summary>
    public class Animal
    {
        #region Fields
        /// <summary>
        /// texture drawn to represent this animal
        /// </summary>
        protected Texture2D texture;
        /// <summary>
        /// tint color to draw the texture with
        /// </summary>
        protected Color color = Color.White;
        /// <summary>
        /// center of the draw texture
        /// </summary>
        protected Vector2 textureCenter;
        /// <summary>
        /// movement speed in pixels/second
        /// </summary>
        protected float moveSpeed;

        /// <summary>
        /// All the behavior that this animal has
        /// </summary>
        protected Dictionary<AnimalType, Behaviors> behaviors;

        /// <summary>
        /// The animal type
        /// </summary>
        public AnimalType AnimalType
        {
            get
            {
                return animaltype;
            }
        }
        protected AnimalType animaltype = AnimalType.Generic;

        /// <summary>
        /// Reaction distance
        /// </summary>
        public float ReactionDistance
        {
            get
            {
                return reactionDistance;
            }
        }
        protected float reactionDistance;

        /// <summary>
        /// Reaction location
        /// </summary>
        public Vector2 ReactionLocation
        {
            get
            {
                return reactionLocation;
            }
        }
        protected Vector2 reactionLocation;

        public bool Fleeing
        {
            get
            {
                return fleeing;
            }
            set 
            { 
                fleeing = value; 
            }
        }
        protected bool fleeing = false;

        public int BoundryWidth
        {
            get
            {
                return boundryWidth;
            }
        }
        protected int boundryWidth;

        public int BoundryHeight
        {
            get
            {
                return boundryHeight;
            }
        }
        protected int boundryHeight;

        /// <summary>
        /// Direction the animal is moving in
        /// </summary>
        public Vector2 Direction
        {
            get
            {
                return direction;
            }
        }
        protected Vector2 direction;

        /// <summary>
        /// Location on screen
        /// </summary>
        public Vector2 Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }
        protected Vector2 location;

        #endregion

        #region Initialization
        /// <summary>
        /// Sets the boundries the animal can move in the texture used in Draw
        /// </summary>
        /// <param name="tex">Texture to use</param>
        /// <param name="screenSize">Size of the sample screen</param>
        public Animal(Texture2D tex, int screenWidth, int screenHeight) 
        {
            if (tex != null)
            {
                texture = tex;
                textureCenter = new Vector2(texture.Width / 2, texture.Height / 2);
            }
            boundryWidth = screenWidth;
            boundryHeight = screenHeight;
            moveSpeed = 0.0f;

            behaviors = new Dictionary<AnimalType, Behaviors>();
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Empty update function
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Draw the Animal with the specified SpriteBatch
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            float rotation = (float)Math.Atan2(direction.Y, direction.X);            

            spriteBatch.Draw(texture, location, null, color,
                rotation, textureCenter, 1.0f, SpriteEffects.None, 0.0f);
        }
        #endregion
    }
}