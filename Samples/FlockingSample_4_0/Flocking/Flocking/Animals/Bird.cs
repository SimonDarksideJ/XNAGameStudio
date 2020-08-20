#region File Description
//-----------------------------------------------------------------------------
// Bird.cs
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
    class Bird : Animal
    {
        #region Fields

        protected Random random;
        Vector2 aiNewDir;
        int aiNumSeen;

        #endregion

        #region Initialization
        /// <summary>
        /// Bird constructor
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="dir">movement direction</param>
        /// <param name="loc">spawn location</param>
        /// <param name="screenSize">screen size</param>
        public Bird(Texture2D tex, Vector2 dir, Vector2 loc, 
            int screenWidth, int screenHeight)
            : base(tex, screenWidth, screenHeight)
        {
            direction = dir;
            direction.Normalize();
            location = loc;
            moveSpeed = 125.0f;
            fleeing = false;
            random = new Random((int)loc.X + (int)loc.Y);
            animaltype = AnimalType.Bird;
            BuildBehaviors();
        }
        #endregion

        #region Update and Draw

        /// <summary>
        /// update bird position, wrapping around the screen edges
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime, ref AIParameters aiParams)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 randomDir = Vector2.Zero;

            randomDir.X = (float)random.NextDouble() - 0.5f;
            randomDir.Y = (float)random.NextDouble() - 0.5f;
            Vector2.Normalize(ref randomDir, out randomDir);

            if (aiNumSeen > 0)
            {
                aiNewDir = (direction * aiParams.MoveInOldDirectionInfluence) +
                    (aiNewDir * (aiParams.MoveInFlockDirectionInfluence / 
                    (float)aiNumSeen));
            }
            else
            {
                aiNewDir = direction * aiParams.MoveInOldDirectionInfluence;
            }

            aiNewDir += (randomDir * aiParams.MoveInRandomDirectionInfluence);
            Vector2.Normalize(ref aiNewDir, out aiNewDir);
            aiNewDir = ChangeDirection(direction, aiNewDir, 
                aiParams.MaxTurnRadians * elapsedTime);
            direction = aiNewDir;

            if (direction.LengthSquared() > .01f)
            {
                Vector2 moveAmount = direction * moveSpeed * elapsedTime;
                location = location + moveAmount;

                //wrap bird to the other side of the screen if needed
                if (location.X < 0.0f)
                {
                    location.X = boundryWidth + location.X;
                }
                else if (location.X > boundryWidth)
                {
                    location.X = location.X - boundryWidth;
                }

                location.Y += direction.Y * moveSpeed * elapsedTime;
                if (location.Y < 0.0f)
                {
                    location.Y = boundryHeight + location.Y;
                }
                else if (location.Y > boundryHeight)
                {
                    location.Y = location.Y - boundryHeight;
                }
            }
        }

        /// <summary>
        /// Draw the bird, tinting it if it's currently fleeing
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Color tintColor = color;
            float rotation = 0.0f;
            rotation = (float)Math.Atan2(direction.Y, direction.X);

            // if the entity is highlighted, we want to make it pulse with a red tint.
            if (fleeing)
            {
                // to do this, we'll first generate a value t, which we'll use to
                // determine how much tint to have.
                float t = (float)Math.Sin(10 * gameTime.TotalGameTime.TotalSeconds);

                // Sin varies from -1 to 1, and we want t to go from 0 to 1, so we'll 
                // scale it now.
                t = .5f + .5f * t;

                // finally, we'll calculate our tint color by using Lerp to generate
                // a color in between Red and White.
                tintColor = new Color(Vector4.Lerp(
                    Color.Red.ToVector4(), Color.White.ToVector4(), t));
            }

            // Draw the animal, centered around its position, and using the 
            //orientation and tint color.
            spriteBatch.Draw(texture, location, null, tintColor,
                rotation, textureCenter, 1.0f, SpriteEffects.None, 0.0f);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Instantiates all the behaviors that this Bird knows about
        /// </summary>
        public void BuildBehaviors()
        {
            Behaviors catReactions = new Behaviors();
            catReactions.Add(new FleeBehavior(this));
            behaviors.Add(AnimalType.Cat, catReactions);

            Behaviors birdReactions = new Behaviors();
            birdReactions.Add(new AlignBehavior(this));
            birdReactions.Add(new CohesionBehavior(this));
            birdReactions.Add(new SeparationBehavior(this));
            behaviors.Add(AnimalType.Bird, birdReactions);
        }
        /// <summary>
        /// Setup the bird to figure out it's new movement direction
        /// </summary>
        /// <param name="AIparams">flock AI parameters</param>
        public void ResetThink()
        {
            Fleeing = false;
            aiNewDir = Vector2.Zero;
            aiNumSeen = 0;
            reactionDistance = 0f;
            reactionLocation = Vector2.Zero;
        }

        /// <summary>
        /// Since we're wrapping movement around the screen, two point at extreme 
        /// sides of the screen are actually very close together, this function 
        /// figures out if destLocation is closer the srcLocation if you wrap around
        /// the screen
        /// </summary>
        /// <param name="srcLocation">screen location of src</param>
        /// <param name="destLocation">screen location of dest</param>
        /// <param name="outVector">relative location of dest to src</param>
        private void ClosestLocation(ref Vector2 srcLocation, 
            ref Vector2 destLocation, out Vector2 outLocation)
        {
            outLocation = new Vector2();
            float x = destLocation.X;
            float y = destLocation.Y;
            float dX = Math.Abs(destLocation.X - srcLocation.X);
            float dY = Math.Abs(destLocation.Y - srcLocation.Y);

            // now see if the distance between birds is closer if going off one
            // side of the map and onto the other.
            if (Math.Abs(boundryWidth - destLocation.X + srcLocation.X) < dX)
            {
                dX = boundryWidth - destLocation.X + srcLocation.X;
                x = destLocation.X - boundryWidth;
            }
            if (Math.Abs(boundryWidth - srcLocation.X + destLocation.X) < dX)
            {
                dX = boundryWidth - srcLocation.X + destLocation.X;
                x = destLocation.X + boundryWidth;
            }

            if (Math.Abs(boundryHeight - destLocation.Y + srcLocation.Y) < dY)
            {
                dY = boundryHeight - destLocation.Y + srcLocation.Y;
                y = destLocation.Y - boundryHeight;
            }
            if (Math.Abs(boundryHeight - srcLocation.Y + destLocation.Y) < dY)
            {
                dY = boundryHeight - srcLocation.Y + destLocation.Y;
                y = destLocation.Y + boundryHeight;
            }
            outLocation.X = x;
            outLocation.Y = y;
        }

        /// <summary>
        /// React to an Animal based on it's type
        /// </summary>
        /// <param name="animal"></param>
        public void ReactTo(Animal animal, ref AIParameters AIparams)
        {
            if (animal != null)
            {
                //setting the the reactionLocation and reactionDistance here is
                //an optimization, many of the possible reactions use the distance
                //and location of theAnimal, so we might as well figure them out
                //only once !
                Vector2 otherLocation = animal.Location;
                ClosestLocation(ref location, ref otherLocation, 
                    out reactionLocation);
                reactionDistance = Vector2.Distance(location, reactionLocation);

                //we only react if theAnimal is close enough that we can see it
                if (reactionDistance < AIparams.DetectionDistance)
                {
                    Behaviors reactions = behaviors[animal.AnimalType];
                    foreach (Behavior reaction in reactions)
                    {
                        reaction.Update(animal, AIparams);
                        if (reaction.Reacted)
                        {
                            aiNewDir += reaction.Reaction;
                            aiNumSeen++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This function clamps turn rates to no more than maxTurnRadians
        /// </summary>
        /// <param name="oldDir">current movement direction</param>
        /// <param name="newDir">desired movement direction</param>
        /// <param name="maxTurnRadians">max turn in radians</param>
        /// <returns></returns>
        private static Vector2 ChangeDirection(
            Vector2 oldDir, Vector2 newDir, float maxTurnRadians)
        {
            float oldAngle = (float)Math.Atan2(oldDir.Y, oldDir.X);
            float desiredAngle = (float)Math.Atan2(newDir.Y, newDir.X);
            float newAngle = MathHelper.Clamp(desiredAngle, WrapAngle(
                    oldAngle - maxTurnRadians), WrapAngle(oldAngle + maxTurnRadians));
            return new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
        }
        /// <summary>
        /// clamps the angle in radians between -Pi and Pi.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }
        #endregion
    }
}