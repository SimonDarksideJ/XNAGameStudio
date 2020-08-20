#region File Description
//-----------------------------------------------------------------------------
// SpriteFontControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace WinFormsGraphicsDevice
{
    /// <summary>
    /// Example control inherits from GraphicsDeviceControl, which allows it to
    /// render using a GraphicsDevice. This control shows how to use ContentManager
    /// inside a WinForms application. It loads a SpriteFont object through the
    /// ContentManager, then uses a SpriteBatch to draw text. The control is not
    /// animated, so it only redraws itself in response to WinForms paint messages.
    /// </summary>
    class SpriteFontControl : GraphicsDeviceControl
    {
        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont font;


        /// <summary>
        /// Initializes the control, creating the ContentManager
        /// and using it to load a SpriteFont.
        /// </summary>
        protected override void Initialize()
        {
            content = new ContentManager(Services, "Content");

            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = content.Load<SpriteFont>("hudFont");
        }


        /// <summary>
        /// Disposes the control, unloading the ContentManager.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                content.Unload();
            }

            base.Dispose(disposing);
        }


        /// <summary>
        /// Draws the control, using SpriteBatch and SpriteFont.
        /// </summary>
        protected override void Draw()
        {
            const string message = "Hello, World!\n" +
                                   "\n" +
                                   "I'm an XNA Framework GraphicsDevice,\n" +
                                   "running inside a WinForms application.\n" +
                                   "\n" +
                                   "This text is drawn using SpriteBatch,\n" +
                                   "with a SpriteFont that was loaded\n" +
                                   "through the ContentManager.\n" +
                                   "\n" +
                                   "The pane to my right contains a\n" +
                                   "spinning 3D triangle.";

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, message, new Vector2(23, 23), Color.White);
            spriteBatch.End();
        }
    }
}
