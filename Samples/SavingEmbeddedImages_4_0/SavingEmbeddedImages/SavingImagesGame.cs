//-----------------------------------------------------------------------------
// SavingImagesGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace SavingEmbeddedImages
{
    /// <summary>
    /// SavingEmbeddedImages shows two ways of transferring content from a game project to
    /// the Windows Phone 7 Media Library:
    /// 
    ///   1. Loading an image texture from the game's content project, converting the
    ///      image to a jpeg, and saving it to the media library.
    ///      
    ///   2. Obtaining an image stream from the game project files and saving the stream
    ///      to the media library
    /// </summary>
    public class SavingImagesGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont mainFont;
        SpriteFont detailFont;

        Texture2D backgroundImage;

        // This image will be loaded from the game project
        Texture2D gameProjectImage;
        // This image will be loaded from the content project
        Texture2D contentProjectImage;

        // Bounding rectangles are used for image display and hit-testing.
        // These coordinates were chosen to look nice, no other special reason.
        Rectangle gameProjectImageBounds = new Rectangle(20, 20, 200, 333);
        Rectangle contentProjectImageBounds = new Rectangle(260, 20, 200, 333);

        // Simple flag to ignore UI input when system dialogs are animating.
        bool imageSaveInProgress = false;


        /// <summary>
        /// Set up the game to run full screen and full resolution.
        /// </summary>
        public SavingImagesGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }


        /// <summary>
        /// Perform pre-game initialization
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // enable tap gestures only for this sample
            TouchPanel.EnabledGestures = GestureType.Tap;
        }


        /// <summary>
        /// Load UI content and fonts.  Also create a drawable texture from both image types.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the background image.
            backgroundImage = Content.Load<Texture2D>("GameBackground");

            // load the fonts
            mainFont = Content.Load<SpriteFont>("mainFont");
            detailFont = Content.Load<SpriteFont>("detailFont");

            // Load the embedded image from the game project using TitleContainer
            // and constructing a Texture2D from the resulting stream object.
            // Note that the image properties are set to "Copy if newer" in the 
            // game project.
            using (Stream gameProjectImageStream = TitleContainer.OpenStream("GameProjectImage.jpg"))
            {
                gameProjectImage = Texture2D.FromStream(GraphicsDevice, gameProjectImageStream);
            }

            // Load the embedded image from the content project using the content loader.
            contentProjectImage = Content.Load<Texture2D>("ContentProjectImage");
        }

        
        /// <summary>
        /// Check for the back button to exit the app, and see if a touch occurred
        /// on one of the images.  
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
                        
            // check for touch input
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                // The text input dialog can allow input to occur between when it closes and when the
                // callback occurs, so the game guards against processing input in this state with a
                // simple flag.
                if (gesture.GestureType == GestureType.Tap && !imageSaveInProgress)
                {
                    Point tapLocation = new Point((int)gesture.Position.X, (int)gesture.Position.Y);

                    // An image touch triggersGuide.BeginShowKeyboardInput to receive
                    // a filename.  A hit-test determines which image was touched, and
                    // the callback is different depending on the source of the image.

                    if (gameProjectImageBounds.Contains(tapLocation))
                    {
                        imageSaveInProgress = true;

                        Guide.BeginShowKeyboardInput(
                            0,
                            "Save Picture",
                            "Enter a filename for your image",
                            "gameProjectImage",
                            SaveGameProjectImageCallback,
                            null);
                    }

                    if (contentProjectImageBounds.Contains(tapLocation))
                    {
                        imageSaveInProgress = true;

                        Guide.BeginShowKeyboardInput(
                            0,
                            "Save Picture",
                            "Enter a filename for your image",
                            "contentProjectImage",
                            SaveContentProjectImageCallback,
                            null);
                    }
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Render the UI.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundImage, new Vector2(0, 0), Color.White);

            spriteBatch.Draw(gameProjectImage, gameProjectImageBounds, null, Color.White);
            spriteBatch.DrawString(
                detailFont, 
                "game project image", 
                new Vector2(gameProjectImageBounds.X, gameProjectImageBounds.Y + gameProjectImageBounds.Height), 
                Color.White);

            spriteBatch.Draw(contentProjectImage, contentProjectImageBounds, null, Color.White);
            spriteBatch.DrawString(
                detailFont,
                "content project image",
                new Vector2(contentProjectImageBounds.X, contentProjectImageBounds.Y + contentProjectImageBounds.Height),
                Color.White);

            spriteBatch.DrawString(mainFont, "Tap an image to save it\nto your media library.", new Vector2(40,500), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Utility function to save a stream to the media library.  Media Library access won't
        /// work if the Zune desktop software is running.
        /// </summary>
        protected void SaveStreamToMediaLibrary(string filename, Stream stream)
        {
            MediaLibrary library = new MediaLibrary();

            string messageBoxTitle;
            string messageBoxContent;
            string[] buttons = { "OK" };
            MessageBoxIcon messageBoxIcon;

            try
            {
                library.SavePicture(filename, stream);

                messageBoxTitle = "Image saved.";
                messageBoxContent = filename + " successfully added to your Media Library.";
                messageBoxIcon = MessageBoxIcon.None;
            }
            catch (InvalidOperationException)
            {
                // The most likely reason for failure is that the Zune desktop software is running.
                // This prevents the phone from saving content to the device's media library.
                messageBoxTitle = "Unable to save image.";
                messageBoxContent = "The image could not be saved to the Media Library.  One reason might be that the phone is tethered to a PC with the Zune software running.";
                // the phone doesn't display an icon, but specifying MessageBoxIcon.Error causes a warning sound effect to be played
                messageBoxIcon = MessageBoxIcon.Error;                
            }

            IAsyncResult messageResult = Guide.BeginShowMessageBox(
                    messageBoxTitle, messageBoxContent,
                    buttons, 0, messageBoxIcon, null, null);

            Guide.EndShowMessageBox(messageResult);

            imageSaveInProgress = false;
        }


        /// <summary>
        /// Called when the user has picked a filename for the GameProject image.
        /// </summary>
        protected void SaveGameProjectImageCallback(IAsyncResult result)
        {
            string filename = Guide.EndShowKeyboardInput(result);
            if (!string.IsNullOrEmpty(filename))
            {
                // Both content images are loaded as textures for display, and could be
                // saved with Texture2D.SaveAsJpeg(), but if you need to go from
                // an image embedded in the .xap, an alternative approach is
                // to open a TitleContainer stream, and save it to the media library.
                using (Stream stream = TitleContainer.OpenStream("GameProjectImage.jpg"))
                {
                    SaveStreamToMediaLibrary(filename, stream);
                }
            }
            else
            {
                imageSaveInProgress = false;
            }
        }
        

        /// <summary>
        /// Called when the user has picked a filename for the ContentProject image.
        /// </summary>
        protected void SaveContentProjectImageCallback(IAsyncResult result)
        {
            string filename = Guide.EndShowKeyboardInput(result);
            if (!string.IsNullOrEmpty(filename))
            {
                /// Write a Texture2D to a stream and then save the stream to the media library.
                // The media library expects a Jpeg image, so make sure it gets the input type it wants.
                using (MemoryStream stream = new MemoryStream())
                {
                    contentProjectImage.SaveAsJpeg(stream, contentProjectImage.Width, contentProjectImage.Height);

                    // Reset the stream position after writing to it.
                    stream.Seek(0, 0);

                    SaveStreamToMediaLibrary(filename, stream);
                }
            }
            else
            {
                imageSaveInProgress = false;
            }
        }
    }
}
