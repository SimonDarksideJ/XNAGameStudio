#region File Description
//-----------------------------------------------------------------------------
// GameCamera.cs
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
using Microsoft.Xna.Framework.Content;
using RobotGameData.Helper;
#endregion

namespace RobotGameData.Camera
{
    /// <summary>
    /// It's a tremble camera
    /// </summary>
    public class GameCamera : CameraBase
    {
        #region Fields

        /// <summary>
        /// The Camera's trembling amount
        /// </summary>
        Vector3 trembleOffset = Vector3.Zero;

        float trembleCircle = 0.0f;
        float trembleTime = 0.0f;
        float trembleAccTime = 0.0f;

        #endregion

        #region Properties

        public Vector3 TrembleOffset
        {
            get { return this.trembleOffset; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameCamera()
            : base()
        {
            trembleOffset = Vector3.Zero;
            trembleCircle = 0.0f;
            trembleTime = 0.0f;
            trembleAccTime = 0.0f;
        }

        /// <summary>
        /// Update the tremble time and position
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            //  Calculates trembling time
            if (trembleAccTime < trembleTime)
            {
                trembleAccTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Set to randomize trembling position
                trembleOffset.X = HelperMath.RandomNormal() * HelperMath.RandomNormal2()
                                    * trembleCircle;

                trembleOffset.Y = HelperMath.RandomNormal() * HelperMath.RandomNormal2()
                                    * trembleCircle;

                trembleOffset.Z = 0.0f;
            }
            // Stop trembling time
            else if (trembleAccTime >= trembleTime)
            {
                trembleAccTime = 0.0f;
                trembleTime = 0.0f;
            }

            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// Tremble the camera
        /// </summary>
        /// <param name="rTime">tremble time</param>
        /// <param name="rCircle">tremble amount</param>
        public void SetTremble(float rTime, float rCircle)
        {
            trembleAccTime = 0.0f;
            trembleTime = rTime;
            trembleCircle = rCircle;
        }
    }
}
