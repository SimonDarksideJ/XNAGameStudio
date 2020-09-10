#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Fields


        Texture2D titleTexture;
        Vector2 titleTextureOrigin;

        ImageMenuEntry simpletonMenuEntry;
        ImageMenuEntry routineMenuEntry;
        ImageMenuEntry trickyMenuEntry;
        ImageMenuEntry backbreakingMenuEntry;
        ImageMenuEntry bedlamMenuEntry;
        

        #endregion


        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
            TransitionOnTime = Pickture.TransitionTime;
            TransitionOffTime = TimeSpan.FromSeconds(1f);

            // Create our menu entries.
            simpletonMenuEntry = new ImageMenuEntry(null);
            routineMenuEntry = new ImageMenuEntry(null);
            trickyMenuEntry = new ImageMenuEntry(null);
            backbreakingMenuEntry = new ImageMenuEntry(null);
            bedlamMenuEntry = new ImageMenuEntry(null);

            // Hook up menu event handlers.
            simpletonMenuEntry.Selected += SimpletonMenuEntrySelected;
            routineMenuEntry.Selected += RoutineMenuEntrySelected;
            trickyMenuEntry.Selected += TrickyMenuEntrySelected;
            backbreakingMenuEntry.Selected += BackbreakingMenuEntrySelected;
            bedlamMenuEntry.Selected += BedlamMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(simpletonMenuEntry);
            MenuEntries.Add(routineMenuEntry);
            MenuEntries.Add(trickyMenuEntry);
            MenuEntries.Add(backbreakingMenuEntry);
            MenuEntries.Add(bedlamMenuEntry);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MainMenuScreen object tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            if ((ScreenManager != null) && (ScreenManager.Game != null) &&
                (ScreenManager.Game.Content != null))
            {
                titleTexture = 
                    ScreenManager.Game.Content.Load<Texture2D>("Textures/Pickture");
                titleTextureOrigin = (titleTexture == null ? Vector2.Zero :
                    new Vector2(titleTexture.Width / 2, titleTexture.Height / 2));

                simpletonMenuEntry.Texture = 
                    ScreenManager.Game.Content.Load<Texture2D>("Textures/Simpleton");
                routineMenuEntry.Texture =
                    ScreenManager.Game.Content.Load<Texture2D>("Textures/Routine");
                trickyMenuEntry.Texture =
                    ScreenManager.Game.Content.Load<Texture2D>("Textures/Tricky");
                backbreakingMenuEntry.Texture =
                    ScreenManager.Game.Content.Load<Texture2D>("Textures/Backbreaking");
                bedlamMenuEntry.Texture =
                    ScreenManager.Game.Content.Load<Texture2D>("Textures/Bedlam");
            }
            base.LoadContent();
        }


        #endregion


        #region Handle Input


        public override void HandleInput(InputState input)
        {
            // play a sound effect for menu changes
            if (input.MenuUp || input.MenuDown)
            {
                Audio.Play("Focus Menu Item");
            }

            base.HandleInput(input);
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            const string message = "Are you sure you want to exit Pickture?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);
            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmExitMessageBox);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        /// <summary>
        /// Selection event for the "Simpleton" option.
        /// </summary>
        void SimpletonMenuEntrySelected(object obj, EventArgs args)
        {
            StartGame(3, false);
        }


        /// <summary>
        /// Selection event for the "Routine" option.
        /// </summary>
        void RoutineMenuEntrySelected(object obj, EventArgs args)
        {
            StartGame(4, false);
        }


        /// <summary>
        /// Selection event for the "Tricky" option.
        /// </summary>
        void TrickyMenuEntrySelected(object obj, EventArgs args)
        {
            StartGame(4, true);
        }


        /// <summary>
        /// Selection event for the "Backbreaking" option.
        /// </summary>
        void BackbreakingMenuEntrySelected(object obj, EventArgs args)
        {
            StartGame(6, true);
        }


        /// <summary>
        /// Selection event for the "Bedlam" option.
        /// </summary>
        void BedlamMenuEntrySelected(object obj, EventArgs args)
        {
            StartGame(10, true);
        }


        /// <summary>
        /// Starts a new game with specific parameters.
        /// </summary>
        /// <param name="boardSize">The size of the (square) board.</param>
        /// <param name="twoSided">
        /// If true, there is a different image on each side of the board.
        /// </param>
        void StartGame(int boardSize, bool twoSided)
        {
            if (boardSize <= 0)
            {
                throw new ArgumentOutOfRangeException("boardSize");
            }
            Board board = new Board(boardSize, boardSize, twoSided);
            ScreenManager.AddScreen(new BoardScreen(board));
            ScreenManager.AddScreen(new ShufflingScreen(board));
        }

        #endregion


        #region Drawing


        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;
            float halfWidth = spriteBatch.GraphicsDevice.Viewport.Width / 2;

            spriteBatch.Begin();

            if (titleTexture != null)
            {
                Color color = new Color(Vector4.One * (1.0f - TransitionPosition));
                spriteBatch.Draw(titleTexture, new Vector2(halfWidth, 128f), null,
                    color, 0f, titleTextureOrigin, 1f, SpriteEffects.None, 0.5f);
            }

            const int spriteHeight = 64;
            float baseY = (spriteBatch.GraphicsDevice.Viewport.Height -
                (spriteHeight * MenuEntries.Count) + titleTexture.Height) / 2;
            Vector2 position = new Vector2(halfWidth, baseY + spriteHeight / 2);

            // Draw each menu entry in turn.
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                MenuEntry menuEntry = MenuEntries[i];

                bool isSelected = IsActive && (i == SelectedEntry);

                menuEntry.Draw(this, position, isSelected, gameTime);

                position.Y += spriteHeight; //menuEntry.GetHeight(this);
            }

            spriteBatch.End();
        }


        #endregion
    }
}
