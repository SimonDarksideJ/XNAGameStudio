#region File Description
//-----------------------------------------------------------------------------
// StyleBase.cs
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

using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Puzzle.Style
{
    /// <summary>
    /// Panel selection class.
    /// This class implements panel update and drawing functions.
    /// DrawCursor (used for drawing the cursor) and RandomShuffle (used for shuffling)
    /// must be defined in the inheritance target for this class. 
    /// 
    /// パネル選択のクラスです。
    /// このクラスにはパネルの更新と描画の機能を実装しています。
    /// クラスの継承先ではカーソルを描画するDrawCursorと、
    /// シャッフル時に使用されるRandomShuffleを定義する必要があります。
    /// </summary>
    public abstract class StyleBase : SceneComponent
    {
        #region Public Types
        /// <summary>
        /// Cursor draw position
        /// 
        /// カーソル描画位置
        /// </summary>
        [Flags]
        public enum PanelTypes
        {
            /// <summary>
            /// Left up
            /// 
            /// 左上
            /// </summary>
            LeftUp = 1,

            /// <summary>
            /// Left down 
            /// 
            /// 左下
            /// </summary>
            LeftDown = 2,

            /// <summary>
            /// Right up
            /// 
            /// 右上
            /// </summary>
            RightUp = 4,

            /// <summary>
            /// Right down
            /// 
            /// 右下
            /// </summary>
            RightDown = 8,

            /// <summary>
            /// All
            /// 
            /// 全て
            /// </summary>
            All = LeftUp | LeftDown | RightUp | RightDown
        }
        #endregion

        #region Fields
        /// <summary>
        /// Active panel color 
        /// 
        /// 動作中のパネルの色
        /// </summary>
        protected readonly Color ActivePanelColor = new Color(0xff, 0xff, 0xff, 0x7f);

        // Movie texture
        // 
        // ムービーテクスチャ
        private Texture2D movieTexture = null;

        // Select enabled status
        // 
        // 選択可否状態
        private bool selectEnabled = false;

        // Panel after-image list
        // 
        // パネルの残像リスト
        private LinkedList<PanelAfterImage> panelAfterImageList;

        // Cursor
        // 
        // カーソル
        protected Point cursor;

        // Cursor texture
        // 
        // カーソルテクスチャ
        protected Texture2D cursorTexture;

        // Stage settings information 
        // 
        // ステージ設定情報
        protected StageSetting stageSetting;

        // Panel management class
        // 
        // パネル管理クラス
        protected PanelManager panelManager;

        // Move count
        // 
        // 移動回数
        protected UInt32 moveCount;

        // Glass texture
        // 
        // ガラステクスチャ
        private Texture2D glassTexture;

        // Lighting texture
        // 
        // ライティングテクスチャ
        private Texture2D glassLightingTexture;

        // Glass texture rectangle
        // 
        // ガラステクスチャの矩形
        private Rectangle glassRect;

        // Lighting texture rectangle
        // 
        // ライティングテクスチャの矩形
        private Rectangle glassLightingRect;

        // Glass texture draw color
        // 
        // ガラステクスチャの描画色
        private Color glassColor;

        // Primitive draw class
        // 
        // 基本描画クラス
        private PrimitiveDraw2D primitiveDraw;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the movie animation texture.
        /// 
        /// ムービーアニメーションのテクスチャを取得または設定します。
        /// </summary>
        public Texture2D MovieTexture
        {
            get { return movieTexture; }
            set { movieTexture = value; }
        }


        /// <summary>
        /// Obtains or sets the select enabled status.
        /// 
        /// 選択可否状態を取得または設定します。
        /// </summary>
        public bool SelectEnabled
        {
            get { return selectEnabled; }
            set { selectEnabled = value; }
        }


        /// <summary>
        /// Obtains or sets the panel after-image list.      
        /// 
        /// 残像パネルのリストを取得または設定します。
        /// </summary>
        public LinkedList<PanelAfterImage> PanelAfterImageList
        {
            get { return panelAfterImageList; }
        }


        /// <summary>
        /// Obtains the move count. 
        /// 
        /// 移動回数を取得します。
        /// </summary>
        public UInt32 MoveCount
        {
            get { return moveCount; }
        }


        /// <summary>
        /// Obtains the panel action status.
        /// 
        /// パネル動作状態を取得します。
        /// </summary>
        public bool IsPanelAction
        {
            get { return GetPanelAction(); }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        protected StyleBase(Game game, StageSetting setting, Point cursor, 
            PanelManager manager)
            : base(game)
        {
            this.cursor = cursor;
            stageSetting = setting;
            panelManager = manager;

            // Creates the panel after-image array.
            // 
            // パネルの残像の配列を作成します。
            panelAfterImageList = new LinkedList<PanelAfterImage>();

            // Creates the line drawing class.
            // 
            // ライン描画のクラスを作成します。
            primitiveDraw = new PrimitiveDraw2D(game.GraphicsDevice);
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントを読み込みます。
        /// </summary>
        protected override void LoadContent()
        {
            string asset;
            
            // Loads the glass texture drawn at completion.
            // 
            // 完成時に描画するガラスのテクスチャを読み込みます。
            asset = "Textures/Puzzle/glass";
            glassTexture = Content.Load<Texture2D>(asset);

            // Loads the reflected light texture.
            // 
            // 反射光のテクスチャを読み込みます。
            asset = "Textures/Puzzle/glass_light";
            glassLightingTexture = Content.Load<Texture2D>(asset);


            glassRect = new Rectangle();
            glassRect.Width = glassTexture.Width;
            glassRect.Height = glassTexture.Height;

            glassLightingRect = new Rectangle();
            glassLightingRect.Width = glassLightingTexture.Width;
            glassLightingRect.Height = glassLightingTexture.Height;

            glassColor = new Color(0xff, 0xff, 0xff, 0x4f);

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates all panels.
        /// 
        /// 全てのパネルの更新処理を行います。
        /// </summary>
        protected void UpdatePanels(GameTime gameTime)
        {
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                for (int y = 0; y < stageSetting.Divide.Y; y++)
                {
                    UpdatePanel(panelManager.GetPanel(x, y));
                }
            }

            // Updates the panel after-image. 
            // 
            // 残像のパネルを更新します。
            UpdateAfterImage(gameTime);
        }


        /// <summary>
        /// Updates the panel.
        ///
        /// パネルの更新処理を行います。
        /// </summary>
        protected virtual void UpdatePanel(PanelData panel)
        {
            // Updates panel rotation.
            // 
            // パネルの回転処理の更新を行います。
            UpdatePanelRotate(panel);

            // Moves the panel.
            // 
            // パネルの移動処理を行います。
            UpdatePanelMove(panel);

            // Updates the panel reflected light.
            // 
            // パネルの反射光の更新処理を行います。
            UpdatePanelGlass(panel);

            // Creates the after-image if the panel is in active status.
            // 
            // パネルが動作状態の場合は残像を作成します。
            if (panel.Status != PanelData.PanelStatus.None)
            {
                // Creates the after-image panel.
                // 
                // 残像のパネルを作成します。
                PanelAfterImage afterimage = CreateAfterImage(panel);

                // Adds the after-image to the array.
                // 
                // 残像を配列に追加します。
                PanelAfterImageList.AddLast(afterimage);
            }

        }


        /// <summary>
        /// Updates the panel rotation.
        /// 
        /// パネルの回転処理の更新を行います。
        /// </summary>
        private static void UpdatePanelRotate(PanelData panel)
        {
            // Processing is not performed if the panel status is not Rotate.
            // 
            // ステータスが回転では無い場合は処理を行いません。
            if (panel.Status != PanelData.PanelStatus.RotateLeft &&
                panel.Status != PanelData.PanelStatus.RotateRight)
                return;


            if (panel.Status == PanelData.PanelStatus.RotateLeft)
            {
                panel.Rotate += 10;
                panel.Rotate %= 360;
            }
            else if (panel.Status == PanelData.PanelStatus.RotateRight)
            {
                panel.Rotate += 350;
                panel.Rotate %= 360;
            }

            // Rotation completed; status is returned.
            // 
            // 回転が終了したのでステータスを戻します。
            if (Math.Abs(panel.Rotate - panel.ToRotate) < 10)
            {
                panel.Status = PanelData.PanelStatus.None;
                panel.Rotate = panel.ToRotate;
            }
        }


        /// <summary>
        /// Moves the panel.
        /// 
        /// パネルの移動処理を行います。
        /// </summary>
        private static void UpdatePanelMove(PanelData panel)
        {
            // Processing is not performed if the panel status is not Move.
            // 
            // ステータスが移動ではない場合は処理を行いません。
            if (panel.Status != PanelData.PanelStatus.Move)
                return;

            if (panel.MoveCount > 0)
            {
                // Moves the panel.
                // 
                // パネルの移動処理を行います。
                panel.MoveCount = MathHelper.Clamp(panel.MoveCount - 0.1f, 0.0f, 1.0f);

                Vector2 fromPosition = panel.FromPosition;
                Vector2 toPosition = panel.ToPosition;
                float amount = panel.MoveCount;
                panel.Position = Vector2.Lerp(toPosition, fromPosition, amount);
            }
            else
            {
                // Movement completed; status is returned.
                // 
                // 移動が完了したのでステータスを戻します。
                panel.Status = PanelData.PanelStatus.None;
                panel.Position = panel.ToPosition;
            }
        }


        /// <summary>
        /// Updates the panel reflected light.
        /// 
        /// パネルの反射光の更新処理を行います。
        /// </summary>
        private void UpdatePanelGlass(PanelData panel)
        {
            // Processing is not performed if the panel status is Active.
            // 
            // ステータスが動作状態の場合は処理を行いません。
            if (panel.Status != PanelData.PanelStatus.None)
                return;

            // Returns the panel draw color.
            // 
            // パネルの描画色を戻します。
            panel.Color = Color.White;
            panel.Flush += 32;

            // Generates glass light at random.
            // 
            // ランダムでガラスのライトを発生させます。
            if (panel.Enabled == false)
            {
                if (panel.Flush > 256 && (Random.Next() % 200) == 0)
                {
                    panel.Flush = -256;
                }
            }
        }


        /// <summary>
        /// Updates the panel after-image.
        /// 
        /// 残像のパネルを更新します。
        /// </summary>
        protected void UpdateAfterImage(GameTime gameTime)
        {
            LinkedListNode<PanelAfterImage> node = PanelAfterImageList.First;
            while (node != null)
            {
                PanelAfterImage afterimage = node.Value;
                LinkedListNode<PanelAfterImage> removeNode = node;

                // Updates the after-image.
                // 
                // 残像の更新処理を行います。
                afterimage.Update(gameTime);

                // Moves to the next node. 
                // 
                // 次のノードへ移動します。
                node = node.Next;

                // Deletes the after image from the array if released.
                // 
                // 開放処理が行われていたら配列から削除します。
                if (afterimage.Disposed)
                {
                    PanelAfterImageList.Remove(removeNode);
                }
            }
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the panel.
        /// 
        /// パネルの描画を行います。
        /// </summary>
        public void DrawPanels(GameTime gameTime)
        {
            // Processing is not performed if there is no texture.
            // 
            // テクスチャが無い場合は処理を行いません。
            if (MovieTexture == null)
                return;

            // Draws Normal status panels.
            // 
            // 通常状態のパネルを描画します。
            DrawNormalPanel();

            // Draws the panel frame.
            // 
            // パネルのフレームを描画します。
            DrawPanelFrame();

            // Draws the panel after-image.
            // 
            // 残像のパネルを描画します。
            DrawEffectPanel(gameTime, Batch);

            // Draws panels undergoing change.
            // 
            // 変化中のパネル描画を描画します。
            DrawActivePanel();
        }


        /// <summary>
        /// Draws Normal status panels.
        /// 
        /// 通常状態のパネルを描画します。
        /// </summary>
        private void DrawNormalPanel()
        {
            Batch.Begin();
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                for (int y = 0; y < stageSetting.Divide.Y; y++)
                {
                    // Obtains the panel.
                    // 
                    // パネルを取得します。
                    PanelData panel = panelManager.GetPanel(x, y);

                    // Panels undergoing change are not drawn.
                    // 
                    // 変化中のパネルは描画処理を行いません。
                    if (panel.Status != PanelData.PanelStatus.None)
                    {
                        continue;
                    }

                    // Draws Normal status panels.
                    // 
                    // 通常状態のパネルを描画します。
                    DrawPanel(panel);

                    // Draws glass texture for completed panels.
                    // 
                    // 完成されたパネルのガラステクスチャを描画します。
                    DrawPanelGlass(panel);
                }
            }
            Batch.End();

        }


        /// <summary>
        /// Draws Normal status panels.
        /// 
        /// 通常状態のパネルを描画します。
        /// </summary>
        private void DrawPanel(PanelData panel)
        {
            Batch.Draw(
                MovieTexture,
                panel.Center + panelManager.DrawOffset,
                panel.SourceRectangle,
                panel.Color,
                MathHelper.ToRadians(panel.Rotate),
                panel.Origin,
                1.0f,
                SpriteEffects.None,
                0.0f);
        }


        /// <summary>
        /// Draws glass texture for completed panels.
        /// 
        /// 完成されたパネルのガラステクスチャを描画します。
        /// </summary>
        private void DrawPanelGlass(PanelData panel)
        {
            // Drawing is not performed if the panel is not completed.
            // 
            // パネルがまだ完成されていない状態なら描画処理を行いません。
            if (panel.Enabled)
                return;

            Rectangle panelRect = panel.RectanglePosition;
            panelRect.X += (int)panelManager.DrawOffset.X;
            panelRect.Y += (int)panelManager.DrawOffset.Y;


            Texture2D texture;

            // Draws the glass.
            // 
            // ガラスを描画します。
            texture = glassTexture;
            Batch.Draw(texture, panelRect, glassRect, glassColor);

            // Draws the lighting effect.
            // 
            // ライティングエフェクトを描画します。
            glassLightingRect.X = (int)panel.Flush;
            texture = glassLightingTexture;
            Batch.Draw(texture, panelRect, glassLightingRect, Color.White);
        }


        /// <summary>
        /// Draws the active panel.
        /// 
        /// 動作中のパネルを描画します。
        /// </summary>
        private void DrawActivePanel()
        {
            // Performs drawing via addition.
            // 
            // 加算で描画を行います。
            Batch.Begin(SpriteBlendMode.AlphaBlend);
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                for (int y = 0; y < stageSetting.Divide.Y; y++)
                {
                    // Obtains the panel.
                    // 
                    // パネルを取得します。
                    PanelData panel = panelManager.GetPanel(x, y);

                    // Stopped panels are not drawn.
                    // 
                    // 停止中のパネルは描画処理を行いません。
                    if (panel.Status == PanelData.PanelStatus.None)
                    {
                        continue;
                    }

                    // Draws the active panel.
                    // 
                    // 動作中のパネルを描画します。
                    DrawPanel(panel);
                }
            }
            Batch.End();
        }


        /// <summary>
        /// Draws the after-image.
        /// 
        /// 残像の描画を行います。
        /// </summary>
        private void DrawEffectPanel(GameTime gameTime, SpriteBatch batch)
        {
            // Performs drawing via addition.
            // 
            // 加算で描画処理を行います。
            Batch.Begin(SpriteBlendMode.Additive);
            foreach (PanelAfterImage afterimage in PanelAfterImageList)
            {
                afterimage.Draw(gameTime, batch);
            }
            Batch.End();

        }


        /// <summary>
        /// Draws lines in the panel rectangle.
        /// 
        /// パネルの矩形にラインを描画します。
        /// </summary>
        private void DrawPanelFrame()
        {
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                for (int y = 0; y < stageSetting.Divide.Y; y++)
                {
                    // Obtains the panel.
                    // 
                    // パネルを取得します。
                    PanelData panel = panelManager.GetPanel(x, y);

                    // Panels undergoing change are not drawn.
                    // 
                    // 変化中のパネルは描画処理を行いません。
                    if (panel.Status != PanelData.PanelStatus.None)
                    {
                        continue;
                    }

                    // Draws the lines.
                    // 
                    // ラインを描画します。
                    DrawPanelFrame(panel);
                }
            }
        }


        /// <summary>
        /// Draws lines in the panel rectangle.
        /// 
        /// パネルの矩形にラインを描画します。
        /// </summary>
        private void DrawPanelFrame(PanelData panel)
        {
            Vector2 panelPosition = panel.Position + panelManager.DrawOffset;
            Color color = new Color(0xff, 0xff, 0xff, 0x20);
            Vector4 rect = new Vector4();
            rect.X = panelPosition.X;
            rect.Y = panelPosition.Y;
            rect.Z = panel.Size.X;
            rect.W = panel.Size.Y;

            primitiveDraw.DrawRect(null, rect, color);
        }


        /// <summary>
        /// Abstract method for cursor drawing.
        /// 
        /// カーソル描画の抽象メソッドです。
        /// </summary>
        public abstract void DrawCursor(GameTime gameTime);


        /// <summary>
        /// Draws the cursor.
        /// 
        /// カーソル描画処理を行います。
        /// </summary>
        protected void DrawCursor(
            GameTime gameTime, Rectangle panelSize, Color color, PanelTypes panelType)
        {
            Color[] colors = {
                color,
                color,
                color,
                color
            };

            DrawCursor(gameTime, panelSize, colors, panelType);
        }


        /// <summary>
        /// Draws the cursor by specifying the four corners.
        /// 
        /// 四隅を指定してカーソルを描画します。
        /// </summary>
        protected virtual void DrawCursor(GameTime gameTime,
            Rectangle panelSize, Color[] colors, PanelTypes panelType)
        {
            // Processing is not performed if the cursor texture is not specified.
            // 
            // カーソルのテクスチャが指定されていない場合は処理を行いません。
            if ((cursorTexture == null) || cursorTexture.IsDisposed)
            {
                return;
            }

            // Specifies the cursor size at the four corners.
            // 
            // 四隅にあるカーソルのサイズを指定します。
            Rectangle cursorSize = new Rectangle(0, 0, 48, 48);

            // Calculates the drawing coordinates at the four corners 
            // based on the panel rectangle.
            // 
            // パネルの矩形から、四隅の描画座標を計算します。
            Vector2[] drawPosition = {
                // Left up 
                // 
                // 左上
                new Vector2(
                    panelSize.X,
                    panelSize.Y),
                // Left down
                // 
                // 左下
                new Vector2(
                    panelSize.X,
                    panelSize.Y + panelSize.Height - cursorSize.Height),
                // Right up 
                // 
                // 右上
                new Vector2(
                    panelSize.X + panelSize.Width - cursorSize.Width,
                    panelSize.Y),
                // Right down
                // 
                // 右下
                new Vector2(
                    panelSize.X + panelSize.Width - cursorSize.Width,
                    panelSize.Y + panelSize.Height - cursorSize.Height)
            };

            // Sets the texture source coordinates.
            // 
            // テクスチャの転送元座標を設定します。
            int cursorType = (cursorSize.Height * 2) * 0;
            Rectangle[] cursorArea = {
                // Left up 
                // 
                // 左上
                new Rectangle(0, cursorType,
                    cursorSize.Width, cursorSize.Height),
                // Left down
                // 
                // 左下
                new Rectangle(0, cursorType + cursorSize.Height,
                    cursorSize.Width, cursorSize.Height),
                // Right up 
                // 
                // 右上
                new Rectangle(cursorSize.Width, cursorType,
                    cursorSize.Width, cursorSize.Height),
                // Right down
                // 
                // 右下
                new Rectangle(cursorSize.Width, cursorType + cursorSize.Height,
                    cursorSize.Width, cursorSize.Height)
            };

            // Draws the cursor at the designated flag location.
            // 
            // 指定されたフラグの箇所のカーソルを描画します。
            for (int i = 0; i < drawPosition.Length; i++)
            {
                // Processing is skipped if the draw cursor
                // flag is not specified.
                // 
                // 描画するカーソルのフラグが指定されていなければ
                // 処理をスキップします。
                if (((int)panelType & (1 << i)) == 0)
                    continue;

                Vector2 position;

                // Draws the cursor frame.
                // 
                // カーソルの外枠を描画します。
                position = drawPosition[i] + panelManager.DrawOffset;
                Batch.Draw(cursorTexture, position, cursorArea[i], colors[i]);

                // Offsets the source coordinates.
                // 
                // 転送元の座標をずらします。
                cursorArea[i].X += 96;

                // Changes the draw color.
                // 
                // 描画色を変更します。
                Vector4 color = colors[i].ToVector4();
                float radian = (float)gameTime.TotalGameTime.TotalSeconds;
                float alpha = Math.Abs((float)Math.Sin(radian));
                color.W = alpha;

                // Draws the internal cursor frame. 
                // 
                // カーソルの内枠を描画します。
                position = drawPosition[i] + panelManager.DrawOffset;
                Batch.Draw(cursorTexture, position, cursorArea[i], new Color(color));
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Abstract method for panel shuffle.
        /// Should be written in the inheritance target.
        ///
        /// パネルシャッフルの抽象メソッドです。
        /// 継承先で記述してください。
        /// </summary>
        public abstract bool RandomShuffle();


        /// <summary>
        /// Obtains the panel action status.
        /// 
        /// パネルの動作状態を取得します。
        /// </summary>
        private bool GetPanelAction()
        {
            for (int x = 0; x < stageSetting.Divide.X; x++)
            {
                for (int y = 0; y < stageSetting.Divide.Y; y++)
                {
                    if (panelManager.GetPanel(x, y).Status !=
                        PanelData.PanelStatus.None)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Creates the panel after-image.
        /// 
        /// 残像のパネルを作成します。
        /// </summary>
        private PanelAfterImage CreateAfterImage(PanelData panel)
        {
            PanelAfterImage afterimage = new PanelAfterImage(Game);
            afterimage.Texture = MovieTexture;
            afterimage.Position = panel.Center + panelManager.DrawOffset;
            afterimage.Size = panel.Size;
            afterimage.Origin = panel.Origin;
            afterimage.Rotate = MathHelper.ToRadians(panel.Rotate);
            afterimage.TexturePosition = panel.TexturePosition;

            return afterimage;
        }
        #endregion
    }
}


