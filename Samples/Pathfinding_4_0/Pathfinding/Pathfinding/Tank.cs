#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
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
using Microsoft.Xna.Framework.Content;
#endregion

namespace Pathfinding
{
    /// <summary>
    /// A simple object that moves towards it's set destination
    /// </summary>
    public class Tank
    {
        #region Constants
        //The "close enough" limit, if the tank is inside this distance to it's
        //destination it's considered at it's destination
        const float atDestinationLimit = 5f;

        #endregion

        #region Fields

        private Texture2D tankTexture;
        private Vector2 tankTextureCenter;
        private Map map;

        #endregion

        #region Properties

        private float scale = 1f;
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                waypoints.Scale = value;
            }
        }

        /// <summary>
        /// Length 1 vector that represents the tanks’ movement and facing direction
        /// </summary>
        protected Vector2 direction;
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        protected bool moving;
        public bool Moving
        {
            get { return moving; }
            set { moving = value; }
        }

        /// <summary>
        /// The tanks' movement speed
        /// </summary>
        const float moveSpeed = 100f;
        public static float MoveSpeed
        {
            get { return moveSpeed; }
        }

        //the location of the tanks' current waypoint
        private Vector2 destination;
        public Vector2 Destination
        {
            get { return destination; }
        }

        //the tanks' location on the map
        private Vector2 location;
        public Vector2 Location
        {
            get { return location; }
        }

        /// <summary>
        /// The list of points the tanks will move to in order from first to last
        /// </summary>
        private WaypointList waypoints;
        public WaypointList Waypoints
        {
            get { return waypoints; }
        }

        /// <summary>
        /// Linear distance to the Tanks' current destination
        /// </summary>
        public float DistanceToDestination
        {
            get { return Vector2.Distance(location, destination); }
        }

        /// <summary>
        /// True when the tank is "close enough" to it's destination
        /// </summary>
        public bool AtDestination
        {
            get { return DistanceToDestination < atDestinationLimit; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Tank constructor
        /// </summary>
        public Tank()
        {
            waypoints = new WaypointList();
        }

        public void Initialize(Map mazeMap)
        {
            location = Vector2.Zero;
            destination = location;
            map = mazeMap;
        }

        /// <summary>
        /// Load the tanks' texture resources
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            tankTexture = content.Load<Texture2D>("tank");

            tankTextureCenter =
                new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);

            waypoints.LoadContent(content);

        }
        #endregion

        #region Update and Draw

        /// <summary>
        /// Draw the Tank
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            waypoints.Draw(spriteBatch);

            float facingDirection = (float)Math.Atan2(
                Direction.Y, Direction.X);

            spriteBatch.Begin();
            spriteBatch.Draw(tankTexture, location, null, Color.White, facingDirection,
                tankTextureCenter, scale, SpriteEffects.None, 0f);
            spriteBatch.End();

        }

        /// <summary>
        /// Update the Tanks' position if it's not "close enough" to 
        /// it's destination
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (moving)
            {
                // If we have any waypoints, the first one on the list is where 
                // we want to go
                if (waypoints.Count >= 1)
                {
                    destination = waypoints.Peek();
                }

                // If we’re at the destination and there is at least one waypoint in 
                // the list, get rid of the first one since we’re there now
                if (AtDestination && waypoints.Count >= 1)
                {
                    waypoints.Dequeue();
                }

                if (!AtDestination)
                {
                    direction = -(location - destination);
                    //This scales the vector to 1, we'll use move Speed and elapsed Time 
                    //to find the how far the tank moves
                    direction.Normalize();
                    location = location + (Direction *
                        MoveSpeed * elapsedTime);
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Set the Tanks' location on the map
        /// </summary>
        /// <param name="newLocation">new location on the map</param>
        public void SetLocation(Vector2 newLocation)
        {
            location = newLocation;
            //we set the destination to the location right here so the tank
            //doesn't move from where we just put it until we give it a different
            //destination
            destination = newLocation;
            direction = Vector2.Zero;
        }


        /// <summary>
        /// Set the Tank to move toward a new destination
        /// </summary>
        public void SetDestination(Vector2 newDestination)
        {
            destination = newDestination;
        }

        public void Reset()
        {
            waypoints.Clear();
            direction = Vector2.Zero;
            moving = false;
            Scale = map.Scale;
            location = map.MapToWorld(map.StartTile, true);
            destination = location;
        }

        #endregion
    }
}
