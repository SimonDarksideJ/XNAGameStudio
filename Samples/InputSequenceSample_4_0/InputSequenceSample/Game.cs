#region File Description
//-----------------------------------------------------------------------------
// Game.cs
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace InputSequenceSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class InputSequenceSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // This is the master list of moves in logical order. This array is kept
        // around in order to draw the move list on the screen in this order.
        Move[] moves;
        // The move list used for move detection at runtime.
        MoveList moveList;

        // The move list is used to match against an input manager for each player.
        InputManager[] inputManagers;
        // Stores each players' most recent move and when they pressed it.
        Move[] playerMoves;
        TimeSpan[] playerMoveTimes;

        // Time until the currently "active" move dissapears from the screen.
        readonly TimeSpan MoveTimeOut = TimeSpan.FromSeconds(1.0);

        // Direction textures.
        Texture2D upTexture;
        Texture2D downTexture;
        Texture2D leftTexture;
        Texture2D rightTexture;
        Texture2D upLeftTexture;
        Texture2D upRightTexture;
        Texture2D downLeftTexture;
        Texture2D downRightTexture;

        // Button textures.
        Texture2D aButtonTexture;
        Texture2D bButtonTexture;
        Texture2D xButtonTexture;
        Texture2D yButtonTexture;

        // Other textures.
        Texture2D plusTexture;
        Texture2D padFaceTexture;

        #endregion

        #region Initialization

        public InputSequenceSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            // Construct the master list of moves.
            moves = new Move[]
                {
                    new Move("Jump",        Buttons.A) { IsSubMove = true },
                    new Move("Punch",       Buttons.X) { IsSubMove = true },
                    new Move("Double Jump", Buttons.A, Buttons.A),
                    new Move("Jump Kick",   Buttons.A | Buttons.X),
                    new Move("Quad Punch",  Buttons.X, Buttons.Y, Buttons.X, Buttons.Y),                    
                    new Move("Fireball",    Direction.Down, Direction.DownRight,
                                            Direction.Right | Buttons.X),
                    new Move("Long Jump",   Direction.Up, Direction.Up, Buttons.A),
                    new Move("Back Flip",   Direction.Down, Direction.Down | Buttons.A),
                    new Move("30 Lives",    Direction.Up, Direction.Up,
                                            Direction.Down, Direction.Down,
                                            Direction.Left, Direction.Right,
                                            Direction.Left, Direction.Right,
                                            Buttons.B, Buttons.A),
                };

            // Construct a move list which will store its own copy of the moves array.
            moveList = new MoveList(moves);            

            // Create an InputManager for each player with a sufficiently large buffer.
            inputManagers = new InputManager[2];
            for (int i = 0; i < inputManagers.Length; ++i)
            {
                inputManagers[i] =
                    new InputManager((PlayerIndex)i, moveList.LongestMoveLength);
            }

            // Give each player a location to store their most recent move.
            playerMoves = new Move[inputManagers.Length];
            playerMoveTimes = new TimeSpan[inputManagers.Length];
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>("Font");

            // Load direction textures.
            upTexture        = Content.Load<Texture2D>("Up");
            downTexture      = Content.Load<Texture2D>("Down");
            leftTexture      = Content.Load<Texture2D>("Left");
            rightTexture     = Content.Load<Texture2D>("Right");
            upLeftTexture    = Content.Load<Texture2D>("UpLeft");
            upRightTexture   = Content.Load<Texture2D>("UpRight");
            downLeftTexture  = Content.Load<Texture2D>("DownLeft");
            downRightTexture = Content.Load<Texture2D>("DownRight");

            // Load button textures.
            aButtonTexture = Content.Load<Texture2D>("A");
            bButtonTexture = Content.Load<Texture2D>("B");
            xButtonTexture = Content.Load<Texture2D>("X");
            yButtonTexture = Content.Load<Texture2D>("Y");

            // Load other textures.
            plusTexture = Content.Load<Texture2D>("Plus");
            padFaceTexture = Content.Load<Texture2D>("PadFace");
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            for (int i = 0; i < inputManagers.Length; ++i)
            {
                // Expire old moves.
                if (gameTime.TotalGameTime - playerMoveTimes[i] > MoveTimeOut)
                {
                    playerMoves[i] = null;
                }

                // Get the updated input manager.
                InputManager inputManager = inputManagers[i];
                inputManager.Update(gameTime);

                // Allows the game to exit.
                if (inputManager.GamePadState.Buttons.Back == ButtonState.Pressed ||
                    inputManager.KeyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }

                // Detection and record the current player's most recent move.
                Move newMove = moveList.DetectMove(inputManager);
                if (newMove != null)
                {
                    playerMoves[i] = newMove;
                    playerMoveTimes[i] = gameTime.TotalGameTime;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();            

            // Calculate some reasonable boundaries within the safe area.
            Vector2 topLeft = new Vector2(50, 50);
            Vector2 bottomRight = new Vector2(
                GraphicsDevice.Viewport.Width - topLeft.X,
                GraphicsDevice.Viewport.Height - topLeft.Y);

            // Keeps track of where to draw next.
            Vector2 position = topLeft;

            // Draw the list of all moves.
            foreach (Move move in moves)
            {
                Vector2 size = MeasureMove(move);

                // If this move would fall off the right edge of the screen,
                if (position.X + size.X > bottomRight.X)
                {
                    // start again on the next line.
                    position.X = topLeft.X;
                    position.Y += size.Y;                    
                }

                DrawMove(move, position);
                position.X += size.X + 30.0f;
            }

            // Skip some space.
            position.Y += 90.0f;

            // Draw the input from each player.
            for (int i = 0; i < inputManagers.Length; ++i)
            {
                position.X = topLeft.X;
                DrawInput(i, position);
                position.Y += 80;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the size of what would be drawn by a call to DrawMove.
        /// </summary>
        private Vector2 MeasureMove(Move move)
        {
            Vector2 textSize = spriteFont.MeasureString(move.Name);
            Vector2 sequenceSize = MeasureSequence(move.Sequence);
            return new Vector2(
                Math.Max(textSize.X, sequenceSize.X),
                textSize.Y + sequenceSize.Y);
        }

        /// <summary>
        /// Draws graphical instructions on how to perform a move.
        /// </summary>
        private void DrawMove(Move move, Vector2 position)
        {
            DrawString(move.Name, position, Color.White);
            position.Y += spriteFont.MeasureString(move.Name).Y;
            DrawSequence(move.Sequence, position);
        }

        /// <summary>
        /// Draws the input buffer and most recently fired action for a given player.
        /// </summary>
        private void DrawInput(int i, Vector2 position)
        {
            InputManager inputManager = inputManagers[i];
            Move move = playerMoves[i];

            // Draw the player's name and currently active move (if any).
            string text = "Player " + inputManager.PlayerIndex + " input  ";
            Vector2 textSize = spriteFont.MeasureString(text);
            DrawString(text, position, Color.White);
            if (move != null)
            {
                DrawString(move.Name,
                    new Vector2(position.X + textSize.X, position.Y), Color.Red);
            }
            
            // Draw the player's input buffer.
            position.Y += textSize.Y;
            DrawSequence(inputManager.Buffer, position);
        }

        /// <summary>
        /// Draws a string with a subtle drop shadow.
        /// </summary>
        private void DrawString(string text, Vector2 position, Color color)
        {            
            spriteBatch.DrawString(spriteFont, text,
                new Vector2(position.X, position.Y + 1), Color.Black);
            spriteBatch.DrawString(spriteFont, text,
                new Vector2(position.X, position.Y), color);
        }

        /// <summary>
        /// Calculates the size of what would be drawn by a call to DrawSequence.
        /// </summary>
        private Vector2 MeasureSequence(IEnumerable<Buttons> sequence)
        {
            float width = 0.0f;
            foreach (Buttons buttons in sequence)
            {
                width += MeasureButtons(buttons).X;
            }
            return new Vector2(width, padFaceTexture.Height);
        }

        /// <summary>
        /// Draws a horizontal series of input steps in a sequence.
        /// </summary>
        private void DrawSequence(IEnumerable<Buttons> sequence, Vector2 position)
        {
            foreach (Buttons buttons in sequence)
            {
                DrawButtons(buttons, position);
                position.X += MeasureButtons(buttons).X;
            }
        }

        /// <summary>
        /// Calculates the size of what would be drawn by a call to DrawButtons.
        /// </summary>
        private Vector2 MeasureButtons(Buttons buttons)
        {
            Buttons direction = Direction.FromButtons(buttons);

            float width;

            // If buttons has a direction,
            if (direction > 0)
            {
                width = GetDirectionTexture(direction).Width;
                // If buttons has at least one non-directional button,
                if ((buttons & ~direction) > 0)
                {
                    width += plusTexture.Width + padFaceTexture.Width;
                }
            }
            else
            {
                width = padFaceTexture.Width;
            }

            return new Vector2(width, padFaceTexture.Height);
        }

        /// <summary>
        /// Draws the combined state of a set of buttons flags. The rendered output
        /// looks like a directional arrow, a group of buttons, or both concatenated
        /// with a plus sign operator.
        /// </summary>
        private void DrawButtons(Buttons buttons, Vector2 position)
        {
            // Get the texture to draw for the direction.
            Buttons direction = Direction.FromButtons(buttons);
            Texture2D directionTexture = GetDirectionTexture(direction);            

            // If there is a direction, draw it.
            if (directionTexture != null)
            {
                spriteBatch.Draw(directionTexture, position, Color.White);
                position.X += directionTexture.Width;
            }

            // If any non-direction button is pressed,
            if ((buttons & ~direction) > 0)
            {
                // Draw a plus if both a direction and one more more buttons is pressed.
                if (directionTexture != null)
                {
                    spriteBatch.Draw(plusTexture, position, Color.White);
                    position.X += plusTexture.Width;
                }

                // Draw a gamepad with all inactive buttons in the background.
                spriteBatch.Draw(padFaceTexture, position, Color.White);

                // Draw each active button over the inactive game pad face.
                if ((buttons & Buttons.A) > 0)
                {
                    spriteBatch.Draw(aButtonTexture, position, Color.White);
                }
                if ((buttons & Buttons.B) > 0)
                {
                    spriteBatch.Draw(bButtonTexture, position, Color.White);
                }
                if ((buttons & Buttons.X) > 0)
                {
                    spriteBatch.Draw(xButtonTexture, position, Color.White);
                }
                if ((buttons & Buttons.Y) > 0)
                {
                    spriteBatch.Draw(yButtonTexture, position, Color.White);
                }
            }
        }

        /// <summary>
        /// Gets the texture for a given direction.
        /// </summary>
        private Texture2D GetDirectionTexture(Buttons direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return upTexture;
                case Direction.Down:
                    return downTexture;
                case Direction.Left:
                    return leftTexture;
                case Direction.Right:
                    return rightTexture;
                case Direction.UpLeft:
                    return upLeftTexture;
                case Direction.UpRight:
                    return upRightTexture;
                case Direction.DownLeft:
                    return downLeftTexture;
                case Direction.DownRight:
                    return downRightTexture;
                default:
                    return null;
            }
        }

        #endregion
    }

    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (InputSequenceSampleGame game = new InputSequenceSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}

