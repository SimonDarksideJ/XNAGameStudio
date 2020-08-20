#region File Description
//-----------------------------------------------------------------------------
// VibrationManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CameraShake
{
    /// <summary>
    /// Handles management of vibration of GamePads and Windows Phone.
    /// </summary>
    public class VibrationManager : GameComponent
    {
        // We track once instance of the manager for static use
        private static VibrationManager instance;

        // We associate each player with a single vibration, represented by a VibrationSettings object
        private readonly Dictionary<PlayerIndex, VibrationSettings> vibrations = 
            new Dictionary<PlayerIndex, VibrationSettings>();

        // We track our paused state separately from the Enabled property to avoid
        // accidentally calling Pause() without calling Resume()
        private bool isPaused = false;

        /// <summary>
        /// Initializes a new VibrationManager.
        /// </summary>
        /// <param name="game"></param>
        public VibrationManager(Game game)
            : base(game)
        {
            // We only allow one manager per game
            if (instance != null)
                throw new InvalidOperationException("Cannot create multiple VibrationManagers");

            // Store the manager instance
            instance = this;

            // Add a setting for each player
            vibrations.Add(PlayerIndex.One, new VibrationSettings());
            vibrations.Add(PlayerIndex.Two, new VibrationSettings());
            vibrations.Add(PlayerIndex.Three, new VibrationSettings());
            vibrations.Add(PlayerIndex.Four, new VibrationSettings());
        }

        /// <summary>
        /// Creates a vibration for a given player. On Windows and Xbox the vibration strength uses a
        /// linear fall off to reduce the strength over time. On Windows Phone we don't have that control
        /// so the phone vibrates at the same strength for the duration of the vibration.
        /// </summary>
        /// <param name="player">The player whose GamePad should vibrate. On Windows Phone this should be
        /// PlayerIndex.One.</param>
        /// <param name="left">The initial strength of the left motor. This should be in the range of 0-1.</param>
        /// <param name="right">The initial strength of the right motor. This should be in the range of 0-1.</param>
        /// <param name="duration">The length time (in seconds) to vibrate the GamePad or phone.</param>
        public static void Vibrate(PlayerIndex player, float left, float right, float duration)
        {
            // We don't allow starting new vibrations when the system is paused.
            if (instance.isPaused)
                return;

            // Get our settings
            VibrationSettings settings = instance.vibrations[player];

            // Initialize the settings object
            settings.Left = left;
            settings.Right = right;
            settings.Duration = duration;
            settings.Timer = 0f;

            // Set our initial vibration for the GamePad.
            GamePad.SetVibration(player, left, right);

#if WINDOWS_PHONE
            // If we're setting the vibration for player one, we also start the VibrateController
            if (player == PlayerIndex.One)
            {
                Microsoft.Devices.VibrateController.Default.Start(TimeSpan.FromSeconds(duration));
            }
#endif
        }

        /// <summary>
        /// Pauses all vibrations.
        /// </summary>
        public static void Pause()
        {
            // Pause our instance
            instance.isPaused = true;

            // Stop the vibration on all GamePads
            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
                GamePad.SetVibration(player, 0, 0);

#if WINDOWS_PHONE
            // Stop the VibrateController for the phone
            Microsoft.Devices.VibrateController.Default.Stop();
#endif
        }

        /// <summary>
        /// Resumes all paused vibrations.
        /// </summary>
        public static void Resume()
        {
            // Unpause the instance
            instance.isPaused = false;

            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
            {
                // We call UpdateVibration for all players which will resume our vibrations
                // from the correct point in our progress.
                VibrationSettings settings = instance.vibrations[player];
                UpdateVibration(player, settings);

#if WINDOWS_PHONE
                // If we're looking at player one and the vibration is not complete, we also
                // start the VibrateController with the remaining time needed to complete
                // the original duration.
                if (player == PlayerIndex.One && settings.Timer < settings.Duration)
                {
                    Microsoft.Devices.VibrateController.Default.Start(TimeSpan.FromSeconds(settings.Duration - settings.Timer));
                }
#endif
            }
        }

        /// <summary>
        /// Cancels vibrations for all players.
        /// </summary>
        public static void CancelAll()
        {
            // Iterate all vibrations, setting their durations to 0 and turning off the vibration
            // of the GamePads.
            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
            {
                VibrationSettings settings = instance.vibrations[player];
                settings.Duration = 0f;
                GamePad.SetVibration(player, 0, 0);
            }

#if WINDOWS_PHONE
            // On the phone we also need to stop the VibrateController.
            Microsoft.Devices.VibrateController.Default.Stop();
#endif
        }

        public override void Update(GameTime gameTime)
        {
            // Do nothing if we're currently paused
            if (instance.isPaused)
                return;

            // Get the elapsed frame time.
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update all of the vibrations
            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
            {
                VibrationSettings settings = vibrations[player];

                // If the vibration is not complete
                if (settings.Timer < settings.Duration)
                {
                    // Update our timer, clamping at the duration
                    settings.Timer = (float)Math.Min(settings.Timer + time, settings.Duration);

                    // Update the vibration
                    UpdateVibration(player, settings);
                }
            }
        }

        /// <summary>
        /// Updates the vibration for a given player.
        /// </summary>
        private static void UpdateVibration(PlayerIndex player, VibrationSettings settings)
        {
            // If the vibration is at its duration, stop the vibration of the GamePad.
            if (settings.Timer >= settings.Duration)
            {
                GamePad.SetVibration(player, 0, 0);
            }
            else
            {
                // Compute our progress in a [0, 1] range
                float progress = settings.Timer / settings.Duration;

                // Calculate our left and right motor strengths while applying a linear decay over time.
                float left = settings.Left * (1f - progress);
                float right = settings.Right * (1f - progress);

                // Set the vibration of the GamePad.
                GamePad.SetVibration(player, left, right);
            }
        }

        /// <summary>
        /// A private helper that maintains information about 
        /// the vibration state for a single player.
        /// </summary>
        class VibrationSettings
        {
            /// <summary>
            /// The strength for the left motor of a GamePad.
            /// </summary>
            public float Left;

            /// <summary>
            /// The strength for the right motor of a GamePad.
            /// </summary>
            public float Right;

            /// <summary>
            /// The duration of the vibration.
            /// </summary>
            public float Duration;

            /// <summary>
            /// A timer tracking how long the vibration has been active.
            /// </summary>
            public float Timer;
        }
    }
}
