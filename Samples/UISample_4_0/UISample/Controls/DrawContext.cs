//-----------------------------------------------------------------------------
// DrawContext.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace UserInterfaceSample.Controls
{
    /// <summary>
    /// DrawContext is a collection of rendering data to pass into Control.Draw().
    /// By passing this data into each Draw call, we controls can access necessary
    /// data when they need it, without introducing dependencies on top-level object
    /// like ScreenManager.
    /// </summary>
    public struct DrawContext
    {
        /// <summary>
        /// The XNA GraphicsDevice
        /// </summary>
        public GraphicsDevice Device;

        /// <summary>
        /// GameTime passed into Game.Draw()
        /// </summary>
        public GameTime GameTime;

        /// <summary>
        /// Shared SpriteBatch for use by any control that wants to draw with it.
        /// Begin() is called on this batch before drawing controls, and End() is
        /// called after drawing controls, so that multiple controls can have
        /// their rendering batched together.
        /// </summary>
        public SpriteBatch SpriteBatch;

        /// <summary>
        /// A single-pixel white texture, useful for drawing boxes and lines within a SpriteBatch.
        /// </summary>
        public Texture2D BlankTexture;

        /// <summary>
        /// Positional offset to draw at. Note that this is a simple positional offset rather
        /// than a full transform, so this API doesn't easily support full heirarchical transforms.
        ///
        /// A control's position will already be added to this vector when Control.Draw() is called.
        /// </summary>
        public Vector2 DrawOffset;
    }
}
