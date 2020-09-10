#region File Description
//-----------------------------------------------------------------------------
// Actor.cs
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
    /// A base class for all active objects in the game.
    /// </summary>
    abstract class Actor
    {
        #region Fields
        /// <summary>
        /// The world which owns this actor.
        /// </summary>
        protected World world;

        protected bool dead = true;
        protected float life = 0f;

        protected Vector2 position = Vector2.Zero;
        protected Vector2 lastPosition = Vector2.Zero;
        protected Vector2 velocity = Vector2.Zero;
        protected float rotation = 0f;

        // collision data
        protected bool collidable = true;
        protected float mass = 1f;
        protected float radius = 16f;
        protected bool collidedThisFrame = false;

        // visual data
        protected VectorPolygon polygon = null;
        protected Color color = Color.White;
        protected bool useMotionBlur = WorldRules.MotionBlur;
        #endregion

        #region Properties
        public World World
        {
            get { return world; }
        }

        public bool Dead
        {
            get { return dead; }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                lastPosition = position;
                position = value;
            }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public bool Collidable
        {
            get { return collidable; }
        }

        public float Mass
        {
            get { return mass; }
        }

        public bool CollidedThisFrame
        {
            get { return collidedThisFrame; }
            set { collidedThisFrame = value; }
        }

        public float Radius
        {
            get { return radius; }
        }

        public Color Color
        {
            get { return color; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new actor.
        /// </summary>
        /// <param name="world">The world that this actor belongs to.</param>
        public Actor(World world)
        {
            if (world == null)
            {
                throw new ArgumentNullException("world");
            }
            this.world = world;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the actor.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public virtual void Update(float elapsedTime) 
        {
            collidedThisFrame = false;
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Render the actor.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="lineBatch">The LineBatch to render to.</param>
        public virtual void Draw(float elapsedTime, LineBatch lineBatch)
        {
            if (polygon != null)
            {
                if (lineBatch == null)
                {
                    throw new ArgumentNullException("lineBatch");
                }
                // create the transformation
                Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);
                Matrix world =  rotationMatrix *
                    Matrix.CreateTranslation(position.X, position.Y, 0f);
                // transform the polygon
                polygon.Transform(world);
                // draw the polygon
                lineBatch.DrawPolygon(polygon, color);

                // draw the motion blur
                if (useMotionBlur && velocity.LengthSquared() > 1024f)
                {
                    // draw several "blur" polygons behind the real polygon
                    Vector2 backwards = Vector2.Normalize(position - lastPosition);
                    float speed = velocity.Length();
                    for (int i = 1; i < speed / 16; ++i)
                    {
                        // calculate the "blur" polygon's position
                        Vector2 blurPosition = this.position - backwards * (i * 4);
                        // calculate the transformation for the "blur" polygon
                        Matrix blurWorld = rotationMatrix *
                            Matrix.CreateTranslation(blurPosition.X, blurPosition.Y, 0);
                        // transform the polygon to the "blur" location
                        polygon.Transform(blurWorld);
                        // calculate the alpha of the "blur" location
                        byte alpha = (byte)(160 / (i + 1));
                        if (alpha < 1)
                            break;
                        // draw the "blur" polygon
                        lineBatch.DrawPolygon(polygon,
                            new Color(color.R, color.G, color.B, alpha));
                    }
                }
            }
        }
        #endregion

        #region Interaction
        /// <summary>
        /// Defines the interaction between this actor and a target actor
        /// when they touch.
        /// </summary>
        /// <param name="target">The actor that is touching this object.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public virtual bool Touch(Actor target) 
        { 
            return true; 
        }


        /// <summary>
        /// Damage this actor by the amount provided.
        /// </summary>
        /// <remarks>
        /// This function is provided in lieu of a Life mutation property to allow 
        /// classes of objects to restrict which kinds of objects may damage them,
        /// and under what circumstances they may be damaged.
        /// </remarks>
        /// <param name="source">The actor responsible for the damage.</param>
        /// <param name="damageAmount">The amount of damage.</param>
        /// <returns>If true, this object was damaged.</returns>
        public virtual bool Damage(Actor source, float damageAmount)
        {
            // reduce life by the given amound
            life -= damageAmount;
            // if life had gone below 0, then we're dead
            // -- 0 health actors are destroyed by any damage
            if (life < 0f)
            {
                Die(source);
            }
            return true;
        }

        
        /// <summary>
        /// Kills this actor, in response to the given actor.
        /// </summary>
        /// <param name="source">The actor responsible for the kill.</param>
        public virtual void Die(Actor source) 
        {
            if (dead == false)
            {
                // arrrggghhhh
                dead = true;
                // remove this actor from the world
                world.Actors.Garbage.Add(this);
            }
        }

        
        /// <summary>
        /// Place this actor in the world.
        /// </summary>
        /// <param name="findSpawnPoint">
        /// If true, the actor's position is changed to a valid, non-colliding point.
        /// </param>
        public virtual void Spawn(bool findSpawnPoint)
        {
            // find a new spawn point if requested
            if (findSpawnPoint)
            {
                position = world.FindSpawnPoint(this);
            }
            // reset the velocity
            velocity = Vector2.Zero;

            // respawn this actor
            if (dead == true)
            {
                // I LIVE
                dead = false;
                // add this object to the world's actors list
                world.Actors.Add(this);
            }
        }
        #endregion
    }
}
