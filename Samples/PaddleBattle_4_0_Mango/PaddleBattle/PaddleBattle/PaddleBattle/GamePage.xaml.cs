using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace PaddleBattle
{
    public partial class GamePage : PhoneApplicationPage
    {
        ContentManager content;
        GameTimer timer;
        SpriteBatch spriteBatch;
        Random random = new Random();

        // Blank texture used to draw the center line of the play area
        Texture2D blank;

        // The settings for the game
        GameSettings settings;

        // UIElementRenderer allows us to draw Silverlight UI into a Texture2D, which we can then draw
        // using SpriteBatch to composite UI on top of our game
        UIElementRenderer uiRenderer;

        // Sound effects that play when the ball bounces off something or a player scores
        SoundEffect plink;
        SoundEffect score;

        // The two paddles and the ball
        Paddle playerPaddle = new Paddle(false);
        Paddle computerPaddle = new Paddle(true);
        Ball ball = new Ball();

        // The scores for the game
        int playerScore = 0;
        int computerScore = 0;

        public GamePage()
        {
            InitializeComponent();

            // Get the content manager from the application
            content = (Application.Current as App).Content;

            // Get the settings from the application;
            settings = (Application.Current as App).Settings;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

            // We use the LayoutUpdated event in order to handle creation of the UIElementRenderer
            LayoutUpdated += new EventHandler(GamePage_LayoutUpdated);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            // Create the blank texture
            blank = new Texture2D(SharedGraphicsDeviceManager.Current.GraphicsDevice, 1, 1);
            blank.SetData(new[] { Color.White });

            // Load the sound effects
            plink = content.Load<SoundEffect>("plink");
            score = content.Load<SoundEffect>("score");

            // Load our three objects
            ball.LoadContent(content);
            playerPaddle.LoadContent(content);
            computerPaddle.LoadContent(content);
            
            // Center each paddle on their respective sides
            playerPaddle.CenterAtLocation(new Vector2(100, 240));
            computerPaddle.CenterAtLocation(new Vector2(700, 240));

            // Reset the ball
            ResetBall();

            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Helper that plays a sound, respective the game options
        /// </summary>
        private void PlaySound(SoundEffect sound)
        {
            if (settings.PlaySounds)
                sound.Play();
        }

        /// <summary>
        /// Resets the ball to the center of the screen and generates a new velocity for it.
        /// </summary>
        private void ResetBall()
        {
            // Put the ball in the center of the screen
            ball.CenterAtLocation(new Vector2(400, 240));

            // Generate an angle for the ball to be launched
            float angle = MathHelper.ToRadians(random.Next(-45, 45));

            // Figure out which player the ball is going towards first
            bool towardsPlayer = random.Next() % 2 == 0;

            // Apply the player decision to flip the angle if it's to head at the player
            if (towardsPlayer)
            {
                angle += MathHelper.Pi;
            }

            // Generate the velocity from this angle
            ball.Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 300f;
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            // Check for input which will drive the player's paddle
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0)
            {
                // We just use the first touch present
                TouchLocation touch = touches[0];

                // Center the paddle vertically on the user's touch, based on the paddle's base collision bounds
                playerPaddle.Position.Y = touch.Position.Y - playerPaddle.BaseCollisionBounds.Height / 2;

                // Make sure the player can't leave the screen
                playerPaddle.ClampToScreen();
            }

            // Update the ball
            ball.Update(e.ElapsedTime);

            // And run the AI for the computer paddle
            computerPaddle.UpdateAI(e.ElapsedTime, ball);
            
            // We now want to clamp the ball so it can't leave the top or bottom of the screen
            if (ball.Bounds.Bottom > 480)
            {
                // Resolve the collision and reverse the ball's direction
                ball.Position.Y = 480 - ball.Bounds.Height;
                ball.Velocity.Y *= -1;

                // Play our sound effect
                PlaySound(plink);
            }
            else if (ball.Bounds.Top < 0)
            {
                // Resolve the collision and reverse the ball's direction
                ball.Position.Y = ball.BaseCollisionBounds.Y;
                ball.Velocity.Y *= -1;

                // Play our sound effect
                PlaySound(plink);
            }

            // Check for collisions between the ball and the paddles based on the direction of the ball
            if (ball.Velocity.X < 0)
            {
                if (playerPaddle.Collide(ball))
                    PlaySound(plink);
            }
            else if (ball.Velocity.X > 0)
            {
                if (computerPaddle.Collide(ball))
                    PlaySound(plink);
            }

            // Check if the ball is off the screen
            int result = ball.IsOffscreen();

            // If result is 1, the player scored a point
            if (result == 1)
            {
                // Add a point to the player
                IncrementPlayerScore();

                // Play the score sound effect
                PlaySound(score);

                // Reset the ball for the next point
                ResetBall();
            }

            // Otherwise if the result is -1, the computer scored a point
            else if (result == -1)
            {
                // Add a point to the computer
                IncrementComputerScore();

                // Play the score sound effect
                PlaySound(score);

                // Reset the ball for the next point
                ResetBall();
            }
        }

        /// <summary>
        /// Helper that increments the player's score and updates the UI text.
        /// </summary>
        private void IncrementPlayerScore()
        {
            playerScore++;
            playerScoreTextBlock.Text = string.Format("Player Score: {0}", playerScore);
        }

        /// <summary>
        /// Helper that increments the computer's score and updates the UI text.
        /// </summary>
        private void IncrementComputerScore()
        {
            computerScore++;
            computerScoreTextBlock.Text = string.Format("Computer Score: {0}", computerScore);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            // Draw the Silverlight UI into the texture
            uiRenderer.Render();

            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            spriteBatch.Begin();

            // Draw the center line of the game
            spriteBatch.Draw(blank, new Rectangle(398, 0, 4, 480), Color.White * .8f);

            // Draw our three game objects
            playerPaddle.Draw(spriteBatch);
            computerPaddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);
            
            // Draw the UI on top of the scene
            spriteBatch.Draw(uiRenderer.Texture, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Invoked when the page's layout changes so we can make sure our UIElementRenderer is properly
        /// constructed for the appropriate size.
        /// </summary>
        void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            // Ensure the page size is valid
            if (width <= 0 || height <= 0)
                return;

            // Do we already have a UIElementRenderer of the correct size?
            if (uiRenderer != null &&
                uiRenderer.Texture != null &&
                uiRenderer.Texture.Width == width &&
                uiRenderer.Texture.Height == height)
            {
                return;
            }

            // Before constructing a new UIElementRenderer, be sure to Dispose the old one
            if (uiRenderer != null)
                uiRenderer.Dispose();

            // Create the renderer
            uiRenderer = new UIElementRenderer(this, width, height);
        }
    }
}