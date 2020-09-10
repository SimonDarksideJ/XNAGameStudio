#region File Description
//-----------------------------------------------------------------------------
// ScoreCard.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using GameStateManagement;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.GamerServices;
using YachtServices;
using Microsoft.Phone.Shell;
#endregion

namespace Yacht
{
    /// <summary>
    /// Possible score types.
    /// </summary>
    enum YachtCombination
    {
        Yacht = 12,
        LargeStraight = 11,
        SmallStraight = 10,
        FourOfAKind = 9,
        FullHouse = 8,
        Choise = 7,
        Sixes = 6,
        Fives = 5,
        Fours = 4,
        Threes = 3,
        Twos = 2,
        Ones = 1
    }

    /// <summary>
    /// A component which displays the current game state.
    /// </summary>
    class GameStateHandler
    {
        #region Constants


        public static string[] ScoreTypesNames = 
        { "Ones", "Twos", "Threes", "Fours", "Fives", "Sixes", "Choice",
            "Full House", "4 of a kind", "Small 1-5", "Large 2-6", "Yacht" };

        #endregion

        #region Fields and Properties


        /// <summary>
        /// Whether component card is initialized.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The game state which the component displays.
        /// </summary>
        public GameState State { get; private set; }

        /// <summary>
        /// Position used when rendering the game state.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Whether a score has been selected from the score card or not.
        /// </summary>
        public bool IsScoreSelect
        {
            get
            {
                return SelectedScore != null;
            }
        }

        /// <summary>
        /// The score that is currently selected.
        /// </summary>
        public YachtCombination? SelectedScore { get; private set; }

        /// <summary>
        /// The currently active player.
        /// </summary>
        public YachtPlayer CurrentPlayer
        {
            get
            {
                return players[State.CurrentPlayer];
            }
        }

        /// <summary>
        /// The player that has won the game, or null if the game has yet to end.
        /// </summary>
        public YachtPlayer WinnerPlayer { get; private set; }

        /// <summary>
        /// Whether the game is over.
        /// </summary>
        public bool IsGameOver { get; private set; }

        /// <summary>
        /// Is the game currently waiting for a remote player to act.
        /// </summary>
        public bool IsWaitingForPlayer { get; private set; }

        YachtPlayer[] players;

        ContentManager contentManager;

        Texture2D scoreCardTexture;
        Texture2D scoreLinesTexture;
        Texture2D leaderBoardTexture;
        Texture2D activeLeaderBoardTexture;
        Texture2D scrollThumbTexture;
        Texture2D starTexture;

        Vector2 scoreOffset = Vector2.Zero;
        Vector2[] scorePosition;
        Vector2[] playerPositions;
        Vector2 totalScore;

        Rectangle[] scoreLine;
        Dice[] currentDice;

        GameTypes type;
        DiceHandler diceHandler;
        InputState input;                
        string message;
        Button startWithAI;
        Rectangle scrollLineRectDestination = new Rectangle(0, 42, 232, 405);
        Rectangle screenBounds;


        #endregion

        #region Initializations



        /// <summary>
        /// Initialize a new game handler component.
        /// </summary>        
        /// <param name="diceHandler">The dice handler to use for managing the players' dice.</param>
        /// <param name="input">The <see cref="InputState"/> to check for touch input.</param>
        /// <param name="name">The name of the human player.</param>
        /// <param name="state">The game state to manage.</param>
        /// <param name="screenBounds">The screen's bounds.</param>
        /// <param name="contentManager">Content manager to use when initializing the player display.</param>
        public GameStateHandler(DiceHandler diceHandler, InputState input, string name, GameState state, 
            Rectangle screenBounds, ContentManager contentManager)
        {

            // Initialize members
            this.diceHandler = diceHandler;
            this.input = input;
            this.type = state == null ? GameTypes.Offline : state.GameType;
            this.screenBounds = screenBounds;
            this.contentManager = contentManager;

            if (state == null)
            {
                State = new GameState();
                LoadNewOfflinePlayers(name);
            }
            else
            {
                State = state;

                players = new YachtPlayer[State.Players.Count];

                HumanPlayer humanPlayer = new HumanPlayer(State.Players[0].Name, diceHandler, State.GameType, input, 
                    screenBounds);
                humanPlayer.LoadAssets(contentManager);
                humanPlayer.GameStateHandler = this;

                players[0] = humanPlayer;

                if (type == GameTypes.Offline)
                {
                    InitializeOfflinePlayers();
                }
                else
                {                    
                    IsWaitingForPlayer = !State.IsStarted;
                    InitializeOnlinePlayers();
                }

                Initialize(false);
            }
        }

        /// <summary>
        /// Loads assets used by the game state handler and performs other visual initializations.
        /// </summary>
        public void LoadAssets()
        {
            scoreCardTexture = contentManager.Load<Texture2D>(@"Images\NameAndTotal");
            scoreLinesTexture = contentManager.Load<Texture2D>(@"Images\Score");
            leaderBoardTexture = contentManager.Load<Texture2D>(@"Images\leaderboardBg");
            activeLeaderBoardTexture = contentManager.Load<Texture2D>(@"Images\leaderboardBg_active");
            Texture2D startWithAITexture = contentManager.Load<Texture2D>(@"Images\startBtn");
            scrollThumbTexture = contentManager.Load<Texture2D>(@"Images\ScrollThumb");
            starTexture = contentManager.Load<Texture2D>(@"Images\dot");

            startWithAI = new Button(startWithAITexture, new Vector2(
                        screenBounds.Width / 2 - startWithAITexture.Width / 2, 680), null, null);
            startWithAI.Click += startWithAI_Click;
        }

        /// <summary>
        /// Initialize the offline players.
        /// </summary>
        private void InitializeOfflinePlayers()
        {
            for (int i = 1; i < State.Players.Count; i++)
            {
                players[i] = new AIPlayer(State.Players[i].Name, diceHandler);
                players[i].GameStateHandler = this;
            }
        }

        /// <summary>
        /// Initialize the online players.
        /// </summary>
        private void InitializeOnlinePlayers()
        {
            // Wait for other players if the game has not started
            IsWaitingForPlayer = !State.IsStarted;

            for (int i = 0; i < players.Length; i++)
            {
                if (i < State.Players.Count)
                {
                    if (State.Players[i].PlayerID == NetworkManager.Instance.playerID)
                    {
                        HumanPlayer humanPlayer = new HumanPlayer(State.Players[i].Name, 
                            diceHandler, State.GameType, input, screenBounds);
                        humanPlayer.LoadAssets(contentManager);
                        players[i] = humanPlayer;

                        if (i != 0)
                        {
                            IsWaitingForPlayer = false;
                        }
                    }
                    else
                    {
                        players[i] = new NetworkPlayer(State.Players[i].Name, screenBounds);
                    }
                    players[i].GameStateHandler = this;
                }
            }
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        /// <param name="initializeScoreTable">True to initialize a new score card.</param>
        private void Initialize(bool initializeScoreTable)
        {
            LoadAssets();

            if (initializeScoreTable)
            {
                // Initialize the score card
                State.Players = new List<PlayerInformation>();
                for (int i = 0; i < players.Length; i++)
                {
                    byte[] score = new byte[12];
                    players[i].GameStateHandler = this;
                    for (int j = 0; j < 12; j++)
                    {
                        score[j] = ServiceConstants.NullScore;
                    }
                    State.Players.Add(new PlayerInformation() { Name = players[i].Name, ScoreCard = score });
                }
            }

            // Initialize the position of the score on the card
            scorePosition = new Vector2[ScoreTypesNames.Length];
            for (int i = 0; i < scorePosition.Length; i++)
            {
                scorePosition[i] = new Vector2(20, 50 + 42 * i);
            }

            totalScore = new Vector2(20, 445);


            // Initialize the player leader board position
            playerPositions = new Vector2[players.Length];
            for (int i = 0; i < playerPositions.Length; i++)
            {
                playerPositions[i] = new Vector2(screenBounds.Right - leaderBoardTexture.Width,
                    screenBounds.Top + 10 + (leaderBoardTexture.Height + 20) * i);
            }

            // Initialize the score line rectangle
            scoreLine = new Rectangle[scorePosition.Length];
            for (int i = 0; i < scorePosition.Length; i++)
            {
                scoreLine[i] = new Rectangle((int)scorePosition[i].X, (int)scorePosition[i].Y, 200, 42);
            }

            IsInitialized = true;
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Handle user input.
        /// </summary>
        /// <param name="sample">Input gesture.</param>
        public void HandleInput(GestureSample sample)
        {
            if (IsWaitingForPlayer)
            {
                startWithAI.HandleInput(sample);
            }

            // Handle the dragging of the score card.
            if (sample.GestureType == GestureType.VerticalDrag)
            {
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 5, (int)sample.Position.Y - 5, 10, 10);
                Rectangle scrollLineBounds = scoreLinesTexture.Bounds;
                scrollLineBounds.Y += 10;

                if (scrollLineBounds.Intersects(touchRect))
                {
                    scoreOffset.Y += sample.Delta.Y;
                }

                scoreOffset.Y = MathHelper.Clamp(scoreOffset.Y,
                    scrollLineRectDestination.Height - scrollLineBounds.Height, 0);
            }
        }

        /// <summary>
        /// Render the current game state.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawScore(spriteBatch);

            DrawLeaderBoard(spriteBatch);

            DrawMessage(spriteBatch);
        }

        /// <summary>
        /// Draw game messages.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        private void DrawMessage(SpriteBatch spriteBatch)
        {
            if (message != null)
            {
                Rectangle screenBounds = spriteBatch.GraphicsDevice.Viewport.Bounds;
                Vector2 measure = YachtGame.Font.MeasureString(message);
                Vector2 position = new Vector2(screenBounds.Center.X - measure.X / 2, screenBounds.Bottom - 300);
                spriteBatch.DrawString(YachtGame.Font, message, position, Color.White);
            }

            if (IsWaitingForPlayer)
            {
                startWithAI.Draw(spriteBatch);
                string text = "Waiting for other players to join";
                Rectangle screenBounds = spriteBatch.GraphicsDevice.Viewport.Bounds;
                Vector2 measure = YachtGame.Font.MeasureString(text);
                Vector2 position = new Vector2(screenBounds.Center.X - measure.X / 2, screenBounds.Bottom - 70);
                spriteBatch.DrawString(YachtGame.Font, text, position, Color.White);
            }
        }

        /// <summary>
        /// Helper method for drawing the score card and the current player's score.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        private void DrawScore(SpriteBatch spriteBatch)
        {
            // Draw the score card
            Rectangle sourceScrollLineRect = scrollLineRectDestination;
            sourceScrollLineRect.Y = 0;
            sourceScrollLineRect.Y -= (int)scoreOffset.Y;
            spriteBatch.Draw(scoreLinesTexture, scrollLineRectDestination, sourceScrollLineRect, Color.White);

            // Draw the score on the score card
            for (int i = 0; i < ScoreTypesNames.Length; i++)
            {
                Vector2 position = scorePosition[i] + scoreOffset;
                if (scrollLineRectDestination.Contains((int)position.X, (int)position.Y))
                {
                    // Draw the score names on the card
                    spriteBatch.DrawString(YachtGame.ScoreFont, ScoreTypesNames[i], position,
                        (YachtCombination)(i + 1) == SelectedScore ? Color.Red : Color.Black);
                }

                if (State.Players[State.CurrentPlayer].ScoreCard != null)
                {
                    string text = State.Players[State.CurrentPlayer].ScoreCard[i].ToString();
                    Color color = Color.Black;

                    // Write scores for score types which have been assigned a score
                    if (text == ServiceConstants.NullScore.ToString() && currentDice != null)
                    {
                        text = CombinationScore((YachtCombination)(i + 1), currentDice).ToString();

                        // The selected score should be highlighted in red
                        color = (YachtCombination)(i + 1) == SelectedScore ? Color.Red : Color.Gray;
                    }

                    // Draw all non-null scores which fit into the currently displayed portion of the score card
                    if (text != ServiceConstants.NullScore.ToString() &&
                        scrollLineRectDestination.Contains((int)position.X, (int)position.Y))
                    {
                        spriteBatch.DrawString(YachtGame.ScoreFont, text,
                            scorePosition[i] + new Vector2(160, 0) + scoreOffset, color);
                    }
                }
            }

            spriteBatch.Draw(scoreCardTexture, new Vector2(0, 10), Color.White);
            float scrollYPos = (scrollLineRectDestination.Height - scrollThumbTexture.Height) /
                (float)(scoreLinesTexture.Height - scrollLineRectDestination.Height) * scoreOffset.Y;
            spriteBatch.Draw(scrollThumbTexture, new Vector2(0, 45 - scrollYPos), Color.White);

            // Draw the name of the player at the top of the score card.
            spriteBatch.DrawString(YachtGame.ScoreFontBold,
                string.Format("#{0} {1}", (State.CurrentPlayer + 1).ToString(),
                players[State.CurrentPlayer].Name), new Vector2(10, 10), Color.Brown);

            // Get and draw the total score for the current player.
            spriteBatch.DrawString(YachtGame.ScoreFontBold, "Total",
                totalScore, Color.Brown);
            spriteBatch.DrawString(YachtGame.ScoreFontBold, State.Players[State.CurrentPlayer].TotalScore.ToString(),
                totalScore + new Vector2(160, 0), Color.Brown);
        }

        /// <summary>
        /// Helper method for drawing the leader board.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to draw textures and string on the screen.</param>
        private void DrawLeaderBoard(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < players.Length; i++)
            {
                // Draw the leader texture according to the player state (active or not)
                spriteBatch.Draw(i == State.CurrentPlayer ? activeLeaderBoardTexture : leaderBoardTexture,
                    playerPositions[i], Color.White);

                // Calculate the position where the player's name is to be drawn
                Vector2 measure = YachtGame.RegularFont.MeasureString(players[i].Name);
                Vector2 playerNamePosition = playerPositions[i] +
                    new Vector2(leaderBoardTexture.Bounds.Width * 3 / 5 - measure.X, 0);

                // Draw the player name
                spriteBatch.DrawString(YachtGame.RegularFont, players[i].Name, playerNamePosition, Color.White);

                // Calculate the player's total score and its drawing position
                string total = State.Players[i].TotalScore.ToString();
                measure = YachtGame.RegularFont.MeasureString(total);
                Vector2 totalScorePosition = playerPositions[i] +
                    new Vector2(leaderBoardTexture.Bounds.Width - measure.X - 20, 0);

                // Draw the total score according to the calculations
                spriteBatch.DrawString(YachtGame.LeaderScoreFont, total, totalScorePosition, Color.White);
                if (players[i] is HumanPlayer)
                {
                    spriteBatch.Draw(starTexture, playerNamePosition - new Vector2(20,-10), Color.White);
                }
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Write the selected score to the score table and move to the next player.
        /// Checks if the game is over and raises the GameOver event as well.
        /// </summary>
        public void FinishTurn()
        {
            // Check if a score was selected
            if (SelectedScore != null)
            {
                if (type == GameTypes.Offline)
                {
                    State.Players[State.CurrentPlayer].ScoreCard[(int)SelectedScore - 1] =
                        CombinationScore((YachtCombination)SelectedScore, currentDice);

                    // Accumulate the total score
                    State.Players[State.CurrentPlayer].TotalScore +=
                        State.Players[State.CurrentPlayer].ScoreCard[(int)SelectedScore - 1];

                    // Move to the next player
                    State.CurrentPlayer = (State.CurrentPlayer + 1) % players.Length;

                    // Accumulate the numbers of turns
                    State.StepsMade++;

                    // Check if all players play 12 turns (so all players have a full score card)
                    if (State.StepsMade == 12 * players.Length)
                    {
                        // Check who the winner is
                        State.CurrentPlayer = HighesPlayerScore();
                        WinnerPlayer = players[State.CurrentPlayer];

                        IsGameOver = true;

                        if (WinnerPlayer is HumanPlayer)
                        {
                            AudioManager.PlaySound("Winner");
                        }
                        else
                        {
                            AudioManager.PlaySound("Loss");
                        }
                    }
                    else
                    {
                        AudioManager.PlaySoundRandom("TurnChange", 2);
                    }                    
                }
                else
                {
                    // The server manages the game state, so simply send the move over
                    YachtStep currentMove = new YachtStep((int)SelectedScore - 1, 
                        CombinationScore((YachtCombination)SelectedScore, currentDice), State.CurrentPlayer, 
                        State.StepsMade);

                    State.CurrentPlayer = (State.CurrentPlayer + 1) % State.Players.Count;

                    NetworkManager.Instance.GameStep(currentMove);
                }

                // Clear the selected score
                SelectedScore = null;

                message = null;
            }
        }

        /// <summary>
        /// Sets the dice which are used to calculate the possible scores.
        /// </summary>
        /// <param name="dice">The dice to use in calculating possible scores.</param>
        public void SetScoreDice(Dice[] dice)
        {
            currentDice = dice;
        }

        /// <summary>
        /// Select a score line to serve as the user's score for the current turn.
        /// </summary>
        /// <param name="selectedScore">The score line to select.</param>
        /// <returns>If line was selected</returns>
        public bool SelectScore(YachtCombination? selectedScore)
        {
            if (selectedScore.HasValue &&
                State.Players[State.CurrentPlayer].ScoreCard[(int)selectedScore - 1] == ServiceConstants.NullScore &&
                currentDice != null)
            {
                SelectedScore = selectedScore;
                AudioManager.PlaySound("ScoreSelect");
                return true;
            }
            else if (selectedScore == null)
            {
                SelectedScore = null;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Checks whether a specified rectangle intersects a specified score line.
        /// </summary>
        /// <param name="rectangle">Rectangle to check for intersection.</param>
        /// <param name="index">Index of the score line to check.</param>
        /// <returns>True if the rectangle intersects the score line specified.</returns>
        public bool IntersectLine(Rectangle rectangle, int index)
        {
            rectangle.Y -= (int)scoreOffset.Y;
            return scoreLine[index].Intersects(rectangle);
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Check which player has the highest score.
        /// </summary>
        /// <returns>The index of the player who has the highest total score.</returns>
        private int HighesPlayerScore()
        {
            int playerIndex = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (AccumulateScore(i) > AccumulateScore(playerIndex))
                {
                    playerIndex = i;
                }
            }

            return playerIndex;
        }

        /// <summary>
        /// Calculate the total score of a player.
        /// </summary>
        /// <param name="playerIndex">The index of the player for whom the score should be calculated.</param>
        /// <returns>The total score of the given player.</returns>
        private int AccumulateScore(int playerIndex)
        {
            int total = 0;
            for (int i = 0; i < 12; i++)
            {
                if (State.Players[playerIndex].ScoreCard[i] != ServiceConstants.NullScore)
                {
                    total += State.Players[playerIndex].ScoreCard[i];
                }
            }
            return total;
        }

        /// <summary>
        /// Return a new initialized score card.
        /// </summary>
        /// <returns>A new score card, which is an array that can hold 12 scores.</returns>
        private byte[] NewScoreCard()
        {
            byte[] score = new byte[12];
            for (int i = 0; i < score.Length; i++)
            {
                score[i] = ServiceConstants.NullScore;
            }
            return score;
        }



        /// <summary>
        /// Load offline players on new game
        /// </summary>
        /// <param name="name"></param>
        private void LoadNewOfflinePlayers(string name)
        {

            // Load and initialize players.
            players = new YachtPlayer[] 
            {
                new HumanPlayer(name, diceHandler, State.GameType, input, screenBounds),
                new AIPlayer("Josh", diceHandler),
                new AIPlayer("Charles", diceHandler),
                new AIPlayer("Alex", diceHandler)
            };

            (players[0] as HumanPlayer).LoadAssets(contentManager);

            Initialize(true);
        }


        #endregion

        #region Event Handlers


        /// <summary>
        /// Handle the "Click" event of the "Start with AI" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void startWithAI_Click(object sender, EventArgs e)
        {
            IsWaitingForPlayer = false;
            // Reset timeout on the server.
            NetworkManager.Instance.ResetTimeout();
        }


        #endregion

        #region Static Methods


        /// <summary>
        /// Calculate the score of the supplied dice according to a specified combination.
        /// </summary>
        /// <param name="combination">The score combination to consider when calculating the dice score.</param>
        /// <param name="dice">An array of five dice. Some of the array members may be null.</param>
        /// <returns>The combination score for the supplied dice.</returns>
        public static byte CombinationScore(YachtCombination combination, Dice[] dice)
        {
            // Make sure the five dice are supplied
            if (dice.Length != 5)
            {
                throw new ArgumentException("The array must contain five members", "dice");
            }

            // Sort the dice.
            dice = (Dice[])dice.Clone();
            Array.Sort(dice);

            // Get the first and the last dice for calculation.
            Dice first = First(dice);
            Dice last = Last(dice);
            switch (combination)
            {
                case YachtCombination.Yacht:
                    // If all dice are the same
                    if (first != null && last != null)
                    {
                        if (Times(dice, first.Value) == 5)
                        {
                            return 50;
                        }
                    }
                    return 0;
                case YachtCombination.LargeStraight:
                    // Dice are 2-6
                    if (first != null && last != null)
                    {
                        if (CheckConsecutiveDice(dice) && last.Value == DiceValue.Six)
                        {
                            return 30;
                        }
                    }
                    return 0;
                case YachtCombination.SmallStraight:
                    // Dice are 1-5
                    if (first != null && last != null)
                    {
                        if (CheckConsecutiveDice(dice) && last.Value == DiceValue.Five)
                        {
                            return 30;
                        }
                    }
                    return 0;
                case YachtCombination.FourOfAKind:
                    // 4 dice are identical
                    if (first != null && last != null)
                    {
                        if (Times(dice, first.Value) >= 4 || Times(dice, last.Value) >= 4)
                        {
                            return Sum(dice, null);
                        }
                    }
                    return 0;
                case YachtCombination.FullHouse:
                    // There is a pair of identical dice, and all other dice have an identical yet different value
                    if (first != null && last != null)
                    {
                        if ((Times(dice, first.Value) == 3 && Times(dice, last.Value) == 2) ||
                        (Times(dice, first.Value) == 2 && Times(dice, last.Value) == 3))
                        {
                            return Sum(dice, null);
                        }
                    }
                    return 0;
                case YachtCombination.Choise:
                    return Sum(dice, null);
                case YachtCombination.Sixes:
                case YachtCombination.Fives:
                case YachtCombination.Fours:
                case YachtCombination.Threes:
                case YachtCombination.Twos:
                case YachtCombination.Ones:
                    // Calculate the sum of the selected die faces.
                    return Sum(dice, (DiceValue)(int)combination);
                default:
                    throw new Exception("Cannot calculate Value for this combination.");
            }
        }



        /// <summary>
        /// Get the first non-null die from a dice array.
        /// </summary>
        /// <param name="dice">The array from which to return the first non-null die.</param>
        /// <returns>The first non-null die or null if all dice are null.</returns>
        static Dice First(Dice[] dice)
        {
            for (int i = 0; i < dice.Length; i++)
            {
                if (dice[i] != null)
                {
                    return dice[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the last non-null die from a dice array.
        /// </summary>
        /// <param name="dice">The array from which to return the last non-null die.</param>
        /// <returns>The last non-null die or null if all dice are null.</returns>
        static Dice Last(Dice[] dice)
        {
            for (int i = dice.Length - 1; i > 0; i--)
            {
                if (dice[i] != null)
                {
                    return dice[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Calculate the sum of a set of dice, only taking specific dice into account, optionally.
        /// </summary>
        /// <param name="dice">The dice to calculate the sum of.</param>
        /// <param name="value">Null to sum all dice, or a specific value to sum only the dice with that value.</param>
        /// <returns>The sum of the dice.</returns>
        static byte Sum(Dice[] dice, DiceValue? value)
        {
            byte sum = 0;

            for (int i = 0; i < dice.Length; i++)
            {
                if (dice[i] != null && (!value.HasValue || dice[i].Value == value))
                {
                    sum += (byte)dice[i].Value;
                }
            }

            return sum;
        }

        /// <summary>
        /// Calculate how many dice are in a given dice array, only taking dice with specific values into account,
        /// optionally.
        /// </summary>
        /// <param name="dice">A dice array to count the dice of.</param>
        /// <param name="value">Null to count all dice, or a specific value to count only the dice with 
        /// that value.</param>
        /// <returns>The amount of dice in the array.</returns>
        static int Times(Dice[] dice, DiceValue? value)
        {
            int count = 0;

            for (int i = 0; i < dice.Length; i++)
            {
                if (dice[i] != null && (!value.HasValue || dice[i].Value == value))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Check an array of dice contains a set of dice with consecutive values.
        /// </summary>
        /// <param name="dice">The array of dice.</param>
        /// <returns>True if the dice are consecutive and false otherwise.</returns>
        /// <remarks>Assumes that the dice are ordered in an ascending order.</remarks>
        static bool CheckConsecutiveDice(Dice[] dice)
        {
            int count = 0;

            for (int i = 0; i < dice.Length - 1; i++)
            {
                if (dice[i] != null && dice[i + 1] != null && dice[i].Value + 1 == dice[i + 1].Value)
                {
                    count++;
                }
            }

            return count == dice.Length - 1;
        }


        #endregion

        /// <summary>
        /// Sets the game state handler's state to match a supplied game state.
        /// </summary>
        /// <param name="state">The game state to make the game state handler match.</param>
        public void SetState(GameState state)
        {
            State.StepsMade = state.StepsMade;
            State.IsStarted = state.IsStarted;
            State.CurrentPlayer = state.CurrentPlayer;
            for (int i = 0; i < players.Length; i++)
            {
                players[i].Name = state.Players[i].Name;
                State.Players[i].Name = state.Players[i].Name;
                State.Players[i].TotalScore = state.Players[i].TotalScore;
                if (i == 0 && players[i] is HumanPlayer)
                {
                    IsWaitingForPlayer = !State.IsStarted;
                }
            }
        }


        public void UpdateScoreCard(byte[] scoreCard)
        {
            players[State.CurrentPlayer].GameStateHandler = this;
            State.Players[State.CurrentPlayer].ScoreCard = scoreCard;
        }

        /// <summary>
        /// Shows an appropriate prompt once the game is over.
        /// </summary>
        /// <param name="endGameState">Information regarding the winning player.</param>
        public void ShowGameOver(EndGameInformation endGameState)
        {
            for (int i = 0; i < State.Players.Count; i++)
            {
                if (State.Players[i].PlayerID == endGameState.PlayerID)
                {
                    State.CurrentPlayer = i;
                    State.Players[i].ScoreCard = endGameState.ScoreCard;
                    WinnerPlayer = players[i];

                    IsGameOver = true;

                    return;
                }
            }
        }
    }
}
