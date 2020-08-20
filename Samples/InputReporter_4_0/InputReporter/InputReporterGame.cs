#region File Description
//-----------------------------------------------------------------------------
// InputReporterGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace InputReporter
{
    /// <summary>
    /// Displays live input values for all connected controllers.
    /// </summary>
    partial class InputReporterGame : Microsoft.Xna.Framework.Game
    {
        #region Image Positions
        private static readonly Vector2[] connectedControllerPositions = new Vector2[4]
            {
                new Vector2(606f, 60f),
                new Vector2(656f, 60f),
                new Vector2(606f, 110f),
                new Vector2(656f, 110f),
            };
        private static readonly Vector2[] selectedControllerPositions = new Vector2[4]
            {
                new Vector2(594f, 36f),
                new Vector2(686f, 36f),
                new Vector2(594f, 137f),
                new Vector2(686f, 137f),
            };
        #endregion


        #region Text Positions
        private static readonly Vector2 titlePosition = 
            new Vector2(180f, 73f);
        private static readonly Vector2 typeCenterPosition = 
            new Vector2(660f, 270f);
        private static readonly Vector2 descriptionColumn1Position = 
            new Vector2(65f, 135f);
        private static readonly Vector2 valueColumn1Position =
            new Vector2(220f, 135f);
        private static readonly Vector2 descriptionColumn2Position =
            new Vector2(310f, 135f);
        private static readonly Vector2 valueColumn2Position =
            new Vector2(472f, 135f);
        private static readonly Vector2 deadZoneInstructionsPosition =
            new Vector2(570f, 380f);
        private static readonly Vector2 exitInstructionsPosition =
            new Vector2(618f, 425f);
        #endregion


        #region Text Colors
        private static readonly Color titleColor = new Color(60, 134, 11);
        private static readonly Color typeColor = new Color(38, 108, 87);
        private static readonly Color descriptionColor = new Color(33, 89, 15);
        private static readonly Color valueColor = new Color(38, 108, 87);
        private static readonly Color disabledColor = new Color(171, 171, 171);
        private static readonly Color instructionsColor = new Color(127, 130, 127);
        #endregion


        #region ChargeSwitch Durations
        private const float deadZoneChargeSwitchDuration = 2f;
        private const float exitChargeSwitchDuration = 2f;
        #endregion


        #region Input Data
        private int selectedPlayer;
        private GamePadState[] gamePadStates = new GamePadState[4];
        private GamePadCapabilities[] gamePadCapabilities = new GamePadCapabilities[4];
        private KeyboardState lastKeyboardState;
        #endregion


        #region Dead Zone Data
        private GamePadDeadZone deadZone = GamePadDeadZone.IndependentAxes;
        public GamePadDeadZone DeadZone
        {
            get { return deadZone; }
            set
            {
                deadZone = value;
                deadZoneString = "(" + deadZone.ToString() + ")";
                if (dataFont != null)
                {
                    Vector2 deadZoneStringSize =
                        dataFont.MeasureString(deadZoneString);
                    deadZoneStringPosition = new Vector2(
                        (float)Math.Floor(deadZoneStringCenterPosition.X - 
                            deadZoneStringSize.X / 2f),
                        (float)Math.Floor(deadZoneStringCenterPosition.Y - 
                            deadZoneStringSize.Y / 2f));
                }
            }
        }
        private string deadZoneString;
        private Vector2 deadZoneStringPosition;
        private Vector2 deadZoneStringCenterPosition;
        #endregion


        #region ChargeSwitches
        private ChargeSwitchExit exitSwitch = 
            new ChargeSwitchExit(exitChargeSwitchDuration);
        private ChargeSwitchDeadZone deadZoneSwitch = 
            new ChargeSwitchDeadZone(deadZoneChargeSwitchDuration);
        #endregion


        #region Graphics Data
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont titleFont;
        private SpriteFont dataFont;
        private SpriteFont dataActiveFont;
        private SpriteFont typeFont;
        private SpriteFont instructionsFont;
        private SpriteFont instructionsActiveFont;
        private Texture2D backgroundTexture;
        private Texture2D[] connectedControllerTextures = new Texture2D[4];
        private Texture2D[] selectedControllerTextures = new Texture2D[4];
        private float dataSpacing;
        #endregion


        #region Initialization
        /// <summary>
        /// Primary constructor.
        /// </summary>
        public InputReporterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
            Content.RootDirectory = "Content";
            exitSwitch.Fire += new ChargeSwitch.FireDelegate(exitSwitch_Fire);
            deadZoneSwitch.Fire += new ChargeSwitch.FireDelegate(ToggleDeadZone);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting 
        /// to run.  This is where it can query for any required services and load any 
        /// non-graphic related content.  Calling base.Initialize will enumerate through
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            selectedPlayer = 0;

            exitSwitch.Reset(exitChargeSwitchDuration);
            deadZoneSwitch.Reset(deadZoneChargeSwitchDuration);

            base.Initialize();

            DeadZone = GamePadDeadZone.IndependentAxes;
        }
        #endregion


        #region Graphics Load/Unload
        /// <summary>
        /// Load your graphics content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            titleFont = Content.Load<SpriteFont>("Fonts\\TitleFont");
            dataFont = Content.Load<SpriteFont>("Fonts\\DataFont");
            dataActiveFont = Content.Load<SpriteFont>("Fonts\\DataActiveFont");
            typeFont = Content.Load<SpriteFont>("Fonts\\TypeFont");
            instructionsFont = Content.Load<SpriteFont>("Fonts\\InstructionsFont");
            instructionsActiveFont =
                Content.Load<SpriteFont>("Fonts\\InstructionsActiveFont");
            dataSpacing = (float)Math.Floor(dataFont.LineSpacing * 1.3f);
            deadZoneStringCenterPosition = new Vector2(687f,
                (float)Math.Floor(deadZoneInstructionsPosition.Y +
                dataFont.LineSpacing * 1.7f));

            backgroundTexture = Content.Load<Texture2D>("Textures\\Background");
            connectedControllerTextures[0] =
                Content.Load<Texture2D>("Textures\\connected_controller1");
            connectedControllerTextures[1] =
                Content.Load<Texture2D>("Textures\\connected_controller2");
            connectedControllerTextures[2] =
                Content.Load<Texture2D>("Textures\\connected_controller3");
            connectedControllerTextures[3] =
                Content.Load<Texture2D>("Textures\\connected_controller4");
            selectedControllerTextures[0] =
                Content.Load<Texture2D>("Textures\\select_controller1");
            selectedControllerTextures[1] =
                Content.Load<Texture2D>("Textures\\select_controller2");
            selectedControllerTextures[2] =
                Content.Load<Texture2D>("Textures\\select_controller3");
            selectedControllerTextures[3] =
                Content.Load<Texture2D>("Textures\\select_controller4");
        }
        #endregion


        #region Updating
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (keyboardState.IsKeyDown(Keys.Space) &&
                !lastKeyboardState.IsKeyDown(Keys.Space))
            {
                ToggleDeadZone();
            }

            bool setSelectedPlayer = false; // give preference to earlier controllers
            for (int i = 0; i < 4; i++)
            {
                gamePadStates[i] = GamePad.GetState((PlayerIndex)i, deadZone);
                gamePadCapabilities[i] = GamePad.GetCapabilities((PlayerIndex)i);
                if (!setSelectedPlayer && IsActiveGamePad(ref gamePadStates[i]))
                {
                    selectedPlayer = i;
                    setSelectedPlayer = true;
                }
            }

            deadZoneSwitch.Update(gameTime, ref gamePadStates[selectedPlayer]);
            exitSwitch.Update(gameTime, ref gamePadStates[selectedPlayer]);

            base.Update(gameTime);

            lastKeyboardState = keyboardState;
        }


        /// <summary>
        /// Determines if the provided GamePadState is "active".
        /// </summary>
        /// <param name="gamePadState">The GamePadState that is checked.</param>
        /// <remarks>
        /// "Active" currently means that at least one of the buttons is being pressed.
        /// </remarks>
        /// <returns>True if "active".</returns>
        private static bool IsActiveGamePad(ref GamePadState gamePadState)
        {
            return (gamePadState.IsConnected &&
                ((gamePadState.Buttons.A == ButtonState.Pressed) ||
                (gamePadState.Buttons.B == ButtonState.Pressed) ||
                (gamePadState.Buttons.X == ButtonState.Pressed) ||
                (gamePadState.Buttons.Y == ButtonState.Pressed) ||
                (gamePadState.Buttons.Start == ButtonState.Pressed) ||
                (gamePadState.Buttons.Back == ButtonState.Pressed) ||
                (gamePadState.Buttons.LeftShoulder == ButtonState.Pressed) ||
                (gamePadState.Buttons.RightShoulder == ButtonState.Pressed) ||
                (gamePadState.Buttons.LeftStick == ButtonState.Pressed) ||
                (gamePadState.Buttons.RightStick == ButtonState.Pressed) ||
                (gamePadState.DPad.Up == ButtonState.Pressed) ||
                (gamePadState.DPad.Left == ButtonState.Pressed) ||
                (gamePadState.DPad.Right == ButtonState.Pressed) ||
                (gamePadState.DPad.Down == ButtonState.Pressed)));
        }
        #endregion


        #region Drawing
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            spriteBatch.Begin();

            // draw the background
            spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

            // draw the connected-controller images
            for (int i = 0; i < 4; i++)
            {
                if (gamePadStates[i].IsConnected)
                {
                    spriteBatch.Draw(connectedControllerTextures[i], 
                        connectedControllerPositions[i], Color.White);
                }
            }
            // draw the selected-player texture (numeral)
            spriteBatch.Draw(selectedControllerTextures[selectedPlayer], 
                selectedControllerPositions[selectedPlayer], Color.White);

            // draw controller title
            string text = InputReporterResources.Title + 
                ((PlayerIndex)selectedPlayer).ToString();
            spriteBatch.DrawString(titleFont, text, titlePosition,
                titleColor);

            // draw controller type
            text = gamePadCapabilities[selectedPlayer].GamePadType.ToString();
            Vector2 textSize = typeFont.MeasureString(text);
            spriteBatch.DrawString(typeFont, text, new Vector2(
                (float)Math.Floor(typeCenterPosition.X - 
                    textSize.X / 2f),
                (float)Math.Floor(typeCenterPosition.Y - 
                    textSize.Y / 2f)),
                typeColor);

            // draw the data
            DrawData(ref gamePadStates[selectedPlayer], 
                ref gamePadCapabilities[selectedPlayer]);

            // draw the instructions
            spriteBatch.DrawString(deadZoneSwitch.Active ? instructionsActiveFont : 
                instructionsFont, InputReporterResources.DeadZoneInstructions, 
                deadZoneInstructionsPosition, instructionsColor);
            spriteBatch.DrawString(instructionsFont, deadZoneString,
                deadZoneStringPosition, instructionsColor);
            spriteBatch.DrawString(exitSwitch.Active ? instructionsActiveFont :
                instructionsFont, InputReporterResources.ExitInstructions, 
                exitInstructionsPosition, instructionsColor);

            spriteBatch.End();
        }


        /// <summary>
        /// Draw all data for a set of GamePad data and capabilities.
        /// </summary>
        /// <param name="gamePadState">The GamePad data.</param>
        /// <param name="gamePadCapabilities">The GamePad capabilities.</param>
        /// <remarks>
        /// The GamePad structures are passed by reference for speed.  They are not
        /// modified in this method.
        /// </remarks>
        private void DrawData(ref GamePadState gamePadState, 
            ref GamePadCapabilities gamePadCapabilities)
        {
            //
            // Draw the first column of data
            //
            Vector2 descriptionPosition = descriptionColumn1Position;
            Vector2 valuePosition = valueColumn1Position;

            // draw left thumbstick data
            DrawValue(InputReporterResources.LeftThumbstickX, ref descriptionPosition,
                gamePadState.ThumbSticks.Left.X.ToString("0.000"), ref valuePosition,
                gamePadCapabilities.HasLeftXThumbStick,
                gamePadState.ThumbSticks.Left.X != 0f);
            DrawValue(InputReporterResources.LeftThumbstickY, ref descriptionPosition,
                gamePadState.ThumbSticks.Left.Y.ToString("0.000"), ref valuePosition,
                gamePadCapabilities.HasLeftYThumbStick,
                gamePadState.ThumbSticks.Left.Y != 0f);

            // draw the right thumbstick data
            DrawValue(InputReporterResources.RightThumbstickX, ref descriptionPosition,
                gamePadState.ThumbSticks.Right.X.ToString("0.000"), ref valuePosition,
                gamePadCapabilities.HasRightXThumbStick,
                gamePadState.ThumbSticks.Right.X != 0f);
            DrawValue(InputReporterResources.RightThumbstickY, ref descriptionPosition,
                gamePadState.ThumbSticks.Right.Y.ToString("0.000"), ref valuePosition,
                gamePadCapabilities.HasRightYThumbStick,
                gamePadState.ThumbSticks.Right.Y != 0f);

            descriptionPosition.Y += dataSpacing;
            valuePosition.Y += dataSpacing;

            // draw the trigger data
            DrawValue(InputReporterResources.LeftTrigger, ref descriptionPosition,
                gamePadState.Triggers.Left.ToString("0.000"), ref valuePosition,
                gamePadCapabilities.HasLeftTrigger,
                gamePadState.Triggers.Left != 0f);
            DrawValue(InputReporterResources.RightTrigger, ref descriptionPosition,
                gamePadState.Triggers.Right.ToString("0.000"), ref valuePosition,
                gamePadCapabilities.HasRightTrigger, 
                gamePadState.Triggers.Right != 0f);

            descriptionPosition.Y += dataSpacing;
            valuePosition.Y += dataSpacing;

            // draw the directional pad data
            DrawValue(InputReporterResources.DPadUp, ref descriptionPosition,
                (gamePadState.DPad.Up == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasDPadUpButton,
                gamePadState.DPad.Up == ButtonState.Pressed);
            DrawValue(InputReporterResources.DPadDown, ref descriptionPosition,
                (gamePadState.DPad.Down == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasDPadDownButton,
                gamePadState.DPad.Down == ButtonState.Pressed);
            DrawValue(InputReporterResources.DPadLeft, ref descriptionPosition,
                (gamePadState.DPad.Left == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed :
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasDPadLeftButton,
                gamePadState.DPad.Left == ButtonState.Pressed);
            DrawValue(InputReporterResources.DPadRight, ref descriptionPosition,
                (gamePadState.DPad.Right == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasDPadRightButton,
                gamePadState.DPad.Right == ButtonState.Pressed);

            descriptionPosition.Y += dataSpacing;
            valuePosition.Y += dataSpacing;

            // draw the vibration data
            if (gamePadCapabilities.HasLeftVibrationMotor)
            {
                if (gamePadCapabilities.HasRightVibrationMotor)
                {
                    spriteBatch.DrawString(dataFont, 
                        InputReporterResources.BothVibrationMotors, descriptionPosition,
                        descriptionColor);
                }
                else
                {
                    spriteBatch.DrawString(dataFont, 
                        InputReporterResources.LeftVibrationMotor, descriptionPosition,
                        descriptionColor);
                }
            }
            else if (gamePadCapabilities.HasRightVibrationMotor)
            {
                spriteBatch.DrawString(dataFont, 
                    InputReporterResources.RightVibrationMotor, descriptionPosition,
                    descriptionColor);
            }
            else
            {
                spriteBatch.DrawString(dataFont, InputReporterResources.NoVibration, 
                    descriptionPosition, descriptionColor);
            }

            //
            // Draw the second column of data
            //
            descriptionPosition = descriptionColumn2Position;
            valuePosition = valueColumn2Position;

            // draw the button data
            DrawValue(InputReporterResources.A, ref descriptionPosition,
                (gamePadState.Buttons.A == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasAButton,
                gamePadState.Buttons.A == ButtonState.Pressed);
            DrawValue(InputReporterResources.B, ref descriptionPosition,
                (gamePadState.Buttons.B == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasBButton,
                gamePadState.Buttons.B == ButtonState.Pressed);
            DrawValue(InputReporterResources.X, ref descriptionPosition,
                (gamePadState.Buttons.X == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasXButton,
                gamePadState.Buttons.X == ButtonState.Pressed);
            DrawValue(InputReporterResources.Y, ref descriptionPosition,
                (gamePadState.Buttons.Y == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasYButton,
                gamePadState.Buttons.Y == ButtonState.Pressed);
            DrawValue(InputReporterResources.LeftShoulder, ref descriptionPosition,
                (gamePadState.Buttons.LeftShoulder == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasLeftShoulderButton,
                gamePadState.Buttons.LeftShoulder == ButtonState.Pressed);
            DrawValue(InputReporterResources.RightShoulder, ref descriptionPosition,
                (gamePadState.Buttons.RightShoulder == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasRightShoulderButton,
                gamePadState.Buttons.RightShoulder == ButtonState.Pressed);
            DrawValue(InputReporterResources.LeftStick, ref descriptionPosition,
                (gamePadState.Buttons.LeftStick == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasLeftStickButton,
                gamePadState.Buttons.LeftStick == ButtonState.Pressed);
            DrawValue(InputReporterResources.RightStick, ref descriptionPosition,
                (gamePadState.Buttons.RightStick == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed : 
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasRightStickButton, 
                gamePadState.Buttons.RightStick == ButtonState.Pressed);
            DrawValue(InputReporterResources.Start, ref descriptionPosition,
                (gamePadState.Buttons.Start == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed :
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasStartButton,
                gamePadState.Buttons.Start == ButtonState.Pressed);
            DrawValue(InputReporterResources.Back, ref descriptionPosition,
                (gamePadState.Buttons.Back == ButtonState.Pressed ? 
                InputReporterResources.ButtonPressed :
                InputReporterResources.ButtonReleased), ref valuePosition,
                gamePadCapabilities.HasBackButton,
                gamePadState.Buttons.Back == ButtonState.Pressed);

            descriptionPosition.Y += dataSpacing;
            valuePosition.Y += dataSpacing;

            // draw the packet number data
            DrawValue(InputReporterResources.PacketNumber, ref descriptionPosition,
                gamePadState.PacketNumber.ToString(), ref valuePosition, 
                gamePadCapabilities.IsConnected, false);
        }


        /// <summary>
        /// Draw a single description/value pair.
        /// </summary>
        /// <param name="description">The description of the value.</param>
        /// <param name="descriptionPosition">The position of the description.</param>
        /// <param name="value">The value itself.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <param name="enabled">If true, the value type is supported.</param>
        /// <param name="active">If true, the value type is active right now.</param>
        /// <remarks>
        /// The positions are modified by this function, moving down one line.
        /// </remarks>
        private void DrawValue(string description, ref Vector2 descriptionPosition, 
            string value, ref Vector2 valuePosition, bool enabled, bool active)
        {
            spriteBatch.DrawString(dataFont, description, descriptionPosition, 
                enabled ? descriptionColor : disabledColor);
            descriptionPosition.Y += dataSpacing;
            spriteBatch.DrawString(active ? dataActiveFont : dataFont,
                value, valuePosition, enabled ? valueColor : disabledColor);
            valuePosition.Y += dataSpacing;
        }
        #endregion


        #region ChargeSwitch Event Handlers
        /// <summary>
        /// Handles the dead-zone ChargeSwitch fire event.  Toggles dead zone types.
        /// </summary>
        private void ToggleDeadZone()
        {
            switch (DeadZone)
            {
                case GamePadDeadZone.IndependentAxes:
                    DeadZone = GamePadDeadZone.Circular;
                    break;
                case GamePadDeadZone.Circular:
                    DeadZone = GamePadDeadZone.None;
                    break;
                case GamePadDeadZone.None:
                    DeadZone = GamePadDeadZone.IndependentAxes;
                    break;
            }
        }


        /// <summary>
        /// Handles the exit ChargeSwitch fire event.  Exits the application.
        /// </summary>
        private void exitSwitch_Fire()
        {
            this.Exit();
        }
        #endregion


        #region Entry Point
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (InputReporterGame game = new InputReporterGame())
            {
                game.Run();
            }
        }
        #endregion
    }
}
