#region File Description
//-----------------------------------------------------------------------------
// Cursor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace PickingSample
{
    /// <summary>
    /// Cursor is a DrawableGameComponent that draws a cursor on the screen.
    /// </summary>
    public class Cursor : DrawableGameComponent
    {
        #region Fields and Properties

        // this constant controls how fast the gamepad moves the cursor. this constant
        // is in pixels per second.
        const float CursorSpeed = 400.0f;

        // this spritebatch is created internally, and is used to draw the cursor.
        SpriteBatch spriteBatch;

        // this is the sprite that is drawn at the current cursor position.
        // textureCenter is used to center the sprite when drawing.
        Texture2D cursorTexture;
        Vector2 textureCenter;

        // Position is the cursor position, and is in screen space. 
        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }

        #endregion

        #region Initialization

        public Cursor(Game game)
            : base(game)
        {
        }

        // LoadContent needs to load the cursor texture and find its center.
        // also, we need to create a SpriteBatch.
        protected override void LoadContent()
        {
            cursorTexture = Game.Content.Load<Texture2D>("cursor");
            textureCenter = new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            // we want to default the cursor to start in the center of the screen
            Viewport vp = GraphicsDevice.Viewport;
            position.X = vp.X + (vp.Width / 2);
            position.Y = vp.Y + (vp.Height / 2);

            base.LoadContent();
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime)
        {
            // We use different input on each platform:
            // On Xbox, we use the GamePad's DPad and left thumbstick to move the cursor around the screen.
            // On Windows, we directly map the cursor to the location of the mouse.
            // On Windows Phone, we use the primary touch point for the location of the cursor.
#if XBOX
            UpdateXboxInput(gameTime);
#elif WINDOWS
            UpdateWindowsInput();
#elif WINDOWS_PHONE
            UpdateWindowsPhoneInput();
#endif
        }

        /// <summary>
        /// Handles input for Xbox 360.
        /// </summary>
        private void UpdateXboxInput(GameTime gameTime)
        {
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            // we'll create a vector2, called delta, which will store how much the
            // cursor position should change.
            Vector2 delta = currentState.ThumbSticks.Left;

            // down on the thumbstick is -1. however, in screen coordinates, values
            // increase as they go down the screen. so, we have to flip the sign of the
            // y component of delta.
            delta.Y *= -1;

            // check the dpad: if any of its buttons are pressed, that will change delta as well.
            if (currentState.DPad.Up == ButtonState.Pressed)
            {
                delta.Y = -1;
            }
            if (currentState.DPad.Down == ButtonState.Pressed)
            {
                delta.Y = 1;
            }
            if (currentState.DPad.Left == ButtonState.Pressed)
            {
                delta.X = -1;
            }
            if (currentState.DPad.Right == ButtonState.Pressed)
            {
                delta.X = 1;
            }

            // normalize delta so that we know the cursor can't move faster than CursorSpeed.
            if (delta != Vector2.Zero)
            {
                delta.Normalize();
            }

            // modify position using delta, the CursorSpeed constant defined above, and
            // the elapsed game time.
            position += delta * CursorSpeed *
                (float)gameTime.ElapsedGameTime.TotalSeconds;

            // clamp the cursor position to the viewport, so that it can't move off the screen.
            Viewport vp = GraphicsDevice.Viewport;
            position.X = MathHelper.Clamp(position.X, vp.X, vp.X + vp.Width);
            position.Y = MathHelper.Clamp(position.Y, vp.Y, vp.Y + vp.Height);
        }

        /// <summary>
        /// Handles input for Windows.
        /// </summary>
        private void UpdateWindowsInput()
        {
            MouseState mouseState = Mouse.GetState();
            position.X = mouseState.X;
            position.Y = mouseState.Y;
        }

        /// <summary>
        /// Handles input for Windows Phone.
        /// </summary>
        private void UpdateWindowsPhoneInput()
        {
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0)
            {
                position = touches[0].Position;
            }
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            // use textureCenter as the origin of the sprite, so that the cursor is 
            // drawn centered around Position.
            spriteBatch.Draw(cursorTexture, Position, null, Color.White, 0.0f,
                textureCenter, 1.0f, SpriteEffects.None, 0.0f);

            spriteBatch.End();
        }

        #endregion

        #region CalculateCursorRay

        // CalculateCursorRay Calculates a world space ray starting at the camera's
        // "eye" and pointing in the direction of the cursor. Viewport.Unproject is used
        // to accomplish this. see the accompanying documentation for more explanation
        // of the math behind this function.
        public Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(Position, 0f);
            Vector3 farSource = new Vector3(Position, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        #endregion
    }
}
