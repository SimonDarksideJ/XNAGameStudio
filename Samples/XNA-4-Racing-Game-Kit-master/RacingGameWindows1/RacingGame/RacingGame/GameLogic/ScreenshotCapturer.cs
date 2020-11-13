#region File Description
//-----------------------------------------------------------------------------
// ScreenshotCapturer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

// Does not work on the Xbox360, no Save method in the Texture class!
#if !XBOX360

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using RacingGame.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Graphics;
#endregion

namespace RacingGame.GameLogic
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public partial class ScreenshotCapturer : GameComponent
    {
        #region Variables
        /// <summary>
        /// Internal screenshot number (will increase by one each screenshot)
        /// </summary>
        private int screenshotNum = 0;
        /// <summary>
        /// Link to BaseGame class instance. Also holds windows title,
        /// which is used instead of Application.ProgramName.
        /// </summary>
        BaseGame game;
        #endregion

        #region Constructor
        public ScreenshotCapturer(BaseGame setGame)
            : base(setGame)
        {
            game = setGame;
            screenshotNum = GetCurrentScreenshotNum();
        }
        #endregion

        #region Make screenshot
        #region Screenshot name builder
        /// <summary>
        /// Screenshot name builder
        /// </summary>
        /// <param name="num">Num</param>
        /// <returns>String</returns>
        private string ScreenshotNameBuilder(int num)
        {
            return Directories.ScreenshotsDirectory + "\\" +
                game.Window.Title + " Screenshot " +
                num.ToString("0000") + ".jpg";
        }
        #endregion

        #region Get current screenshot num
        /// <summary>
        /// Get current screenshot num
        /// </summary>
        /// <returns>Int</returns>
        private int GetCurrentScreenshotNum()
        {
            // We must search for last screenshot we can found in list using own
            // fast filesearch
            int i = 0, j = 0, k = 0, l = -1;
            // First check if at least 1 screenshot exist
            if (File.Exists(ScreenshotNameBuilder(0)) == true)
            {
                // First scan for screenshot num/1000
                for (i = 1; i < 10; i++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000)) == false)
                        break;
                }

                // This i*1000 does not exist, continue scan next level
                // screenshotnr/100
                i--;
                for (j = 1; j < 10; j++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100)) == false)
                        break;
                }

                // This i*1000+j*100 does not exist, continue scan next level
                // screenshotnr/10
                j--;
                for (k = 1; k < 10; k++)
                {
                    if (File.Exists(ScreenshotNameBuilder(
                            i * 1000 + j * 100 + k * 10)) == false)
                        break;
                }

                // This i*1000+j*100+k*10 does not exist, continue scan next level
                // screenshotnr/1
                k--;
                for (l = 1; l < 10; l++)
                {
                    if (File.Exists(ScreenshotNameBuilder(
                            i * 1000 + j * 100 + k * 10 + l)) == false)
                        break;
                }

                // This i*1000+j*100+k*10+l does not exist, we have now last
                // screenshot nr!!!
                l--;
            }

            return i * 1000 + j * 100 + k * 10 + l;
        }
        #endregion

        /// <summary>
        /// Make screenshot
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "App should not crash from making a screenshot, " +
            "but exceptions are often thrown by this code, see inside.")]
        private void MakeScreenshot()
        {
            try
            {
                //NOTE: This doesn't always work on all cards, especially if
                // desktop mode switches in fullscreen mode!

                screenshotNum++;
                // Make sure screenshots directory exists
                if (Directory.Exists(Directories.ScreenshotsDirectory) == false)
                    Directory.CreateDirectory(Directories.ScreenshotsDirectory);

                int width = BaseGame.Device.PresentationParameters.BackBufferWidth;
                int height = BaseGame.Device.PresentationParameters.BackBufferHeight;

                using (
                    var tex = new Texture2D(BaseGame.Device, width, height, false,
                                            BaseGame.Device.PresentationParameters.BackBufferFormat))
                {
                    int[] backbuffer = new int[width*height];
                    BaseGame.Device.GetBackBufferData(backbuffer);

                    tex.SetData(backbuffer);

                    FileHelper.StorageContainerMRE.WaitOne();
                    FileHelper.StorageContainerMRE.Reset();
                    // Open a storage container
                    StorageDevice storageDevice = FileHelper.XnaUserDevice;
                    if ((storageDevice != null) && storageDevice.IsConnected)
                    {
                        IAsyncResult async = storageDevice.BeginOpenContainer("RacingGame", null, null);

                        async.AsyncWaitHandle.WaitOne();

                        using (StorageContainer container = storageDevice.EndOpenContainer(async))
                        {
                            async.AsyncWaitHandle.Close();
                            using (Stream stream = container.CreateFile(ScreenshotNameBuilder(screenshotNum)))
                            {
                                tex.SaveAsJpeg(stream, width, height);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Failed to save Screenshot: " + ex.ToString());
            }
        }

        #endregion

        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (Input.KeyboardKeyJustPressed(Keys.PrintScreen))
                MakeScreenshot();

            base.Update(gameTime);
        }
        #endregion
    }
}
#endif