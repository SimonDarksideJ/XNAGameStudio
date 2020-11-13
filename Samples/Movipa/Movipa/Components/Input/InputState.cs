#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace Movipa.Components.Input
{
    /// <summary>
    /// This class manages input state.
    /// Specifically, it manages the following states:
    /// When there is no key input, when key input is started,  
    /// while key input is being performed, and when key input is ended.
    /// It also determines key repeat while key input is being performed.
    ///
    /// 入力状態を管理します。
    /// 入力されていない状態、入力された瞬間、入力が持続している状態、
    /// そして入力が終わった瞬間の状態を管理します。
    /// また、入力が持続している状態のリピート判定なども行います。
    /// </summary>
    public class InputState
    {
        #region Fields
        private int startInterval = 60;
        private int repeatInterval = 10;
        private VirtualKeyState state = VirtualKeyState.Free;
        private int pressCount = 0;
        private bool isPress = false;
        #endregion

        #region Properties
        /// <summary>
        /// Specify the key state.
        /// If it is equal to the virtual key state, return True.
        ///
        /// キー状態を指定して同じであればTrueを返します。
        /// </summary>
        /// <param name="virtualKeyState"></param>
        /// <returns></returns>
        public bool this[VirtualKeyState virtualKeyState]
        {
            get { return (virtualKeyState == State); }
        }


        /// <summary>
        /// Obtains or sets the number of frames to start key repeat.
        ///
        /// キーリピートを開始するまでのフレーム数を取得または設定します。
        /// </summary>
        public int StartInterval
        {
            get { return startInterval; }
            set { startInterval = value; }
        }


        /// <summary>
        /// Obtains or sets the frame interval for key repeat.
        ///
        /// キーリピートのフレーム間隔を取得または設定します。
        /// </summary>
        public int RepeatInterval
        {
            get { return repeatInterval; }
            set { repeatInterval = value; }
        }


        /// <summary>
        /// Obtains the key state for when it is pressed. 
        /// The value is masked by "3". Determine the key state by the following bits:
        /// 
        /// bits
        ///  00  : key is not pressed.
        ///  01  : key was pressed.
        ///  11  : key is being pressed.
        ///  10  : key was released.
        /// 
        /// キーの押下状態を取得します。
        /// 値は3でマスクされ、ビットによって押下状態を判定します。
        /// 
        /// bits
        ///  00  : 離されています。
        ///  01  : 押されました。
        ///  11  : 押されています。
        ///  10  : 離されました。
        /// </summary>
        public VirtualKeyState State
        {
            get { return (VirtualKeyState)((int)state & 0x03); }
        }


        /// <summary>
        /// Obtains the repeat state of the key.
        ///
        /// キーのリピート状態を取得します。
        /// </summary>
        public bool Repeat
        {
            get
            {
                // Does not determine key repeat when the key is not pressed.
                // 
                // キーが離されている時はリピート判定をしません。
                if (State == VirtualKeyState.Free || State == VirtualKeyState.Release)
                    return false;

                if (State == VirtualKeyState.Push)
                    return true;

                if (pressCount < StartInterval)
                    return false;

                return (((pressCount - StartInterval) % RepeatInterval) == 0);
            }
        }


        /// <summary>
        /// Obtains or sets the key state for when it is pressed.
        ///
        /// キーの押下状態を取得または設定します。
        /// </summary>
        public bool IsPress
        {
            get { return isPress; }
            set { isPress = value; }
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the key state.
        ///
        /// キー状態の更新処理を行います。
        /// </summary>
        public void Update()
        {
            // Updates the frame in which the key is pressed.
            // When obtaining the key state, uses the properties and masks it by "3".
            //
            // キーの押下フレームを更新します。
            // 取得する場合はプロパティ使用し、3でマスクを行います。
            state = (VirtualKeyState)((int)State << 1);
            if (IsPress)
                state = (VirtualKeyState)((int)State | 1);

            // Increments the interval value.
            // 
            // インターバルの処理を行います。
            pressCount = (IsPress) ? pressCount + 1 : 0;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Sets the key state for when it is pressed.
        ///
        /// キーの押下状態を設定します。
        /// </summary>
        public void SetPress(bool press)
        {
            isPress = press;
        }


        /// <summary>
        /// Checks if any of the keys is in the "Push" state.
        ///
        /// 複数のキーのいずれかが、Push状態になっているかチェックします。
        /// </summary>
        public static bool IsPush(params InputState[] keys)
        {
            foreach (InputState inputState in keys)
            {
                if (inputState[VirtualKeyState.Push])
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Checks if any of the keys is in the "Push" or "Repeat" state.
        ///
        /// 複数のキーのいずれかが、PushまたはRepeat状態になっているかチェックします。
        /// </summary>
        public static bool IsPushRepeat(params InputState[] keys)
        {
            foreach (InputState inputState in keys)
            {
                if (inputState[VirtualKeyState.Push] || inputState.Repeat)
                    return true;
            }

            return false;
        }
        #endregion
    }
}