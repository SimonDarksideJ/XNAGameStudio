#region File Description
//-----------------------------------------------------------------------------
// Flock.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Flocking
{
    /// <summary>
    /// This class manages all the birds in the flock and handles 
    /// their update and draw
    /// </summary>
    class Flock
    {
        #region Constants
        //Number of FLock members
        const int flockSize = 40;
        #endregion

        #region Fields

        //birds that fly out of the boundry(screen) will wrap around to 
        //the other side
        int boundryWidth;
        int boundryHeight;

        /// <summary>
        /// Tecture used to draw the Flock
        /// </summary>
        Texture2D birdTexture;

        /// <summary>
        /// List of Flock Members
        /// </summary>
        List<Bird> flock;

        /// <summary>
        /// Parameters flock members use to move and think
        /// </summary>
        public AIParameters FlockParams
        {
            get
            {
                return FlockParams;
            }

            set
            {
                flockParams = value;
            }           
        }
        protected AIParameters flockParams;
        

        #endregion

        #region Initialization

        /// <summary>
        /// Setup the flock boundaries and generate individual members of the flock
        /// </summary>
        /// <param name="tex"> The texture to be used by the birds</param>
        /// <param name="screenWidth">Width of the screen</param>
        /// <param name="screenHeight">Height of the screen</param>
        /// <param name="flockParameters">Behavior of the flock</param>
        public Flock(Texture2D tex, int screenWidth, int screenHeight,
            AIParameters flockParameters)
        {
            boundryWidth = screenWidth;
            boundryHeight = screenHeight;

            birdTexture = tex;

            flock = new List<Bird>();
            flockParams = flockParameters;

            ResetFlock();
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Update each flock member, Each bird want to fly with or flee from everything
        /// it sees depending on what type it is
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="cat"></param>
        public void Update(GameTime gameTime, Cat cat)
        {
            foreach (Bird thisBird in flock)
            {
                thisBird.ResetThink();

                foreach (Bird otherBird in flock)
                {
                    //this check is so we don't try to fly to ourself!
                    if (thisBird != otherBird)
                    {
                        thisBird.ReactTo(otherBird, ref flockParams);
                    }
                }

                //Look for the cat
                thisBird.ReactTo(cat, ref flockParams);

                thisBird.Update(gameTime, ref flockParams);
            }
        }

        /// <summary>
        /// Calls Draw on every member of the Flock
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Bird theBird in flock)
            {
                theBird.Draw(spriteBatch, gameTime);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear the current flock if it exists and randomly generate a new one
        /// </summary>
        public void ResetFlock()
        {
            flock.Clear();
            flock.Capacity = flockSize;

            Bird tempBird;
            Vector2 tempDir;
            Vector2 tempLoc;

            Random random = new Random();

            for (int i = 0; i < flockSize; i++)
            {
                tempLoc = new Vector2((float)
                    random.Next(boundryWidth), (float)random.Next(boundryHeight));
                tempDir = new Vector2((float)
                    random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f);
                tempDir.Normalize();

                tempBird = new Bird(birdTexture, tempDir, tempLoc, 
                    boundryWidth, boundryHeight);
                flock.Add(tempBird);
            }
        }
        #endregion
    }
}
