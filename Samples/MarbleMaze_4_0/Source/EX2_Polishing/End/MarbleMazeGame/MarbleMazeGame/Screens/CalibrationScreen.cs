#region File Description
//-----------------------------------------------------------------------------
// CalibrationScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Devices.Sensors;
using System.Threading;
#endregion

namespace MarbleMazeGame
{
    class CalibrationScreen : GameScreen
    {
        #region Fields
        Texture2D background;
        SpriteFont font;
        bool isCalibrating;
        GameplayScreen gameplayScreen;
        Thread thread;

        // Calibration data
        Microsoft.Devices.Sensors.Accelerometer accelerometer;
        Vector3 accelerometerState = Vector3.Zero;
        Vector3 accelerometerCalibrationData = Vector3.Zero;
        DateTime startTime;
        long samplesCount = 0;
        #endregion

        #region Initialization
        public CalibrationScreen(GameplayScreen gameplayScreen)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            IsPopup = true;
            this.gameplayScreen = gameplayScreen;
        }
        #endregion

        #region Loading
        /// <summary>
        /// Load the screen resources
        /// </summary>
        public override void LoadContent()
        {
            background = Load<Texture2D>(@"Images\titleScreen");
            font = Load<SpriteFont>(@"Fonts\MenuFont");

            // Start calibrating in an additional thread
            thread = new Thread(new ThreadStart(Calibrate));
            isCalibrating = true;
            startTime = DateTime.Now;
            thread.Start();
        }
        #endregion

        #region Update
        /// <summary>
        /// Screen update logic
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If additional thread is running, skip
            if (!isCalibrating)
            {
                gameplayScreen.AccelerometerCalibrationData = 
                    accelerometerCalibrationData;
                foreach (GameScreen screen in ScreenManager.GetScreens())
                    if (screen.GetType() == typeof(BackgroundScreen))
                    {
                        screen.ExitScreen();
                        break;
                    }

                (ScreenManager.GetScreens()[0] as GameplayScreen).IsActive = true;

                ExitScreen();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        #endregion

        #region Render
        /// <summary>
        /// Render screen 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw Background
            spriteBatch.Draw(background, new Vector2(0, 0),
                 Color.White * TransitionAlpha);

            if (isCalibrating)
            {
                string text = "Calibrating...";
                Vector2 size = font.MeasureString(text);
                Vector2 position = new Vector2(
                    (ScreenManager.GraphicsDevice.Viewport.Width - size.X) / 2,
                    (ScreenManager.GraphicsDevice.Viewport.Height - size.Y) / 2);
                spriteBatch.DrawString(font, text, position, Color.White);
            }

            spriteBatch.End();
        }
        #endregion

        #region Private function
        private void Calibrate()
        {
            //Initialize the accelerometer
            accelerometer = new Microsoft.Devices.Sensors.Accelerometer();

            if (accelerometer.State == SensorState.Initializing || 
                accelerometer.State == SensorState.Ready)
            {
                accelerometer.ReadingChanged += (s, e) =>
                {
                    accelerometerState = 
                        new Vector3((float)e.X, (float)e.Y, (float)e.Z);

                    samplesCount++;
                    accelerometerCalibrationData += accelerometerState;

                    if (DateTime.Now >= startTime.AddSeconds(5))
                    {
                        accelerometer.Stop();

                        accelerometerCalibrationData.X /= samplesCount;
                        accelerometerCalibrationData.Y /= samplesCount;
                        accelerometerCalibrationData.Z /= samplesCount;

                        isCalibrating = false;
                    }
                };
            }
            accelerometer.Start();
        }
        #endregion
    }
}
