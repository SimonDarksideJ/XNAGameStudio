#region File Description
//-----------------------------------------------------------------------------
// ResultBase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using Movipa.Components.Input;
using Movipa.Util;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Result
{
    /// <summary>
    /// Conducts basic result processing. This class is inherited.
    /// This class loads common assets, and implements update and draw 
    /// functions. Fade processing is performed in the Update method, while
    /// main update processing is written in the inherited UpdateMain. 
    /// The text string used for drawing is defined in the inherited
    /// GetSequenceString and drawn with DrawSequenceString.
    ///
    /// リザルトの基本処理を行います。このクラスは継承して利用します。
    /// このクラスでは、共通のアセットを読み込み、更新と描画を行う機能を
    /// 実装しています。フェードの処理はUpdateメソッド内で行われ、
    /// メインの更新処理は継承先のUpdateMainに記述します。
    /// 描画に使用する文字列は継承先のGetSequenceStringで設定し、
    /// DrawSequenceStringで描画しています。
    /// </summary>
    public class ResultBase : SceneComponent
    {
        #region Private Types
        /// <summary>
        /// Processing status
        /// 
        /// 処理状態
        /// </summary>
        protected enum Phase
        {
            /// <summary>
            /// Start animation in progress
            /// 
            /// 開始アニメーション中
            /// </summary>
            Start,

            /// <summary>
            /// Selected status after animation finishes
            /// 
            /// アニメーション終了後の選択状態
            /// </summary>
            Select
        }
        #endregion


        #region Fields
        // Processing status
        // 
        // 処理状態
        protected Phase phase;

        // Stage completion result
        // 
        // ステージのクリア結果
        protected StageResult result;

        // BackgroundMusic cue
        // 
        // BackgroundMusicのキュー
        protected Cue bgm;

        // Camera
        // 
        // カメラ
        protected readonly Vector3 cameraPosition;
        protected readonly Vector3 cameraLookAt;

        // Background sphere model data
        // 
        // 背景の球体モデルデータ
        protected BasicModelData[] spheres;

        // Background texture 
        // 
        // 背景テクスチャ
        protected Texture2D wallpaperTexture;

        // Layout
        protected SceneData sceneData;
        protected SequencePlayData seqStart;
        protected SequencePlayData seqPosition;

        // Navigate button draw flag
        // 
        // ナビゲートボタンの描画フラグ
        protected bool drawNavigate;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public ResultBase(Game game, StageResult stageResult)
            : base(game)
        {
            result = stageResult;

            cameraPosition = new Vector3(0.0f, 0.0f, 200.0f);
            cameraLookAt = Vector3.Zero;
        }


        /// <summary>
        /// Performs initialization processing.
        /// 
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Sets the initial processing status.
            // 
            // 処理の初期状態を設定します。
            phase = Phase.Start;

            // Sets the Navigate button draw status.
            // 
            // ナビゲートボタンの描画状態を設定
            drawNavigate = false;

            // Sets the Fade-in.
            // 
            // フェードインの設定を行います。
            GameData.FadeSeqComponent.Start(FadeType.Normal, FadeMode.FadeIn);

            // Plays the BackgroundMusic and obtains the Cue.
            // 
            // BackgroundMusicを再生し、Cueを取得します。
            bgm = GameData.Sound.PlayBackgroundMusic(Sounds.GameClearBackgroundMusic);

            base.Initialize();
        }


        /// <summary>
        /// Loads the content.
        /// 
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the background texture.
            // 
            // 背景テクスチャを読み込みます。
            string asset = "Textures/Wallpaper/Wallpaper_005";
            wallpaperTexture = Content.Load<Texture2D>(asset);

            // Loads and sets the sphere model.
            // 
            // 球体モデルの読み込みと設定をします。
            spheres = new BasicModelData[2];
            spheres[0] = new BasicModelData(Content.Load<Model>("Models/sphere01"));
            spheres[0].Scale = 0.9f;
            spheres[1] = new BasicModelData(Content.Load<Model>("Models/sphere02"));
            spheres[1].Scale = 0.88f;

            base.LoadContent();
        }


        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected override void UnloadContent()
        {
            // Stops the BackgroundMusic.
            // 
            // BackgroundMusicを停止します。
            SoundComponent.Stop(bgm);

            base.UnloadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Performs update processing.
        /// 
        /// 更新処理を行います。
        /// </summary>
        /// <param name="gameTime">GameTime</param>
        public override void Update(GameTime gameTime)
        {
            // Updates the sphere model.
            // 
            // 球体モデルの更新処理を行います。
            UpdateModels();

            if (GameData.FadeSeqComponent.FadeMode == FadeMode.FadeIn)
            {
                // Changes the processing status after the fade-in finishes.
                // 
                // フェードインが終了したら、処理状態を変更します。
                if (!GameData.FadeSeqComponent.IsPlay)
                {
                    GameData.FadeSeqComponent.FadeMode = FadeMode.None;
                }
            }
            else if (GameData.FadeSeqComponent.FadeMode == FadeMode.None)
            {
                // Performs main update processing.
                // 
                // メインの更新処理を行います。
                UpdateMain(gameTime);
            }
            else if (GameData.FadeSeqComponent.FadeMode == FadeMode.FadeOut)
            {
                // Performs update processing at fade-out.
                // 
                // フェードアウト時の更新処理を行います。
                UpdateFadeOut();
            }

            // Updates the sequence except during fade-in.
            // 
            // フェードイン以外で、シーケンスの更新処理を行います。
            if (GameData.FadeSeqComponent.FadeMode != FadeMode.FadeIn)
            {
                UpdateSequence(gameTime);
            }


            base.Update(gameTime);
        }


        /// <summary>
        /// Performs update except during a fade.
        /// 
        /// フェード処理中以外の更新処理を行います。
        /// </summary>
        protected virtual void UpdateMain(GameTime gameTime)
        {
        }


        /// <summary>
        /// Performs update processing at fade-out.
        /// 
        /// フェードアウト時の更新処理を行います。
        /// </summary>
        private void UpdateFadeOut()
        {
            // Sets the BackgroundMusic volume.
            // 
            // BackgroundMusicのボリュームを設定します。
            float volume = 1.0f - (GameData.FadeSeqComponent.Count / 60.0f);
            SoundComponent.SetVolume(bgm, volume);

            // Performs release processing after the fade-out finishes.
            // 
            // フェードアウトが終了したら、開放処理を行います。
            if (!GameData.FadeSeqComponent.IsPlay)
            {
                Dispose();
            }
        }


        /// <summary>
        /// Updates the model.
        /// 
        /// モデルの更新処理を行います。
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateModels()
        {
            Vector3 rotate;

            rotate = spheres[0].Rotate;
            rotate.Y += MathHelper.ToRadians(0.1f);
            spheres[0].Rotate = rotate;

            rotate = spheres[1].Rotate;
            rotate.Y -= MathHelper.ToRadians(0.03f);
            spheres[1].Rotate = rotate;
        }


        /// <summary>
        /// Updates the sequence.
        /// 
        /// シーケンスの更新処理を行います。
        /// </summary>
        private void UpdateSequence(GameTime gameTime)
        {
            seqStart.Update(gameTime.ElapsedGameTime);
            seqPosition.Update(gameTime.ElapsedGameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing processing.
        /// 
        /// 描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Draws the background.
            // 
            // 背景の描画を行います。
            Batch.Begin();
            Batch.Draw(wallpaperTexture, Vector2.Zero, Color.White);
            Batch.End();

            // Clears the depth buffer and draws the sphere model.
            // 
            // 深度バッファをクリアし、球体モデルを描画します。
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawSpheres();

            Batch.Begin();

            // Draws the sequence.
            // 
            // シーケンスを描画します。
            seqStart.Draw(Batch, null);

            // Draws the text string.
            // 
            // 文字列を描画します。
            DrawSequenceString();

            // Draws the Navigate button.
            // 
            // ナビゲートボタンを描画します。
            if (drawNavigate)
            {
                DrawNavigate(gameTime, false);
            }

            Batch.End();

            base.Draw(gameTime);
        }

        
        /// <summary>
        /// Draws the cursor sphere.
        /// 
        /// カーソルの球体を描画します。
        /// </summary>
        private void DrawSpheres()
        {
            GraphicsDevice graphicsDevice = GraphicsDevice;
            Matrix view;
            view = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            for (int i = 0; i < spheres.Length; i++)
            {
                spheres[i].SetRenderState(graphicsDevice, SpriteBlendMode.Additive);
                spheres[i].Draw(view, GameData.Projection);
            }
        }


        /// <summary>
        /// Draws the sequence text string.
        /// 
        /// シーケンスの文字列を描画します。
        /// </summary>
        private void DrawSequenceString()
        {
            SpriteFont font = LargeFont;
            SequenceBankData sequenceBank = seqPosition.SequenceData;

            for (int i = 0; i < sequenceBank.SequenceGroupList.Count; i++)
            {
                SequenceGroupData seqBodyData = sequenceBank.SequenceGroupList[i];
                SequenceObjectData seqPartsData = seqBodyData.CurrentObjectList;

                // Processing is skipped if the parts data cannot be obtained.
                // 
                // パーツデータが取得できない場合は処理をスキップします。
                if (seqPartsData == null)
                {
                    continue;
                }

                List<PatternObjectData> patternObjects = seqPartsData.PatternObjectList;
                foreach (PatternObjectData patPartsData in patternObjects)
                {
                    DrawData putInfoData = patPartsData.InterpolationDrawData;
                    Color color = putInfoData.Color;
                    Point point = putInfoData.Position;
                    Vector2 position = new Vector2(point.X, point.Y);

                    // Obtains the drawn text string.
                    // 
                    // 描画文字列を取得します。
                    string text = GetSequenceString(i);

                    // Sets the draw position to right aligned.
                    // 
                    // 描画位置を右寄せに設定します。
                    position.X -= font.MeasureString(text).X;

                    Batch.DrawString(font, text, position, color);
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns the text string to display in the sequence.
        /// 
        /// シーケンスに表示する文字列を返します。
        /// </summary>
        protected virtual string GetSequenceString(int id)
        {
            return String.Empty;
        }
        #endregion

    }
}


