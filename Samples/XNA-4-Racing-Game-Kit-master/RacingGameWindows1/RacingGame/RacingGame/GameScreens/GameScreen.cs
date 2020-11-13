#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Properties;
using RacingGame.Landscapes;
using Microsoft.Xna.Framework;
using RacingGame.Shaders;
using RacingGame.Sounds;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// GameScreen, just manages the on screen display for the game.
    /// </summary>
    class GameScreen : IGameScreen
    {
        #region Variables
        #endregion

        #region Constructor
        /// <summary>
        /// Create game screen
        /// </summary>
        public GameScreen()
        {
            // Load level
            RacingGameManager.LoadLevel(TrackSelection.SelectedTrack);

            // Reset player variables (start new game, reset time and position)
            RacingGameManager.Player.Reset();

            // Fix light direction (was changed by CarSelection screen!)
            // LightDirection will normalize
            BaseGame.LightDirection = LensFlare.DefaultLightPos;

            // Start gear sound
            Sound.StartGearSound();

            // Play game music
            Sound.Play(Sound.Sounds.GameMusic);
        }
        #endregion

		#region Update
		/// <summary>
		/// Unimplemented
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{

		}
		#endregion

		#region Render
		/// <summary>
        /// Render game screen. Called each frame.
        /// </summary>
        public bool Render()
        {
            ShadowMapShader.PrepareGameShadows();

            // This starts both menu and in game post screen shader!
            BaseGame.UI.PostScreenGlowShader.Start();

            // Render background sky and lensflare.
            BaseGame.UI.RenderGameBackground();

            // Render landscape with track and all objects
            RacingGameManager.Landscape.Render();

            // Render car with matrix we got from CarPhysics
            RacingGameManager.CarModel.RenderCar(
                RacingGameManager.currentCarNumber,
                RacingGameManager.CarColor,
                false,
                RacingGameManager.Player.CarRenderMatrix);

            // And flush all models to be rendered
            BaseGame.MeshRenderManager.Render();

            // Use data from best replay for the shadow car
            Matrix bestReplayCarMatrix =
                RacingGameManager.Landscape.BestReplay.GetCarMatrixAtTime(
                RacingGameManager.Player.GameTimeMilliseconds / 1000.0f);
            // For rendering rotate car to stay correctly on the road
            bestReplayCarMatrix =
                Matrix.CreateRotationX(MathHelper.Pi / 2.0f) *
                Matrix.CreateRotationZ(MathHelper.Pi) *
                bestReplayCarMatrix;

            // Also render the shadow car (if the game has started)!
            if (RacingGameManager.Player.GameTimeMilliseconds > 0)
                RacingGameManager.CarModel.RenderCar(
                    0, RacingGameManager.CarColor,
                    true, bestReplayCarMatrix);

            // Show shadows we calculated above
            if (BaseGame.AllowShadowMapping)
                ShaderEffect.shadowMapping.ShowShadows();

            // Apply post screen shader here before doing the UI
            BaseGame.UI.PostScreenGlowShader.Show();

            // Play motor sound
            Sound.UpdateGearSound(RacingGameManager.Player.Speed,
                RacingGameManager.Player.Acceleration);

            // Show on screen UI for the game.
            // Note: Could be improved by using the latest checkpoints and
            // check times this way!
            BaseGame.UI.RenderGameUI(
                (int)RacingGameManager.Player.GameTimeMilliseconds,
                // Best time and current lap
                (int)RacingGameManager.Player.BestTimeMilliseconds,
                RacingGameManager.Player.CurrentLap + 1,
                RacingGameManager.Player.Speed * CarPhysics.MeterPerSecToMph,
                // Gear logic with sound (could be improved ^^)
                1 + (int)(5 * RacingGameManager.Player.Speed /
                CarPhysics.MaxPossibleSpeed),
                // Motormeter
                0.5f * RacingGameManager.Player.Speed /
                CarPhysics.MaxPossibleSpeed +
                // This could be improved
                0.5f * RacingGameManager.Player.Acceleration,
                RacingGameManager.Landscape.CurrentTrackName,
                Highscores.GetTop5LapTimes(TrackSelection.SelectedTrackNumber));

            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBackJustPressed ||
                (RacingGameManager.Player.GameOver &&
                (Input.KeyboardSpaceJustPressed ||
                Input.GamePadAJustPressed ||
                Input.GamePadBJustPressed ||
                Input.GamePadXJustPressed ||
                Input.GamePadXJustPressed ||
                Input.MouseLeftButtonJustPressed)))
            {
                // Stop motor sound
                Sound.StopGearSound();

                // Play menu music again
                Sound.Play(Sound.Sounds.MenuMusic);

                // Return to menu
                return true;
            }

            return false;
        }
        #endregion
    }
}
