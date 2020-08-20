#region File Information
//-----------------------------------------------------------------------------
// MicrophoneEchoSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace MicrophoneEchoSample
{
#region Helper Microphone Extension Method
    public static class MicrophoneExtensions
    {
        // Provides a simple method to check if a microphone is connected.
        // There is no guarantee that the microphone will not get disconnected at any time.
        // This method helps in simplifying the microphone enumeration code.
        public static bool IsConnected(this Microphone microphone)
        {
            try
            {
                MicrophoneState state = microphone.State;
                return true;
            }
            catch (NoMicrophoneConnectedException)
            {
                return false;
            }
        }
    }
#endregion Helper Microphone Extension Method

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MicrophoneEchoSampleGame : Microsoft.Xna.Framework.Game
    {
#region Fields
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // The most recent microphone samples.
        byte[] micSamples;
        // A circular buffer that we feedback into from micSamples
        byte[] echoBuffer;
        // Tracks the position into the echo buffer.
        int echoBufferPosition = 0;
        // DynamicSoundEffectInstance is used to playback the captured audio after processing it for echo.
        DynamicSoundEffectInstance dynamicSound;
        // Microphone used for recording.
        Microphone activeMicrophone;
        // Follow these instructions.
#if WINDOWS_PHONE
        const string instructions = @"Tap to start or DoubleTap to stop recording";
#else
        const string instructions = @"Press 'A' to start and 'B' to stop recording";
#endif
        // For handling input
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        // Used to communicate the microphone status to the user.
        string microphoneStatus = string.Empty;

        // Echo processing constants.
        // Delay applied in seconds
        private const float echoDelay = 0.15f;
        // Rate of echo decay.
        private const float echoAmount = 0.5f;

        // On big endian systems audio samples need to be swapped because 
        // BinaryReader/BinaryWriter assume little endian.
        bool bigEndian = !BitConverter.IsLittleEndian;

        // Used for drawing the audio waveform
        BasicEffect effect;
        VertexPositionColor[] vertexPosColor;
#endregion Fields

#region LoadContent, Update, HandleInput and Draw
        public MicrophoneEchoSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);           
#endif
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;

            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.DoubleTap;
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("MyFont");
            InitializeMicrophone();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Picks a microphone to start recording - if one isn't picked already.
            InitializeMicrophone();
            // Handle input to start/stop recording
            HandleInput();
            // Check and update microphone status.
            UpdateMicrophoneStatus();

            base.Update(gameTime);
        }

        /// <summary>
        /// Handles input for starting and stopping the recording.
        /// </summary>
        private void HandleInput()
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                if (gesture.GestureType == GestureType.Tap)
                {
                    StartMicrophone();
                }
                else if (gesture.GestureType == GestureType.DoubleTap)
                {
                    StopMicrophone();
                }
            }
            else
            {
                previousGamePadState = currentGamePadState;
                previousKeyboardState = currentKeyboardState;

                currentGamePadState = GamePad.GetState(PlayerIndex.One);
                currentKeyboardState = Keyboard.GetState();

                if (currentGamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A) ||
                    currentKeyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A))
                {
                    StartMicrophone();
                }

                if (currentGamePadState.IsButtonDown(Buttons.B) && previousGamePadState.IsButtonUp(Buttons.B) ||
                    currentKeyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
                {
                    StopMicrophone();
                }
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawWaveform();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the audio waveform being played back.
        /// </summary>
        private void DrawWaveform()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, instructions, new Vector2(10f, 20f), Color.White);
            spriteBatch.DrawString(font, microphoneStatus, new Vector2(10f, 50f), Color.White);
            if (echoBuffer != null)
            {
                int sampleCount = echoBuffer.Length / sizeof(short);
                for (int index = 0; index < echoBuffer.Length; index += sizeof(short))
                {
                    int sampleIndex = index / sizeof(short);
                    vertexPosColor[sampleIndex].Position.X = sampleIndex * ((float)GraphicsDevice.Viewport.Width / sampleCount);
                    vertexPosColor[sampleIndex].Position.Y = (GraphicsDevice.Viewport.Height / 2) - ((float)ReadSample(echoBuffer, index) 
                                                             / Int16.MaxValue * (GraphicsDevice.Viewport.Height / 2));
                }
                effect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertexPosColor, 0, vertexPosColor.Length - 1);
            }
            spriteBatch.End();
        }
#endregion LoadContent, Update and Draw

#region Process Microphone
        /// <summary>
        /// Finds a good microphone to use and sets up everything to start recording and playback.
        /// Once a microphone is selected the game uses it throughout its lifetime.
        /// If it gets disconnected it will tell the user to reconnect it.
        /// </summary>
        private void InitializeMicrophone()
        {
            // We already have a microphone, skip out early.
            if (activeMicrophone != null) { return; }

            try
            {
                // Find the first microphone that's ready to rock.
                activeMicrophone = PickFirstConnectedMicrophone();
                if (activeMicrophone != null)
                {
                    // Set the capture buffer size for kow latency.
                    // Microphone will call the game back when it has captured at least that much audio data.
                    activeMicrophone.BufferDuration = TimeSpan.FromMilliseconds(100);
                    // Subscribe to the event that's raised when the capture buffer is filled.
                    activeMicrophone.BufferReady += BufferReady; 

                    // We will put the mic samples in this buffer.  We only want to allocate it once.
                    micSamples = new byte[activeMicrophone.GetSampleSizeInBytes(activeMicrophone.BufferDuration)];

                    // This is a circular buffer.  Samples from the mic will be mixed with the oldest sample in this buffer
                    // and written back out to this buffer.  This feedback creates an echo effect.
                    echoBuffer = new byte[activeMicrophone.GetSampleSizeInBytes(TimeSpan.FromSeconds(echoDelay))];

                    // Create a DynamicSoundEffectInstance in the right format to playback the captured audio.
                    dynamicSound = new DynamicSoundEffectInstance(activeMicrophone.SampleRate, AudioChannels.Mono);
                    dynamicSound.Play();

                    // Success - now allocate everything we need to draw the audio waveform
                    // Now allocate the graphics resources to draw the waveform
                    effect = new BasicEffect(GraphicsDevice);
                    effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * 
                        Matrix.CreateOrthographicOffCenter(GraphicsDevice.Viewport.Bounds.Left, GraphicsDevice.Viewport.Bounds.Right, 
                         GraphicsDevice.Viewport.Bounds.Bottom, GraphicsDevice.Viewport.Bounds.Top, -1f, 1f);
                    int sampleCount = echoBuffer.Length / sizeof(short);
                    vertexPosColor = new VertexPositionColor[sampleCount];
                    for (int index = 0; index < sampleCount; ++index)
                    {
                        vertexPosColor[index] = new VertexPositionColor(new Vector3(), Color.White);
                    }
                }
            }
            catch (NoMicrophoneConnectedException)
            {
                // Uh oh, the microphone was disconnected in the middle of initialization.
                // Let's clean up everything so we can look for another microphone again on the next update.
                activeMicrophone.BufferReady -= BufferReady;
                activeMicrophone = null;
            }
        }

        /// <summary>
        /// Start the microphone.
        /// </summary>
        private void StartMicrophone()
        {
            // Can't start a microphone that doesn't exist.
            if (activeMicrophone == null) { return; }

            try
            {
                activeMicrophone.Start();
            }
            catch (NoMicrophoneConnectedException)
            {
                UpdateMicrophoneStatus();
            }
        }


        /// <summary>
        /// Stop the microphone.
        /// </summary>
        private void StopMicrophone()
        {
            // Can't stop a microphone that doesn't exist.
            if (activeMicrophone == null) { return; }

            try
            {
                // Stop the microphone
                activeMicrophone.Stop();
                // And clear the echo buffer
                Array.Clear(echoBuffer, 0, echoBuffer.Length);
            }
            catch (NoMicrophoneConnectedException)
            {
                UpdateMicrophoneStatus();
            }
        }


        /// <summary>
        /// Look for a good microphone to start recording.
        /// </summary>
        /// <returns></returns>
        private Microphone PickFirstConnectedMicrophone()
        {
            // Let's pick the default microphone if it's ready.
            if (Microphone.Default != null && Microphone.Default.IsConnected())
            {
                return Microphone.Default;
            }

            // Default microphone seems to be disconnected so look for another microphone that we can use.
            // And if the default was null then the list will be empty and we'll skip the search.
            foreach (Microphone microphone in Microphone.All)
            {
                if (microphone.IsConnected())
                {
                    return microphone;
                }
            }

            // There are no microphones hooked up to the system!
            return null;
        }


        /// <summary>
        /// Keep track of the microphone status to communicate to the user.
        /// </summary>
        private void UpdateMicrophoneStatus()
        {
            // We don't have any microphones connected to the system.
            if (activeMicrophone == null)
            {
                microphoneStatus = "Waiting for microphone connection...";
            }
            else
            {
                try
                {
                    // Update the status - if the microphone gets disconnected this will throw
                    microphoneStatus = string.Format("{0} is {1}", activeMicrophone.Name, activeMicrophone.State);
                }
                catch (NoMicrophoneConnectedException)
                {
                    // Microphone got disconnected - Let's ask the user to reconnect it.
                    microphoneStatus = string.Format("Please reconnect {0}", activeMicrophone.Name);
                    // Clear the echo buffer
                    Array.Clear(echoBuffer, 0, echoBuffer.Length);
                }
            }
        }

        /// <summary>
        /// This is called each time a microphone buffer has been filled.
        /// </summary>
        void BufferReady(object sender, EventArgs e)
        {
            try
            {
                // Copy the captured audio data into the pre-allocated array.
                activeMicrophone.GetData(micSamples, 0, micSamples.Length);
                ProcessEcho();
            }
            catch (NoMicrophoneConnectedException)
            {
                // Microphone was disconnected - let the user know.
                UpdateMicrophoneStatus();
            }
        }

        /// <summary>
        ///  Captured audio is processed for echo in following steps -
        ///   1) Mix each sample with a delayed sample from the echo buffer
        ///   2) Write mixed sample back into echoBuffer so it can echo back later
        ///   3) Submit echo buffer to dynamicSound.
        /// </summary>
        private void ProcessEcho()
        {
            for (int index = 0; index < micSamples.Length; index += sizeof(short))
            {
                short micSample = ReadSample(micSamples, index);
                short echoSample = ReadSample(echoBuffer, echoBufferPosition);

                // Mix the echo back into the buffer
                short outputSample = (short)((float)micSample * (1.0f - echoAmount) + (float)echoSample * echoAmount);
                WriteSample(echoBuffer, echoBufferPosition, outputSample);
                echoBufferPosition += sizeof(short);

                // Play back the encho buffer if it's filled.
                if (echoBufferPosition == echoBuffer.Length)
                {
                    dynamicSound.SubmitBuffer(echoBuffer, 0, echoBuffer.Length);
                    // Reset the position to the beginning of the buffer.
                    echoBufferPosition = 0;
                }
            }
        }

        /// <summary>
        /// Returns a sample value from the passed buffer,
        /// taking into account the endian-ness of the system.
        /// </summary>
        private short ReadSample(byte[] buffer, int index)
        {
            // Ensure we're doing aligned reads.
            if (index % sizeof(short) != 0)
            {
                throw new ArgumentException("index");
            }

            if (index >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            short sample = 0;
            if (bigEndian)
            {
                sample = (short)(buffer[index] << 8 | buffer[index + 1] & 0xff);
            }
            else
            {
                sample = (short)(buffer[index] & 0xff | buffer[index + 1] << 8);
            }

            return sample;
        }

        /// <summary>
        /// Writes the passed sample value to the buffer,
        /// taking into account the endian-ness of the system.
        /// </summary>
        private void WriteSample(byte[] buffer, int index, short sample)
        {
            // Ensure we're doing aligned writes.
            if (index % sizeof(short) != 0)
            {
                throw new ArgumentException("index");
            }

            if (index >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (bigEndian)
            {
                buffer[index] = (byte)(sample >> 8);
                buffer[index + 1] = (byte)sample;
            }
            else
            {
                buffer[index] = (byte)sample;
                buffer[index + 1] = (byte)(sample >> 8);
            }
        }
#endregion Process Microphone
    }
}
