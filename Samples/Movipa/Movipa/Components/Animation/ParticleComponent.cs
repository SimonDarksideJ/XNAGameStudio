#region File Description
//-----------------------------------------------------------------------------
// ParticleComponent.cs
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
using Microsoft.Xna.Framework.Input;

using Movipa.Util;
using MovipaLibrary;
#endregion

namespace Movipa.Components.Animation
{
    /// <summary>
    /// This component is for animations used in puzzles.
    /// This class inherits PuzzleAnimation to draw particles
    /// with SpriteBatch.
    ///
    /// パズルで使用するアニメーションのコンポーネントです。
    /// このクラスはPuzzleAnimationを継承し、パーティクルを
    /// SpriteBatchで描画します。
    /// </summary>
    public class ParticleComponent : PuzzleAnimation
    {
        #region Fields
        private Matrix projection;
        private Matrix view;

        private Vector3 cameraUpVector;
        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        private float cameraRotate = 0;
        private float cameraDistance = 100.0f;

        private UInt32 particleMax;
        private float particleJumpPower;
        private float particleMoveSpeed;
        private UInt32 particleGenerateCount;


        private Texture2D spriteTexture;
        private LinkedList<Particle> particleList;

        // Class to draw a floor
        // 
        // 床を描画するクラス
        private PrimitiveFloor primitiveFloor;

        // Animation information
        // 
        // アニメーション情報
        private ParticleInfo particleInfo;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains the movie information.
        ///
        /// ムービー情報を取得します。
        /// </summary>
        public new ParticleInfo Info
        {
            get { return particleInfo; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public ParticleComponent(Game game, ParticleInfo info)
            : base(game, info)
        {
            particleInfo = info;

            // Creates a class to draw a floor.
            // 
            // 床を描画するクラスを作成します。
            primitiveFloor = new PrimitiveFloor(game.GraphicsDevice);
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
            cameraUpVector = Info.CameraUpVector;
            cameraPosition = Info.CameraPosition;
            cameraLookAt = Info.CameraLookAt;

            // Obtains the setting information of the particle.
            particleMax = Info.ParticleMax;
            particleJumpPower = Info.ParticleJumpPower;
            particleMoveSpeed = Info.ParticleMoveSpeed;
            particleGenerateCount = Info.ParticleGenerateCount;

            // Creates a particle array.
            // 
            // パーティクルの配列を作成します。
            particleList = new LinkedList<Particle>();

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
            // Loads the particle texture.
            // 
            // パーティクルのテクスチャを読み込みます。
            string asset = Info.ParticleTexture;
            spriteTexture = Content.Load<Texture2D>(asset);

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the particle and camera.
        ///
        /// パーティクルとカメラの更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Updates the camera.
            // 
            // カメラの更新処理を行います。
            UpdateCamera();

            // Updates the particle.
            // 
            // パーティクルの更新処理を行います。
            UpdateParticles();

            base.Update(gameTime);
        }


        /// <summary>
        /// Updates all the particles.
        /// 
        /// 全てのパーティクルの更新処理を行います。
        /// </summary>
        private void UpdateParticles()
        {
            // Creates a particle.
            // 
            // パーティクルを作成します。
            CreateParticle();

            // Moves all the particles.
            // 
            // 全てのパーティクルを移動します。
            LinkedListNode<Particle> node = particleList.First;
            LinkedListNode<Particle> removeNode;
            while (node != null)
            {
                Particle particle = node.Value;
                UpdateParticle(particle);

                // Saves the node to determine the end of the animation.
                // 
                // アニメーション終了判定用にノードを保持します。
                removeNode = node;

                // Moves to the next node.
                // 
                // 次のノードへ移動します。
                node = node.Next;

                // Checks the determination node to see whether it has ended.
                // If it has ended, removes this node from the list.
                // 
                // 終了判定用のノードをチェックし、終了していた場合は
                // ノードをリストから削除します。
                if (!removeNode.Value.Enable)
                {
                    particleList.Remove(removeNode);
                }
            }
        }


        /// <summary>
        /// Updates the single particle. 
        ///
        /// 単体のパーティクルの更新処理を行います。
        /// </summary>
        private static void UpdateParticle(Particle particle)
        {
            // Changes the fall velocity for the distance.
            // 
            // 移動量の落下速度を変更します。
            particle.Velocity += Particle.Gravity;

            // Moves the particle.
            // 
            // パーティクルの移動をします。
            particle.Position += particle.Velocity;

            // If the position of the particle falls below the floor, 
            // terminates the animation.
            //
            // パーティクルの位置が床より下に行くと、
            // アニメーションの終了処理を行います。
            if (particle.Position.Y < 0)
                particle.Enable = false;
        }


        /// <summary>
        /// Updates the camera.
        /// </summary>
        private void UpdateCamera()
        {
            // Rotates the camera.
            // 
            // カメラを回転させます。
            cameraRotate += 0.001f;
            cameraPosition.X = (float)Math.Sin(cameraRotate) * cameraDistance;
            cameraPosition.Z = (float)Math.Cos(cameraRotate) * cameraDistance;

            // Creates a view from the camera.
            // 
            // カメラからビューを作成します。
            view = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraUpVector);
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
            GraphicsDevice.Clear(Color.Black);

            // Draws the floor.
            // 
            // 床を描画します。
            primitiveFloor.SetRenderState(GraphicsDevice, SpriteBlendMode.AlphaBlend);
            primitiveFloor.Draw(projection, view);

            // Draws the particle.
            // 
            // パーティクルを描画します。
            DrawParticle();
        }


        /// <summary>
        /// Draws the particle.
        ///
        /// パーティクルを描画します。
        /// </summary>
        private void DrawParticle()
        {
            Batch.Begin(SpriteBlendMode.Additive);

            foreach (Particle particle in particleList)
            {
                Vector3 position = particle.Position;

                // Converts world coordinates to screen coordinates.
                // 
                // ワールド座標からスクリーン座標に変換します。
                Vector2 pos1 = WorldToScreen(position);

                // Converts world coordinates to screen coordinates by 
                // turning them upside down.
                // 
                // 位置を上下反転させて、スクリーン座標に変換します。
                position.Y = -position.Y;
                Vector2 pos2 = WorldToScreen(position);

                // Draws the particle.
                // 
                // パーティクルを描画します。
                Batch.Draw(spriteTexture, pos1, Color.White);
                Batch.Draw(spriteTexture, pos2, Color.Blue);
            }

            Batch.End();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates new particles.
        ///
        /// パーティクルの生成処理を行います。
        /// </summary>
        private void CreateParticle()
        {
            for (int i = 0; i < particleGenerateCount; i++)
            {
                // Checks the number of current particles. 
                // If it exceeds the maximum number of allowed particles, 
                // stops creating new particles.
                //
                // パーティクルの最大数をチェックし、上限に達していたら
                // 生成処理を行いません。
                if (particleList.Count >= particleMax)
                {
                    return;
                }

                // Create a new particle.
                // 
                // パーティクルを新たに作成します。
                Particle particle = new Particle();
                float direction = (float)Random.NextDouble() * 360.0f;
                float speed = ((float)Random.NextDouble() + 0.1f) * particleMoveSpeed;
                float jump = ((float)Random.NextDouble() + 0.1f) * particleJumpPower;

                particle.Enable = true;
                particle.Position = Vector3.Zero;

                Vector3 velocity = new Vector3();
                velocity.X += (float)Math.Sin(direction) * speed;
                velocity.Y += jump;
                velocity.Z += (float)Math.Cos(direction) * speed;
                particle.Velocity = velocity;

                // Adds the particle to the array.
                // 
                // パーティクルを配列に追加します。
                particleList.AddLast(particle);
            }

        }


        /// <summary>
        /// Converts world coordinates to screen coordinates.
        ///
        /// ワールド座標からスクリーン座標に変換します。
        /// </summary>
        private Vector2 WorldToScreen(Vector3 position)
        {
            Vector4 v4 = Vector4.Transform(position, Matrix.Identity);
            v4 = Vector4.Transform(v4, view);
            v4 = Vector4.Transform(v4, projection);

            Vector2 screenSize = new Vector2(Info.Size.X, Info.Size.Y);
            Vector2 screenHalf = screenSize * 0.5f;

            float x = (v4.X / v4.W + 1) * screenHalf.X;
            float y = (1 - v4.Y / v4.W) * screenHalf.Y;

            return new Vector2(x, y);
        }
        #endregion
    }
}

