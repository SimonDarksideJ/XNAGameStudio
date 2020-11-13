#region File Description
//-----------------------------------------------------------------------------
// RevolveComponent.cs
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
    /// Implements the Revolve Mode style.
    /// This class inherits the StyleBase class and implements 
    /// panel switching. It rotates four panels expanded one 
    /// down and one to the right of the cursor position.
    /// 
    /// リボルヴモードのスタイルを実装します。
    /// このクラスはStyleBaseクラスを継承し、パネルの入れ替え処理を実装しています。
    /// カーソルの位置から、右と下方向に1枚ずつパネルを拡張した4枚のパネルを
    /// 回転する処理を行っています。
    /// </summary>
    public class RevolveComponent : StyleBase
    {
        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public RevolveComponent(Game game, 
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
            // Checks the select enabled status.
            // 
            // 選択有効状態をチェックします。
            if (SelectEnabled)
            {
                // Moves the cursor.
                // 
                // カーソルの移動処理を行います。
                UpdateCursor();

                // Replaces the cursor.
                // 
                // カーソルの入れ替え処理を行います。
                RevolvePanel();
            }

            // Updates all panels.
            // 
            // 全てのパネルの更新処理を行います。
            UpdatePanels(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// Moves the cursor.
        /// 
        /// カーソル移動処理を行います。
        /// </summary>
        private void UpdateCursor()
        {
            // Manages the SoundEffect by the flag only once so that the SoundEffect 
            // will not be played simultaneously.
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

                // Sets the SoundEffect playback flag.
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

                // Sets the SoundEffect playback flag.
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

                // Sets the SoundEffect playback flag.
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

                // Sets the SoundEffect playback flag.
                // 
                // SoundEffectの再生フラグを設定します。
                sePlay = true;
            }


            // Plays the SoundEffect if the sound playback flag is set.
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
            int newX = cursor.X;
            int newY = cursor.Y - 1;

            if (newY < 0)
            {
                newY = stageSetting.Divide.Y - 2;
            }

            cursor = new Point(newX, newY);
        }


        /// <summary>
        /// Performs Down key processing.
        /// 
        /// 下キーが押されたときの処理を行います。
        /// </summary>
        private void InputDownKey()
        {
            int newX = cursor.X;
            int newY = (cursor.Y + 1) % (stageSetting.Divide.Y - 1);
            cursor = new Point(newX, newY);
        }


        /// <summary>
        /// Performs Left key processing.
        /// 
        /// 左キーが押されたときの処理を行います。
        /// </summary>
        private void InputLeftKey()
        {
            int newX = cursor.X - 1;
            int newY = cursor.Y;

            if (newX < 0)
            {
                newX = stageSetting.Divide.X - 2;
            }

            cursor = new Point(newX, newY);
        }


        /// <summary>
        /// Performs Right key processing.
        /// 
        /// 右キーが押されたときの処理を行います。
        /// </summary>
        private void InputRightKey()
        {
            int newX = (cursor.X + 1) % (stageSetting.Divide.X - 1);
            int newY = cursor.Y;
            cursor = new Point(newX, newY);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws cursors at all positions. 
        /// 
        /// 全ての位置のカーソルを描画します。
        /// </summary>
        public override void DrawCursor(GameTime gameTime)
        {
            // Sets rectangles of four panels consisting of the panels
            // at the cursor position expanded to the right and down.
            // 
            // カーソル位置のパネルと、右方向と下方向に1枚ずつのばした
            // 4枚のパネルの矩形を設定します。
            Rectangle[] cursorRect = {
                new Rectangle(
                    (int)(cursor.X * panelManager.PanelSize.X),
                    (int)(cursor.Y * panelManager.PanelSize.Y),
                    (int)(panelManager.PanelSize.X),
                    (int)(panelManager.PanelSize.Y)
                ),
                new Rectangle(
                    (int)(cursor.X * panelManager.PanelSize.X),
                    (int)((cursor.Y * panelManager.PanelSize.Y) +
                        panelManager.PanelSize.Y),
                    (int)(panelManager.PanelSize.X),
                    (int)(panelManager.PanelSize.Y)
                ),
                new Rectangle(
                    (int)((cursor.X * panelManager.PanelSize.X) +
                        panelManager.PanelSize.X),
                    (int)(cursor.Y * panelManager.PanelSize.Y),
                    (int)(panelManager.PanelSize.X),
                    (int)(panelManager.PanelSize.Y)
                ),
                new Rectangle(
                    (int)((cursor.X * panelManager.PanelSize.X) +
                        panelManager.PanelSize.X),
                    (int)((cursor.Y * panelManager.PanelSize.Y) +
                        panelManager.PanelSize.Y),
                    (int)(panelManager.PanelSize.X),
                    (int)(panelManager.PanelSize.Y)
                ),
            };

            // Sets the four-panel cursor draw flag.
            // 
            // 4枚のパネルのカーソル描画フラグを設定します。
            PanelTypes[] types = {
                PanelTypes.LeftUp | PanelTypes.LeftDown | PanelTypes.RightUp,
                PanelTypes.LeftUp | PanelTypes.LeftDown | PanelTypes.RightDown,
                PanelTypes.LeftUp | PanelTypes.RightUp | PanelTypes.RightDown,
                PanelTypes.LeftDown | PanelTypes.RightUp | PanelTypes.RightDown
            };


            Batch.Begin();
            
            // Draws the four-panel cursor.
            // 
            // 4枚のパネルのカーソルを描画します。
            for (int i = 0; i < cursorRect.Length; i++)
            {
                DrawCursor(gameTime, cursorRect[i], Color.White, types[i]);
            }

            // Draws the central cursor.
            // 
            // 中央のカーソルを描画します。
            DrawCenterCursor();

            Batch.End();
        }


        /// <summary>
        /// Draws the central cursor.
        /// 
        /// 中央のカーソルを描画します。
        /// </summary>
        public void DrawCenterCursor()
        {
            Rectangle cursorSize = new Rectangle(0, 0, 48, 48);
            
            // Calculates the coordinates of the texture source.
            // 
            // テクスチャの転送元の座標を計算します。
            Rectangle centerCursorArea = new Rectangle();
            centerCursorArea.X = 192;
            centerCursorArea.Y = (cursorSize.Height * 2) * 0;
            centerCursorArea.Width = cursorSize.Width;
            centerCursorArea.Height = cursorSize.Height;

            // Calculates the source coordinates.
            // 
            // 転送先の座標を計算します。
            Vector2 centerCursorPosition = new Vector2();
            centerCursorPosition.X = 
                (cursor.X * panelManager.PanelSize.X) + panelManager.PanelSize.X;
            centerCursorPosition.Y = 
                (cursor.Y * panelManager.PanelSize.Y) + panelManager.PanelSize.Y;
            centerCursorPosition.X -= (cursorSize.Width * 0.5f);
            centerCursorPosition.Y -= (cursorSize.Height * 0.5f);

            // Draws the cursor.
            // 
            // カーソルを描画します。
            Vector2 position = centerCursorPosition + panelManager.DrawOffset;
            Batch.Draw(cursorTexture, position, centerCursorArea, Color.White);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Switches the panels.
        /// 
        /// パネルの入れ替え処理を行います。
        /// </summary>
        private void RevolvePanel()
        {
            VirtualPadState virtualPad =
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            if (buttons.A[VirtualKeyState.Push])
            {
                // Left rotates and switches the panel when the A button is pressed.
                // 
                // Aボタンが押されたら左回転のパネル入れ替え処理を行います。
                if (RevolvePanelLeft())
                {
                    // Plays the SoundEffect if the panel switch is successful.
                    // 
                    // 入れ替えに成功したらSoundEffectを再生します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                    // Adds the move count.
                    // 
                    // 移動カウントを加算します。
                    moveCount++;
                }
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Right rotates and switches the panel when the B button is pressed.
                // 
                // Bボタンが押されたら右回転のパネル入れ替え処理を行います。
                if (RevolvePanelRight())
                {
                    // Plays the SoundEffect if the panel switching is successful.
                    // 
                    // 入れ替えに成功したらSoundEffectを再生します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                    // Adds the move count.
                    // 
                    // 移動カウントを加算します。
                    moveCount++;
                }
            }
        }


        /// <summary>
        /// Left rotates and switches the panel.
        /// 
        /// 左回転のパネル入れ替え処理を行います。
        /// </summary>
        private bool RevolvePanelLeft()
        {
            Point[] panelPoints = new Point[] {
                new Point(cursor.X + 0, cursor.Y + 0),
                new Point(cursor.X + 1, cursor.Y + 0),
                new Point(cursor.X + 1, cursor.Y + 1),
                new Point(cursor.X + 0, cursor.Y + 1),
            };

            // Specifies the panel array and performs switching. 
            // 
            // パネルの配列を指定して入れ替え処理を行います。
            return RevolvePanel(panelPoints);
        }


        /// <summary>
        /// Right rotates and switches the panel.
        /// 
        /// 右回転のパネル入れ替え処理を行います。
        /// </summary>
        private bool RevolvePanelRight()
        {
            Point[] panelPoints = new Point[] {
                new Point(cursor.X + 0, cursor.Y + 0),
                new Point(cursor.X + 0, cursor.Y + 1),
                new Point(cursor.X + 1, cursor.Y + 1),
                new Point(cursor.X + 1, cursor.Y + 0),
            };

            // Specifies the panel array and performs switching. 
            // 
            // パネルの配列を指定して入れ替え処理を行います。
            return RevolvePanel(panelPoints);
        }


        /// <summary>
        /// Specifies the panel array and performs switching.
        /// 
        /// パネルの配列を指定して入れ替え処理を行います。
        /// </summary>
        private bool RevolvePanel(Point[] panelPoints)
        {
            // Processing is not performed if the panel is in action.
            // 
            // パネルが動作中の場合は処理を行いません。
            if (IsPanelAction)
                return false;

            // Sets the switching panel array.
            // 
            // 入れ替えるパネルの配列を設定します。
            PanelData[] panels = { 
                panelManager.GetPanel(panelPoints[0]),
                panelManager.GetPanel(panelPoints[1]),
                panelManager.GetPanel(panelPoints[2]),
                panelManager.GetPanel(panelPoints[3]),
            };

            // Processing is not performed if the switching panel status is not Normal.
            // 
            // 入れ替えるパネルが通常状態ではない場合、処理を行いません。
            foreach (PanelData panelData in panels)
            {
                if (panelData.Status != PanelData.PanelStatus.None)
                {
                    return false;
                }
            }

            // Sets the panel From and To positions.
            // 
            // パネルの移動元と、移動先を設定します。
            Vector2[,] panelFromTo = {
                {
                    panels[0].Position,
                    panels[1].Position,
                },
                {
                    panels[1].Position,
                    panels[2].Position,
                },
                {
                    panels[2].Position,
                    panels[3].Position,
                },
                {
                    panels[3].Position,
                    panels[0].Position,
                },
            };

            // Changes and sets the draw position.
            // 
            // 描画位置の変更と、設定を行います。
            for (int i = 0; i < 4; i++)
            {
                panels[i].FromPosition = panelFromTo[i, 0];
                panels[i].ToPosition = panelFromTo[i, 1];
                panels[i].Status = PanelData.PanelStatus.Move;
                panels[i].Color = ActivePanelColor;
                panels[i].MoveCount = 1.0f;
            }

            // Exchanges the panel data.
            // 
            // パネルのデータを交換します。
            panelManager.SetPanel(panelPoints[0], panels[3]);
            panelManager.SetPanel(panelPoints[1], panels[0]);
            panelManager.SetPanel(panelPoints[2], panels[1]);
            panelManager.SetPanel(panelPoints[3], panels[2]);

            return true;
        }


        /// <summary>
        /// Switches panels at random.
        /// 
        /// ランダムでパネルを入れ替えます。
        /// </summary>
        public override bool RandomShuffle()
        {
            // Processing is not performed if the panel is in action.
            // 
            // パネルが動作中なら処理を行いません。
            if (IsPanelAction)
                return false;

            // Obtains the target panel.
            // 
            // 対象のパネルを取得します。
            cursor = panelManager.GetRandomPanel(stageSetting);

            // Sets the rotation direction at random.
            // 
            // ランダムで回転方向を設定します。
            if (Random.Next(2) == 0)
            {
                // Performs left rotation panel switching.
                // 
                // 左回転のパネル入れ替え処理を行います。
                if (RevolvePanelLeft())
                {
                    // Plays the SoundEffect if panel switching is successful.
                    // 
                    // 入れ替えが成功したらSoundEffectを再生します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
                }
            }
            else
            {
                // Performs right rotation panel switching.
                // 
                // 右回転のパネル入れ替え処理を行います。
                if (RevolvePanelRight())
                {
                    // Plays the SoundEffect if panel switching is successful.
                    // 
                    // 入れ替えが成功したらSoundEffectを再生します。
                    GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);
                }
            }

            return true;
        }
        #endregion
    }
}


