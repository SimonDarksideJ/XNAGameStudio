#region File Description
//-----------------------------------------------------------------------------
// BasePage.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ModelViewerDemo
{
    public class BasePage : PhoneApplicationPage
    {
        static FrameRateCounter frameRateCounter;

        GameTimer timer;
        SpriteBatch spriteBatch;
        SpriteFont font;
        UIElementRenderer uiRenderer;
        float uiOpacity = 0f;

        public BasePage()
        {
            // When we're in the designer, we won't have a graphics device nor will we want to load content
            if (!DesignerProperties.IsInDesignTool)
            {
                spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

                font = (Application.Current as App).Content.Load<SpriteFont>("FpsFont");
                frameRateCounter = new FrameRateCounter();

                DataContext = Renderer.Current.State;

                // Create a timer for this page
                timer = new GameTimer();
                timer.UpdateInterval = TimeSpan.FromTicks(333333);
                timer.Update += OnUpdate;
                timer.Draw += OnDraw;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Hook the LayoutUpdated to know when the page has calculated its layout
            LayoutUpdated += OnLayoutUpdated;

            timer.Start();
            uiOpacity = 0f;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            timer.Stop();
            base.OnNavigatedFrom(e);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            // Ensure the page size is valid
            if (width <= 0 || height <= 0)
                return;

            // Do we already have a UIElementRenderer of the correct size?
            if (uiRenderer != null &&
                uiRenderer.Texture != null &&
                uiRenderer.Texture.Width == width &&
                uiRenderer.Texture.Height == height)
            {
                return;
            }

            // Before constructing a new UIElementRenderer, be sure to Dispose the old one
            if (uiRenderer != null)
                uiRenderer.Dispose();

            // Create the renderer
            uiRenderer = new UIElementRenderer(this, width, height);
        }

        protected virtual void Update(GameTimerEventArgs e) { }

        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            uiOpacity = MathHelper.Clamp(uiOpacity + (float)e.ElapsedTime.TotalSeconds * 2f, 0f, 1f);
            frameRateCounter.OnUpdate(e.ElapsedTime);
            Renderer.Current.Update(e.ElapsedTime, e.TotalTime);
            Update(e);
        }

        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            // Render the page to a texture. We do this before clearing as it changes render targets and
            // as such will cause issues if we do it in the middle of our actual drawing.
            uiRenderer.Render();

            // Update the frame rate counter with the time
            frameRateCounter.OnDraw(e.ElapsedTime);

            // Clear the background
            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the current scene
            Renderer.Current.Draw();

            // Draw the UI texture with SpriteBatch
            spriteBatch.Begin();
            spriteBatch.Draw(uiRenderer.Texture, Vector2.Zero, Color.White * uiOpacity);

            // Optionally draw the frame rate counters
            if (Renderer.Current.State.ShowFrameRate)
            {
                spriteBatch.DrawString(font, frameRateCounter.UpdatesPerSecond, new Vector2(10f, 10f), Color.Red);
                spriteBatch.DrawString(font, frameRateCounter.FramesPerSecond, new Vector2(10f, 30f), Color.Red);
            }

            spriteBatch.End();
        }

    }
}
