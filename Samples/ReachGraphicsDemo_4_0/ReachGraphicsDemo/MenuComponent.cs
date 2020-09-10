#region File Description
//-----------------------------------------------------------------------------
// MenuComponent.cs
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Base class for all the different screens used in the demo. This provides
    /// a simple touch menu which can display a list of options, and detect when
    /// a menu item is clicked.
    /// </summary>
    class MenuComponent : DrawableGameComponent
    {
        // Properties.
        new public DemoGame Game { get { return (DemoGame)base.Game; } }

        public SpriteBatch SpriteBatch { get { return Game.SpriteBatch; } }
        public SpriteFont Font { get { return Game.Font; } }
        public SpriteFont BigFont { get { return Game.BigFont; } }

        protected List<MenuEntry> Entries { get; private set; }

        protected Vector2 LastTouchPoint { get; private set; }


        // Fields.
        bool touchDown = true;
        int touchSelection = -1;

        static TimeSpan attractTimer;
        static MouseState lastInputState = new MouseState(-1, -1, -1, 0, 0, 0, 0, 0);


        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuComponent(DemoGame game)
            : base(game)
        {
            Entries = new List<MenuEntry>();
        }


        /// <summary>
        /// Initializes the menu, computing the screen position of each entry.
        /// </summary>
        public override void Initialize()
        {
            Vector2 pos = new Vector2(MenuEntry.Border, 800 - MenuEntry.Border - Entries.Count * MenuEntry.Height);

            foreach (MenuEntry entry in Entries)
            {
                entry.Position = pos;

                pos.Y += MenuEntry.Height;
            }

            base.Initialize();
        }


        /// <summary>
        /// Resets the menu, whenever we transition to or from a different screen.
        /// </summary>
        virtual public void Reset()
        {
            if (touchSelection >= 0)
                Entries[touchSelection].IsFocused = false;

            touchDown = true;
            touchSelection = -1;
        }


        /// <summary>
        /// Updates the menu state, processing user input.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // We read input using the mouse API, which will report the first touch point
            // when run on the phone, but also works on Windows using a regular mouse.
            MouseState input = Game.IsActive ? Mouse.GetState() : new MouseState();

            // Scale input if we are running in an unusual screen resolution.
            int touchX = input.X * 480 / Game.Graphics.PreferredBackBufferWidth;
            int touchY = input.Y * 800 / Game.Graphics.PreferredBackBufferHeight;

            // Process the input.
            if (input.LeftButton == ButtonState.Pressed)
            {
                HandleTouchDown(touchX, touchY);
            }
            else
            {
                HandleTouchUp();
            }

            HandleAttractMode(gameTime, input);
        }


        /// <summary>
        /// Handles input while a touch is occurring.
        /// </summary>
        void HandleTouchDown(int touchX, int touchY)
        {
            // Hit test the touch position against the list of menu items.
            int currentEntry = -1;

            for (int i = 0; i < Entries.Count; i++)
            {
                if ((touchY >= Entries[i].Position.Y) && (touchY < Entries[i].Position.Y + MenuEntry.Height))
                {
                    currentEntry = i;
                    break;
                }
            }

            if (touchDown)
            {
                // Are we already processing a touch?
                if (touchSelection >= 0)
                {
                    if (currentEntry == touchSelection || Entries[touchSelection].IsDraggable)
                    {
                        // Pass drag input to the currently selected item.
                        Entries[touchSelection].IsFocused = true;

                        Entries[touchSelection].OnDragged(touchX - LastTouchPoint.X);
                    }
                    else
                    {
                        // If the drag moves off the selected item, unfocus it.
                        Entries[touchSelection].IsFocused = false;
                    }
                }
                else
                {
                    // If the touch was not on any menu item, process a backgroun drag.
                    OnDrag(new Vector2(touchX, touchY) - LastTouchPoint);
                }
            }
            else
            {
                // We are not currently processing a touch.
                touchDown = true;
                touchSelection = currentEntry;

                if (touchSelection >= 0)
                {
                    // Focus the menu item that has just been touched.
                    Entries[touchSelection].IsFocused = true;
                }
            }

            // Store the most recent touch location.
            LastTouchPoint = new Vector2(touchX, touchY);
        }


        /// <summary>
        /// Handles input when the touch is released.
        /// </summary>
        void HandleTouchUp()
        {
            if (touchDown && touchSelection >= 0 && Entries[touchSelection].IsFocused)
            {
                // If we were touching a menu item, and just released it, process the click action.
                Entries[touchSelection].IsFocused = false;
                Entries[touchSelection].OnClicked();
            }

            touchDown = false;
            touchSelection = -1;
        }


        /// <summary>
        /// If no input is provided, we go into an automatic attract mode, which cycles
        /// through the various options. This was great for leaving the demo unattended
        /// at the kiosk during the MIX10 conference!
        /// </summary>
        void HandleAttractMode(GameTime gameTime, MouseState input)
        {
            if (input != lastInputState || touchDown)
            {
                // If input has changed, reset the timer.
                attractTimer = TimeSpan.FromSeconds(-15);
                lastInputState = input;
            }
            else
            {
                // If no input occurs, increment the timer.
                attractTimer += gameTime.ElapsedGameTime;

                if (attractTimer > AttractDelay)
                {
                    // Timeout! Run the attract action.
                    attractTimer = TimeSpan.Zero;
                    OnAttract();
                }
            }
        }


        /// <summary>
        /// Allows subclasses to customize their attract behavior. The default is
        /// to simulate a click on the last menu entry, which is usually "back".
        /// </summary>
        protected virtual void OnAttract()
        {
            Entries[Entries.Count - 1].OnClicked();
        }

        
        /// <summary>
        /// Allows subclasses to customize how long they wait before cycling through the attract sequence.
        /// </summary>
        protected virtual TimeSpan AttractDelay { get { return TimeSpan.FromSeconds(10); } }


        /// <summary>
        /// Draws the list of menu entries.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin(0, null, null, null, null, null, Game.ScaleMatrix);

            foreach (MenuEntry entry in Entries)
            {
                entry.Draw(SpriteBatch, Font, Game.BlankTexture);
            }

            SpriteBatch.End();
        }


        /// <summary>
        /// Draws the menu title.
        /// </summary>
        protected void DrawTitle(string title, Color? backgroundColor, Color titleColor)
        {
            if (backgroundColor.HasValue)
                GraphicsDevice.Clear(backgroundColor.Value);

            SpriteBatch.Begin(0, null, null, null, null, null, Game.ScaleMatrix);
            SpriteBatch.DrawString(BigFont, title, new Vector2(480, 24), titleColor, MathHelper.PiOver2, Vector2.Zero, 1, 0, 0);
            SpriteBatch.End();
        }


        /// <summary>
        /// Handles a drag on the background of the screen.
        /// </summary>
        protected virtual void OnDrag(Vector2 delta)
        {
        }
    }
}
