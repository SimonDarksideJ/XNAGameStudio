#region File Description
//-----------------------------------------------------------------------------
// SceneComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Movipa.Util;
#endregion

namespace Movipa.Components
{
    /// <summary>
    /// Constituent component of the scene.
    /// All scenes are created by inheriting this Component.
    /// 
    /// シーンを構成するコンポーネントです。
    /// 各シーンはこのコンポーネントを継承して作成されます。
    /// </summary>
    public class SceneComponent : DrawableGameComponent
    {
        #region Fields
        private MovipaGame movipaGame;
        private List<GameComponent> sceneComponents;
        private List<NavigateData> navigate;
        private Random random;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains ContentManager.
        /// 
        /// ContentManagerを取得します。
        /// </summary>
        public ContentManager Content
        {
            get { return movipaGame.Content; }
        }


        /// <summary>
        /// Obtains SpriteBatch. 
        /// 
        /// SpriteBatchを取得します。
        /// </summary>
        public SpriteBatch Batch
        {
            get { return movipaGame.Batch; }
        }


        public SpriteFont MediumFont
        {
            get { return movipaGame.MediumFont; }
        }


        public SpriteFont LargeFont
        {
            get { return movipaGame.LargeFont; }
        }


        /// <summary>
        /// Obtains the Navigate List.
        /// 
        /// ナビゲートリストを取得します。
        /// </summary>
        public List<NavigateData> Navigate
        {
            get { return navigate; }
        }


        /// <summary>
        /// Obtains Random.
        /// 
        /// ランダムを取得します。
        /// </summary>
        public Random Random
        {
            get { return random; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="game"></param>
        public SceneComponent(Game game)
            : base(game)
        {
            movipaGame = game as MovipaGame;
            
            sceneComponents = new List<GameComponent>();
            navigate = new List<NavigateData>();
            random = new Random();
        }


        /// <summary>
        /// Initializes the instance.
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            InitializeNavigate();
            base.Initialize();
        }


        /// <summary>
        /// Initializes the Navigate button.
        /// This process is invoked after invoking the Initialize method.
        /// 
        /// ナビゲートボタンの初期化を行います。
        /// この処理はInitializeメソッドを呼び出した後に呼び出されます。
        /// </summary>
        protected virtual void InitializeNavigate()
        {
        }

        /// <summary>
        /// Releases all resources.
        ///
        /// 全てのリソースを開放します。
        /// </summary>
        protected override void UnloadContent()
        {
            // End of component added via AddComponent 
            // 
            // AddComponentで追加されたコンポーネントの終了
            foreach (GameComponent component in sceneComponents)
            {
                component.Dispose();
            }

            base.UnloadContent();
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the Navigate button.
        /// 
        /// ナビゲートボタンを描画します。
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="batchBeginEnd">SpriteBatch.Begin/End execute flag</param>
        ///  
        /// <param name="batchBeginEnd">SpriteBatch.Begin/Endの実行フラグ</param>
        public void DrawNavigate(GameTime gameTime, bool batchBeginEnd)
        {
            DrawNavigate(gameTime, Batch, 1.0f, batchBeginEnd);
        }


        /// <summary>
        /// Draws the Navigate button.
        ///
        /// ナビゲートボタンを描画します。
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="batch">SpriteBatch</param>
        /// <param name="batchBeginEnd">SpriteBatch.Begin/End execute flag</param>
        ///  
        /// <param name="batchBeginEnd">SpriteBatch.Begin/Endの実行フラグ</param>
        public void DrawNavigate(
            GameTime gameTime, SpriteBatch batch, bool batchBeginEnd)
        {
            DrawNavigate(gameTime, batch, 1.0f, batchBeginEnd);
        }


        /// <summary>
        /// Draws the Navigate button.
        /// 
        /// ナビゲートボタンを描画します。
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="batch">SpriteBatch</param>
        /// <param name="alpha">Transparency</param>
        ///  
        /// <param name="alpha">透明度</param>
        /// <param name="batchBeginEnd">SpriteBatch.Begin/End execute flag</param>
        ///  
        /// <param name="batchBeginEnd">SpriteBatch.Begin/Endの実行フラグ</param>
        public void DrawNavigate(
            GameTime gameTime, SpriteBatch batch, float alpha, bool batchBeginEnd)
        {
            SpriteFont font = movipaGame.MediumFont;

            string navigateText = String.Empty;

            // Collects the drawing text strings.
            // 
            // 描画文字列をまとめます。
            for (int i = 0; i < Navigate.Count; i++)
            {
                navigateText += Navigate[i].Message + "\n";
            }

            // Trims the final carriage return.
            // 
            // 最後の改行を取り除きます。
            navigateText = navigateText.TrimEnd('\n');

            // Calculates the draw position.
            // 
            // 描画位置を計算します。
            Vector2 measure = font.MeasureString(navigateText);
            // Coordinates at bottom right of screen 
            // 画面右下の座標　
            Vector2 safePosition = GameData.ScreenSizeVector2 * 0.95f; 
            Vector2 basePosition = safePosition - measure;
            Vector2 position;

            if (batchBeginEnd)
                batch.Begin();

            // Draws the normal text string.
            // 
            // 通常の文字列描画します。
            position = basePosition;
            for (int i = 0; i < Navigate.Count; i++)
            {
                Color color = new Color(new Vector4(1.0f, 1.0f, 1.0f, alpha));
                batch.DrawString(font, Navigate[i].Message, position, color);
                position.Y += font.LineSpacing;
            }

            batch.End();
            batch.Begin(SpriteBlendMode.Additive);

            // If the text string is flashing, overwrites it by adding.
            // 
            // 文字列が点滅していれば加算で上書きします。
            position = basePosition;
            for (int i = 0; i < Navigate.Count; i++)
            {
                if (Navigate[i].Blink)
                {
                    float radian = MathHelper.ToRadians(
                        (float)gameTime.TotalGameTime.TotalMilliseconds * 0.1f);
                    float w = Math.Abs((float)Math.Cos(radian)) * alpha;
                    Color color = new Color(new Vector4(1, 1, 1, w));

                    batch.DrawString(font, Navigate[i].Message, position, color);
                }
                position.Y += font.LineSpacing;
            }

            batch.End();


            batch.Begin();

            if (batchBeginEnd)
                batch.End();

        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Adds the Component.
        /// </summary>
        /// <param name="component">Component to add</param>
        public void AddComponent(GameComponent component)
        {
            sceneComponents.Add(component);
            Game.Components.Add(component);
        }


        /// <summary>
        /// Obtains value from GameData.AppSettings.
        /// 
        /// GameData.AppSettingsから値を取得します。
        /// </summary>
        /// <param name="key">Key</param>
        ///  
        /// <param name="key">キー</param>

        /// <returns>Text string stored in key</returns>
        ///  
        /// <returns>キーに格納された文字列</returns>
        public static string AppSettings(string key)
        {
#if XBOX360
            return GameData.AppSettings[key];
#else
            return GameData.AppSettings[key + "_Win"];
#endif
        }
        #endregion
    }
}


