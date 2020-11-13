#region File Description
//-----------------------------------------------------------------------------
// BasePlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
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
#endregion

namespace RacingGame.GameLogic
{
    /// <summary>
    /// Base player helper class, holds all the current game values:
    /// Game time, game over, level number, victory.
    /// More stuff that has impact to the CarController or ChaseCamera classes
    /// should be included here, else add them directly to the Player class,
    /// which handles all the game logic.
    /// For example adding items or powerups should be done in this class
    /// if they affect the speed or physics of our car.
    /// For multiplayer purposes this class should be extended to assign
    /// a player id to each player and link the network stuff up here.
    /// </summary>
    public class BasePlayer
    {
        #region Global game parameters (game time, game over, etc.)
        /// <summary>
        /// Current game time in ms. Used for time display in game. Also used to
        /// update the sun position and for the highscores.
        /// Will be stopped if we die or if we are still zooming in.
        /// </summary>
        protected float currentGameTimeMilliseconds = 0;

        /// <summary>
        /// Current lap. Increases and when we reach 3, the game is won.
        /// </summary>
        protected int lap;

        /// <summary>
        /// Current lap
        /// </summary>
        public int CurrentLap
        {
            get
            {
                return lap;
            }
        }

        /// <summary>
        /// Remember best lap time, unused until we complete the first lap.
        /// Then it is set every lap, always using the best and fastest lap time.
        /// </summary>
        private float bestLapTimeMilliseconds = 0;

        /// <summary>
        /// Best lap time we have archived in this game
        /// </summary>
        public float BestTimeMilliseconds
        {
            get
            {
                return bestLapTimeMilliseconds;
            }
        }

        /// <summary>
        /// Start new lap, will reset all lap variables and the game time.
        /// If all laps are done the game is over.
        /// </summary>
        protected void StartNewLap()
        {
            lap++;

            RacingGameManager.Landscape.StartNewLap();

            // Got new best time?
            if (bestLapTimeMilliseconds == 0 ||
                currentGameTimeMilliseconds < bestLapTimeMilliseconds)
                bestLapTimeMilliseconds = currentGameTimeMilliseconds;

            // Start at 0:00.00 again
            currentGameTimeMilliseconds = zoomInTime;
        }

        /// <summary>
        /// Game time ms, will return negative values if currently zooming in!
        /// </summary>
        /// <returns>Int</returns>
        public float GameTimeMilliseconds
        {
            get
            {
                return currentGameTimeMilliseconds - zoomInTime;
            }
        }

        /// <summary>
        /// How long do we zoom in.
        /// </summary>
        public const int StartGameZoomTimeMilliseconds = 5000;

        /// <summary>
        /// Zoom in time
        /// </summary>
        private float zoomInTime = StartGameZoomTimeMilliseconds;

        /// <summary>
        /// Zoom in time
        /// </summary>
        /// <returns>Float</returns>
        protected float ZoomInTime
        {
            get
            {
                return zoomInTime;
            }
            set
            {
                zoomInTime = value;
            }
        }

        /// <summary>
        /// The amount of time to remain fully zoomed in waiting for start light;
        /// </summary>
        public const int StartGameZoomedInTime = 3000;

        /// <summary>
        /// Won or lost?
        /// </summary>
        protected bool victory;

        /// <summary>
        /// Property for Victory
        /// </summary>
        public bool Victory
        {
            get
            {
                return victory;
            }
        }

        /// <summary>
        /// Level num, set when starting game!
        /// </summary>
        protected int levelNum;

        public int LevelNum
        {
            get
            {
                return levelNum;
            }
        }

        /// <summary>
        /// Game over?
        /// </summary>
        protected bool isGameOver;

        /// <summary>
        /// Is game over?
        /// </summary>
        /// <returns>Bool</returns>
        public bool GameOver
        {
            get
            {
                return isGameOver;
            }
        }

        /// <summary>
        /// Did the player win the game? Makes only sense if GameOver is true!
        /// </summary>
        public bool WonGame
        {
            get
            {
                return victory;
            }
        }

        /// <summary>
        /// Remember if we already uploaded our highscore for this game.
        /// Don't do this twice (e.g. when pressing esc).
        /// </summary>
        private bool alreadyUploadedHighscore = false;

        /// <summary>
        /// Set game over and upload highscore
        /// </summary>
        public void SetGameOverAndUploadHighscore()
        {
            // Set gameOver to true to mark this game as ended.
            isGameOver = true;

            // Upload highscore
            if (alreadyUploadedHighscore == false)
            {
                alreadyUploadedHighscore = true;
                Highscores.SubmitHighscore(levelNum,
                    (int)currentGameTimeMilliseconds);
            }
        }

        /// <summary>
        /// Helper to determinate if user can control the car.
        /// If game just started we still zoom into the chase camera.
        /// </summary>
        /// <returns>Bool</returns>
        public bool CanControlCar
        {
            get
            {
                return zoomInTime <= 0 &&
                    GameOver == false;
            }
        }

        private bool firstFrame = true;
        #endregion

        #region Reset everything for starting a new game
        /// <summary>
        /// Reset all player entries for restarting a game.
        /// In derived classes reset all the variables we need to reset for
        /// a new game there (e.g. car speed in CarController or
        /// cameraWobbel in ChaseCamera).
        /// </summary>
        public virtual void Reset()
        {
            levelNum = TrackSelection.SelectedTrackNumber;
            isGameOver = false;
            alreadyUploadedHighscore = false;
            currentGameTimeMilliseconds = 0;
            bestLapTimeMilliseconds = 0;
            lap = 0;
            victory = false;
            zoomInTime = StartGameZoomTimeMilliseconds;
            firstFrame = true;
        }

        /// <summary>
        /// Clear variables for game over
        /// </summary>
        public virtual void ClearVariablesForGameOver()
        {
        }
        #endregion

        #region Handle game logic
        /// <summary>
        /// Update game logic, called every frame. In Rocket Commander we did
        /// all the game logic in one big method inside the player class, but it
        /// was hard to add new game logic and many small things were also in
        /// the GameAsteroidManager. For this game we split everything up into
        /// much more classes and every class handles only its own variables.
        /// For example this class just handles the game time and zoom in time,
        /// for the car speed and physics just go into the CarController class.
        /// </summary>
        public virtual void Update()
        {
            // Since there is no loading screen, we need to skip the first frame because
            // the loading will cause ElapsedTimeThisFrameInMilliseconds to be too high
            if (firstFrame)
            {
                firstFrame = false;
                return;
            }

            // Handle zoomInTime at the beginning of a game
            if (!RacingGameManager.InMenu && zoomInTime > 0)
            {
                float lastZoomInTime = zoomInTime;
                zoomInTime -= BaseGame.ElapsedTimeThisFrameInMilliseconds;

                if (zoomInTime < 2000 &&
                    (int)((lastZoomInTime + 1000) / 1000) != (int)((zoomInTime + 1000) / 1000))
                {
                    // Handle start traffic light object (red, yellow, green!)
                    RacingGameManager.Landscape.ReplaceStartLightObject(
                        2 - (int)((zoomInTime + 1000) / 1000));
                }
            }

            // Don't handle any more game logic if game is over or still zooming in.
            if (CanControlCar == false)
                return;

            // Increase game time
            currentGameTimeMilliseconds += BaseGame.ElapsedTimeThisFrameInMilliseconds;
        }
        #endregion
    }
}
