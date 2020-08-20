#region File Information
//-----------------------------------------------------------------------------
// PhoneScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// Provides an abstraction that allows a dynamic menu to be shown both vertically and horizontally
    /// by breaking the control space into two containers.  When held vertically, the two containers
    /// are on top of each other, and when held horizontally, the containers are side by side.
    /// </summary>
    public class PhoneScreen : Control
    {
        #region Fields

        private const int CONTAINER_WIDTH = 400;
        private const int CONTAINER_HEIGHT = 400;

        // Assuming orientation of 480 x 800
        private const int VERTICAL_CONTAINER1_LEFT = 40;
        private const int VERTICAL_CONTAINER1_TOP = 0;
        private const int VERTICAL_CONTAINER2_LEFT = 40;
        private const int VERTICAL_CONTAINER2_TOP = 400;

        // Assuming orientation of 800 x 480
        private const int HORIZONTAL_CONTAINER1_LEFT = 0;
        private const int HORIZONTAL_CONTAINER1_TOP = 40;
        private const int HORIZONTAL_CONTAINER2_LEFT = 400;
        private const int HORIZONTAL_CONTAINER2_TOP = 40;

        private Container container1 = new Container();
        private Container container2 = new Container();

        private DisplayOrientation currentOrientation = DisplayOrientation.Portrait;

        #endregion

        #region Properties

        /// <summary>
        /// The first container
        /// </summary>
        public Container Container1
        {
            get { return container1; }
        }

        /// <summary>
        /// The second container
        /// </summary>
        public Container Container2
        {
            get { return container2; }
        }

        /// <summary>
        /// The current orientation of the screen
        /// </summary>
        public DisplayOrientation CurrentOrientation
        {
            get
            {
                return currentOrientation;
            }
            set
            {
                currentOrientation = value;
                UpdateOrientation();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the control and its containers
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            container1.Initialize();
            container2.Initialize();

            // Initialize the orientation
            UpdateOrientation();
        }

        /// <summary>
        /// Loads the control and its containers
        /// </summary>
        public override void LoadContent(GraphicsDevice _graphics, ContentManager _content)
        {
            base.LoadContent(_graphics, _content);

            container1.LoadContent(_graphics, _content);
            container2.LoadContent(_graphics, _content);
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the control
        /// </summary>
        public override void Update(GameTime gameTime, List<GestureSample> gestures)
        {
            base.Update(gameTime, gestures);

            if (container1.Visible)
            {
                container1.Update(gameTime, gestures);
            }
            if (container2.Visible)
            {
                container2.Update(gameTime, gestures);
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the control
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            if (container1.Visible)
            {
                container1.Draw(gameTime, spriteBatch);
            }
            if (container2.Visible)
            {
                container2.Draw(gameTime, spriteBatch);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Changes the position of the containers according to the orientation of
        /// the phone
        /// </summary>
        private void UpdateOrientation()
        {
            Container1.Width = CONTAINER_WIDTH;
            Container1.Height = CONTAINER_HEIGHT;
            Container2.Width = CONTAINER_WIDTH;
            Container2.Height = CONTAINER_HEIGHT;
            switch (currentOrientation)
            {
                case DisplayOrientation.Portrait:
                    Container1.Left = VERTICAL_CONTAINER1_LEFT;
                    Container1.Top = VERTICAL_CONTAINER1_TOP;
                    Container2.Left = VERTICAL_CONTAINER2_LEFT;
                    Container2.Top = VERTICAL_CONTAINER2_TOP;
                    break;
                case DisplayOrientation.LandscapeLeft:
                case DisplayOrientation.LandscapeRight:
                    Container1.Left = HORIZONTAL_CONTAINER1_LEFT;
                    Container1.Top = HORIZONTAL_CONTAINER1_TOP;
                    Container2.Left = HORIZONTAL_CONTAINER2_LEFT;
                    Container2.Top = HORIZONTAL_CONTAINER2_TOP;
                    break;
            }
        }

        #endregion
    }
}
