#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameScreens;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Landscapes;
using RacingGame.Properties;
using RacingGame.Sounds;
using RacingGame.Tracks;
using Texture = RacingGame.Graphics.Texture;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.GameLogic
{
    /// <summary>
    /// Player helper class, holds all the current game properties:
    /// Fuel, Health, Speed, Lifes and Score.
    /// Note: This class is just used in RacingGame and we only have
    /// 1 instance of it for the current player for the current game.
    /// If we want to have more than 1 player (e.g. in multiplayer mode)
    /// you should add a multiplayer class and have all player instances there.
    /// </summary>
    public class Player : ChaseCamera
    {
        #region Variables
        /// <summary>
        /// Remember all lap times for the victory screen.
        /// </summary>
        private List<float> lapTimes = new List<float>();

        /// <summary>
        /// The number of laps in each race
        /// </summary>
        private const int LapCount = 3;

        /// <summary>
        /// Add lap time
        /// </summary>
        /// <param name="setLapTime">Lap time</param>
        public void AddLapTime(float setLapTime)
        {
            lapTimes.Add(setLapTime);
        }

        /// <summary>
        /// The amount of time (in milliseconds) the car has
        /// been in the air since last touching the ground
        /// If the car is in the air and does not reach the
        /// ground again for too long, its game over!
        /// </summary>
        private float inAirTimeMilliseconds = 0.0f;

        /// <summary>
        /// The amount of time (in milliseconds) the car must be
        /// in the air before game over occurs
        /// </summary>
        private const float InAirTimeoutMilliseconds = 3000.0f;
        #endregion

        #region Constructor
        /// <summary>
        /// Create chase camera
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        /// <param name="setCameraPos">Set camera pos</param>
        public Player(Vector3 setCarPosition)
            : base(setCarPosition)
        {
        }
        #endregion

        #region Reset
        /// <summary>
        /// Reset player values.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            lapTimes.Clear();
        }
        #endregion

        #region Handle game logic
        /// <summary>
        /// Update game logic, called every frame.
        /// </summary>
        public override void Update()
        {
            // Don't handle any more game logic if game is over.
            if (RacingGameManager.InGame &&
                ZoomInTime <= 0)
            {
                // Game over? Then show end screen!
                if (isGameOver)
                {
                    // Just rotate around, don't use camera class!
                    cameraPos = CarPosition + new Vector3(0, -5, +20) +
                        Vector3.TransformNormal(new Vector3(30, 0, 0),
                        Matrix.CreateRotationZ(BaseGame.TotalTimeMilliseconds / 2593.0f));
                    BaseGame.ViewMatrix = Matrix.CreateLookAt(
                        cameraPos, CarPosition, CarUpVector);
                    int rank = Highscores.GetRankFromCurrentTime(
                        this.levelNum, (int)this.BestTimeMilliseconds);
                    this.currentGameTimeMilliseconds = this.BestTimeMilliseconds;

                    if (victory)
                    {
                        // Display Victory message
                        TextureFont.WriteTextCentered(
                            BaseGame.Width / 2, BaseGame.Height / 7,
                            "Victory! You won.",
                            Color.LightGreen, 1.25f);
                    }
                    else
                    {
                        // Display game over message
                        TextureFont.WriteTextCentered(
                            BaseGame.Width / 2, BaseGame.Height / 7,
                            "Game Over! You lost.",
                            Color.Red, 1.25f);
                    }

                    for (int num = 0; num < lapTimes.Count; num++)
                        TextureFont.WriteTextCentered(
                            BaseGame.Width / 2,
                            BaseGame.Height / 7 + BaseGame.YToRes(35) * (1 + num),
                            "Lap " + (num + 1) + " Time: " +
                            (((int)lapTimes[num]) / 60).ToString("00") + ":" +
                            (((int)lapTimes[num]) % 60).ToString("00") + "." +
                            (((int)(lapTimes[num] * 100)) % 100).ToString("00"),
                            Color.White, 1.25f);
                    TextureFont.WriteTextCentered(
                        BaseGame.Width / 2,
                        BaseGame.Height / 7 + BaseGame.YToRes(35) * (1 + lapTimes.Count),
                        "Rank: " + (1 + rank),
                        Color.White, 1.25f);

                    // Don't continue processing game logic
                    return;
                }

                // Check if car is in the air,
                // used to check if the player died.
                if (this.isCarOnGround == false)
                    inAirTimeMilliseconds +=
                        BaseGame.ElapsedTimeThisFrameInMilliseconds;
                else
                    // Back on ground, reset
                    inAirTimeMilliseconds = 0;

                // Game not over yet, check if we lost or won.
                // Check if we have fallen from the track
                float trackDistance = Vector3.Distance(CarPosition, groundPlanePos);
                if (trackDistance > 20 ||
                    inAirTimeMilliseconds > InAirTimeoutMilliseconds)
                {
                    // Reset player variables (stop car, etc.)
                    ClearVariablesForGameOver();

                    // And indicate that game is over and we lost!
                    isGameOver = true;
                    victory = false;
                    Sound.Play(Sound.Sounds.CarLose);

                    // Also stop engine sound
                    Sound.StopGearSound();
                }

                // Finished all laps? Then we won!
                if (CurrentLap >= LapCount)
                {
                    // Reset player variables (stop car, etc.)
                    ClearVariablesForGameOver();

                    // When you win, you start an extra lap we don't want to show
                    this.lap--;

                    // Then game is over and we won!
                    isGameOver = true;
                    victory = true;
                    Sound.Play(Sound.Sounds.Victory);

                    // Also stop engine sound
                    Sound.StopGearSound();
                }
            }

            base.Update();
        }
        #endregion
    }
}
