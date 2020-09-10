#region File Description
//-----------------------------------------------------------------------------
// World.cs
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

namespace VectorRumble
{
    /// <summary>
    /// Owns all game state and executes all game-wide logic.
    /// </summary>
    class World
    {
        #region Constants
        /// <summary>
        /// The number of seconds before the first power-up appears in a game.
        /// </summary>
        const float initialPowerUpDelay = 10f;

        /// <summary>
        /// The time between each power-up spawn.
        /// </summary>
        const float powerUpDelay = 20f;

        /// <summary>
        /// The number of stars to generate in the starfield.
        /// </summary>
        const int starCount = 2048;

        /// <summary>
        /// How far starfield should generate outside the dimensions of the game field.
        /// </summary>
        const int starfieldBuffer = 512;
        #endregion

        #region Fields
        Random random = new Random();

        /// <summary>
        /// The dimensions of the game board.
        /// </summary>
        Vector2 dimensions;

        /// <summary>
        /// The safe dimensions of the game board.
        /// </summary>
        Rectangle safeDimensions;

        /// <summary>
        /// The timer to see if another power-up can arrive.
        /// </summary>
        float powerUpTimer;

        /// <summary>
        /// The audio manager that all objects in the world will use.
        /// </summary>
        private AudioManager audioManager;

        /// <summary>
        /// All ships that might enter the game.
        /// </summary>
        Ship[] ships;

        /// <summary>
        /// The walls in the game.
        /// </summary>
        Vector2[] walls;

        /// <summary>
        /// The starfield effect behind the game-board.
        /// </summary>
        Starfield starfield;

        /// <summary>
        /// All actors in the game.
        /// </summary>
        CollectCollection<Actor> actors;

        /// <summary>
        /// All particle-systems in the game.
        /// </summary>
        CollectCollection<ParticleSystem> particleSystems;

        /// <summary>
        /// Cached list of collision results, for more optimal collision detection.
        /// </summary>
        List<CollisionResult> collisionResults = new List<CollisionResult>();
        #endregion

        #region Properties
        public AudioManager AudioManager
        {
            get { return audioManager; }
            set { audioManager = value; }
        }

        public Starfield Starfield
        {
            get { return starfield; }
        }

        public Ship[] Ships
        {
            get { return ships; }
        }

        public CollectCollection<Actor> Actors
        {
            get { return actors; }
        }

        public CollectCollection<ParticleSystem> ParticleSystems
        {
            get { return particleSystems; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Construct a new World object, holding the game simulation.
        /// </summary>
        public World(Vector2 dimensions)
        {
            this.dimensions = dimensions;
            safeDimensions = new Rectangle(
                (int)(dimensions.X * 0.05f), (int)(dimensions.Y * 0.05f), 
                (int)(dimensions.X * 0.90f), (int)(dimensions.Y * 0.90f));
            // create the players
            ships = new Ship[4];
            ships[0] = new Ship(this, PlayerIndex.One);
            ships[1] = new Ship(this, PlayerIndex.Two);
            ships[2] = new Ship(this, PlayerIndex.Three);
            ships[3] = new Ship(this, PlayerIndex.Four);
            // create the starfield
            starfield = new Starfield(starCount, new Rectangle(
                starfieldBuffer * -1,
                starfieldBuffer  * -1,
                (int)this.dimensions.X + starfieldBuffer * 2,
                (int)this.dimensions.Y + starfieldBuffer * 2));
            // create a new list of actors
            actors = new CollectCollection<Actor>(this);
            // create a new list of particle systems
            particleSystems = new CollectCollection<ParticleSystem>(this);
        }
        #endregion

        #region New Game
        public void StartNewGame()
        {
            // create the walls
            CreateWalls();

            // clear out the actors list
            actors.Clear();
            // add the world actor
            WorldActor worldActor = new WorldActor(this);
            actors.Add(worldActor);
            // add the players to the actor list - they won't be removed
            for (int i = 0; i < ships.Length; i++)
            {
                actors.Add(ships[i]);
            }

            // spawn asteroids
            switch (WorldRules.AsteroidDensity)
            {
                case AsteroidDensity.None:
                    break;
                case AsteroidDensity.Low:
                    SpawnAsteroids(4, 3, 0);
                    break;
                case AsteroidDensity.Medium:
                    SpawnAsteroids(6, 4, 1);
                    break;
                case AsteroidDensity.High:
                    SpawnAsteroids(8, 6, 2);
                    break;
            }

            // set up the power-up timer for the initial delay
            powerUpTimer = initialPowerUpDelay;

            // set up the starfield
            starfield.SetTargetPosition(dimensions * 0.5f);
        }


        /// <summary>
        /// Initialize the walls based on the current world rules.
        /// </summary>
        private void CreateWalls()
        {
            switch (WorldRules.WallStyle)
            {
                case WallStyle.None:
                    walls = new Vector2[8];
                    break;
                case WallStyle.One:
                    walls = new Vector2[10];
                    break;
                case WallStyle.Two:
                    walls = new Vector2[12];
                    break;
                case WallStyle.Three:
                    walls = new Vector2[14];
                    break;
            }

            // The outer boundaries
            walls[0] = new Vector2(safeDimensions.X, safeDimensions.Y);
            walls[1] = new Vector2(safeDimensions.X, 
                safeDimensions.Y + safeDimensions.Height);
            walls[2] = new Vector2(safeDimensions.X + safeDimensions.Width,
                safeDimensions.Y);
            walls[3] = new Vector2(safeDimensions.X + safeDimensions.Width,
                safeDimensions.Y + safeDimensions.Height);
            walls[4] = new Vector2(safeDimensions.X, safeDimensions.Y);
            walls[5] = new Vector2(safeDimensions.X + safeDimensions.Width,
                safeDimensions.Y);
            walls[6] = new Vector2(safeDimensions.X,
                safeDimensions.Y + safeDimensions.Height);
            walls[7] = new Vector2(safeDimensions.X + safeDimensions.Width,
                safeDimensions.Y + safeDimensions.Height);

            int quarterX = safeDimensions.Width / 4;
            int quarterY = safeDimensions.Height / 4;
            int halfY = safeDimensions.Height / 2;

            switch (WorldRules.WallStyle)
            {
                case WallStyle.One:
                    // Cross line
                    walls[8] = new Vector2(safeDimensions.X + quarterX, 
                        safeDimensions.Y + halfY);
                    walls[9] = new Vector2(safeDimensions.X + 3 * quarterX,
                        safeDimensions.Y + halfY);
                    break;
                case WallStyle.Two:
                    walls[8] = new Vector2(safeDimensions.X + quarterX, 
                        safeDimensions.Y + quarterY);
                    walls[9] = new Vector2(safeDimensions.X + quarterX, 
                        safeDimensions.Y + 3 * quarterY);
                    walls[10] = new Vector2(safeDimensions.X + 3 * quarterX, 
                        safeDimensions.Y + quarterY);
                    walls[11] = new Vector2(safeDimensions.X + 3 * quarterX, 
                        safeDimensions.Y + 3 * quarterY);
                    break;
                case WallStyle.Three:
                    // Cross line
                    walls[8] = new Vector2(safeDimensions.X + quarterX,
                        safeDimensions.Y + halfY);
                    walls[9] = new Vector2(safeDimensions.X + 3 * quarterX,
                        safeDimensions.Y + halfY);
                    walls[10] = new Vector2(safeDimensions.X + quarterX, 
                        safeDimensions.Y + quarterY);
                    walls[11] = new Vector2(safeDimensions.X + quarterX, 
                        safeDimensions.Y + 3 * quarterY);
                    walls[12] = new Vector2(safeDimensions.X + 3 * quarterX, 
                        safeDimensions.Y + quarterY);
                    walls[13] = new Vector2(safeDimensions.X + 3 * quarterX, 
                        safeDimensions.Y + 3 * quarterY);
                    break;
            }
        }


        /// <summary>
        /// Create many asteroids and add them to the game world.
        /// </summary>
        /// <param name="smallCount">The number of "small" asteroids to create.</param>
        /// <param name="mediumCount">The number of "medium" asteroids to create.
        /// </param>
        /// <param name="largeCount">The number of "large" asteroids to create.</param>
        private void SpawnAsteroids(int smallCount, int mediumCount, int largeCount)
        {
            // create small asteroids
            for (int i = 0; i < smallCount; ++i)
            {
                float radius = 8.0f + 16.0f * (float)random.NextDouble();

                Asteroid asteroid = new Asteroid(this, radius);
                asteroid.Spawn(true);
            }
            // create medium-sized asteroids
            for (int i = 0; i < mediumCount; ++i)
            {
                float radius = 24.0f + 16.0f * (float)random.NextDouble();

                Asteroid asteroid = new Asteroid(this, radius);
                asteroid.Spawn(true);
            }
            // create large asteroids
            for (int i = 0; i < largeCount; ++i)
            {
                float radius = 32.0f + 64.0f * (float)random.NextDouble();

                Asteroid asteroid = new Asteroid(this, radius);
                asteroid.Spawn(true);
            }
        }
        
        
        /// <summary>
        /// Create a new power-up in the world, if possible
        /// </summary>
        void SpawnPowerUp()
        {
            // check if there is a powerup in the world
            for (int i = 0; i < actors.Count; ++i)
            {
                if (actors[i] is PowerUp)
                {
                    return;
                }
            }
            // create the new power-up
            PowerUp powerup = null;
            switch (random.Next(3))
            {
                case 0:
                    powerup = new DoubleLaserPowerUp(this);
                    break;
                case 1:
                    powerup = new TripleLaserPowerUp(this);
                    break;
                case 2:
                    powerup = new RocketPowerUp(this);
                    break;
            }
            // add the new power-up to the world
            powerup.Spawn(true);
        }        
        #endregion

        #region Update and Draw
        /// <summary>
        /// Update the world simulation.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public void Update(float elapsedTime)
        {
            // update all actors
            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].Update(elapsedTime);
            }

            // update collision
            MoveWorld(elapsedTime);

            // update particle systems
            for (int i = 0; i < particleSystems.Count; i++)
            {
                particleSystems[i].Update(elapsedTime);
                if (particleSystems[i].IsActive == false)
                {
                    particleSystems.Garbage.Add(particleSystems[i]);
                }
            }

            // update the starfield
            Vector2 starfieldTarget = Vector2.Zero;
            int playingPlayers = 0;
            for (int i = 0; i < ships.Length; i++)
            {
                if (ships[i].Playing)
                {
                    starfieldTarget += ships[i].Position;
                    playingPlayers++;
                }
            }
            if (playingPlayers > 0)
            {
                starfield.SetTargetPosition(starfieldTarget / playingPlayers);
            }
            starfield.Update(elapsedTime);

            // check if we can create a new power-up yet
            if (powerUpTimer > 0f)
            {
                powerUpTimer = Math.Max(powerUpTimer - elapsedTime, 0f);
            }
            if (powerUpTimer <= 0.0f)
            {
                SpawnPowerUp();
                powerUpTimer = powerUpDelay;
            }

            // clean up the lists
            actors.Collect();
            particleSystems.Collect();
        }


        /// <summary>
        /// Draw the walls.
        /// </summary>
        /// <param name="lineBatch">The LineBatch to render to.</param>
        public void DrawWalls(LineBatch lineBatch)
        {
            if (lineBatch == null)
            {
                throw new ArgumentNullException("lineBatch");
            }
            // draw each wall-line
            for (int wall = 0; wall < walls.Length / 2; wall++)
            {
                lineBatch.DrawLine(walls[wall * 2], walls[wall * 2 + 1], Color.Yellow);
            }
        }
        #endregion

        #region Collision
        /// <summary>
        /// Move all of the actors in the world.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        private void MoveWorld(float elapsedTime)
        {
            Vector2 point = Vector2.Zero;
            // move each actor
            for (int i = 0; i < actors.Count; ++i)
            {
                if (actors[i].Dead)
                {
                    continue;
                }
                // determine how far they are going to move
                Vector2 movement = actors[i].Velocity * elapsedTime;
                // only allow actors that have not collided yet this frame to collide
                // -- otherwise, objects can "double-hit" and trade their momentum fast
                if (actors[i].CollidedThisFrame == false)
                {
                    movement = MoveAndCollide(actors[i], movement);
                }
                // determine the new position
                actors[i].Position += movement;
                // determine if their new position taks them through a wall
                for (int w = 0; w < walls.Length / 2; ++w)
                {
                    if (actors[i] is Projectile)
                    {
                        if (Collision.LineLineIntersect(actors[i].Position, 
                            actors[i].Position - movement, walls[w * 2], 
                            walls[w * 2 + 1], out point))
                        {
                            actors[i].Touch(actors[0]);
                        }
                    }
                    else
                    {
                        Collision.CircleLineCollisionResult result = 
                            new Collision.CircleLineCollisionResult();
                        if (Collision.CircleLineCollide(actors[i].Position, 
                            actors[i].Radius, walls[w * 2], walls[w * 2 + 1], 
                            ref result))
                        {
                            // if a non-projectile hits a wall, bounce slightly
                            float vn = Vector2.Dot(actors[i].Velocity, result.Normal);
                            actors[i].Velocity -= (2.0f * vn) * result.Normal;
                            actors[i].Position += result.Normal * result.Distance;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Move the given actor by the given movement, colliding and adjusting
        /// as necessary.
        /// </summary>
        /// <param name="actor">The actor who is moving.</param>
        /// <param name="movement">The desired movement vector for this update.</param>
        /// <returns>The movement vector after considering all collisions.</returns>
        private Vector2 MoveAndCollide(Actor actor, Vector2 movement)
        {
            if (actor == null)
            {
                throw new ArgumentNullException("actor");
            }
            // make sure we care about where this actor goes
            if (actor.Dead || (actor.Collidable == false))
            {
                return movement;
            }
            // make sure the movement is significant
            if (movement.LengthSquared() <= 0f)
            {
                return movement;
            }

            // generate the list of collisions
            Collide(actor, movement);

            // determine if we had any collisions
            if (collisionResults.Count > 0)
            {
                collisionResults.Sort(CollisionResult.Compare);
                foreach (CollisionResult collision in collisionResults)
                {
                    // let the two actors touch each other, and see what happens
                    if (actor.Touch(collision.Actor) && collision.Actor.Touch(actor))
                    {
                        actor.CollidedThisFrame = 
                            collision.Actor.CollidedThisFrame = true;
                        // they should react to the other, even if they just died
                        AdjustVelocities(actor, collision.Actor);
                        return Vector2.Zero;
                    }
                }
            }

            return movement;
        }


        /// <summary>
        /// Determine all collisions that will happen as the given actor moves.
        /// </summary>
        /// <param name="actor">The actor that is moving.</param>
        /// <param name="movement">The actor's movement vector this update.</param>
        /// <remarks>The results are stored in the cached list.</remarks>
        public void Collide(Actor actor, Vector2 movement)
        {
            collisionResults.Clear();

            if (actor == null)
            {
                throw new ArgumentNullException("actor");
            }
            if (actor.Dead || (actor.Collidable == false))
            {
                return;
            }

            // determine the movement direction and scalar
            float movementLength = movement.Length();
            if (movementLength <= 0f)
            {
                return;
            }

            // check each actor
            foreach (Actor checkActor in actors)
            {
                if ((actor == checkActor) || checkActor.Dead || !checkActor.Collidable)
                {
                    continue;
                }

                // calculate the target vector
                Vector2 checkVector = checkActor.Position - actor.Position;
                float distanceBetween = checkVector.Length() - 
                    (checkActor.Radius + actor.Radius);

                // check if they could possibly touch no matter the direction
                if (movementLength < distanceBetween)
                {
                    continue;
                }

                // determine how much of the movement is bringing the two together
                float movementTowards = Vector2.Dot(movement, checkVector);

                // check to see if the movement is away from each other
                if (movementTowards < 0f)
                {
                    continue;
                }

                if (movementTowards < distanceBetween)
                {
                    continue;
                }

                CollisionResult result = new CollisionResult();
                result.Distance = distanceBetween;
                result.Normal = Vector2.Normalize(checkVector);
                result.Actor = checkActor;

                collisionResults.Add(result);
            }
        }


        /// <summary>
        /// Adjust the velocities of the two actors as if they have collided,
        /// distributing their velocities according to their masses.
        /// </summary>
        /// <param name="actor1">The first actor.</param>
        /// <param name="actor2">The second actor.</param>
        private static void AdjustVelocities(Actor actor1, Actor actor2)
        {
            // don't adjust velocities if at least one has negative mass
            if ((actor1.Mass <= 0f) || (actor2.Mass <= 0f))
            {
                return;
            }

            // determine the vectors normal and tangent to the collision
            Vector2 collisionNormal = Vector2.Normalize(
                actor2.Position - actor1.Position);
            Vector2 collisionTangent = new Vector2(
                -collisionNormal.Y, collisionNormal.X);

            // determine the velocity components along the normal and tangent vectors
            float velocityNormal1 = Vector2.Dot(actor1.Velocity, collisionNormal);
            float velocityTangent1 = Vector2.Dot(actor1.Velocity, collisionTangent);
            float velocityNormal2 = Vector2.Dot(actor2.Velocity, collisionNormal);
            float velocityTangent2 = Vector2.Dot(actor2.Velocity, collisionTangent);

            // determine the new velocities along the normal
            float velocityNormal1New = ((velocityNormal1 * (actor1.Mass - actor2.Mass))
                + (2f * actor2.Mass * velocityNormal2)) / (actor1.Mass + actor2.Mass);
            float velocityNormal2New = ((velocityNormal2 * (actor2.Mass - actor1.Mass))
                + (2f * actor1.Mass * velocityNormal1)) / (actor1.Mass + actor2.Mass);

            // determine the new total velocities
            actor1.Velocity = (velocityNormal1New * collisionNormal) + 
                (velocityTangent1 * collisionTangent);
            actor2.Velocity = (velocityNormal2New * collisionNormal) + 
                (velocityTangent2 * collisionTangent);
        }
        
        
        /// <summary>
        /// Find a valid point for the actor to spawn.
        /// </summary>
        /// <param name="actor">The actor to find a location for.</param>
        /// <remarks>This query is not bounded, which would be needed in a more complex
        /// game with a likelihood of no valid spawn locations.</remarks>
        /// <returns>A valid location for the user to spawn.</returns>
        public Vector2 FindSpawnPoint(Actor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException("actor");
            }

            Vector2 spawnPoint;
            float radius = actor.Radius;

            // fudge the radius slightly so we're not right on top of another actor
            if (actor is Ship)
            {
                radius *= 2f;
            }
            else
            {
                radius *= 1.1f;
            }
            radius = (float)Math.Ceiling(radius);

            Vector2 spawnMinimum = new Vector2(
                safeDimensions.X + radius, 
                safeDimensions.Y + radius);
            Vector2 spawnDimensions = new Vector2(
                (float)Math.Floor(safeDimensions.Width - 2f * radius),
                (float)Math.Floor(safeDimensions.Height - 2f * radius));
            Vector2 spawnMaximum = spawnMinimum + spawnDimensions;

            Collision.CircleLineCollisionResult result = 
                new Collision.CircleLineCollisionResult();
            bool valid = true;
            while (true)
            {
                valid = true;
                // generate a new spawn point
                spawnPoint = new Vector2(
                    spawnMinimum.X + spawnDimensions.X * (float)random.NextDouble(),
                    spawnMinimum.Y + spawnDimensions.Y * (float)random.NextDouble());
                if ((spawnPoint.X < spawnMinimum.X) ||
                    (spawnPoint.Y < spawnMinimum.Y) ||
                    (spawnPoint.X > spawnMaximum.X) ||
                    (spawnPoint.Y > spawnMaximum.Y))
                {
                    continue;
                }
                // if we don't collide, then one is good enough
                if (actor.Collidable == false)
                {
                    break; 
                }
                // check against the walls
                if (valid == true)
                {
                    for (int wall = 0; wall < walls.Length / 2; wall++)
                    {
                        if (Collision.CircleLineCollide(spawnPoint, radius, 
                            walls[wall * 2], walls[wall * 2 + 1], ref result))
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                // check against all other actors
                if (valid == true)
                {
                    foreach (Actor checkActor in actors)
                    {
                        if ((actor == checkActor) || checkActor.Dead)
                        {
                            continue;
                        }
                        if (Collision.CircleCircleIntersect(checkActor.Position,
                            checkActor.Radius, spawnPoint, radius))
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                // if we have gotten this far, then the spawn point is good
                if (valid == true)
                {
                    break;
                }
            }

            return spawnPoint;
        }
        #endregion
    }
}
