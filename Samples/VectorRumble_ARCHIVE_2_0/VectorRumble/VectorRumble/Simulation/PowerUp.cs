#region File Description
//-----------------------------------------------------------------------------
// PowerUp.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Base class for all power-ups that exist in the game.
    /// </summary>
    abstract class PowerUp : Actor
    {
        #region Constants
        /// <summary>
        /// The scalar between time elapsed and the scale period.
        /// </summary>
        protected float timeToScalePeriod = 8f;
        #endregion

        #region Fields
        /// <summary>
        /// A second polygon for rendering the power-up.
        /// </summary>
        protected VectorPolygon innerPolygon;

        /// <summary>
        /// Keeps track of the period of the scale "pulse" of the power-up.
        /// </summary>
        protected float scalePeriodElapsed;

        /// <summary>
        /// Colors for the particle systems created when spawned and touched.
        /// </summary>
        protected Color[] particleColors = 
            {
                Color.White, Color.Lime, Color.Red, Color.CornflowerBlue, Color.Yellow
            };
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new power-up.
        /// </summary>
        /// <param name="world">The world that this power-up belongs to.</param>
        public PowerUp(World world) : base(world) 
        {
            this.mass = 500f;
            this.polygon = VectorPolygon.CreateCircle(Vector2.Zero, 16f, 16);
            this.innerPolygon = VectorPolygon.CreateCircle(Vector2.Zero, 10f, 16);
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Render the power-up.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="lineBatch">The LineBatch to render to.</param>
        public override void Draw(float elapsedTime, LineBatch lineBatch)
        {
            // update the scale period
            scalePeriodElapsed += elapsedTime * timeToScalePeriod;
            while (scalePeriodElapsed < 0)
            {
                scalePeriodElapsed += MathHelper.TwoPi;
            }
            while (scalePeriodElapsed > MathHelper.TwoPi)
            {
                scalePeriodElapsed -= MathHelper.TwoPi;
            }
            // draw the polygons 
            if (polygon != null)
            {
                if (lineBatch == null)
                {
                    throw new ArgumentNullException("lineBatch");
                }
                // calculate the transformation
                Matrix scaleMatrix = Matrix.CreateScale(0.8f + 
                    0.1f * (float)Math.Cos(scalePeriodElapsed));
                Matrix world = scaleMatrix * 
                    Matrix.CreateTranslation(position.X, position.Y, 0f);
                // transform the polygons
                polygon.Transform(world);
                innerPolygon.Transform(world);
                // draw the polygons
                lineBatch.DrawPolygon(polygon, Color.White);
                if (innerPolygon != null)
                {
                    lineBatch.DrawPolygon(innerPolygon, color, true);
                }
                // draw the motion blur
                if (useMotionBlur && velocity.LengthSquared() > 1024f)
                {
                    // draw several "blur" polygons behind the real polygons
                    Vector2 backwards = Vector2.Normalize(position - lastPosition);
                    float speed = velocity.Length();
                    for (int i = 1; i < speed / 16; ++i)
                    {
                        // calculate the "blur" position
                        Vector2 blurPosition = this.position - backwards * (i * 4);
                        // calculate the transformation
                        Matrix blurWorld = scaleMatrix *
                            Matrix.CreateTranslation(blurPosition.X, blurPosition.Y, 0);
                        // transform the polygons
                        polygon.Transform(blurWorld);
                        if (innerPolygon != null)
                        {
                            innerPolygon.Transform(blurWorld);
                        }
                        // calculate the alpha of the "blur" polygons polygons
                        byte alpha = (byte)(160 / (i + 1));
                        if (alpha < 1)
                            break;
                        // draw the "blur" polygons
                        lineBatch.DrawPolygon(polygon, new Color(255, 255, 255, alpha));
                        if (innerPolygon != null)
                        {
                            lineBatch.DrawPolygon(innerPolygon,
                                new Color(color.R, color.G, color.B, alpha), true);
                        }
                    }
                }
            }
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Defines the interaction between this power-up and a target actor
        /// when they touch.
        /// </summary>
        /// <param name="target">The actor that is touching this object.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(Actor target)
        {
            // if it touched a ship, then create a particle system and play a sound
            Ship ship = target as Ship;
            if (ship != null)
            {
                // tickle the ship's vibration motors
                ship.FireGamepadMotors(0f, 0.25f);
                // add a particle system for effect
                world.ParticleSystems.Add(new ParticleSystem(this.Position, 
                    Vector2.Zero, 24, 32f, 64f, 2f, 0.1f, particleColors));
                // play the "power-up picked up" cue
                world.AudioManager.PlayCue("powerUpTouch");
                // kill the power-up
                Die(target);
                return false;
            }

            return base.Touch(target);
        }

        /// <summary>
        /// Damage this power-up by the amount provided.
        /// </summary>
        /// <remarks>
        /// Power-ups cannot be damaged.
        /// </remarks>
        /// <param name="source">The actor responsible for the damage.</param>
        /// <param name="damageAmount">The amount of damage.</param>
        /// <returns>If true, this object was damaged.</returns>
        public override bool Damage(Actor source, float damageAmount)
        {
            return false;
        }


        /// <summary>
        /// Place this power-up in the world.
        /// </summary>
        /// <param name="findSpawnPoint">
        /// If true, the power-up's position is changed to a valid, non-colliding point.
        /// </param>
        public override void Spawn(bool findSpawnPoint)
        {
            // spawn the power-up
            base.Spawn(findSpawnPoint);
            // recreate the particle array using the current color
            this.particleColors = new Color[] { this.color, Color.White };
            // add a particle effect and play a sound effect
            world.ParticleSystems.Add(new ParticleSystem(this.position, 
                Vector2.Zero, 24, 32f, 64f, 2f, 0.1f, particleColors));
            world.AudioManager.PlayCue("powerUpSpawn");
        }
        #endregion
    }
}
