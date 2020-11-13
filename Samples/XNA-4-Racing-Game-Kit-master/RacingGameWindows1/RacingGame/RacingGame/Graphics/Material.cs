#region File Description
//-----------------------------------------------------------------------------
// Material.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Helpers;
using RacingGame.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// Material class for DirectX materials used for Models. Consists of
    /// normal DirectX material settings (ambient, diffuse, specular),
    /// the diffuse texture and optionally of normal map, height map and shader
    /// parameters.
    /// </summary>
    public class Material : IDisposable
    {
        #region Constants
        /// <summary>
        /// Default color values are:
        /// 0.15f for ambient and 1.0f for diffuse and 1.0f specular.
        /// </summary>
        public static readonly Color
            DefaultAmbientColor = new Color(40, 40, 40),
            DefaultDiffuseColor = new Color(210, 210, 210),
            DefaultSpecularColor = new Color(255, 255, 255);

        /// <summary>
        /// Default specular power (24)
        /// </summary>
        const float DefaultSpecularPower = 24.0f;

        /// <summary>
        /// Parallax amount for parallax and offset shaders.
        /// </summary>
        public const float DefaultParallaxAmount = 0.04f;
        #endregion

        #region Variables
        /// <summary>
        /// Colors
        /// </summary>
        public Color diffuseColor = DefaultDiffuseColor,
            ambientColor = DefaultAmbientColor,
            specularColor = DefaultSpecularColor;

        /// <summary>
        /// Specular power
        /// </summary>
        public float specularPower = DefaultSpecularPower;

        /// <summary>
        /// Diffuse texture for the material. Can be null for unused.
        /// </summary>
        public Texture diffuseTexture = null;
        /// <summary>
        /// Normal texture in case we use normal mapping. Can be null for unused.
        /// </summary>
        public Texture normalTexture = null;
        /// <summary>
        /// Height texture in case we use parallax mapping. Can be null for unused.
        /// </summary>
        public Texture heightTexture = null;
        /// <summary>
        /// Detail texture, used for landscape rendering. Can be null for unused.
        /// </summary>
        public Texture detailTexture = null;
        /// <summary>
        /// Parallax amount for parallax and offset shaders.
        /// </summary>
        public float parallaxAmount = DefaultParallaxAmount;
        #endregion

        #region Properties
        /// <summary>
        /// Checks if the diffuse texture has alpha
        /// </summary>
        public bool HasAlpha
        {
            get
            {
                if (diffuseTexture != null)
                    return diffuseTexture.HasAlphaPixels;
                else
                    return false;
            }
        }
        #endregion

        #region Constructors
        #region Default Constructors
        /// <summary>
        /// Create material, just using default values.
        /// </summary>
        public Material()
        {
        }

        /// <summary>
        /// Create material, just using default color values.
        /// </summary>
        public Material(string setDiffuseTexture)
        {
            diffuseTexture = new Texture(setDiffuseTexture);
        }

        /// <summary>
        /// Create material
        /// </summary>
        public Material(Color setAmbientColor, Color setDiffuseColor,
            string setDiffuseTexture)
        {
            ambientColor = setAmbientColor;
            diffuseColor = setDiffuseColor;
            diffuseTexture = new Texture(setDiffuseTexture);
            // Leave rest to default
        }

        /// <summary>
        /// Create material
        /// </summary>
        public Material(Color setAmbientColor, Color setDiffuseColor,
            Texture setDiffuseTexture)
        {
            ambientColor = setAmbientColor;
            diffuseColor = setDiffuseColor;
            diffuseTexture = setDiffuseTexture;
            // Leave rest to default
        }

        /// <summary>
        /// Create material
        /// </summary>
        public Material(string setDiffuseTexture, string setNormalTexture)
        {
            diffuseTexture = new Texture(setDiffuseTexture);
            normalTexture = new Texture(setNormalTexture);
            // Leave rest to default
        }

        /// <summary>
        /// Create material
        /// </summary>
        public Material(string setDiffuseTexture, string setNormalTexture,
            string setHeightTexture)
        {
            diffuseTexture = new Texture(setDiffuseTexture);
            normalTexture = new Texture(setNormalTexture);
            heightTexture = new Texture(setHeightTexture);
            // Leave rest to default
        }

        /// <summary>
        /// Create material
        /// </summary>
        public Material(Color setAmbientColor, Color setDiffuseColor,
            Color setSpecularColor, string setDiffuseTexture,
            string setNormalTexture, string setHeightTexture,
            string setDetailTexture)
        {
            ambientColor = setAmbientColor;
            diffuseColor = setDiffuseColor;
            specularColor = setSpecularColor;
            diffuseTexture = new Texture(setDiffuseTexture);
            if (String.IsNullOrEmpty(setNormalTexture) == false)
                normalTexture = new Texture(setNormalTexture);
            if (String.IsNullOrEmpty(setHeightTexture) == false)
                heightTexture = new Texture(setHeightTexture);
            if (String.IsNullOrEmpty(setDetailTexture) == false)
                detailTexture = new Texture(setDetailTexture);
            // Leave rest to default
        }
        #endregion

        #region Create material from effect settings
        /// <summary>
        /// Create material
        /// </summary>
        /// <param name="effect">Effect</param>
        public Material(Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");

            EffectParameter diffuseTextureParameter =
                effect.Parameters["diffuseTexture"];
            if (diffuseTextureParameter != null)
                diffuseTexture = new Texture(
                    diffuseTextureParameter.GetValueTexture2D());

            EffectParameter normalTextureParameter =
                effect.Parameters["normalTexture"];
            if (normalTextureParameter != null)
                normalTexture = new Texture(
                    normalTextureParameter.GetValueTexture2D());

            EffectParameter diffuseColorParameter =
                effect.Parameters["diffuseColor"];
            if (diffuseColorParameter != null)
                diffuseColor = new Color(diffuseColorParameter.GetValueVector4());

            EffectParameter ambientColorParameter =
                effect.Parameters["ambientColor"];
            if (ambientColorParameter != null)
                ambientColor = new Color(ambientColorParameter.GetValueVector4());

            EffectParameter specularColorParameter =
                effect.Parameters["specularColor"];
            if (specularColorParameter != null)
                specularColor = new Color(specularColorParameter.GetValueVector4());

            EffectParameter specularPowerParameter =
                effect.Parameters["specularPower"];
            if (specularPowerParameter != null)
                specularPower = specularPowerParameter.GetValueSingle();
        }
        #endregion
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (diffuseTexture != null)
                    diffuseTexture.Dispose();
                if (normalTexture != null)
                    normalTexture.Dispose();
                if (heightTexture != null)
                    heightTexture.Dispose();
                if (detailTexture != null)
                    detailTexture.Dispose();
            }
        }
        #endregion
    }
}
