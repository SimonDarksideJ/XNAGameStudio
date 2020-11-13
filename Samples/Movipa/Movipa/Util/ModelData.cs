#region File Description
//-----------------------------------------------------------------------------
// ModelData.cs
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
#endregion

namespace Movipa.Util
{
    /// <summary>
    /// 3Dのモデルデータを管理します。
    /// モデルデータと位置とスケールの情報を持ち、
    /// 最小限の描画機能を提供します。
    /// </summary>
    public abstract class ModelData : PrimitiveRenderState, IDisposable
    {
        #region Fields
        // 開放済みフラグ
        private bool disposed = false;

        // 3Dモデルデータ
        protected Model model = null;

        // 位置
        protected Vector3 position = Vector3.Zero;
        protected Vector3 rotate = Vector3.Zero;
        protected Vector3 yawPitchRoll = Vector3.Zero;

        // スケール
        protected float scale = 1.0f;

        // ボーンマトリックス
        private Matrix[] boneTransforms;
        #endregion

        #region Properties
        /// <summary>
        /// 開放状態を取得します。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }


        /// <summary>
        /// モデルを取得します。
        /// </summary>
        public Model Model
        {
            get { return model; }
        }


        /// <summary>
        /// 位置を取得または設定します。
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }


        /// <summary>
        /// 回転を取得または設定します。
        /// </summary>
        public Vector3 Rotate
        {
            get { return rotate; }
            set { rotate = value; }
        }


        /// <summary>
        /// ヨーとピッチとロールを取得または設定します。
        /// </summary>
        public Vector3 YawPitchRoll
        {
            get { return yawPitchRoll; }
            set { yawPitchRoll = value; }
        }


        /// <summary>
        /// ヨーを取得または設定します。
        /// </summary>
        public float Yaw
        {
            get { return yawPitchRoll.X; }
            set { yawPitchRoll.X = value; }
        }


        /// <summary>
        /// ピッチを取得または設定します。
        /// </summary>
        public float Pitch
        {
            get { return yawPitchRoll.Y; }
            set { yawPitchRoll.Y = value; }
        }


        /// <summary>
        /// ロールを取得または設定します。
        /// </summary>
        public float Roll
        {
            get { return yawPitchRoll.Z; }
            set { yawPitchRoll.Z = value; }
        }


        /// <summary>
        /// スケールを取得または設定します。
        /// </summary>
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new ModelData object.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        protected ModelData(Model model)
        {
            this.model = model;

            boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
            }
        }

        #endregion


        #region Draw Methods
        /// <summary>
        /// 3Dモデルを描画します。
        /// </summary>
        public virtual void Draw(Matrix view, Matrix projection)
        {
            Draw(Matrix.Identity, view, projection);
        }


        /// <summary>
        /// 3Dモデルを描画します。
        /// </summary>
        public virtual void Draw(Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                // エフェクトのパラメータを設定します。
                SetEffectParameters(mesh, world, view, projection);

                // モデルを描画します。
                mesh.Draw();

                ClearTexturesFromEffects(mesh);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ワールドマトリックスを取得します。
        /// </summary>
        public Matrix GetWorldMatrix(ModelMesh mesh, Matrix world)
        {
            if (model == null || mesh == null)
                return Matrix.Identity;

            return boneTransforms[mesh.ParentBone.Index] *
                                Matrix.CreateScale(Scale) *
                                Matrix.CreateRotationX(Rotate.X) *
                                Matrix.CreateRotationY(Rotate.Y) *
                                Matrix.CreateRotationZ(Rotate.Z) *
                                Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) *
                                Matrix.CreateTranslation(Position) *
                                world;
        }


        /// <summary>
        /// エフェクトのパラメータを設定します。
        /// </summary>
        protected virtual void SetEffectParameters(
            ModelMesh mesh, Matrix world, Matrix view, Matrix projection)
        {
            foreach (Effect effect in mesh.Effects)
            {
                SetEffectParameters(effect, GetWorldMatrix(mesh, world), view, 
                    projection);
            }
        }


        /// <summary>
        /// エフェクトのパラメータを設定します。
        /// </summary>
        protected virtual void SetEffectParameters(
            Effect effect, Matrix world, Matrix view, Matrix projection)
        {
            BasicEffect basicEffect = effect as BasicEffect;
            if (basicEffect != null)
            {
                // BasicEffectのパラメータを設定します。

                basicEffect.World = world;
                basicEffect.View = view;
                basicEffect.Projection = projection;

                basicEffect.EnableDefaultLighting();
                basicEffect.PreferPerPixelLighting = true;
            }
            else
            {
                // その他のEffectが設定されているので、
                // Effectの名前が"World", "View", "Projection"と
                // 同じ物を探して、値を設定します。

                foreach (EffectParameter effectParameter in effect.Parameters)
                {
                    if (effectParameter.Name == "World")
                    {
                        effectParameter.SetValue(world);
                    }
                    else if (effectParameter.Name == "View")
                    {
                        effectParameter.SetValue(view);
                    }
                    else if (effectParameter.Name == "Projection")
                    {
                        effectParameter.SetValue(projection);
                    }
                }
            }
        }


        /// <summary>
        /// To work around an XNA Game Studio 2.0 bug, textures from render targets
        /// or resolve targets must be cleared manually, or they can interfere with
        /// automatic restoration after the graphics device is lost.
        /// </summary>
        private void ClearTexturesFromEffects(ModelMesh mesh)
        {
            foreach (Effect effect in mesh.Effects)
            {
                ClearTexturesFromEffects(effect);
            }
        }


        /// <summary>
        /// To work around an XNA Game Studio 2.0 bug, textures from render targets
        /// or resolve targets must be cleared manually, or they can interfere with
        /// automatic restoration after the graphics device is lost.
        /// </summary>
        protected virtual void ClearTexturesFromEffects(Effect effect) { }


        #endregion

    }
}
