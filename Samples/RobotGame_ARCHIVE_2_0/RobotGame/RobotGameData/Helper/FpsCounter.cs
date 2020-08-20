#region File Description
//-----------------------------------------------------------------------------
// FpsCounter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace RobotGameData.Helper
{
    /// <summary>
    /// Process frame per second
    /// </summary>
    public class FpsCounter
    {
        #region Fields

        protected int fps = 0;
        protected int frameCount = 0;
        protected float totalElapsedTime = 0.0f;

        #endregion

        #region Properties

        public int Fps
        {
            get { return fps; }
        }

        #endregion

        /// <summary>
        /// Initialize members
        /// </summary>
        public void Initialize()
        {
            fps = 0;
            frameCount = 0;
            totalElapsedTime = 0.0f;
        }

        /// <summary>
        /// Updates the frame count
        /// </summary>
        public void Update(GameTime gameTime)
        {
            totalElapsedTime += (float)gameTime.ElapsedRealTime.TotalSeconds;
            frameCount++;

            //  Calculate frame count during one second
            if (totalElapsedTime >= 1.0f)
            {
                fps = frameCount;
                frameCount = 0;
                totalElapsedTime = 0.0f;
            }
        }
    }
}


