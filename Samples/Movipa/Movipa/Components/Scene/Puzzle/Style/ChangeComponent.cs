#region File Description
//-----------------------------------------------------------------------------
// ChangeComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Movipa.Components.Input;
using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Puzzle.Style
{
    /// <summary>
    /// Implements the Change Mode style.
    /// This class inherits the StyleBase class and implements panel swapping.
    /// It swaps between the two panels and rotates the panels.
    /// 
    /// チェンジモードのスタイルを実装します。
    /// このクラスはStyleBaseクラスを継承し、パネルの入れ替え処理を実装しています。
    /// 2つのパネルを入れ替える処理と、パネルを回転させる処理を行っています。
    /// </summary>
    public class ChangeComponent : StyleBase
    {
        #region Fields
        // Panel holding status
        // 
        // パネルの保持状態
        private bool isPanelHold = false;

        // Coordinates of holding panel
        // 
        // ホールドしているパネルの座標
        private Point panelHold = new Point();
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance. 
        /// 
        /// インスタンスの初期化をします。
        /// </summary>
        public ChangeComponent(Game game, 
            StageSetting setting, Point cursor, PanelManager manager)
            : base(game, setting, cursor, manager)
        {
        }


        /// <summary>
        /// Loads the content.
        ///
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the cursor texture.
            // 
            // カーソルのテクスチャを読み込みます。
            string asset = "Textures/Puzzle/cursor";
            cursorTexture = Content.Load<Texture2D>(asset);

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        /// 
        /// 更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Checks for selection enabled status.
            // 
            // 選択有効状態をチェックします。
            if (SelectEnabled)
            {
                // Moves the cursor.
                // 
                // カーソルの移動処理を行います。
                UpdateCursor();

                // Performs panel operation.
                // 
                // パネルの操作を行います。
                UpdatePanel();
            }

            // Updates all panels.
            // 
            // 全てのパネルの更新処理を行います。
            UpdatePanels(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Performs panel operation.
        /// 
        /// パネルの操作を行います。
        /// </summary>
        private void UpdatePanel()
        {
            // Processing is not performed if the panel is currently in action.
            // 
            // パネルが動作中ならば処理を行いません。
            if (IsPanelAction)
                return;

            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            if (buttons.A[VirtualKeyState.Push])
            {
                // Obtains the panel.
                // 
                // パネルを取得します。
                PanelData panel = panelManager.GetPanel(cursor);

                // Processing is not performed if the panel is disabled.
                // 
                // パネルが無効の状態ならば処理をしません。
                if (!panel.Enabled)
                    return;

                // Checks the hold status.
                // 
                // ホールド状態をチェックします。
                if (isPanelHold == false)
                {
                    // There is no held panel, so the 
                    // selected panel is held.
                    // 
                    // ホールドしているパネルが無いので
                    // 選択しているパネルをホールドします。
                    if (HoldPanel())
                    {
                        // Plays the SoundEffect if the hold is successful.
                        // 
                        // ホールドに成功したらSoundEffectを再生します。
                        GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
                    }
                }
                else
                {
                    // There is already a held panel, so the 
                    // selected panel is swapped with the panel
                    // that has already been held.
                    // 
                    // 既にホールドしているパネルがあるので、
                    // 選択しているパネルと、ホールド済みのパネルと
                    // 入れ替える処理を行います。
                    if (SwapPanel())
                    {
                        // Plays the SoundEffect if the swap is successful.
                        // 
                        // 入れ替えに成功したらSoundEffectを再生します。
                        GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                        // Calculates the movement count. 
                        // 
                        // 移動カウントを加算します。
                        moveCount++;
                    }
                }
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Releases the held panel.
                // 
                // ホールドしているパネルを開放します。
                ReleasePanel();
            }

            // Controls panel rotation.
            // 
            // パネルの回転の制御を行います。
            RotatePanel();
        }


        /// <summary>
        /// Performs cursor movement processing.
        /// 
        /// カーソル移動処理を行います。
        /// </summary>
        private void UpdateCursor()
        {
            // Manages the SoundEffect by the flag only once so that
            // the SoundEffect will not be played simultaneously.
            // 
            // SoundEffectの同時再生を防ぐために一度フラグで管理します。
            bool sePlay = false;

            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadDPad dPad = virtualPad.DPad;
            VirtualPadDPad leftStick = virtualPad.ThumbSticks.Left;

            if (InputState.IsPushRepeat(dPad.Up, leftStick.Up))
            {
                // Performs Up key processing.
                // 
                // 上キーが押されたときの処理を行います。
                InputUpKey();

                // Sets the SoundEffect play flag.
                // 
                // SoundEffectの再生フラグを設定します。
                sePlay = true;
            }

            if (InputState.IsPushRepeat(dPad.Down, leftStick.Down))
            {
                // Performs Down key processing.
                // 
                // 下キーが押されたときの処理を行います。
                InputDownKey();

                // Sets the SoundEffect play flag.
                // 
                // SoundEffectの再生フラグを設定します。
                sePlay = true;
            }

            if (InputState.IsPushRepeat(dPad.Left, leftStick.Left))
            {
                // Performs Left key processing.
                // 
                // 左キーが押されたときの処理を行います。
                InputLeftKey();

                // Sets the SoundEffect play flag.
                // 
                // SoundEffectの再生フラグを設定します。
                sePlay = true;
            }

            if (InputState.IsPushRepeat(dPad.Right, leftStick.Right))
            {
                // Performs Right key processing.
                // 
                // 右キーが押されたときの処理を行います。
                InputRightKey();

                // Sets the SoundEffect play flag.
                // 
                // SoundEffectの再生フラグを設定します。
                sePlay = true;
            }


            // Plays sound if the sound play flag is set.
            // 
            // サウンド再生フラグが設定されていれば再生します。
            if (sePlay)
            {
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor2);
            }
        }


        /// <summary>
        /// Performs Up key processing.
        /// 
        /// 上キーが押されたときの処理を行います。
        /// </summary>
        private void InputUpKey()
        {
            // Sets the cursor position.
            // 
            // カーソル位置を設定
            int newX = cursor.X;
            int newY = cursor.Y - 1;

            if (newY < 0)
            {
                newY = stageSetting.Divide.Y - 1;
            }

            cursor.X = newX;
            cursor.Y = newY;
        }


        /// <summary>
        /// Performs Down key processing.
        /// 
        /// 下キーが押されたときの処理を行います。
        /// </summary>
        private void InputDownKey()
        {
            // Sets the cursor position.
            // 
            // カーソル位置を設定
            int newX = cursor.X;
            int newY = (cursor.Y + 1) % stageSetting.Divide.Y;
            cursor.X = newX;
            cursor.Y = newY;
        }


        /// <summary>
        /// Performs Left key processing.
        /// 
        /// 左キーが押されたときの処理を行います。
        /// </summary>
        private void InputLeftKey()
        {
            // Sets the cursor position.
            // 
            // カーソル位置を設定
            int newX = cursor.X - 1;
            int newY = cursor.Y;

            if (newX < 0)
            {
                newX = stageSetting.Divide.X - 1;
            }

            cursor.X = newX;
            cursor.Y = newY;
        }


        /// <summary>
        /// Performs Right key processing.
        /// 
        /// 右キーが押されたときの処理を行います。
        /// </summary>
        private void InputRightKey()
        {
            // Sets the cursor position.
            // 
            // カーソル位置を設定
            int newX = (cursor.X + 1) % stageSetting.Divide.X;
            int newY = cursor.Y;

            cursor.X = newX;
            cursor.Y = newY;
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws all position cursors.
        /// 
        /// 全ての位置のカーソルを描画します。
        /// </summary>
        public override void DrawCursor(GameTime gameTime)
        {
            Batch.Begin();

            // Draws the hold cursor.
            // 
            // ホールドカーソルを描画します。
            DrawHoldCursor(gameTime);

            // Draws the normal cursor.
            // 
            // 通常のカーソルを描画します。
            DrawNormalCursor(gameTime);
            
            Batch.End();
        }


        /// <summary>
        /// Draws the normal cursor.
        /// 
        /// 通常のカーソルを描画します。
        /// </summary>
        private void DrawNormalCursor(GameTime gameTime)
        {
            // Obtains the panel at the cursor position.
            // 
            // カーソル位置にあるパネルを取得します。
            PanelData panel = panelManager.GetPanel(cursor);

            // Obtains the cursor rectangle.
            // 
            // カーソルの矩形を取得します。
            Rectangle rectangle = panel.RectanglePosition;

            // Draws all four corners of the cursor.
            // 
            // 四隅全てのカーソルを描画します。
            DrawCursor(gameTime, rectangle, Color.White, PanelTypes.All);
        }


        /// <summary>
        /// Draws the hold cursor.
        /// 
        /// ホールドカーソルを描画します。
        /// </summary>
        private void DrawHoldCursor(GameTime gameTime)
        {
            // Drawing processing is not performed if there is no hold status. 
            // 
            // ホールド状態が無い場合は描画処理を行いません。
            if (!isPanelHold)
                return;

            // Obtains the held panels.
            // 
            // ホールドしているパネルを取得します。
            PanelData panel = panelManager.GetPanel(panelHold);

            // Obtains the cursor rectangle.
            // 
            // カーソルの矩形を取得します。
            Rectangle rectangle = panel.RectanglePosition;

            // Draws all four corners of the cursor.
            // 
            // 四隅全てのカーソルを描画します。
            DrawCursor(gameTime, rectangle, Color.Red, PanelTypes.All);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Swaps the panels.
        /// 
        /// パネルの交換処理を行います。
        /// </summary>
        private bool SwapPanel()
        {
            // Processing is not performed if it is the same panel.
            // 
            // 同じパネルなら処理を行いません。
            if (cursor.Equals(panelHold) == true)
            {
                return false;
            }

            // Arranges the swap panels in an array.
            // 
            // 入れ替えるパネルを配列にセットします。
            PanelData[] panels =
            {
                panelManager.GetPanel(panelHold),
                panelManager.GetPanel(cursor),
            };

            // Processing is not performed if the panel is currently in action.
            // 
            // パネルが動作中なら処理を行いません。
            if (panels[0].Status != PanelData.PanelStatus.None ||
                panels[1].Status != PanelData.PanelStatus.None)
            {
                return false;
            }

            // Sets the panel From and To positions.
            // 
            // パネルの移動元と、移動先を設定します。
            Vector2[,] panelFromTo = {
                {
                    panels[0].Position,
                    panels[1].Position
                },
                {
                    panels[1].Position,
                    panels[0].Position
                }
            };

            // Changes the draw position.
            // 
            // 描画位置の変更をします。
            panels[0].FromPosition = panelFromTo[0, 0];
            panels[0].ToPosition = panelFromTo[0, 1];
            panels[1].FromPosition = panelFromTo[1, 0];
            panels[1].ToPosition = panelFromTo[1, 1];

            // Swaps the panel data.
            // 
            // パネルのデータを交換します。
            panelManager.SetPanel(panelHold, panels[1]);
            panelManager.SetPanel(cursor, panels[0]);

            // Sets the panel status.
            // 
            // パネルの状態を設定します。
            panels[0].Status = PanelData.PanelStatus.Move;
            panels[1].Status = PanelData.PanelStatus.Move;
            panels[0].Color = ActivePanelColor;
            panels[1].Color = ActivePanelColor;
            panels[0].MoveCount = 1.0f;
            panels[1].MoveCount = 1.0f;

            // Cancels the hold status.
            // 
            // ホールド状態を解除します。
            isPanelHold = false;

            return true;

        }


        /// <summary>
        /// Holds the panel.
        /// 
        /// パネルをホールドします。
        /// </summary>
        private bool HoldPanel()
        {
            // Obtains the panel at the cursor position.
            // 
            // カーソル位置にあるパネルを取得します。
            PanelData panel = panelManager.GetPanel(cursor);

            // Processing is not performed if the panel is currently in action.
            //
            // パネルが動作中なら処理を行いません。
            if (panel.Status != PanelData.PanelStatus.None)
                return false;

            // Sets to the held panel.
            //
            // ホールドパネルに設定します。
            panelHold.X = cursor.X;
            panelHold.Y = cursor.Y;
            isPanelHold = true;

            return true;
        }


        /// <summary>
        /// Releases the held panel.
        /// 
        /// ホールドしているパネルを開放します。
        /// </summary>
        private void ReleasePanel()
        {
            // If there are held panels, the hold status
            // is cancelled and the canceled SoundEffect is played. 
            // 
            // ホールドしているパネルがあるなら、ホールド状態を
            // 解除してキャンセルのSoundEffectを再生します。
            if (isPanelHold == true)
            {
                isPanelHold = false;
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCancel);
            }
        }


        /// <summary>
        /// Controls panel rotation.
        /// 
        /// パネルの回転の制御を行います。
        /// </summary>
        private void RotatePanel()
        {

            // Processing is not performed if rotation is disabled.
            // 
            // 回転が無効の場合は処理を行いません。
            if (stageSetting.Rotate == StageSetting.RotateMode.Off)
                return;

            // Processing is not performed in Hold status.
            // 
            // ホールド状態なら処理を行いません。
            if (isPanelHold)
                return;


            VirtualPadState virtualPad = 
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            
            // Obtains the panel at the cursor position.
            // 
            // カーソル位置にあるパネルを取得します。
            PanelData panel = panelManager.GetPanel(cursor);

            // Processing is not performed if the panel status is Completed.
            // 
            // パネルの状態が完成済みなら処理を行いません。
            if (!panel.Enabled)
                return;

            if (buttons.Y[VirtualKeyState.Push])
            {
                // Rotates the panel to the left when the Y button is pressed.
                // 
                // Yボタンが押されたらパネルを左回転させます。
                if (RotatePanelLeft(panel))
                {
                    // Plays the SoundEffect during rotation.
                    // 
                    // 回転時のSoundEffectを再生します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                    // Calculates the number of moves.
                    // 
                    // 移動回数を加算します。
                    moveCount++;
                }
            }
            else if (buttons.X[VirtualKeyState.Push])
            {
                    // Rotates the panel to the right when the X button is pressed.
                    // 
                // Xボタンが押されたらパネルを右回転させます。
                if (RotatePanelRight(panel))
                {
                    // Plays the SoundEffect during rotation.
                    // 
                    // 回転時のSoundEffectを再生します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                    // Calculates the number of moves.
                    // 
                    // 移動回数を加算します。
                    moveCount++;
                }
            }
        }


        /// <summary>
        /// Rotates the panel to the left.
        /// 
        /// パネルを左回転させます。
        /// </summary>
        private bool RotatePanelLeft(PanelData panelData)
        {
            if (panelData.Status != PanelData.PanelStatus.None)
                return false;

            // Changes the panel status.
            // 
            // パネルの状態を変更します。
            panelData.Color = ActivePanelColor;
            panelData.ToRotate += 90;
            panelData.ToRotate %= 360;
            panelData.Status = PanelData.PanelStatus.RotateLeft;

            return true;
        }


        /// <summary>
        /// Rotates the panel to the right.
        /// 
        /// パネルを右回転させます。
        /// </summary>
        private bool RotatePanelRight(PanelData panelData)
        {
            if (panelData.Status != PanelData.PanelStatus.None)
                return false;

            // Changes the panel status.
            // 
            // パネルの状態を変更します。
            panelData.Color = ActivePanelColor;
            panelData.ToRotate += 270;
            panelData.ToRotate %= 360;
            panelData.Status = PanelData.PanelStatus.RotateRight;

            return true;
        }


        /// <summary>
        /// Replaces the panels at random.
        /// 
        /// ランダムでパネルを入れ替えます。
        /// </summary>
        public override bool RandomShuffle()
        {
            // Processing is not performed if the panel is currently in action.
            // 
            // パネルが動作中なら処理を行いません。
            if (IsPanelAction)
                return false;

            // Randomly sets rotation.
            // 
            // 回転の処理をランダムで設定します。
            bool rotate = false;
            if (stageSetting.Rotate == StageSetting.RotateMode.On)
            {
                // If rotation is enabled and the randomly generated value is
                // 0, shuffle rotation is enabled.
                // 
                // 回転が有効で、ランダムで出した数値が0なら
                // 回転のシャッフルを有効にします。
                rotate = (Random.Next(0, 2) == 0);
            }

            if (rotate)
            {
                // Rotates the panel at random.
                //
                // ランダムでパネルを回転させます。
                RandomShuffleRotate();
            }
            else
            {
                // Replaces the panels at random.
                // 
                // ランダムでパネルを入れ替えます。
                RandomShuffleSwap();
            }

            return true;
        }


        /// <summary>
        /// Rotates the panel at random.
        /// 
        /// ランダムでパネルを回転させます。
        /// </summary>
        private void RandomShuffleRotate()
        {
            // Obtains the target panel.
            // 
            // 対象のパネルを取得します。
            cursor = panelManager.GetRandomPanel(stageSetting);

            // Obtains the panel at the cursor position.
            //
            // カーソル位置にあるパネルを取得します。
            PanelData panel = panelManager.GetPanel(cursor);

            // Randomly sets the rotation direction.
            // 
            // ランダムで回転方向を設定します。
            if (Random.Next(2) == 0)
            {
                RotatePanelLeft(panel);
            }
            else
            {
                RotatePanelRight(panel);
            }

            // Plays the SoundEffect during rotation.
            // 
            // 回転のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }


        /// <summary>
        /// Replaces the panel at random.
        /// 
        /// ランダムでパネルを入れ替えます。
        /// </summary>
        private void RandomShuffleSwap()
        {
            Point holdPanel = new Point();
            Point swapPanel = new Point();

            // Selects the panel to hold.
            // 
            // ホールドするパネルを選択します。
            holdPanel = panelManager.GetRandomPanel(stageSetting);

            // Holds the panel.
            // 
            // パネルをホールドします。
            cursor = holdPanel;
            HoldPanel();

            // Selects a panel at random until a panel 
            // other than the held panel is selected.
            // 
            // ホールドしたパネルとは別のパネルが選択されるまで
            // ランダムに選択します。
            do
            {
                if (panelManager.PanelCompleteCount(stageSetting) != 1)
                {
                    // Obtains the swap destination panel.
                    // 
                    // 交換先のパネルを取得します。
                    swapPanel = panelManager.GetRandomPanel(stageSetting);
                }
                else
                {
                    // If there is only one panel that is not being shuffled, 
                    // this will be the only panel available for selection, 
                    // resulting in an infinite loop, so it must be avoided.
                    // 
                    // シャッフルされていないパネルが1個だけの場合は
                    // それしか選択されず無限ループされるので回避
                    swapPanel.X = Random.Next(0, stageSetting.Divide.X);
                    swapPanel.Y = Random.Next(0, stageSetting.Divide.Y);
                }
            } while (holdPanel.Equals(swapPanel));
            cursor = swapPanel;

            // Swaps the panels.
            // 
            // パネルの交換処理を行います。
            SwapPanel();

            // Plays the panel swap SoundEffect.
            // 
            // パネル交換のSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
        }
        #endregion
    }
}


