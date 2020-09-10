#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Minjie
{
    /// <summary>
    /// This screen implements the actual game logic.
    /// </summary>
    /// <remarks>Based on a similar class in the Game State Management sample.</remarks>
    class GameplayScreen : GameScreen
    {
        #region Constant Data


        /// <summary>
        /// The size of the game board.
        /// </summary>
        const int boardSize = 10;

        /// <summary>
        /// The size of the tile model.
        /// </summary>
        const float tileSize = 40.2f;

        /// <summary>
        /// The starting height of the pieces over the board.
        /// </summary>
        const float startingPieceHeight = 50f;

        /// <summary>
        /// The number of frames in the falling-piece animation.
        /// </summary>
        const int fallingPieceFrames = 10;


        #endregion
        
        #region Fields


        ContentManager content;
        Texture2D backgroundTexture;
        Model tileModel, highlightTileModel;
        Model whitePieceModel, blackPieceModel;
        Model whitePieceTileModel, blackPieceTileModel;
        Vector3 boardPosition;
        int fallingPieceFrame = 0;
        int lastPieceCount = 0;

        Board board = new Board(boardSize);
        Player player1, player2;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(bool multiplayer)
        {
            board.Initialize();
            lastPieceCount = board.PieceCount;

            player1 = new LocalPlayer(BoardColors.White, PlayerIndex.One, boardSize);
            if (multiplayer)
            {
                player2 = new LocalPlayer(BoardColors.Black, PlayerIndex.Two,
                    boardSize);
            }
            else
            {
                player2 = new AiPlayer(BoardColors.Black);
            }

            float boardPositionValue = -0.5f * tileSize * (float)boardSize + 
                tileSize / 2f;
            boardPosition = new Vector3(boardPositionValue, 1f, boardPositionValue);

            AudioManager.PlayMusic("Music_Game");
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = content.Load<Texture2D>("Gameplay/game_screen");
            tileModel = content.Load<Model>("Gameplay//tile");
            highlightTileModel = content.Load<Model>("Gameplay//tile_highlight");
            whitePieceModel = content.Load<Model>("Gameplay//p2_piece");
            blackPieceModel = content.Load<Model>("Gameplay//p1_piece");
            whitePieceTileModel = content.Load<Model>("Gameplay//p2_piece_tile");
            blackPieceTileModel = content.Load<Model>("Gameplay//p1_piece_tile");

            // reset the camera
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            SimpleArcCamera.Reset((float)viewport.Width / (float)viewport.Height);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                SimpleArcCamera.Update(gameTime);
                if (fallingPieceFrame > 0)
                {
                    fallingPieceFrame--;
                    if (fallingPieceFrame <= 0)
                    {
                        AudioManager.PlayCue("Drop");
                    }
                }
                if (fallingPieceFrame <= 0)
                {
                    player1.Update(board);
                    if (board.PieceCount > lastPieceCount)
                    {
                        lastPieceCount = board.PieceCount;
                        fallingPieceFrame = fallingPieceFrames;
                    }
                }
                if (fallingPieceFrame <= 0)
                {
                    player2.Update(board);
                    if (board.PieceCount > lastPieceCount)
                    {
                        lastPieceCount = board.PieceCount;
                        fallingPieceFrame = fallingPieceFrames;
                    }
                }
                if (board.GameOver)
                {
                    GameResult gameResult = GameResult.Tied;
                    if (board.WhitePieceCount > board.BlackPieceCount)
                    {
                        gameResult = GameResult.Player1Won;
                    }
                    else if (board.BlackPieceCount > board.WhitePieceCount)
                    {
                        gameResult = GameResult.Player2Won;
                    }
                    ExitScreen();
                    ScreenManager.AddScreen(new GameOverScreen(gameResult));
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.ExitGame)
            {
                ExitScreen();
                ScreenManager.AddScreen(new TitleScreen());
            }

            // update the input state for local players and the camera
            SimpleArcCamera.InputState = input;
            LocalPlayer.InputState = input;
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(Color.White);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

            ScreenManager.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Point cursorPosition = LocalPlayer.CursorPosition;
            for (int row = 0; row < boardSize; row++)
            {
                for (int column = 0; column < boardSize; column++)
                {
                    if ((fallingPieceFrame > 0) && (board.LastMove.Row == row) &&
                        (board.LastMove.Column == column))
                    {
                        int fall = fallingPieceFrames - fallingPieceFrame;
                        float height = startingPieceHeight - (fall + fall * fall / 2);
                        // do not let the piece fall through the board
                        if (height < 6)
                        {
                            height = 6;
                        }
                        DrawModel(board.CurrentColor == BoardColors.White ?
                            blackPieceModel : whitePieceModel, row, column, height);
                        DrawModel(tileModel, row, column, 0f);
                    }
                    else
                    {
                        if ((cursorPosition.X == row) && (cursorPosition.Y == column))
                        {
                            if (board[row, column] == BoardColors.Empty)
                            {
                                DrawModel(highlightTileModel, row, column, 0f);
                            }
                            DrawModel(board.CurrentColor == BoardColors.White ?
                                whitePieceModel : blackPieceModel, row, column,
                                startingPieceHeight);
                        }
                        else
                        {
                            DrawModel(tileModel, row, column, 0f);
                        }
                        if (board[row, column] == BoardColors.Black)
                        {
                            DrawModel(blackPieceTileModel, row, column, 0f);
                        }
                        else if (board[row, column] == BoardColors.White)
                        {
                            DrawModel(whitePieceTileModel, row, column, 0f);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Draw the given model at the appropriate board position.
        /// </summary>
        /// <param name="model">The model to draw.</param>
        /// <param name="row">The board row.</param>
        /// <param name="column">The board column.</param>
        /// <param name="height">The height above the board.</param>
        /// <remarks>
        /// We know that none of these models have nontrivial transforms, 
        /// so we can skip the bone transform code.
        /// </remarks>
        private void DrawModel(Model model, int row, int column, float height)
        {
            // if the model is null, fail silently - given nothing to draw
            if (model == null)
            {
                return;
            }

            Matrix worldMatrix = Matrix.CreateTranslation(boardPosition +
                new Vector3(row * tileSize, 1f + height, column * tileSize));
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                foreach (BasicEffect basicEffect in modelMesh.Effects)
                {
                    basicEffect.EnableDefaultLighting();
                    basicEffect.View = SimpleArcCamera.ViewMatrix;
                    basicEffect.Projection = SimpleArcCamera.ProjectionMatrix;
                    basicEffect.World = worldMatrix;
                }
                modelMesh.Draw();
            }
        }


        #endregion
    }
}
