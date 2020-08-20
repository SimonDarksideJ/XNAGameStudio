#region File Description
//-----------------------------------------------------------------------------
// BackgroundMusicManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace WP7MusicManagement
{
    /// <summary>
    /// A game component that can be used to play background music, ensuring
    /// that the GameHasControl property is respected and that music playback
    /// will resume after any video playback.
    /// </summary>
    /// <remarks>
    /// The BackgroundMusicManager has two primary responsibilities:
    /// 
    /// 1) If the game wishes to play a song, the BackgroundMusicManager will monitor the
    ///    MediaPlayer.GameHasControl property to ensure that the game plays the music when
    ///    it is allowed, without playing over the user's music.
    /// 2) If the game's music is paused for some reason (e.g. the headphones were unplugged)
    ///    the BackgroundMusicManager will resume the playback automatically.
    ///    
    /// The BackgroundMusicManager helps not only with the Windows Phone Certification
    /// Requirements for using GameHasControl, but also makes it easier to resume media after
    /// certain events, such as watching videos or unplugging headphones, both of which will
    /// pause music with no automatic resuming. 
    /// </remarks>
    public class BackgroundMusicManager : GameComponent
    {
        // We don't want to poll constantly, so we set an amount of time (in seconds) of
        // how often we should poll. By default, we're going to poll every second.
        const float PollDelay = 1f;

        // A simple float for our polling timer.
        private float gameHasControlTimer;

        // We keep a member variable around to tell us if we have control of the music.
        private bool gameHasControl = false;

        // The song the game wants currently playing.
        private Song currentSong;

        /// <summary>
        /// Gets whether or not the game music is currently playing.
        /// </summary>
        public bool IsGameMusicPlaying { get; private set; }

        /// <summary>
        /// Invoked if the game tries to play a song and the game doesn't have control. This
        /// allows games the chance to prompt the user and turn off their music if they accept.
        /// </summary>
        public event EventHandler<EventArgs> PromptGameHasControl;

        /// <summary>
        /// Invoked if song playback fails to let the game prompt the user or respond
        /// as necessary.
        /// </summary>
        public event EventHandler<EventArgs> PlaybackFailed;

        /// <summary>
        /// Initializes a new BackgroundMusicManager.
        /// </summary>
        /// <param name="game">The Game that is using this manager.</param>
        public BackgroundMusicManager(Game game)
            : base(game)
        {
            // Grab the GameHasControl as our initial value
            gameHasControl = MediaPlayer.GameHasControl;

            // Hook the game's activated event so we can respond if the game gets backgrounded
            // such as when a video is played with MediaPlayerLauncher.
            game.Activated += game_Activated;
        }

        /// <summary>
        /// Event handler that is invoked when the game is activated.
        /// </summary>
        void game_Activated(object sender, EventArgs e)
        {
            // See if we have control of the music
            gameHasControl = MediaPlayer.GameHasControl;

            // If we have control, a song we want to play, and the media player isn't playing,
            // play our song. This will happen when coming back from deactivation with certain
            // launchers (mainly the MediaPlayerLauncher) which don't automatically play/resume
            // the song for us. We can detect this case and restart the song ourselves, that way
            // the user doesn't end up with a game without background music.
            if (gameHasControl && currentSong != null && MediaPlayer.State != MediaState.Playing)
                PlaySongSafe();
        }

        /// <summary>
        /// Plays a given song as the background music.
        /// </summary>
        /// <param name="song">The song to play.</param>
        public void Play(Song song)
        {
            // Store the song in our member variable.
            currentSong = song;

            // If we have control, play the song immediately.
            if (gameHasControl)
                PlaySongSafe();

            // Otherwise invoke our event so the game can check with the player
            // to see if they want to stop the current song so the game's music
            // will play.
            else if (PromptGameHasControl != null)
                PromptGameHasControl(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stops playing our background music.
        /// </summary>
        public void Stop()
        {
            // Null out our member variable
            currentSong = null;

            // If we have control, stop the media player.
            if (gameHasControl)
                MediaPlayer.Stop();
        }

        /// <summary>
        /// Allows the component to handle its update logic.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Update our timer
            gameHasControlTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // If we've passed our poll delay, we want to handle our update
            if (gameHasControlTimer >= PollDelay)
            {
                // Reset the timer back to zero
                gameHasControlTimer = 0f;

                // Check to see if we have control of the media player
                gameHasControl = MediaPlayer.GameHasControl;

                // Get the current state and song from the MediaPlayer
                MediaState currentState = MediaPlayer.State;
                Song activeSong = MediaPlayer.Queue.ActiveSong;

                // If we have control of the music...
                if (gameHasControl)
                {
                    // If we have a song that we want playing...
                    if (currentSong != null)
                    {
                        // If the media player isn't playing anything...
                        if (currentState != MediaState.Playing)
                        {
                            // If the song is paused, for example because the headphones
                            // were removed, we call Resume() to continue playback.
                            if (currentState == MediaState.Paused)
                            {
                                ResumeSongSafe();
                            }

                            // Otherwise we play our desired song.
                            else
                            {
                                PlaySongSafe();
                            }
                        }
                    }

                    // If we don't have a song we want playing, we want to make sure we stop
                    // any music we may have previously had playing.
                    else
                    {
                        if (currentState != MediaState.Stopped)
                            MediaPlayer.Stop();
                    }
                }

                // Store a value indicating if the game music is playing
                IsGameMusicPlaying = (currentState == MediaState.Playing) && gameHasControl;
            }
        }

        /// <summary>
        /// Helper method to wrap MediaPlayer.Play to handle exceptions.
        /// </summary>
        private void PlaySongSafe()
        {
            // Make sure we have a song to play
            if (currentSong == null)
                return;

            try
            {
                MediaPlayer.Play(currentSong);
            }
            catch (InvalidOperationException)
            {
                // Media playback will fail if Zune is connected. We don't want the
                // game to crash, however, so we catch the exception.

                // Null out the song so we don't keep trying to play it. That would
                // cause us to keep catching exceptions and will likely cause the game
                // to hitch occassionally.
                currentSong = null;

                // Invoke our PlaybackFailed event in case the game wants to handle this
                // scenario in some custom way.
                if (PlaybackFailed != null)
                    PlaybackFailed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Helper method to wrap MediaPlayer.Resume to handle exceptions.
        /// </summary>
        private void ResumeSongSafe()
        {
            try
            {
                MediaPlayer.Resume();
            }
            catch (InvalidOperationException)
            {
                // Media playback will fail if Zune is connected. We don't want the
                // game to crash, however, so we catch the exception.

                // Null out the song so we don't keep trying to resume it. That would
                // cause us to keep catching exceptions and will likely cause the game
                // to hitch occassionally.
                currentSong = null;

                // Invoke our PlaybackFailed event in case the game wants to handle this
                // scenario in some custom way.
                if (PlaybackFailed != null)
                    PlaybackFailed(this, EventArgs.Empty);
            }
        }
    }
}
