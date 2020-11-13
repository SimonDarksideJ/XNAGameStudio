#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelData.cs
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

using SkinnedModel;
using Movipa.Util;
#endregion

namespace Movipa.Util
{
    /// <summary>
    /// Contains the skin model parameters.
    /// Inherits the ModelData class and expands the skin animation parameters.
    /// Includes a dedicated Draw method since BasicEffect is not used for drawing. 
    /// 
    /// スキンモデルのパラメータを持ちます。
    /// ModelDataクラスを継承し、スキンアニメーションのパラメータを拡張しています。
    /// 描画にはBasicEffectを使用しないので、専用のDrawメソッドを持ちます。
    /// </summary>
    public class SkinnedModelData : ModelData
    {
        #region Fields
        private AnimationPlayer animationPlayer;
        private AnimationClip animationClip;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the animation player.
        /// 
        /// アニメーションプレイヤーを取得または設定します。
        /// </summary>
        public AnimationPlayer AnimationPlayer
        {
            get { return animationPlayer; }
            set { animationPlayer = value; }
        }


        /// <summary>
        /// Obtains or sets the animation clip.
        ///
        /// アニメーションクリップを取得または設定します。
        /// </summary>
        public AnimationClip AnimationClip
        {
            get { return animationClip; }
            set { animationClip = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SkinnedModelData(Model model, string animationClipName)
            : base(model)
        {
            this.model = model;
            SkinningData skinningData = Model.Tag as SkinningData;
            AnimationPlayer = new AnimationPlayer(skinningData);
            AnimationClip = skinningData.AnimationClips[animationClipName];
            AnimationPlayer.StartClip(AnimationClip); 
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the skin model.
        /// 
        /// スキンモデルを描画します。
        /// </summary>
        public override void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Draw(world, view, projection, true, null, null, null, null, null);
        }



        /// <summary>
        /// Draws the skin model.
        /// 
        /// スキンモデルを描画します。
        /// </summary>
        public void Draw(
            Matrix world, Matrix view, Matrix projection, bool lightingEnabled)
        {
            Draw(world, view, projection, lightingEnabled, null, null, null, null, null);
        }


        /// <summary>
        /// Draws the skin model.
        /// 
        /// スキンモデルを描画します。
        /// </summary>
        public void Draw(
            Matrix world,
            Matrix view,
            Matrix projection,
            bool lightingEnabled,
            Vector3? light1Color,
            Vector3? light2Color)
        {
            Draw(
                world,
                view, 
                projection,
                lightingEnabled,
                light1Color,
                null,
                light2Color,
                null, 
                null);
        }


        /// <summary>
        /// Draws the skin model.
        /// 
        /// スキンモデルを描画します。
        /// </summary>
        public void Draw(
            Matrix world,
            Matrix view, 
            Matrix projection,
            bool lightingEnabled,
            Vector3? light1Color, 
            Vector3? light1Direction, 
            Vector3? light2Color,
            Vector3? light2Direction,
            float? ambientColor)
        {
            Matrix[] bones = GetBones(world);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Sets the effect parameters.
                    // 
                    // エフェクトのパラメータを設定します。
                    EffectParameterCollection parameters = effect.Parameters;

                    parameters["Bones"].SetValue(bones);
                    parameters["View"].SetValue(view);
                    parameters["Projection"].SetValue(projection);

                    parameters["LightingEnabled"].SetValue(lightingEnabled);

                    if (light1Color != null && light1Color.HasValue)
                        parameters["Light1Color"].SetValue(light1Color.Value);

                    if (light1Direction != null && light1Direction.HasValue)
                        parameters["Light1Direction"].SetValue(light1Direction.Value);

                    if (light2Color != null && light2Color.HasValue)
                        parameters["Light2Color"].SetValue(light2Color.Value);

                    if (light2Direction != null && light2Direction.HasValue)
                        parameters["Light2Direction"].SetValue(light2Direction.Value);

                    if (ambientColor != null && ambientColor.HasValue)
                        parameters["AmbientColor"].SetValue(ambientColor.Value);
                }

                mesh.Draw();
            }
        }
        #endregion

        #region Helper Methods


        /// <summary>
        /// Obtains the bones.
        /// 
        /// ボーンを取得します。
        /// </summary>
        private Matrix[] GetBones(Matrix world)
        {
            Matrix[] bones = AnimationPlayer.GetSkinTransforms();

            Matrix worldMatrix = world *
            Matrix.CreateScale(Scale) *
            Matrix.CreateRotationX(Rotate.X) *
            Matrix.CreateRotationY(Rotate.Y) *
            Matrix.CreateRotationZ(Rotate.Z) *
            Matrix.CreateTranslation(Position);

            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = bones[i] * worldMatrix;
            }

            return bones;
        }
        #endregion
    }
}