#region File Description
//-----------------------------------------------------------------------------
// BasicModelData.cs
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
    /// Manages the basic model data.
    /// Includes parameters available in BasicEffect. 
    /// The values of the parameters can be utilized in drawing. Note that 
    /// models without BasicEffect cannot be drawn.
    /// 
    /// 標準のモデルデータを管理します。
    /// BasicEffectで扱えるパラメータが用意されており、
    /// 描画時にその値を反映させることが出来ます。
    /// ただし、BasicEffectを持たないモデルは描画することが出来ません。
    /// </summary>
    public class BasicModelData : ModelData
    {
        #region Public Types
        /// <summary>
        /// Manages light parameters including light color,
        /// direction and reflection color.
        /// 
        /// ライトのパラメータを管理します。
        /// ライトの色と方向、反射の色などのパラメータがあります。
        /// </summary>
        public class DirectionalLight
        {
            #region Fields
            private Vector3 diffuseColor = Vector3.Zero;
            private Vector3 direction = Vector3.Up;
            private bool enabled = false;
            private Vector3 specularColor = Vector3.Zero;
            #endregion

            #region Properties
            /// <summary>
            /// Obtains or sets diffuse reflection light.
            /// 
            /// 拡散反射光を取得または設定します。
            /// </summary>
            public Vector3 DiffuseColor
            {
                get { return diffuseColor; }
                set { diffuseColor = value; }
            }


            /// <summary>
            /// Obtains or sets the light direction. 
            /// 
            /// ライトの向きを取得または設定します。
            /// </summary>
            public Vector3 Direction
            {
                get { return direction; }
                set { direction = value; }
            }


            /// <summary>
            /// Obtains or sets the light enabled status.
            /// 
            /// ライトの有効状態を取得または設定します。
            /// </summary>
            public bool Enabled
            {
                get { return enabled; }
                set { enabled = value; }
            }


            /// <summary>
            /// Obtains or sets the specular color. 
            /// 
            /// スペキュラの色を取得または設定します。
            /// </summary>
            public Vector3 SpecularColor
            {
                get { return specularColor; }
                set { specularColor = value; }
            }
            #endregion
        }
        #endregion

        #region Fields
        protected float alpha;
        protected Vector3 ambientLightColor;
        protected Vector3 diffuseColor;
        protected Vector3 emissiveColor;
        protected bool fogEnabled;
        protected Vector3 fogColor;
        protected float fogStart;
        protected float fogEnd;
        protected bool lightingEnabled;
        protected bool preferPerPixelLighting;
        protected Vector3 specularColor;
        protected float specularPower;
        protected DirectionalLight directionalLight0;
        protected DirectionalLight directionalLight1;
        protected DirectionalLight directionalLight2;
        protected Texture2D texture;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the diffuse reflection light. 
        /// 
        /// 拡散反射光を取得または設定します。
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; }
        }


        /// <summary>
        /// Obtains or sets the diffuse reflection light. 
        /// 
        /// 拡散反射光を取得または設定します。
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }


        /// <summary>
        /// Obtains or sets the brightness. 
        /// 
        /// 輝度を取得または設定します。
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            set { emissiveColor = value; }
        }


        /// <summary>
        /// Obtains or sets the alpha value.
        /// 
        /// アルファ値を取得または設定します。
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }


        /// <summary>
        /// Obtains or sets the fog enabled status.
        /// 
        /// フォグの有効状態を取得または設定します。
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }
            set { fogEnabled = value; }
        }


        /// <summary>
        /// Obtains or sets the fog color.
        /// 
        /// フォグの色を取得または設定します。
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }


        /// <summary>
        /// Obtains or sets the fog start position.
        /// 
        /// フォグの開始位置を取得または設定します。
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }
            set { fogStart = value; }
        }


        /// <summary>
        /// Obtains or sets the fog end position.
        /// 
        ///フォグの終了位置を取得または設定します。
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }
            set { fogEnd = value; }
        }


        /// <summary>
        /// Obtains or sets the lighting enabled status.
        /// 
        /// ライティングの有効状態を取得または設定します。
        /// </summary>
        public bool LightingEnabled
        {
            get { return lightingEnabled; }
            set { lightingEnabled = value; }
        }


        /// <summary>
        /// Obtains or sets the PreferPerPixel lighting enabled status.
        /// 
        /// なんとかライティングの有効状態を取得または設定します。
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return preferPerPixelLighting; }
            set { preferPerPixelLighting = value; }
        }


        /// <summary>
        /// Obtains or sets the specular color.
        /// 
        /// スペキュラの色を取得または設定します。
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }


        /// <summary>
        /// Obtains or sets the specular power.
        /// 
        /// スペキュラの強さを取得または設定します。
        /// </summary>
        public float SpecularPower
        {
            get { return specularPower; }
            set { specularPower = value; }
        }


        /// <summary>
        /// Obtains or sets the Light0 parameters.
        /// 
        /// ライト0のパラメータを取得または設定します。
        /// </summary>
        public DirectionalLight DirectionalLight0
        {
            get { return directionalLight0; }
            set { directionalLight0 = value; }
        }


        /// <summary>
        /// Obtains or sets the Light1 parameters.
        /// 
        /// ライト1のパラメータを取得または設定します。
        /// </summary>
        public DirectionalLight DirectionalLight1
        {
            get { return directionalLight1; }
            set { directionalLight1 = value; }
        }


        /// <summary>
        /// Obtains or sets the Light2 parameters.
        /// 
        /// ライト2のパラメータを取得または設定します。
        /// </summary>
        public DirectionalLight DirectionalLight2
        {
            get { return directionalLight2; }
            set { directionalLight2 = value; }
        }

        
        /// <summary>
        /// Obtains or sets the texture.
        /// 
        /// テクスチャを取得または設定します。
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public BasicModelData(Model model)
            : base(model)
        {
            // Sets the initial values of BasicEffect.
            // 
            // BasicEffectの初期値を設定します。
            alpha = 1.0f;
            ambientLightColor = new Vector3(0, 0, 0);
            diffuseColor = new Vector3(0.5882353f, 0.5882353f, 0.5882353f);
            emissiveColor = new Vector3(0.3f, 0.3f, 0.3f);
            fogEnabled = false;
            fogColor = new Vector3(0, 0, 0);
            fogStart = 0;
            fogEnd = 1;
            lightingEnabled = false;
            preferPerPixelLighting = false;
            specularColor = new Vector3(0, 0, 0);
            specularPower = 1.99999988f;
            directionalLight0 = new DirectionalLight();
            directionalLight1 = new DirectionalLight();
            directionalLight2 = new DirectionalLight();
            texture = null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets the effect parameters.
        /// 
        /// エフェクトのパラメータを設定します。
        /// </summary>
        protected override void SetEffectParameters(
            Effect effect, Matrix world, Matrix view, Matrix projection)
        {
            BasicDirectionalLight directionalLight;

            // Processing is not performed if unable to convert to BasicEffect.
            // 
            // BasicEffectに変換できない場合は処理を行わないようにします。
            BasicEffect basicEffect = effect as BasicEffect;
            if (basicEffect == null)
                return;

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            // Registers the lighting settings.
            // 
            // ライティングの設定をします。
            basicEffect.Alpha = Alpha;
            basicEffect.LightingEnabled = LightingEnabled;
            basicEffect.PreferPerPixelLighting = PreferPerPixelLighting;
            basicEffect.AmbientLightColor = AmbientLightColor;
            basicEffect.DiffuseColor = DiffuseColor;
            basicEffect.EmissiveColor = EmissiveColor;
            basicEffect.SpecularColor = SpecularColor;
            basicEffect.SpecularPower = SpecularPower;

            // Registers the DirectionLight0 settings.
            // 
            // DirectionalLight0の設定をします。
            directionalLight = basicEffect.DirectionalLight0;
            directionalLight.Enabled = DirectionalLight0.Enabled;
            directionalLight.DiffuseColor = DirectionalLight0.DiffuseColor;
            directionalLight.Direction = DirectionalLight0.Direction;
            directionalLight.SpecularColor = DirectionalLight0.SpecularColor;

            // Registers the DirectionLight1 settings.
            // 
            // DirectionalLight1の設定をします。
            directionalLight = basicEffect.DirectionalLight1;
            directionalLight.Enabled = DirectionalLight1.Enabled;
            directionalLight.DiffuseColor = DirectionalLight1.DiffuseColor;
            directionalLight.Direction = DirectionalLight1.Direction;
            directionalLight.SpecularColor = DirectionalLight1.SpecularColor;

            // Registers the DirectionLight2 settings.
            // 
            // DirectionalLight2の設定をします。
            directionalLight = basicEffect.DirectionalLight2;
            directionalLight.Enabled = DirectionalLight2.Enabled;
            directionalLight.DiffuseColor = DirectionalLight2.DiffuseColor;
            directionalLight.Direction = DirectionalLight2.Direction;
            directionalLight.SpecularColor = DirectionalLight2.SpecularColor;


            // Registers the fog settings.
            // 
            // フォグの設定をします。
            basicEffect.FogEnabled = FogEnabled;
            basicEffect.FogColor = FogColor;
            basicEffect.FogStart = FogStart;
            basicEffect.FogEnd = FogEnd;

            // Registers the texture settings.
            // 
            // テクスチャの設定をします。
            if (Texture != null)
                basicEffect.Texture = Texture;
        }


        /// <summary>
        /// To work around an XNA Game Studio 2.0 bug, textures from render targets
        /// or resolve targets must be cleared manually, or they can interfere with
        /// automatic restoration after the graphics device is lost.
        /// </summary>
        protected override void ClearTexturesFromEffects(Effect effect)
        {
            // Processing is not performed if unable to convert to BasicEffect.
            // 
            // BasicEffectに変換できない場合は処理を行わないようにします。
            if (Texture != null)
            {
                BasicEffect basicEffect = effect as BasicEffect;
                if (basicEffect != null)
                {
                    basicEffect.Texture = null;
                }
            }
        }

        #endregion
    }
}