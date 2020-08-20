#region File Description
//-----------------------------------------------------------------------------
// ParticleSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Particles2DPipelineSample
{
    /// <summary>
    /// This is the main type for the ParticleSample, and inherits from the Framework's
    /// Game class. It creates three different kinds of ParticleSystems, and then adds
    /// them to its components collection. It also has keeps a random number generator,
    /// a SpriteBatch, and a ContentManager that the different classes in this sample
    /// can share.
    /// </summary>
    public class ParticleSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields and Properties

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Used to draw the instructions on the screen.
        SpriteFont font;

        // we want a sprite to represent our smokingEmitter
        Texture2D emitterSprite;

        // Here's the really fun part of the sample, the particle systems! These are
        // drawable game components, so we can just add them to the components
        // collection. Read more about each particle system in their respective source
        // files.
        ParticleSystem explosion;
        ParticleSystem smoke;
        ParticleSystem smokePlume;

        // For our Emitter test, we need both a ParticleEmitter and ParticleSystem
        ParticleEmitter emitter;
        ParticleSystem emitterSystem;

        // State is an enum that represents which effect we're currently demoing.
        enum State
        {
            Explosions,
            SmokePlume,
            Emitter
        };
        // the number of values in the "State" enum.
        const int NumStates = 3;
        State currentState = State.Explosions;

        // a timer that will tell us when it's time to trigger another explosion.
        const float TimeBetweenExplosions = 2.0f;
        float timeTillExplosion = 0.0f;

        // keep a timer that will tell us when it's time to add more particles to the
        // smoke plume.
        const float TimeBetweenSmokePlumePuffs = .5f;
        float timeTillPuff = 0.0f;

        // keep track of the last frame's keyboard and gamepad state, so that we know
        // if the user has pressed a button.
        KeyboardState lastKeyboardState;
        GamePadState lastGamepadState;

        #endregion

        #region Initialization

        public ParticleSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE
			graphics.IsFullScreen = true;

            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            Content.RootDirectory = "Content";

            // create the particle systems and add them to the components list.
            explosion = new ParticleSystem(this, "ExplosionSettings") { DrawOrder = ParticleSystem.AdditiveDrawOrder };
            Components.Add(explosion);

            smoke = new ParticleSystem(this, "ExplosionSmokeSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            Components.Add(smoke);

            smokePlume = new ParticleSystem(this, "SmokePlumeSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            Components.Add(smokePlume);

            emitterSystem = new ParticleSystem(this, "EmitterSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            Components.Add(emitterSystem);
            emitter = new ParticleEmitter(emitterSystem, 60, new Vector2(400, 240));
            
			// enable the tap gesture for changing particle effects
			TouchPanel.EnabledGestures = GestureType.Tap;
        }

        /// <summary>
        /// Load your graphics content. 
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            emitterSprite = Content.Load<Texture2D>("BlockEmitter");
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // check the input devices to see if someone has decided they want to see
            // the other effect, if they want to quit.
            HandleInput();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (currentState)
            {
                // if we should be demoing the explosions effect, check to see if it's
                // time for a new explosion.
                case State.Explosions:
                    UpdateExplosions(dt);
                    break;
                // if we're showing off the smoke plume, check to see if it's time for a
                // new puff of smoke.
                case State.SmokePlume:
                    UpdateSmokePlume(dt);
                    break;
                // if we're demoing the emitter attached to the mouse, update the emitter
                case State.Emitter:
                    UpdateEmitter(gameTime);
                    break;
            }

            // the base update will handle updating the particle systems themselves,
            // because we added them to the components collection.
            base.Update(gameTime);
        }

        // this function is called when we want to demo the use of the ParticleEmitter.
        // it figures out our new emitter location and updates the emitter which in 
        // turn handles creating any particles for the system.
        private void UpdateEmitter(GameTime gameTime)
        {
            // start with our current position
            Vector2 newPosition = emitter.Position;

#if XBOX
            // Xbox will use the GamePad's LeftThumbstick to move it around
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            Vector2 thumbstick = gamePadState.ThumbSticks.Left;

            // reverse the Y axis to match our SpriteBatch coordinates
            thumbstick.Y *= -1;

            newPosition += thumbstick * 10f;
#else
            // Windows and Windows Phone use our Mouse class to update
            // the position of the emitter.
            MouseState mouseState = Mouse.GetState();
            newPosition = new Vector2(mouseState.X, mouseState.Y);
#endif

            // updating the emitter not only assigns a new location, but handles creating
            // the particles for our system based on the particlesPerSecond parameter of
            // the ParticleEmitter constructor.
            emitter.Update(gameTime, newPosition);
        }
        
        // this function is called when we want to demo the smoke plume effect. it
        // updates the timeTillPuff timer, and adds more particles to the plume when
        // necessary.
        private void UpdateSmokePlume(float dt)
        {
            timeTillPuff -= dt;
            if (timeTillPuff < 0)
            {
                Vector2 where = Vector2.Zero;
                // add more particles at the bottom of the screen, halfway across.
                where.X = graphics.GraphicsDevice.Viewport.Width / 2;
                where.Y = graphics.GraphicsDevice.Viewport.Height;
                smokePlume.AddParticles(where, Vector2.Zero);

                // and then reset the timer.
                timeTillPuff = TimeBetweenSmokePlumePuffs;
            }
        }

        // this function is called when we want to demo the explosion effect. it
        // updates the timeTillExplosion timer, and starts another explosion effect
        // when the timer reaches zero.
        private void UpdateExplosions(float dt)
        {
            timeTillExplosion -= dt;
            if (timeTillExplosion < 0)
            {
                Vector2 where = Vector2.Zero;
                // create the explosion at some random point on the screen.
                where.X = ParticleHelpers.RandomBetween(0, graphics.GraphicsDevice.Viewport.Width);
                where.Y = ParticleHelpers.RandomBetween(0, graphics.GraphicsDevice.Viewport.Height);

                // the overall explosion effect is actually comprised of two particle
                // systems: the fiery bit, and the smoke behind it. add particles to
                // both of those systems.
                explosion.AddParticles(where, Vector2.Zero);
                smoke.AddParticles(where, Vector2.Zero);

                // reset the timer.
                timeTillExplosion = TimeBetweenExplosions;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // draw some instructions on the screen
            string message = string.Format("Current effect: {0}!\n" +
                "Hit the A button or space bar, or tap the screen, to switch.\n\n" +
                "Free particles:\n" +
                "    ExplosionParticleSystem:      {1}\n" +
                "    ExplosionSmokeParticleSystem: {2}\n" +
                "    SmokePlumeParticleSystem:     {3}\n" +
                "    EmitterParticleSystem:        {4}",
                currentState, explosion.FreeParticleCount,
                smoke.FreeParticleCount, smokePlume.FreeParticleCount,
                emitterSystem.FreeParticleCount);
            spriteBatch.DrawString(font, message, new Vector2(50, 50), Color.White);

            // draw a sprite to represent our emitter for that state
            if (currentState == State.Emitter)
            {
                spriteBatch.Draw(
                    emitterSprite,
                    emitter.Position -
                        new Vector2(emitterSprite.Width / 2, emitterSprite.Height / 2),
                    Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // This function will check to see if the user has just pushed the A button or
        // the space bar. If so, we should go to the next effect.
        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed || 
                currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();


            // check to see if someone has just released the space bar.            
            bool keyboardSpace =
                currentKeyboardState.IsKeyUp(Keys.Space) &&
                lastKeyboardState.IsKeyDown(Keys.Space);


            // check the gamepad to see if someone has just released the A button.
            bool gamepadA =
                currentGamePadState.Buttons.A == ButtonState.Pressed &&
                lastGamepadState.Buttons.A == ButtonState.Released;

			// check our gestures to see if someone has tapped the screen. we want
			// to read all available gestures even if a tap occurred so we clear
			// the queue.
			bool tapGesture = false;
			while (TouchPanel.IsGestureAvailable)
			{
				GestureSample sample = TouchPanel.ReadGesture();
				if (sample.GestureType == GestureType.Tap)
				{
					tapGesture = true;
				}
			}
            

            // if either the A button or the space bar was just released, or the screen
			// was tapped, move to the next state. Doing modulus by the number of 
			// states lets us wrap back around to the first state.
            if (keyboardSpace || gamepadA || tapGesture)
            {
                currentState = (State)((int)(currentState + 1) % NumStates);
            }

            lastKeyboardState = currentKeyboardState;
            lastGamepadState = currentGamePadState;
        }

        #endregion
    }
}
