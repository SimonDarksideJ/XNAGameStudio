#region File Description
//-----------------------------------------------------------------------------
// LightingEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// Wraps the lighting shader to cache effect parameters.
    /// </summary>
    class LightingEffect
    {
        Effect effect;
        public Effect Effect
        {
            get { return effect; }
        }

        #region Effect Techniques
        private EffectTechnique noTextureTechnique;
        public EffectTechnique NoTextureTechnique
        {
            get { return noTextureTechnique; }
        }

        private EffectTechnique singleTextureTechnique;
        public EffectTechnique SingleTextureTechnique
        {
            get { return singleTextureTechnique; }
        }
        #endregion

        #region Effect parameters
        EffectParameter world;
        public EffectParameter World
        {
            get { return world; }
        }
       
        EffectParameter worldView;
        public EffectParameter WorldView
        {
            get { return worldView; }
        }

        EffectParameter worldViewProjection;
        public EffectParameter WorldViewProjection
        {
            get { return worldViewProjection; }
        }
        

        EffectParameter lightPos;
        public EffectParameter LightPos
        {
            get { return lightPos; }
        }

        EffectParameter cameraPos;
        public EffectParameter CameraPos
        {
            get { return cameraPos; }
        }

        EffectParameter glowScale;
        public EffectParameter GlowScale
        {
            get { return glowScale; }
        }

        EffectParameter colorOverride;
        public EffectParameter ColorOverride
        {
            get { return colorOverride; }
        }

        EffectParameter texCoordScale;
        public EffectParameter TexCoordScale
        {
            get { return texCoordScale; }
        }

        EffectParameter texCoordTranslation;
        public EffectParameter TexCoordTranslation
        {
            get { return texCoordTranslation; }
        }

        EffectParameter diffuseTexture;
        public EffectParameter DiffuseTexture
        {
            get { return diffuseTexture; }
        }

        #endregion

        public void LoadContent()
        {
            effect = Pickture.Instance.Content.Load<Effect>("Shaders/Lighting");

            // cache all of the effect techniques
            noTextureTechnique = effect.Techniques["NoTexture"];
            singleTextureTechnique = effect.Techniques["SingleTexture"];

            // set the default technique to single-texture
            effect.CurrentTechnique = singleTextureTechnique;

            // cache all of the effect parameters
            world = effect.Parameters["World"];
            worldView = effect.Parameters["WorldView"];
            worldViewProjection = effect.Parameters["WorldViewProjection"];
            lightPos = effect.Parameters["LightPos"];
            cameraPos = effect.Parameters["CameraPos"];
            glowScale = effect.Parameters["GlowScale"];
            colorOverride = effect.Parameters["ColorOverride"];
            texCoordScale = effect.Parameters["TexCoordScale"];
            texCoordTranslation = effect.Parameters["TexCoordTranslation"];
            diffuseTexture = effect.Parameters["DiffuseTexture"];
        }
    }
}