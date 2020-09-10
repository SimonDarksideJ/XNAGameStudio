#region File Description
//-----------------------------------------------------------------------------
// SwordSlash.cs
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

namespace NinjAcademy
{
    /// <summary>
    /// Used to represent sword slashes on the screen during the game. This component will take care of enabling and
    /// disabling itself.
    /// </summary>
    class SwordSlash : TexturedDrawableGameComponent
    {
        #region Private Sub-Types


        enum State
        {
            Static,
            Appearing,
            Fading
        }


        #endregion

        #region Fields/Properties


        readonly Vector2 textureOrigin = new Vector2(6, 75);

        Vector2 source;

        State state = State.Appearing;

        TimeSpan fadeDuration = TimeSpan.FromMilliseconds(500);
        TimeSpan growthDuration = TimeSpan.FromMilliseconds(100);
        TimeSpan timer = TimeSpan.Zero;

        float desiredScale;

        float alpha = 1;

        /// <summary>
        /// Rotation in radians to use when rendering the sword slash.
        /// </summary>
        public float Rotation { get; set; }

        Vector2 scaleVector = new Vector2(1, 1);
        /// <summary>
        /// Scale factor for the sword slash. Only the slash's length is scaled.
        /// </summary>
        public float Stretch
        {
            get
            {
                return scaleVector.Y;
            }

            set
            {
                scaleVector.Y = value;
            }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new sword slash instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Screen where the element will appear.</param>
        /// <param name="texture">Texture asset which represents the sword slash.</param>
        public SwordSlash(Game game, GameScreen gameScreen, Texture2D texture)
            : base(game, gameScreen, texture)
        {
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the sword slash's appearance.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            timer += gameTime.ElapsedGameTime;

            switch (state)
            {
                case State.Static:
                    // No update required in this case
                    break;
                case State.Appearing:
                    // Cause the slash to grow
                    Stretch = (float)(desiredScale * timer.TotalMilliseconds / growthDuration.TotalMilliseconds);

                    if (timer >= growthDuration)
                    {
                        Fade(fadeDuration);
                    }
                    break;
                case State.Fading:
                    // Cause the slash to fade, and ultimately vanish
                    alpha = (float)(1 - timer.TotalMilliseconds / fadeDuration.TotalMilliseconds);

                    if (timer >= fadeDuration)
                    {
                        Enabled = false;
                        Visible = false;
                    }
                    break;
                default:
                    break;
            }
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(texture, source, null, Color.White * alpha, Rotation, textureOrigin, scaleVector,
                SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Readies the sword slash to be displayed by initializing it. This causes the slash component to enable
        /// itself.
        /// </summary>
        public void Reset()
        {
            // Make the slash active and visible
            alpha = 1;
            Enabled = true;
            Visible = true;
        }

        /// <summary>
        /// Cause the sword slash to fade over the course of a specified time span. The component will become inactive
        /// after the specified time span.
        /// </summary>
        /// <param name="fadeDuration">The time it should take the sword slash to fully disappear.</param>
        public void Fade(TimeSpan fadeDuration)
        {
            timer = TimeSpan.Zero;

            this.fadeDuration = fadeDuration;

            state = State.Fading;
        }

        /// <summary>
        /// Positions the slash so that it begins and ends at specified points. The slash will remain as specified
        /// until changed.
        /// </summary>
        /// <param name="source">The slash's origin.</param>
        /// <param name="destination">The slash's endpoint.</param>
        public void PositionSlash(Vector2 source, Vector2 destination)
        {
            state = State.Static;
            this.source = source;

            InitializeSlashForCoordinates(source, destination);

            Stretch = desiredScale;
        }

        /// <summary>
        /// Displays a sword slash on the screen. The slash will expand and then fade over a specified periods of time.
        /// </summary>
        /// <param name="source">The slash's origin.</param>
        /// <param name="destination">The slash's endpoint.</param>
        /// <param name="growthDuration">The amount of time it should take the slash to stretch from the origin to the
        /// destination.</param>
        /// <param name="fadeDuration">Amount of time it should take the slash to fade after reaching its 
        /// full size.</param>
        /// <remarks>Once the slash disappears, it will disable itself.</remarks>
        public void Slash(Vector2 source, Vector2 destination, TimeSpan growthDuration, TimeSpan fadeDuration)
        {                        
            this.source = source;
            this.fadeDuration = fadeDuration;
            this.growthDuration = growthDuration;

            state = State.Appearing;
            Stretch = 0;

            InitializeSlashForCoordinates(source, destination);            

            // Initialize the timer to use while the sword slash appears
            timer = TimeSpan.Zero;            
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Initializes the component's members for displaying a sword slash between two specified positions.
        /// </summary>
        /// <param name="source">The slash's origin.</param>
        /// <param name="destination">The slash's endpoint.</param>
        public void InitializeSlashForCoordinates(Vector2 source, Vector2 destination)
        {            
            // Find the scale required to properly display the slash
            desiredScale = (source - destination).Length() / Bounds.Height();

            // Calculate the required rotation for the sword slash (flip the Y as the screen's Y-axis is flipped)
            Vector2 desiredDirectionUnitVector = destination - source;
            desiredDirectionUnitVector.Y = -desiredDirectionUnitVector.Y;
            desiredDirectionUnitVector.Normalize();

            Rotation = (float)Math.Acos(Vector2.Dot(desiredDirectionUnitVector, Vector2.UnitY));

            if (desiredDirectionUnitVector.X < 0)
            {
                Rotation = -Rotation;
            }
        }


        #endregion
    }
}
