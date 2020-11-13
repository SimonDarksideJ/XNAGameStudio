#region File Description
//-----------------------------------------------------------------------------
// SlideComponent.cs
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
    /// Implements the Slide Mode style.
    /// This class inherits the StyleBase class and implements panel switching.
    /// It switches panels extended from the cursor position in all four directions
    /// as far as the movie end panel by sliding them.
    /// 
    /// スライドモードのスタイルを実装します。
    /// このクラスはStyleBaseクラスを継承し、パネルの入れ替え処理を実装しています。
    /// カーソルの位置から、ムービーの端のパネルまで上下左右に伸ばしたパネルを
    /// スライドさせて入れ替える処理を行っています。
    /// </summary>
    public class SlideComponent : StyleBase
    {
        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SlideComponent(Game game,
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

                // Slides the panels in the vertical direction.
                // 
                // パネルを縦方向にスライドする処理を行います。
                UpdateSlidePanel();
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
            // Sets the  cursor position.
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


        /// <summary>
        /// Performs update processing to judge panel slide.
        /// 
        /// パネルのスライドを判定する更新処理を行います。
        /// </summary>
        private void UpdateSlidePanel()
        {
            bool action = false;

            VirtualPadState virtualPad =
                GameData.Input.VirtualPadStates[PlayerIndex.One];
            VirtualPadButtons buttons = virtualPad.Buttons;

            if (buttons.Y[VirtualKeyState.Push])
            {
                // Slides the panel up when the Y button is pressed.
                // 
                // Yボタンが押されていたら、上方向にパネルをスライドします。
                action = SlidePanelUp();
            }
            else if (buttons.A[VirtualKeyState.Push])
            {
                // Slides the panel down when the A button is pressed.
                // 
                // Aボタンが押されていたら、下方向にパネルをスライドします。
                action = SlidePanelDown();
            }
            else if (buttons.X[VirtualKeyState.Push])
            {
                // Slides the panel to the left when the X button is pressed.
                // 
                // Xボタンが押されていたら、左方向にパネルをスライドします。
                action = SlidePanelLeft();
            }
            else if (buttons.B[VirtualKeyState.Push])
            {
                // Slides the panel to the right when the B button is pressed.
                // 
                // Bボタンが押されていたら、右方向にパネルをスライドします。
                action = SlidePanelRight();
            }

            // Performs processing in response to panel action.
            // 
            // パネルが動作したら処理を行います。
            if (action)
            {
                // Plays the SoundEffect when sliding.
                // 
                // スライド時のSoundEffectを再生します。
                GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

                // Adds up the move count.
                // 
                // 移動回数を加算します。
                moveCount++;
            }
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
            Batch.Begin();

            // Draws the center cursor.
            // 
            // 中央のカーソルを描画します。
            DrawCenterCursor(gameTime);

            // Draws the vertical cursor.
            // 
            // 横方向のカーソルを描画します。
            DrawVerticalCursor(gameTime);

            // Draws the horizontal cursor.
            // 
            // 縦方向のカーソルを描画します。
            DrawHorizontalCursor(gameTime);
            
            Batch.End();
        }


        /// <summary>
        /// Draws the center cursor.
        /// 
        /// 中央のカーソルを描画します。
        /// </summary>
        private void DrawCenterCursor(GameTime gameTime)
        {
            Rectangle cursorRect = new Rectangle();
            cursorRect.X = (int)(cursor.X * panelManager.PanelSize.X);
            cursorRect.Y = (int)(cursor.Y * panelManager.PanelSize.Y);
            cursorRect.Width = (int)(panelManager.PanelSize.X);
            cursorRect.Height = (int)(panelManager.PanelSize.Y);
            DrawCursor(gameTime, cursorRect, Color.Red, PanelTypes.All);
        }


        /// <summary>
        /// Draws the vertical cursor.
        /// 
        /// 横方向のカーソルを描画します。
        /// </summary>
        private void DrawVerticalCursor(GameTime gameTime)
        {
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                // Processing is not performed if the draw location is 
                // the same as the cursor position.
                // 
                // 描画先がカーソル位置と同じ場合は処理を行いません。
                if (x == cursor.X)
                    continue;

                Rectangle cursorRect = new Rectangle();
                cursorRect.X = (int)(x * panelManager.PanelSize.X);
                cursorRect.Y = (int)(cursor.Y * panelManager.PanelSize.Y);
                cursorRect.Width = (int)(panelManager.PanelSize.X);
                cursorRect.Height = (int)(panelManager.PanelSize.Y);

                DrawCursor(gameTime, cursorRect, Color.White, PanelTypes.All);
            }
        }


        /// <summary>
        /// Draws the horizontal cursor.
        /// 
        /// 縦方向のカーソルを描画します。
        /// </summary>
        private void DrawHorizontalCursor(GameTime gameTime)
        {
            for (int y = 0; y < stageSetting.Divide.Y; y++)
            {
                // Proccessing is not performed if the draw location is 
                // the same as the cursor position.
                // 
                // 描画先がカーソル位置と同じ場合は処理を行いません。
                if (y == cursor.Y)
                    continue;

                Rectangle cursorRect = new Rectangle();
                cursorRect.X = (int)(cursor.X * panelManager.PanelSize.X);
                cursorRect.Y = (int)(y * panelManager.PanelSize.Y);
                cursorRect.Width = (int)(panelManager.PanelSize.X);
                cursorRect.Height = (int)(panelManager.PanelSize.Y);

                DrawCursor(gameTime, cursorRect, Color.White, PanelTypes.All);
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Slides the panels up.
        /// 
        /// パネルを上方向にスライドさせます。
        /// </summary>
        private bool SlidePanelUp()
        {
            List<Point> panelPointList = new List<Point>();
            for (int y = stageSetting.Divide.Y - 1; y >= 0; y--)
            {
                panelPointList.Add(new Point(cursor.X, y));
            }

            if (panelPointList.Count == 0)
            {
                return false;
            }

            // Specifies the panel array to slide and switches the panels.
            // 
            // スライドするパネルの配列を指定して入れ替えます。
            return SlidePanel(panelPointList);
        }


        /// <summary>
        /// Slides the panels down.
        /// 
        /// パネルを下方向にスライドさせます。
        /// </summary>
        private bool SlidePanelDown()
        {
            List<Point> panelPointList = new List<Point>();
            for (int y = 0; y < stageSetting.Divide.Y; y++)
            {
                panelPointList.Add(new Point(cursor.X, y));
            }

            if (panelPointList.Count == 0)
            {
                return false;
            }

            // Specifies the panel array to slide and switches the panels.
            // 
            // スライドするパネルの配列を指定して入れ替えます。
            return SlidePanel(panelPointList);
        }


        /// <summary>
        /// Slides the panels to the left.
        /// 
        /// パネルを左方向にスライドさせます。
        /// </summary>
        private bool SlidePanelLeft()
        {
            List<Point> panelPointList = new List<Point>();
            for (int x = stageSetting.Divide.X - 1; x >= 0; x--)
            {
                panelPointList.Add(new Point(x, cursor.Y));
            }

            if (panelPointList.Count == 0)
            {
                return false;
            }

            // Specifies the panel array to slide and switches the panels.
            // 
            // スライドするパネルの配列を指定して入れ替えます。
            return SlidePanel(panelPointList);
        }


        /// <summary>
        /// Slides the panels to the right.
        /// 
        /// パネルを右方向にスライドさせます。
        /// </summary>
        private bool SlidePanelRight()
        {
            List<Point> panelPointList = new List<Point>();
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                panelPointList.Add(new Point(x, cursor.Y));
            }

            if (panelPointList.Count == 0)
            {
                return false;
            }

            // Specifies the panel array to slide and switches the panels.
            // 
            // スライドするパネルの配列を指定して入れ替えます。
            return SlidePanel(panelPointList);
        }


        /// <summary>
        /// Specifies the panel array to slide and switches the panels.
        /// 
        /// スライドするパネルの配列を指定して入れ替えます。
        /// </summary>
        private bool SlidePanel(List<Point> panelPointList)
        {
            // Processing is not performed if the panel status is not Normal.
            // 
            // パネルが通常状態ではない場合は処理を行いません。
            if (IsPanelAction)
                return false;

            // Processing is not performed if there is no switching panel array.
            // 
            // 入れ替えるパネルの配列が無い場合は処理を行いません。
            if (panelPointList == null || panelPointList.Count == 0)
                return false;

            // Obtains the switching panel list.
            // 
            // 入れ替えるパネルのリストを取得します。
            List<PanelData> panels = new List<PanelData>();
            foreach (Point point in panelPointList)
            {
                panels.Add(panelManager.GetPanel(point));
            }

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
            List<Vector2[]> panelFromTo = new List<Vector2[]>();
            for (int i = 0; i < panels.Count; i++)
            {
                int currentId = i;
                int nextId = (i + 1) % panels.Count;

                Vector2[] item = {
                    panels[currentId].Position,
                    panels[nextId].Position,
                };
                panelFromTo.Add(item);
            }

            // Changes and sets the draw position.
            // 
            // 描画位置の変更と、設定を行います。
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].FromPosition = panelFromTo[i][0];
                panels[i].ToPosition = panelFromTo[i][1];
                panels[i].Status = PanelData.PanelStatus.Move;
                panels[i].Color = ActivePanelColor;
                panels[i].MoveCount = 1.0f;
            }

            // Exchanges the panel data.
            // 
            // パネルのデータを交換します。
            for (int i = 0; i < panels.Count; i++)
            {
                int panelId = (i + (panels.Count - 1)) % panels.Count;
                panelManager.SetPanel(panelPointList[i], panels[panelId]);
            }

            return true;
        }


        /// <summary>
        /// Switches panels at random.
        /// 
        /// ランダムでパネルを入れ替えます。
        /// </summary>
        public override bool RandomShuffle()
        {
            // Processing is not performed while panels are in action.
            // 
            // パネルが動作中なら処理を行いません。
            if (IsPanelAction)
                return false;

            // Obtains the target panels.
            // 
            // 対象のパネルを取得します。
            cursor = panelManager.GetRandomPanel(stageSetting);

            // Selects the panel sliding direction at random.
            // 
            // パネルのスライド方向をランダムで決定します。
            int rndValue = Random.Next(0, 4);
            if (rndValue == 0)
            {
                // Slides the panel up.
                // 
                // 上方向にスライドします。
                SlidePanelUp();
            }
            else if (rndValue == 1)
            {
                // Slides the panel down.
                // 
                // 下方向にスライドします。
                SlidePanelDown();
            }
            else if (rndValue == 2)
            {
                // Slides the panel to the left.
                // 
                // 左方向にスライドします。
                SlidePanelLeft();
            }
            else if (rndValue == 3)
            {
                // Slides the panel to the right.
                // 
                // 右方向にスライドします。
                SlidePanelRight();
            }

            // Plays the sliding SoundEffect.
            // 
            // スライドのSoundEffectを再生します。
            GameData.Sound.PlaySoundEffect(Sounds.SoundEffectCursor1);

            return true;
        }
        #endregion
    }
}


