#region File Description
//-----------------------------------------------------------------------------
// InputComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Movipa.Components.Input
{
    /// <summary>
    /// This component manages input information from a device.
    /// The input state of the game pad and keyboard is reflected to the state
    /// of the virtual pad.  When checking which virtual pad is assigned to the 
    /// keyboard or when adding a new keyboard, refer to the value of VirtualKeyMaps.
    ///
    /// デバイスからの入力を管理するコンポーネントです。
    /// ゲームパッドとキーボードの入力状態から仮想パッドの状態に反映します。
    /// キーボードがどの仮想パッドに割り当てられているか、または追加する場合は
    /// VirtualKeyMapsの値を参照してください。
    /// </summary>
    public class InputComponent : GameComponent
    {
        #region Fields
        private readonly PlayerIndex[] Players = {
            PlayerIndex.One,
            PlayerIndex.Two,
            PlayerIndex.Three,
            PlayerIndex.Four
        };

        private float deadZone;
        private Dictionary<PlayerIndex, KeyboardState> keyboardStates;
        private Dictionary<PlayerIndex, GamePadState> gamePadStates;
        private Dictionary<PlayerIndex, VirtualPadState> virtualPadStates;
        private Dictionary<InputState, Keys> virtualKeyMaps;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the state of the keyboard.
        ///
        /// キーボード状態を取得します。
        /// </summary>
        public Dictionary<PlayerIndex, KeyboardState> KeyboardStates
        {
            get { return keyboardStates; }
        }


        /// <summary>
        /// Obtains the state of the game pad.
        ///
        /// ゲームパッド状態を取得します。
        /// </summary>
        public Dictionary<PlayerIndex, GamePadState> GamePadStates
        {
            get { return gamePadStates; }
        }


        /// <summary>
        /// Obtains the state of the virtual pad.
        ///
        /// 仮想パッド状態を取得します。
        /// </summary>
        public Dictionary<PlayerIndex, VirtualPadState> VirtualPadStates
        {
            get { return virtualPadStates; }
        }


        /// <summary>
        /// Obtains the key map.
        ///
        /// キーマップを取得します。
        /// </summary>
        public Dictionary<InputState, Keys> VirtualKeyMaps
        {
            get { return virtualKeyMaps; }
        }


        /// <summary>
        /// Obtains or sets the non-response area of the analog stick.
        ///
        /// アナログスティックの無反応領域を取得または設定します。
        /// </summary>
        public float DeadZone
        {
            get { return deadZone; }
            set { deadZone = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public InputComponent(Game game)
            : base(game)
        {
        }


        /// <summary>
        /// Performs initialization.
        ///
        /// 初期化を行います。
        /// </summary>
        public override void Initialize()
        {
            deadZone = 0.5f;
            keyboardStates = new Dictionary<PlayerIndex, KeyboardState>();
            gamePadStates = new Dictionary<PlayerIndex, GamePadState>();
            virtualPadStates = new Dictionary<PlayerIndex, VirtualPadState>();
            virtualKeyMaps = new Dictionary<InputState, Keys>();

            foreach (PlayerIndex index in Players)
            {
                keyboardStates.Add(index, Keyboard.GetState(index));
                gamePadStates.Add(index, GamePad.GetState(index));
                virtualPadStates.Add(index, new VirtualPadState());
            }

            InitializeVirtualKeyMaps();

            base.Initialize();
        }


        /// <summary>
        /// Perform initialization for key mapping.
        /// 
        /// キーマッピングの初期化を行います。
        /// </summary>
        public void InitializeVirtualKeyMaps()
        {
            VirtualPadState virtualPad = virtualPadStates[PlayerIndex.One];

            virtualKeyMaps.Clear();

            // Arrow
            virtualKeyMaps.Add(virtualPad.ThumbSticks.Left.Up, Keys.Up);
            virtualKeyMaps.Add(virtualPad.ThumbSticks.Left.Down, Keys.Down);
            virtualKeyMaps.Add(virtualPad.ThumbSticks.Left.Left, Keys.Left);
            virtualKeyMaps.Add(virtualPad.ThumbSticks.Left.Right, Keys.Right);
            virtualKeyMaps.Add(virtualPad.DPad.Up, Keys.Up);
            virtualKeyMaps.Add(virtualPad.DPad.Down, Keys.Down);
            virtualKeyMaps.Add(virtualPad.DPad.Left, Keys.Left);
            virtualKeyMaps.Add(virtualPad.DPad.Right, Keys.Right);

            // Buttons
            virtualKeyMaps.Add(virtualPad.Buttons.A, Keys.Enter);
            virtualKeyMaps.Add(virtualPad.Buttons.B, Keys.Escape);
            virtualKeyMaps.Add(virtualPad.Buttons.X, Keys.X);
            virtualKeyMaps.Add(virtualPad.Buttons.Y, Keys.Y);
            virtualKeyMaps.Add(virtualPad.Buttons.LeftShoulder, Keys.Q);
            virtualKeyMaps.Add(virtualPad.Buttons.RightShoulder, Keys.E);
            virtualKeyMaps.Add(virtualPad.Buttons.Start, Keys.Tab);
            virtualKeyMaps.Add(virtualPad.Buttons.Back, Keys.Escape);
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the keys.
        ///
        /// キーの更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            UpdateGamePadState();
            UpdateKeyboardState();

            AssignVirtualPad();
            UpdateVirtualPad();

            base.Update(gameTime);
        }

        /// <summary>
        /// Updates the game pad information.
        ///
        /// ゲームパッドの情報を更新します。
        /// </summary>
        private void UpdateGamePadState()
        {
            foreach (PlayerIndex index in Players)
            {
                gamePadStates[index] = GamePad.GetState(index);
            }
        }


        /// <summary>
        /// Updates the keyboard information.
        ///
        /// キーボードの情報を更新します。
        /// </summary>
        private void UpdateKeyboardState()
        {
            foreach (PlayerIndex index in Players)
            {
                keyboardStates[index] = Keyboard.GetState(index);
            }
        }


        /// <summary>
        /// Updates the virtual key information.
        ///
        /// 仮想キーの情報を更新します。
        /// </summary>
        private void UpdateVirtualPad()
        {
            foreach (PlayerIndex index in Players)
            {
                virtualPadStates[index].Update();
            }
        }



        #endregion

        #region Helper Methods
        /// <summary>
        /// Sets the state for when a virtual key is pressed.
        ///
        /// 仮想キーの押下状態を設定します。
        /// </summary>
        private void AssignVirtualPad()
        {
            foreach (PlayerIndex index in Players)
            {
                AssignVirtualPad(index);
            }
        }


        /// <summary>
        /// Sets the state for when a virtual key is pressed.
        ///
        /// 仮想キーの押下状態を設定します。
        /// </summary>
        private void AssignVirtualPad(PlayerIndex index)
        {
            GamePadState gamePad = gamePadStates[index];
            VirtualPadState virtualPad = virtualPadStates[index];

            // Clear
            virtualPad.SetPress(false);

            // Buttons
            VirtualPadButtons buttons = virtualPad.Buttons;
            buttons.A.IsPress = gamePad.IsButtonDown(Buttons.A);
            buttons.B.IsPress = gamePad.IsButtonDown(Buttons.B);
            buttons.X.IsPress = gamePad.IsButtonDown(Buttons.X);
            buttons.Y.IsPress = gamePad.IsButtonDown(Buttons.Y);
            buttons.LeftShoulder.IsPress = gamePad.IsButtonDown(Buttons.LeftShoulder);
            buttons.RightShoulder.IsPress = gamePad.IsButtonDown(Buttons.RightShoulder);
            buttons.LeftStick.IsPress = gamePad.IsButtonDown(Buttons.LeftStick);
            buttons.RightStick.IsPress = gamePad.IsButtonDown(Buttons.RightStick);
            buttons.Back.IsPress = gamePad.IsButtonDown(Buttons.Back);
            buttons.Start.IsPress = gamePad.IsButtonDown(Buttons.Start);

            // DPad
            VirtualPadDPad dPad = virtualPad.DPad;
            dPad.Up.IsPress = gamePad.IsButtonDown(Buttons.DPadUp);
            dPad.Down.IsPress = gamePad.IsButtonDown(Buttons.DPadDown);
            dPad.Left.IsPress = gamePad.IsButtonDown(Buttons.DPadLeft);
            dPad.Right.IsPress = gamePad.IsButtonDown(Buttons.DPadRight);

            // ThumbSticks
            VirtualPadThumbSticks thumbSticks = virtualPad.ThumbSticks;
            Vector2 leftThumbStick = gamePad.ThumbSticks.Left;
            Vector2 rightThumbStick = gamePad.ThumbSticks.Right;
            thumbSticks.Left.Up.IsPress = (leftThumbStick.Y > deadZone);
            thumbSticks.Left.Down.IsPress = (leftThumbStick.Y < -deadZone);
            thumbSticks.Left.Left.IsPress = (leftThumbStick.X < -deadZone);
            thumbSticks.Left.Right.IsPress = (leftThumbStick.X > deadZone);
            thumbSticks.Right.Up.IsPress = (rightThumbStick.Y > deadZone);
            thumbSticks.Right.Down.IsPress = (rightThumbStick.Y < -deadZone);
            thumbSticks.Right.Left.IsPress = (rightThumbStick.X < -deadZone);
            thumbSticks.Right.Right.IsPress = (rightThumbStick.X > deadZone);

            // Triggers
            VirtualPadTriggers triggers = virtualPad.Triggers;
            triggers.Left.IsPress = gamePad.IsButtonDown(Buttons.LeftTrigger);
            triggers.Right.IsPress = gamePad.IsButtonDown(Buttons.RightTrigger);

            // Keybord Mapping
            foreach (KeyValuePair<InputState, Keys> pair in virtualKeyMaps)
            {
                bool keyDown = keyboardStates[index].IsKeyDown(pair.Value);
                pair.Key.IsPress = (keyDown) ? true : pair.Key.IsPress;
            }
        }

        #endregion
    }
}