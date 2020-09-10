#region File Description
//-----------------------------------------------------------------------------
// Wp7MusicManagementGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace WP7MusicManagement
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Wp7MusicManagementGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        // Some graphics objects we use to draw the buttons
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D blank;

        // A simple component to help us manage background music
        BackgroundMusicManager musicManager;

        // The background music we want playing
        Song gameMusic;

        // The on screen buttons
        List<SimpleButton> buttons = new List<SimpleButton>();

        #region Initialization

        public Wp7MusicManagementGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Create our music manager component and add it to the game.
            musicManager = new BackgroundMusicManager(this);
            musicManager.PromptGameHasControl += MusicManagerPromptGameHasControl;
            musicManager.PlaybackFailed += MusicManagerPlaybackFailed;
            Components.Add(musicManager);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Enable the tap gesture so we can tap on our various buttons.
            TouchPanel.EnabledGestures = GestureType.Tap;

            // Create some buttons to test various launchers to see how music is handled during
            // various scenarios.

            // Add a button to play a video. When a video plays, the music automatically pauses,
            // but it won't automatically resume when the game comes back. This is the main
            // scenario that is remedied with the BackgroundMusicManager as it will automatically
            // restart our song when we are activated again.
            int y = 50;
            SimpleButton button = new SimpleButton(new Rectangle(300, y, 200, 50), "Play Video");
            button.Tapped += PlayViewButtonTapped;
            buttons.Add(button);


            // Add a button to get a picture from the library or camera. When the photo chooser
            // appears, game music is automatically paused. When the chooser is done and the game
            // comes back, the XNA Framework automatically resumes the music from the location
            // at which it paused.
            y += 100;
            button = new SimpleButton(new Rectangle(300, y, 200, 50), "Get Picture");
            button.Tapped += GetPictureButtonTapped;
            buttons.Add(button);


            // Add a button to view a website in IE. When IE appears, game music is automatically
            // paused. When the user comes back to the app, the game music is restarted automatically
            // by the XNA Framework but does not resume at the location at which it paused.
            y += 100;
            button = new SimpleButton(new Rectangle(300, y, 200, 50), "View Website");
            button.Tapped += ViewWebsiteButtonTapped;
            buttons.Add(button);


            // Add one last button that will toggle our music playback
            y += 100;
            button = new SimpleButton(new Rectangle(300, y, 200, 50), "Toggle Music");
            button.Tapped += ToggleMusicTapped;
            buttons.Add(button);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create our blank texture
            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new[] { Color.White });

            // Load the font for our buttons
            font = Content.Load<SpriteFont>("Font");

            // Load the song
            gameMusic = Content.Load<Song>("Music");

            // Instruct our manager to play our background music.
            musicManager.Play(gameMusic);
        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Handle all of the available gestures
            while (TouchPanel.IsGestureAvailable)
            {
                // Read the gesture from the TouchPanel
                GestureSample gesture = TouchPanel.ReadGesture();

                // If the user tapped the screen, we check all buttons to see if they were tapped.
                if (gesture.GestureType == GestureType.Tap)
                {
                    Point tapPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);
                    foreach (SimpleButton button in buttons)
                    {
                        button.CheckForTap(tapPoint);
                    }
                }
            }

            base.Update(gameTime);
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Simply call Draw on all of our buttons.
            spriteBatch.Begin();
            foreach (SimpleButton button in buttons)
                button.Draw(spriteBatch, blank, font);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Music Manager Event Handlers

        /// <summary>
        /// Invoked if the user is listening to music when we tell the music manager to play our song.
        /// We can respond by prompting the user to turn off their music, which will cause our game music
        /// to start playing.
        /// </summary>
        private void MusicManagerPromptGameHasControl(object sender, EventArgs e)
        {
            // Show a message box to see if the user wants to turn off their music for the game's music.
            Guide.BeginShowMessageBox(
                "Use game music?",
                "Would you like to turn off your music to listen to the game's music?",
                new[] { "Yes", "No" },
                0,
                MessageBoxIcon.None,
                result =>
                {
                    // Get the choice from the result
                    int? choice = Guide.EndShowMessageBox(result);

                    // If the user hit the yes button, stop the media player. Our music manager will
                    // see that we have a song the game wants to play and that the game will now have control
                    // and will automatically start playing our game song.
                    if (choice.HasValue && choice.Value == 0)
                        MediaPlayer.Stop();
                },
                null);
        }

        /// <summary>
        /// Invoked if music playback fails. The most likely case for this is that the Phone is connected to a PC
        /// that has Zune open, such as while debugging. Most games can probably just ignore this event, but we 
        /// can prompt the user so that they know why we're not playing any music.
        /// </summary>
        private void MusicManagerPlaybackFailed(object sender, EventArgs e)
        {
            // We're going to show a message box so the user knows why music didn't start playing.
            Guide.BeginShowMessageBox(
                "Music playback failed",
                "Music playback cannot begin if the phone is connected to a PC running Zune.",
                new[] { "Ok" },
                0,
                MessageBoxIcon.None,
                null,
                null);
        }

        #endregion

        #region Button Event Handlers

        /// <summary>
        /// Event handler that plays a video.
        /// </summary>
        private void PlayViewButtonTapped(object sender, EventArgs e)
        {
            // Create and show the MediaPlayerLauncher to play a video
            // in our game package.
            new MediaPlayerLauncher
            {
                Controls = MediaPlaybackControls.None,
                Media = new Uri("Video.wmv", UriKind.Relative),
                Location = MediaLocationType.Install,
            }.Show();
        }

        /// <summary>
        /// Event handler that shows the PhotoChooserTask.
        /// </summary>
        private void GetPictureButtonTapped(object sender, EventArgs e)
        {
            // Create and show the photo chooser task. If we cared about the
            // photo that was selected, we'd need to do more plumbing to
            // hook events in our Activated method, but since we are just using
            // this to show music experience in games, we don't have to worry
            // about that.
            new PhotoChooserTask { ShowCamera = true }.Show();
        }

        /// <summary>
        /// Event handler that shows the WebBrowserTask.
        /// </summary>
        private void ViewWebsiteButtonTapped(object sender, EventArgs e)
        {
            // Create and show the WebBrowserTask
            new WebBrowserTask { URL = "http://create.msdn.com" }.Show();
        }

        /// <summary>
        /// Event handler that toggles our music playback.
        /// </summary>
        private void ToggleMusicTapped(object sender, EventArgs e)
        {
            // If the music is playing, stop it.
            if (musicManager.IsGameMusicPlaying)
            {
                musicManager.Stop();
            }

            // Otherwise start playing our song.
            else
            {
                musicManager.Play(gameMusic);
            }
        }

        #endregion
    }
}
