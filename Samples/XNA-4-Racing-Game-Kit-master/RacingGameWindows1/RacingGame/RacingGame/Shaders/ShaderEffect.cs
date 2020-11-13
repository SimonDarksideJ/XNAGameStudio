#region File Description
//-----------------------------------------------------------------------------
// ShaderEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RacingGame.Graphics;
using RacingGame.Helpers;
using Texture = RacingGame.Graphics.Texture;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture;
#endregion

namespace RacingGame.Shaders
{
    /// <summary>
    /// Shader effect class. You can either directly use this class by
    /// providing a fx filename in the constructor or derive from this class
    /// for special shader functionality (see post screen shaders for a more
    /// complex example).
    /// </summary>
    public class ShaderEffect : IDisposable
    {
        #region Some shaders
        /// <summary>
        /// Line rendering shader
        /// </summary>
        public static ShaderEffect lineRendering =
            new ShaderEffect("LineRendering.fx");

        /// <summary>
        /// Simple shader with just per pixel lighting for testing.
        /// </summary>
        public static ShaderEffect lighting =
            new ShaderEffect("LightingShader.fx");

        /// <summary>
        /// Normal mapping shader for simple objects and the landscape rendering.
        /// </summary>
        public static ShaderEffect normalMapping =
            new ShaderEffect("NormalMapping.fx");

        /// <summary>
        /// Landscape normal mapping shader for the landscape rendering with
        /// detail texture support, everything else should use normalMapping.
        /// </summary>
        public static ShaderEffect landscapeNormalMapping =
            new ShaderEffect("LandscapeNormalMapping.fx");

        /// <summary>
        /// Shadow mapping shader
        /// </summary>
        public static ShadowMapShader shadowMapping =
            new ShadowMapShader();
        #endregion

        #region Variables
        /// <summary>
        /// Content name for this shader
        /// </summary>
        private string shaderContentName = "";

        /// <summary>
        /// Effect
        /// </summary>
        protected Effect effect = null;
        /// <summary>
        /// Effect handles for shaders.
        /// </summary>
        protected EffectParameter worldViewProj,
            viewProj,
            world,
            viewInverse,
            projection,
            lightDir,
            ambientColor,
            diffuseColor,
            specularColor,
            specularPower,
            alphaFactor,
            scale,
            diffuseTexture,
            normalTexture,
            heightTexture,
            reflectionCubeTexture,
            detailTexture,
            parallaxAmount,
            carHueColorChange;
        #endregion

        #region Properties
        /// <summary>
        /// Is this shader valid to render? If not we can't perform any rendering.
        /// </summary>
        /// <returns>Bool</returns>
        public bool Valid
        {
            get
            {
                return effect != null;
            }
        }

        /// <summary>
        /// Effect
        /// </summary>
        /// <returns>Effect</returns>
        public Effect Effect
        {
            get
            {
                return effect;
            }
        }

        /// <summary>
        /// Number of techniques
        /// </summary>
        /// <returns>Int</returns>
        public int NumberOfTechniques
        {
            get
            {
                return effect.Techniques.Count;
            }
        }

        /// <summary>
        /// Get technique
        /// </summary>
        /// <param name="techniqueName">Technique name</param>
        /// <returns>Effect technique</returns>
        public EffectTechnique GetTechnique(string techniqueName)
        {
            return effect.Techniques[techniqueName];
        }

        /// <summary>
        /// World parameter
        /// </summary>
        /// <returns>Effect parameter</returns>
        public EffectParameter WorldParameter
        {
            get
            {
                return world;
            }
        }

        /// <summary>
        /// Set value helper to set an effect parameter.
        /// </summary>
        /// <param name="param">Param</param>
        /// <param name="setMatrix">Set matrix</param>
        private static void SetValue(EffectParameter param,
            ref Matrix lastUsedMatrix, Matrix newMatrix)
        {
            // Always update, matrices change every frame anyway!
            lastUsedMatrix = newMatrix;
            param.SetValue(newMatrix);
        }

        /// <summary>
        /// Set value helper to set an effect parameter.
        /// </summary>
        /// <param name="param">Param</param>
        /// <param name="lastUsedVector">Last used vector</param>
        /// <param name="newVector">New vector</param>
        private static void SetValue(EffectParameter param,
            ref Vector3 lastUsedVector, Vector3 newVector)
        {
            if (param != null &&
                lastUsedVector != newVector)
            {
                lastUsedVector = newVector;
                param.SetValue(newVector);
            }
        }

        /// <summary>
        /// Set value helper to set an effect parameter.
        /// </summary>
        /// <param name="param">Param</param>
        /// <param name="lastUsedColor">Last used color</param>
        /// <param name="newColor">New color</param>
        private static void SetValue(EffectParameter param,
            ref Color lastUsedColor, Color newColor)
        {
            // Note: This check eats few % of the performance, but the color
            // often stays the change (around 50%).
            if (param != null &&
                //slower: lastUsedColor != newColor)
                lastUsedColor.PackedValue != newColor.PackedValue)
            {
                lastUsedColor = newColor;
                param.SetValue(newColor.ToVector4());
            }
        }

        /// <summary>
        /// Set value helper to set an effect parameter.
        /// </summary>
        /// <param name="param">Param</param>
        /// <param name="lastUsedValue">Last used value</param>
        /// <param name="newValue">New value</param>
        private static void SetValue(EffectParameter param,
            ref float lastUsedValue, float newValue)
        {
            if (param != null &&
                lastUsedValue != newValue)
            {
                lastUsedValue = newValue;
                param.SetValue(newValue);
            }
        }

        /// <summary>
        /// Set value helper to set an effect parameter.
        /// </summary>
        /// <param name="param">Param</param>
        /// <param name="lastUsedValue">Last used value</param>
        /// <param name="newValue">New value</param>
        private static void SetValue(EffectParameter param,
            ref XnaTexture lastUsedValue, XnaTexture newValue)
        {
            if (param != null &&
                lastUsedValue != newValue)
            {
                lastUsedValue = newValue;
                param.SetValue(newValue);
            }
        }

        protected Matrix lastUsedWorldViewProjMatrix = Matrix.Identity;
        /// <summary>
        /// Set world view proj matrix
        /// </summary>
        protected Matrix WorldViewProjMatrix
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedWorldViewProjMatrix;
            }
            set
            {
                SetValue(worldViewProj, ref lastUsedWorldViewProjMatrix, value);
            }
        }

        protected Matrix lastUsedViewProjMatrix = Matrix.Identity;
        /// <summary>
        /// Set view proj matrix
        /// </summary>
        protected Matrix ViewProjMatrix
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedViewProjMatrix;
            }
            set
            {
                SetValue(viewProj, ref lastUsedViewProjMatrix, value);
            }
        }

        /// <summary>
        /// Set world matrix
        /// </summary>
        public Matrix WorldMatrix
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return Matrix.Identity;//makes REALLY no sense!
            }
            set
            {
                // Faster, we checked world matrix in constructor.
                world.SetValue(value);
            }
        }

        protected Matrix lastUsedInverseViewMatrix = Matrix.Identity;
        /// <summary>
        /// Set view inverse matrix
        /// </summary>
        protected Matrix InverseViewMatrix
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedInverseViewMatrix;
            }
            set
            {
                SetValue(viewInverse, ref lastUsedInverseViewMatrix, value);
            }
        }

        protected Matrix lastUsedProjectionMatrix = Matrix.Identity;
        /// <summary>
        /// Set projection matrix
        /// </summary>
        protected Matrix ProjectionMatrix
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedProjectionMatrix;
            }
            set
            {
                SetValue(projection, ref lastUsedProjectionMatrix, value);
            }
        }

        protected Vector3 lastUsedLightDir = Vector3.Zero;
        /// <summary>
        /// Set light direction
        /// </summary>
        protected Vector3 LightDir
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedLightDir;
            }
            set
            {
                // Make sure lightDir is normalized (fx files are optimized
                // to work with a normalized lightDir vector)
                value.Normalize();
                // Set negative value, shader is optimized not to negate dir!
                SetValue(lightDir, ref lastUsedLightDir, -value);
            }
        }

        protected Color lastUsedAmbientColor = ColorHelper.Empty;
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color AmbientColor
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedAmbientColor;
            }
            set
            {
                SetValue(ambientColor, ref lastUsedAmbientColor, value);
            }
        }

        protected Color lastUsedDiffuseColor = ColorHelper.Empty;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color DiffuseColor
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedDiffuseColor;
            }
            set
            {
                SetValue(diffuseColor, ref lastUsedDiffuseColor, value);
            }
        }

        protected Color lastUsedSpecularColor = ColorHelper.Empty;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color SpecularColor
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedSpecularColor;
            }
            set
            {
                SetValue(specularColor, ref lastUsedSpecularColor, value);
            }
        }

        private float lastUsedSpecularPower = 0;
        /// <summary>
        /// SpecularPower for specular color
        /// </summary>
        public float SpecularPower
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedSpecularPower;
            }
            set
            {
                SetValue(specularPower, ref lastUsedSpecularPower, value);
            }
        }

        private float lastUsedAlphaFactor = 0;
        /// <summary>
        /// Alpha factor
        /// </summary>
        /// <returns>Float</returns>
        public float AlphaFactor
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedAlphaFactor;
            }
            set
            {
                SetValue(alphaFactor, ref lastUsedAlphaFactor, value);
            }
        }

        protected XnaTexture lastUsedDiffuseTexture = null;
        /// <summary>
        /// Set diffuse texture
        /// </summary>
        public Texture DiffuseTexture
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return null;//makes no sense!
            }
            set
            {
                SetValue(diffuseTexture, ref lastUsedDiffuseTexture,
                    value != null ? value.XnaTexture : null);
            }
        }

        protected XnaTexture lastUsedNormalTexture = null;
        /// <summary>
        /// Set normal texture for normal mapping
        /// </summary>
        public Texture NormalTexture
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return null;//makes no sense!
            }
            set
            {
                SetValue(normalTexture, ref lastUsedNormalTexture,
                    value != null ? value.XnaTexture : null);
            }
        }

        protected XnaTexture lastUsedHeightTexture = null;
        /// <summary>
        /// Set height texture for parallax mapping
        /// </summary>
        public Texture HeightTexture
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return null;//makes no sense!
            }
            set
            {
                SetValue(heightTexture, ref lastUsedHeightTexture,
                    value != null ? value.XnaTexture : null);
            }
        }

        protected TextureCube lastUsedReflectionCubeTexture = null;
        /// <summary>
        /// Set reflection cube map texture for reflection stuff.
        /// </summary>
        public TextureCube ReflectionCubeTexture
        {
            get
            {
                return lastUsedReflectionCubeTexture;
            }
            set
            {
                if (reflectionCubeTexture != null &&
                    lastUsedReflectionCubeTexture != value)
                {
                    lastUsedReflectionCubeTexture = value;
                    reflectionCubeTexture.SetValue(value);
                }
            }
        }

        protected XnaTexture lastUsedDetailTexture = null;
        /// <summary>
        /// Set height texture for parallax mapping
        /// </summary>
        public Texture DetailTexture
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return null;//makes no sense!
            }
            set
            {
                SetValue(detailTexture, ref lastUsedDetailTexture,
                    value != null ? value.XnaTexture : null);
            }
        }

        protected float lastUsedParallaxAmount = -1.0f;
        /// <summary>
        /// Parallax amount for parallax and offset shaders.
        /// </summary>
        public float ParallaxAmount
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedParallaxAmount;
            }
            set
            {
                SetValue(parallaxAmount, ref lastUsedParallaxAmount, value);
            }
        }

        protected Color lastUsedCarHueColorChange = ColorHelper.Empty;
        /// <summary>
        /// Shadow car color for the special ShadowCar shader.
        /// </summary>
        public Color CarHueColorChange
        {
            get
            {
                // Note: Only implemented for stupid FxCop rule,
                // you should never "get" a shader texture this way!
                return lastUsedCarHueColorChange;
            }
            set
            {
                SetValue(carHueColorChange, ref lastUsedCarHueColorChange, value);
            }
        }
        #endregion

        #region Constructor
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ShaderEffect(string shaderName)
        {
            if (BaseGame.Device == null)
                throw new InvalidOperationException(
                    "XNA device is not initialized, can't create ShaderEffect.");

            shaderContentName = Path.GetFileNameWithoutExtension(shaderName);

            Reload();
        }
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
                // Dispose shader effect
                if (effect != null)
                    effect.Dispose();
            }
        }
        #endregion

        #region Reload effect
        /// <summary>
        /// Reload effect (can be useful if we change the fx file dynamically).
        /// </summary>
        public void Reload()
        {
            // Load shader
            effect = BaseGame.Content.Load<Effect>(
                Path.Combine(Directories.ContentDirectory + "\\shaders",
                shaderContentName));

            // Reset and get all avialable parameters.
            // This is especially important for derived classes.
            ResetParameters();
            GetParameters();
        }
        #endregion

        #region Reset parameters
        /// <summary>
        /// Reset parameters
        /// </summary>
        protected virtual void ResetParameters()
        {
            lastUsedInverseViewMatrix = Matrix.Identity;
            lastUsedAmbientColor = ColorHelper.Empty;
            lastUsedDiffuseTexture = null;
        }
        #endregion

        #region Get parameters
        /// <summary>
        /// Get parameters, override to support more
        /// </summary>
        protected virtual void GetParameters()
        {
            worldViewProj = effect.Parameters["worldViewProj"];
            viewProj = effect.Parameters["viewProj"];
            world = effect.Parameters["world"];
            viewInverse = effect.Parameters["viewInverse"];
            projection = effect.Parameters["projection"];
            lightDir = effect.Parameters["lightDir"];
            ambientColor = effect.Parameters["ambientColor"];
            diffuseColor = effect.Parameters["diffuseColor"];
            specularColor = effect.Parameters["specularColor"];
            specularPower = effect.Parameters["specularPower"];
            alphaFactor = effect.Parameters["alphaFactor"];
            // Default alpha factor to 1.0f for hotels and stuff
            AlphaFactor = 1.0f;
            scale = effect.Parameters["scale"];
            diffuseTexture = effect.Parameters["diffuseTexture"];
            normalTexture = effect.Parameters["normalTexture"];
            heightTexture = effect.Parameters["heightTexture"];
            reflectionCubeTexture = effect.Parameters["reflectionCubeTexture"];
            detailTexture = effect.Parameters["detailTexture"];
            parallaxAmount = effect.Parameters["parallaxAmount"];
            carHueColorChange = effect.Parameters["carHueColorChange"];
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// Set parameters, this overload sets all material parameters too.
        /// </summary>
        public virtual void SetParameters(Material setMat)
        {
            if (worldViewProj != null)
                worldViewProj.SetValue(BaseGame.WorldViewProjectionMatrix);
            if (viewProj != null)
                viewProj.SetValue(BaseGame.ViewProjectionMatrix);
            if (world != null)
                world.SetValue(BaseGame.WorldMatrix);
            if (viewInverse != null)
                viewInverse.SetValue(BaseGame.InverseViewMatrix);
            if (lightDir != null)
                lightDir.SetValue(BaseGame.LightDirection);

            // Set the reflection cube texture only once
            if (lastUsedReflectionCubeTexture == null &&
                reflectionCubeTexture != null)
            {
                ReflectionCubeTexture = BaseGame.UI.SkyCubeMapTexture;
            }

            // Set all material properties
            if (setMat != null)
            {
                AmbientColor = setMat.ambientColor;
                DiffuseColor = setMat.diffuseColor;
                SpecularColor = setMat.specularColor;
                SpecularPower = setMat.specularPower;
                DiffuseTexture = setMat.diffuseTexture;
                NormalTexture = setMat.normalTexture;
                HeightTexture = setMat.heightTexture;
                ParallaxAmount = setMat.parallaxAmount;
                DetailTexture = setMat.detailTexture;
            }
        }

        /// <summary>
        /// Set parameters, override to set more
        /// </summary>
        public virtual void SetParameters()
        {
            SetParameters(null);
        }

        /// <summary>
        /// Set parameters, this overload sets all material parameters too.
        /// </summary>
        public virtual void SetParametersOptimizedGeneral()
        {
            if (worldViewProj != null)
                worldViewProj.SetValue(BaseGame.WorldViewProjectionMatrix);
            if (viewProj != null)
                viewProj.SetValue(BaseGame.ViewProjectionMatrix);
            if (world != null)
                world.SetValue(BaseGame.WorldMatrix);
            if (viewInverse != null)
                viewInverse.SetValue(BaseGame.InverseViewMatrix);
            if (lightDir != null)
                lightDir.SetValue(BaseGame.LightDirection);

            // Set the reflection cube texture only once
            if (lastUsedReflectionCubeTexture == null &&
                reflectionCubeTexture != null)
            {
                ReflectionCubeTexture = BaseGame.UI.SkyCubeMapTexture;
            }

            // lastUsed parameters for colors and textures are not used,
            // but we overwrite the values in SetParametersOptimized.
            // We fix this by clearing all lastUsed values we will use later.
            lastUsedAmbientColor = ColorHelper.Empty;
            lastUsedDiffuseColor = ColorHelper.Empty;
            lastUsedSpecularColor = ColorHelper.Empty;
            lastUsedDiffuseTexture = null;
            lastUsedNormalTexture = null;
        }

        /// <summary>
        /// Set parameters, this overload sets all material parameters too.
        /// </summary>
        public virtual void SetParametersOptimized(Material setMat)
        {
            if (setMat == null)
                throw new ArgumentNullException("setMat");

            // No need to set world matrix, will be done later in mesh rendering
            // in the MeshRenderManager. All the rest is set with help of the
            // SetParametersOptimizedGeneral above.

            // Only update ambient, diffuse, specular and the textures, the rest
            // will not change for a material change in MeshRenderManager.
            ambientColor.SetValue(setMat.ambientColor.ToVector4());
            diffuseColor.SetValue(setMat.diffuseColor.ToVector4());
            specularColor.SetValue(setMat.specularColor.ToVector4());
            if (setMat.diffuseTexture != null)
                diffuseTexture.SetValue(setMat.diffuseTexture.XnaTexture);
            if (setMat.normalTexture != null)
                normalTexture.SetValue(setMat.normalTexture.XnaTexture);
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
            {
                effect.CurrentTechnique.Passes[num].Apply();
            }
        }
        #endregion

        #region Render
        /// <summary>
        /// Render
        /// </summary>
        /// <param name="setMat">Set matrix</param>
        /// <param name="passName">Pass name</param>
        /// <param name="renderDelegate">Render delegate</param>
        public void Render(Material setMat,
            string techniqueName,
            BaseGame.RenderHandler renderCode)
        {
            if (techniqueName == null)
                throw new ArgumentNullException("techniqueName");
            if (renderCode == null)
                throw new ArgumentNullException("renderCode");

            SetParameters(setMat);

            // Start shader
            effect.CurrentTechnique = effect.Techniques[techniqueName];


                // Render all passes (usually just one)
                //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
                {
                    EffectPass pass = effect.CurrentTechnique.Passes[num];

                    pass.Apply();
                    renderCode();
                }
        }

        /// <summary>
        /// Render
        /// </summary>
        /// <param name="techniqueName">Technique name</param>
        /// <param name="renderDelegate">Render delegate</param>
        public void Render(string techniqueName,
            BaseGame.RenderHandler renderDelegate)
        {
            Render(null, techniqueName, renderDelegate);
        }
        #endregion

        #region Render single pass shader
        /// <summary>
        /// Render single pass shader
        /// </summary>
        /// <param name="renderDelegate">Render delegate</param>
        public void RenderSinglePassShader(
            BaseGame.RenderHandler renderCode)
        {
            if (renderCode == null)
                throw new ArgumentNullException("renderCode");

            // Start effect (current technique should be set)

                // Start first pass
                effect.CurrentTechnique.Passes[0].Apply();

                // Render
                renderCode();

        }
        #endregion
    }
}
