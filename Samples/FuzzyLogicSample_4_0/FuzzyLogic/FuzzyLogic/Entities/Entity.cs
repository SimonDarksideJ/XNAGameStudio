#region File Description
//-----------------------------------------------------------------------------
// Entity.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// An Entity is an abstract class that contains the code that is common between
    /// the tank and the mouse. It is in charge of drawing and updating the tank and
    /// mouse objects.
    /// </summary>
    public abstract class Entity
    {
        #region Fields

        /// <summary>
        /// The texture that will be drawn to represent this entity.
        /// </summary>
        private Texture2D texture;

        #endregion

        #region Properties

        /// <summary>
        /// How fast can this entity move? This property is implemented by subclasses
        /// of Entity.
        /// </summary>
        abstract public float MaxSpeed { get; }

        /// <summary>
        /// How fast can this entity turn? This property is implemented by subclasses
        /// of Entity.
        /// </summary>
        abstract public float TurnSpeed { get; }

        /// <summary>
        /// Determines what texture file is loaded and drawn for this entity. 
        /// This property is implemented by subclasses of Entity.
        /// </summary>
        abstract public string TextureFile { get; }

        /// <summary>
        /// Gets and sets the entity's position on the screen.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Vector2 position;

        /// <summary>
        /// Gets and sets the entity's orientation. This value is in terms of radians.
        /// </summary>
        public float Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }
        private float orientation;

        /// <summary>
        /// Gets and sets the current speed of the entity. Typically, behaviors will 
        /// modify this value.
        /// </summary>
        public float CurrentSpeed
        {
            get { return currentSpeed; }
            set { currentSpeed = value; }
        }
        private float currentSpeed;

        /// <summary>
        /// The entity's current behavior. The behavior is in charge of updating the 
        /// entity's speed and orientation.
        /// </summary>
        public Behavior CurrentBehavior
        {
            get { return currentBehavior; }
            set { currentBehavior = value; }
        }
        private Behavior currentBehavior;

        /// <summary>
        /// If the entity is highlighted, it will have a pulsing red tint when it is
        /// drawn. The tank itself and its prey when it is chasing a mouse.
        /// </summary>
        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set { isHighlighted = value; }
        }
        private bool isHighlighted;

        /// <summary>
        /// Entities keep track of a rectangle of the view port so that they know 
        /// where they can go on screen. This is exposed through a property so that 
        /// behaviors have access to the same information.
        /// </summary>
        public Rectangle LevelBoundary
        {
            get { return levelBoundary; }
        }
        private Rectangle levelBoundary;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor takes in the game that is hosting this entity and stores
        /// it.
        /// </summary>
        protected Entity(Rectangle levelBoundary)
        {
            this.levelBoundary = levelBoundary;
        }

        /// <summary>
        /// LoadContent will load the entity's texture.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>(TextureFile);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw will draw the entity using the specified SpriteBatch.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Color tintColor = Color.White;

            // If the entity is highlighted, we want to make it pulse with a red tint.
            if (IsHighlighted)
            {
                // To do this, we'll first generate a value t, which we'll use to
                // determine how much tint to have.
                float t = (float)Math.Sin(10 * gameTime.TotalGameTime.TotalSeconds);

                // Sin varies from -1 to 1, and we want t to go from 0 to 1, so we'll 
                // scale it now.
                t = .5f + .5f * t;

                // Finally, we'll calculate our tint color by using Lerp to generate
                // a color in between Red and White.
                tintColor = new Color(Vector4.Lerp(
                    Color.Red.ToVector4(), Color.White.ToVector4(), t));
            }

            // Draw the entity, centered around its position, and using the orientation
            // and tint color.
            Vector2 textureCenter = new Vector2(texture.Width / 2, texture.Height / 2);
            spriteBatch.Draw(texture, Position, null, tintColor,
                orientation, textureCenter, 1.0f, SpriteEffects.None, 0.0f);
        }

        #endregion

        #region Update

        /// <summary>
        /// Update chooses a new behavior, allows that behavior to update, and then
        /// moves the entity forward.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Use ChooseBehavior to decide what the next behavior is. this is an 
            // abstract method that our subclasses will implement.
            ChooseBehavior(gameTime);

            if (CurrentBehavior != null)
            {
                CurrentBehavior.Update();
            }

            Vector2 heading = new Vector2(
                (float)Math.Cos(Orientation), (float)Math.Sin(Orientation));

            Position += heading * CurrentSpeed;
            Position = ClampToLevelBoundary(Position);
        }

        /// <summary>
        /// ChooseBehavior will be defined by Entity's subclasses, and is used to decide
        /// which behavior an entity will use next. For example, this is where the tank
        /// will change from idling to chasing.
        /// </summary>
        protected abstract void ChooseBehavior(GameTime gameTime);

        /// <summary>
        /// This function takes a Vector2 as input, and returns that vector "clamped"
        /// to the current graphics title safe area. We use this function to make sure that 
        /// no one can go off of the screen.
        /// </summary>
        /// <param name="vector">an input vector</param>
        /// <returns>the input vector, clamped between the minimum and maximum of the
        /// title safe area.</returns>
        private Vector2 ClampToLevelBoundary(Vector2 vector)
        {
            vector.X =
                MathHelper.Clamp(vector.X, levelBoundary.X, levelBoundary.Right);
            vector.Y =
                MathHelper.Clamp(vector.Y, levelBoundary.Y, levelBoundary.Bottom);
            return vector;
        }

        #endregion

    }
}
