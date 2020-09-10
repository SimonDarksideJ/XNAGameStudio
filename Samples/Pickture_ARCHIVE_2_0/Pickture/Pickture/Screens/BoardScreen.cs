#region File Description
//-----------------------------------------------------------------------------
// BoardScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Pickture
{
    /// <summary>
    /// Screen responsible for updating and drawing the puzzle board. This screen should
    /// never be on the top of the screen stack; game logic should be in another screen
    /// on top of this one.
    /// </summary>
    class BoardScreen : GameScreen
    {
        Board board;

        public BoardScreen(Board board)
        {
            this.board = board;

            TransitionOnTime = Pickture.TransitionTime;
            TransitionOffTime = TimeSpan.FromSeconds(0.75f);
        }

        public override void LoadContent()
        {
            board.LoadContent();

            base.LoadContent();
        }

        public override void UnloadContent()
        {
            board.UnloadContent();

            base.UnloadContent();
        }

        public override void Update(GameTime gameTime,
            bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            board.Update(gameTime);
            
            // When transitioning
            if (ScreenState == ScreenState.TransitionOff ||
                ScreenState == ScreenState.TransitionOn )
            {
                foreach (Chip chip in board.Chips)
                {
                    // fade to black (or the opposite)
                    chip.ColorOverride = 1f - TransitionPosition;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {            
            board.Draw();
        }
    }
}