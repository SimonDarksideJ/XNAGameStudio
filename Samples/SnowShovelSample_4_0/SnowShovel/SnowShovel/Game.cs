//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

#if WINDOWS_PHONE
using Microsoft.Devices.Sensors;
#endif

namespace SnowShovel
{

    /// <summary>
    /// SnowShovel is a simple game that illustrates use of the accelerometer.
    /// Tilt the phone or use the arrow keys to pick up snowflakes with the shovel
    /// before time runs out.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        /// the snowflake class encapsulates data for drawing
        private class Snowflake
        {
            static Random rand = new Random();
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float AngularVelocity;
            public int TextureIndex;
            public Color Tint;

            /// <summary>
            /// Create a new snowflake with a random position, velocity, and spritesheet index
            /// </summary>
            public Snowflake(Vector2 postion, Vector2 velocity, int index)
            {
                Position = postion;
                Velocity = velocity;
                TextureIndex = index;

                Scale = 1.0f - (float)(0.5 * rand.NextDouble());
                AngularVelocity = (float)rand.NextDouble() * 0.05f;

                Tint.A = 255;
                Tint.R = (byte)(255 - (40.0 * rand.NextDouble()));
                Tint.G = (byte)(255 - (40.0 * rand.NextDouble()));
                Tint.B = (byte)(255 - (20.0 * rand.NextDouble()));
            }

            /// <summary>
            /// Update the snowflake's position
            /// </summary>
            public void Update(Rectangle screen)
            {
                Position += Velocity;

                if (Position.X < 0)
                {
                    Velocity.X = -Velocity.X;
                    Position.X = 0f;
                }
                else if (Position.X > screen.Width)
                {
                    Velocity.X = -Velocity.X;
                    Position.X = screen.Width;
                }

                if (Position.Y < 0)
                {
                    Velocity.Y = -Velocity.Y;
                    Position.Y = 0f;
                }
                else if (Position.Y > screen.Height)
                {
                    Velocity.Y = -Velocity.Y;
                    Position.Y = screen.Height;
                }

                // this is just for visuals, no need for bounds check
                Rotation += AngularVelocity;
            }
        }

        /// <summary>
        /// the game has 3 simple states: the pre-game menu, playing the game, and the game-over screen
        /// </summary>
        enum GameState
        {
            PreGame,
            Game,
            PostGame
        }

        GameState gameState;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        TouchCollection touchCollection;
        GamePadState gamepadState;
        KeyboardState keyboardState;

        Texture2D shovelTexture;
        Texture2D snowTexture;

        Vector2 shovelPosition;
        Vector2 shovelVelocity;
        float shovelRotation;

        Rectangle worldRect;
        Matrix worldToScreenMatrix;

        SpriteFont titleFont;
        SpriteFont scoreFont;

        SoundEffect sound;

        Random rand;

        List<Snowflake> snowFlakes = new List<Snowflake>();

        TimeSpan timeRemaining;
        TimeSpan timeElapsed;

        const int pointsPerSnowflake = 100;

        int score;
        int nextWaveSnowflakeCount;
        int nextWaveMilliseconds;

#if WINDOWS_PHONE
        Accelerometer Accelerometer;
        Vector3 CurrentAccelerometerReading;
        SensorState CurrentAccelerometerState;
#endif


#if WINDOWS_PHONE
        /// <summary>
        /// Callback when the accelerometer has new data
        /// </summary>
        public void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            CurrentAccelerometerReading.X = (float)e.X;
            CurrentAccelerometerReading.Y = (float)e.Y;
            CurrentAccelerometerReading.Z = (float)e.Z;
        }

#endif

        /// <summary>
        /// Initialize the Game
        /// </summary>
        public Game()
        {
            rand = new Random();

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // on the phone, we want to draw on top of the notification bar,
            // but running in a window on Windows is a better experience.
            graphics.IsFullScreen = true;
#endif

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if WINDOWS_PHONE
            // start the accelerometer if it's supported and hook up the event handler
            Accelerometer = new Accelerometer();
            if (Accelerometer.State == SensorState.Ready)
            {
                Accelerometer.ReadingChanged += AccelerometerReadingChanged;
                Accelerometer.Start();
                CurrentAccelerometerState = SensorState.Ready;
            }
#endif
            // the original art was designed for Zune HD, so for now we'll
            // keep art & gameplay at HD resolution and scale up for Windows Phone
            worldRect = new Rectangle(0, 0, 272, 480);

            worldToScreenMatrix = Matrix.CreateScale(
                (float)GraphicsDevice.Viewport.Width / (float)worldRect.Width,
                (float)GraphicsDevice.Viewport.Height / (float)worldRect.Height,
                1);

            SetGameState(GameState.PreGame);   

            base.Initialize();
        }

        /// <summary>
        /// Move from one game state to the next
        /// </summary>
        private void SetGameState(GameState state)
        {
            gameState = state;

            snowFlakes.Clear();

            switch (state)
            {
                case GameState.PreGame:
                    score = 0;
                    timeElapsed = TimeSpan.Zero;
                    nextWaveMilliseconds = 10000;
                    nextWaveSnowflakeCount = 5;
                    shovelPosition.X = worldRect.Center.X;
                    shovelPosition.Y = worldRect.Center.Y;
                    shovelVelocity.X = 0;
                    shovelVelocity.Y = 0;
                    break;

                case GameState.Game:
                    timeRemaining = TimeSpan.FromSeconds(10);
                    break;

                case GameState.PostGame:
                    timeRemaining = TimeSpan.Zero;
                    break;
            }

            // every game state change is followed by a wave of snowflakes
            Snow(nextWaveSnowflakeCount);
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load the graphics, sound and fonts
            shovelTexture = Content.Load<Texture2D>("shovel");
            snowTexture = Content.Load<Texture2D>("snowflakes");

            titleFont = Content.Load<SpriteFont>("TitleFont");
            scoreFont = Content.Load<SpriteFont>("ScoreFont");

            sound = Content.Load<SoundEffect>("plink");
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            touchCollection = TouchPanel.GetState();
            gamepadState = GamePad.GetState(PlayerIndex.One);
#if WINDOWS_PHONE
            keyboardState = new KeyboardState();
#else
            keyboardState = Keyboard.GetState();
#endif

            // Allows the game to exit from any state
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (timeRemaining.TotalMilliseconds > 0)
            {
                timeElapsed += gameTime.ElapsedGameTime;
                timeRemaining -= gameTime.ElapsedGameTime;
            }

            // all the game states can have snowflakes, so do the update here
            foreach (Snowflake snowflake in snowFlakes)
            {
                snowflake.Update(worldRect);
            }

            // next do a per state update
            switch (gameState)
            {
                case GameState.PreGame:
                    UpdatePreGame();
                    break;

                case GameState.PostGame:
                    UpdatePostGame();
                    break;

                case GameState.Game:
                    UpdateGame(gameTime);
                    break;

                default:
                    throw new NotImplementedException();
            }

            base.Update(gameTime);
        }

        // handle the pre-game menu and start
        private void UpdatePreGame()
        {
            if (touchCollection.Count > 0 || 
                gamepadState.IsButtonDown(Buttons.A) || 
                keyboardState.IsKeyDown(Keys.Space))
            {
                SetGameState(GameState.Game);
            }
        }

        // show the score, end the game
        private void UpdatePostGame()
        {
            if ((touchCollection.Count > 0 && touchCollection[0].State == TouchLocationState.Pressed) ||
                gamepadState.IsButtonDown(Buttons.A) ||
                keyboardState.IsKeyDown(Keys.Space))
            {
                SetGameState(GameState.PreGame);
            }
        }

        // game play
        private void UpdateGame(GameTime gameTime)
        {
            if (timeRemaining < TimeSpan.Zero)
            {
                // clean up and exit
                SetGameState(GameState.PostGame);
                return;
            }

            if (snowFlakes.Count == 0)
            {
                // next wave!
                // each wave gets additional snowflakes and a shorter bonus time,
                // down to a minumum of half a second
                nextWaveSnowflakeCount += 5;

                if (nextWaveMilliseconds > 500)
                {
                    nextWaveMilliseconds -= 500;
                }

                timeRemaining += TimeSpan.FromMilliseconds(nextWaveMilliseconds);

                Snow(nextWaveSnowflakeCount);
            }

            // handle input for keyboard, accelerometer, or gamepad
            float pixelSeconds = 10.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float accelX = gamepadState.ThumbSticks.Left.X;
            float accelY = -gamepadState.ThumbSticks.Left.Y;

            if (touchCollection.Count > 0)
            {
                Vector2 shovelPositionScreen = Vector2.Transform(shovelPosition, worldToScreenMatrix);
                Vector2 touchDifference = touchCollection[0].Position - shovelPositionScreen;
                accelX = MathHelper.Clamp(touchDifference.X / GraphicsDevice.Viewport.Width * 2.0f, -1.0f, 1.0f);
                accelY = MathHelper.Clamp(touchDifference.Y / GraphicsDevice.Viewport.Height * 2.0f, -1.0f, 1.0f);
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                accelX = -1;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                accelX = 1;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                accelY = -1;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                accelY = 1;
            }

#if WINDOWS_PHONE            
            if (CurrentAccelerometerState == SensorState.Ready)
            {
                accelX = CurrentAccelerometerReading.X;
                accelY = -CurrentAccelerometerReading.Y; // need to flip the y-value
            }
#endif

            shovelVelocity.X += accelX * pixelSeconds;
            shovelVelocity.Y += accelY * pixelSeconds;

            shovelPosition.X += shovelVelocity.X;
            shovelPosition.Y += shovelVelocity.Y;

            // make the shovel scoop face the direction of acceleration, or the last
            // known direction if acceleration is zero
            if (accelX != 0 || accelY != 0)
            {
                shovelRotation = (float)((Math.PI * 2.0) - Math.Atan2(accelX, accelY));
            }

            // clamp X position and velocity to the screen bounds.
            if (shovelPosition.X < 0)
            {
                shovelVelocity.X = 0;
                shovelPosition.X = 0;
            }
            else if (shovelPosition.X > worldRect.Width)
            {
                shovelVelocity.X = 0;
                shovelPosition.X = worldRect.Width;
            }

            // clamp y position and velocity to the screen bounds
            if (shovelPosition.Y < 0)
            {
                shovelVelocity.Y = 0;
                shovelPosition.Y = 0;
            }
            else if (shovelPosition.Y > worldRect.Height)
            {
                shovelVelocity.Y = 0;
                shovelPosition.Y = worldRect.Height;
            }

            // can't erase a list item while traversing, so use a remove list
            List<Snowflake> removeList = new List<Snowflake>();

            foreach (Snowflake snowflake in snowFlakes)
            {
                // Ideally we'd take shovel rotation into account and remove
                // a snowflake when you hit it with the scoop end.  But this is
                // a quick & cheap hit-test
                Rectangle shovelRect = new Rectangle(
                    (int)shovelPosition.X - shovelTexture.Width / 2, 
                    (int)shovelPosition.Y - shovelTexture.Height / 2, 
                    shovelTexture.Width, 
                    shovelTexture.Height);
                
                if (shovelRect.Contains((int)snowflake.Position.X, (int)snowflake.Position.Y))
                {
                    score += pointsPerSnowflake;
                    sound.Play();

                    removeList.Add(snowflake);
                }
            }

            foreach (Snowflake snowflake in removeList)
            {
                snowFlakes.Remove(snowflake);
            }

            removeList.Clear();
        }

        // generate new snowflakes
        private void Snow(int snowflakeCount)
        {
            for (int i = 0; i < snowflakeCount; i++)
            {
                Snowflake snowflake = new Snowflake(
                    // snow within bounds, please
                    new Vector2((float)rand.NextDouble() * worldRect.Width, (float)rand.NextDouble() * worldRect.Height),
                    // start with a velocity of -1.0 to 1.0
                    new Vector2((float)rand.NextDouble() * 2.0f - 1.0f, (float)rand.NextDouble() * 2.0f - 1.0f),
                    // MAGIC NUMBER there are 5 sprites in the snowflake sprite sheet
                    rand.Next(5));

                snowFlakes.Add(snowflake);
            }
        }

        // draws text with 1-pixel drop shadow
        private void DrawStringHelper(SpriteBatch batch, SpriteFont font, string text, int x, int y, Color color)
        {
            batch.DrawString(font, text, new Vector2(x + 1, y + 1), Color.Black);
            batch.DrawString(font, text, new Vector2(x, y), color);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                SpriteSortMode.Immediate, 
                BlendState.NonPremultiplied,
                null,
                null,
                null,
                null,
                worldToScreenMatrix);

            // draw my shovel
            spriteBatch.Draw(shovelTexture,
                shovelPosition,
                null,
                Color.White,
                shovelRotation,
                new Vector2(shovelTexture.Width / 2, shovelTexture.Height / 2),
                1.0f,
                SpriteEffects.None,
                0);

            // and all the snow
            foreach (Snowflake snowflake in snowFlakes)
            {
                spriteBatch.Draw(snowTexture,
                    snowflake.Position,
                    new Rectangle(snowflake.TextureIndex * snowTexture.Height, 0, snowTexture.Height, snowTexture.Height),
                    snowflake.Tint,
                    snowflake.Rotation,
                    new Vector2(snowTexture.Height / 2, snowTexture.Height / 2),
                    snowflake.Scale,
                    SpriteEffects.None,
                    0);
            }

            // draw the text
            Vector2 stringDimensions;

            DrawStringHelper(spriteBatch, titleFont, "Snow Shovel", 0, 0, Color.Wheat);

            string timeRemainingString = string.Format("Time: {0:00}:{1:00.0}", timeRemaining.Minutes, timeRemaining.Seconds + timeRemaining.Milliseconds / 1000.0f);

            // the time remaining text gets more red the closer to zero it is
            Color timeColor = Color.White;
            if (timeRemaining.TotalMilliseconds < 10000)
            {
                timeColor.B = (byte)(255 * timeRemaining.TotalMilliseconds / 10000);
                timeColor.G = (byte)(255 * timeRemaining.TotalMilliseconds / 10000);
            }

            DrawStringHelper(spriteBatch, scoreFont, timeRemainingString, 0, 30, timeColor);

            string scoreString = "Score: " + score.ToString();
            stringDimensions = scoreFont.MeasureString(scoreString);
            DrawStringHelper(spriteBatch, scoreFont, scoreString, (int)(worldRect.Width - stringDimensions.X), 30, Color.White);

            string timeElapsedString = string.Format("Elapsed Time: {0:00}:{1:00.0}", timeElapsed.Minutes, timeElapsed.Seconds + timeElapsed.Milliseconds / 1000.0f);
            stringDimensions = scoreFont.MeasureString(timeElapsedString);
            DrawStringHelper(spriteBatch, scoreFont, timeElapsedString, (int)(worldRect.Width - stringDimensions.X) / 2, worldRect.Height - (int)stringDimensions.Y, Color.White);

            switch (gameState)
            {
                case GameState.PreGame:
                    {
                        string[] instructionStrings = { "Shovel snow before", "time runs out!", "Tap screen to start" };

                        for (int i = 0; i < instructionStrings.Length; i++)
                        {
                            stringDimensions = scoreFont.MeasureString(instructionStrings[i]);
                            DrawStringHelper(spriteBatch, scoreFont, instructionStrings[i], (int)(worldRect.Width - stringDimensions.X) / 2, 100 + i * 30, Color.White);
                        }
                    }
                    break;

                case GameState.PostGame:
                    {
                        string[] instructionStrings = { "GAME OVER", "Old Man Winter has Pwned you", "Tap screen to restart" };

                        for (int i = 0; i < instructionStrings.Length; i++)
                        {
                            stringDimensions = scoreFont.MeasureString(instructionStrings[i]);
                            DrawStringHelper(spriteBatch, scoreFont, instructionStrings[i], (int)(worldRect.Width - stringDimensions.X) / 2, 100 + i * 30, Color.Aqua);
                        }
                    }
                    break;

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
