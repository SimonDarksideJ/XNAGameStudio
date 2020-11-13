#region File Description
//-----------------------------------------------------------------------------
// ModelAnimationComponent.cs
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
using SkinnedModel;
#endregion

namespace Movipa.Components.Animation.ModelAnimation
{
    /// <summary>
    /// This component is for animations used in puzzles.
    /// This class inherits PuzzleAnimation to animate and draw
    /// skin models.
    ///
    /// パズルで使用するアニメーションのコンポーネントです。
    /// このクラスはPuzzleAnimationを継承し、スキンモデルを
    /// アニメーションさせて描画します。
    /// </summary>
    public class ModelAnimationComponent : PuzzleAnimation
    {
        #region Fields
        private readonly Color ClearColor;
        private Matrix projection;
        private Matrix view;
        private Vector3 cameraUpVector;
        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        private List<SkinnedModelData> modelList;
        private SkinnedModelAnimationInfo skinnedModelAnimationInfo;
        #endregion

        #region Property
        /// <summary>
        /// Obtains the movie information.
        ///
        /// ムービー情報を取得します。
        /// </summary>
        public new SkinnedModelAnimationInfo Info
        {
            get { return skinnedModelAnimationInfo; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public ModelAnimationComponent(Game game, SkinnedModelAnimationInfo info)
            : base(game, info)
        {
            skinnedModelAnimationInfo = info;

            // Sets the color to clear the background.
            // 
            // 背景をクリアする色を設定します。
            ClearColor = Color.CornflowerBlue;
        }


        /// <summary>
        /// Performs initialization.
        ///
        /// 初期化処理を行います。
        /// </summary>
        public override void Initialize()
        {
            // Obtains the setting information of the camera.
            // 
            // カメラの設定を取得します。
            cameraPosition = Info.CameraPosition;
            cameraLookAt = Info.CameraLookAt;
            cameraUpVector = Info.CameraUpVector;

            // Creates a projection.
            // 
            // プロジェクションを作成します。
            float aspect = (float)Info.Size.X / (float)Info.Size.Y;
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, aspect, 0.1f, 1000.0f);

            base.Initialize();
        }


        /// <summary>
        /// Loads the contents.
        ///
        /// コンテントの読み込み処理を行います。
        /// </summary>
        protected override void LoadContent()
        {
            // Loads the skin model data.
            // 
            // モデルデータを読み込みます。
            modelList = new List<SkinnedModelData>();
            List<SkinnedModelInfo> list = Info.SkinnedModelInfoCollection;
            foreach (SkinnedModelInfo skinnedModelInfo in list)
            {
                LoadModel(skinnedModelInfo);
            }

            base.LoadContent();
        }


        /// <summary>
        /// Loads the skin model data.
        ///
        /// モデルデータを読み込みます。
        /// </summary>
        private void LoadModel(SkinnedModelInfo skinnedModelInfo)
        {
            // Loads the skin model data.
            // 
            // モデルデータを読み込みます。
            Model model = Content.Load<Model>(skinnedModelInfo.ModelAsset);
            SkinnedModelData modelData = new SkinnedModelData(model, "Take 001");

            // Sets the animation data.
            // 
            // アニメーションデータを設定します。
            SkinningData skinningData = modelData.Model.Tag as SkinningData;
            modelData.AnimationPlayer = new AnimationPlayer(skinningData);
            modelData.AnimationClip =
                skinningData.AnimationClips[skinnedModelInfo.AnimationClip];
            modelData.AnimationPlayer.StartClip(modelData.AnimationClip);

            // Obtains the position of the skin model.
            // 
            // ポジションを取得します。
            modelData.Position = skinnedModelInfo.Position;

            // Adds the skin model data to the list.
            // 
            // リストに追加します。
            modelList.Add(modelData);
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the skin model and camera.
        ///
        /// モデルとカメラの更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Updates the animation for all the skin models.
            // 
            // 全てのモデルのアニメーションを更新します。
            foreach (SkinnedModelData model in modelList)
            {
                model.AnimationPlayer.Update(
                    gameTime.ElapsedGameTime, true, Matrix.Identity);
            }

            // Updates the camera.
            // 
            // カメラの更新をします。
            view = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraUpVector);

            base.Update(gameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Performs drawing for the render target.
        ///
        /// レンダーターゲットへの描画処理を行います。
        /// </summary>
        protected override void DrawRenderTarget()
        {
            // Clears the background.
            // 
            // 背景をクリアします。
            GraphicsDevice.Clear(ClearColor);

            // Enable the depth buffer.
            GraphicsDevice.RenderState.DepthBufferEnable = true;

            // Draws all the skin models.
            // 
            // 全てのモデルを描画します。
            foreach (SkinnedModelData model in modelList)
            {
                model.Draw(view, projection);
            }
        }
        #endregion
    }
}

