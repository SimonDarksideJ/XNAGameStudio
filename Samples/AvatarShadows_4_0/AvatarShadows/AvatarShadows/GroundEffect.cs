#region File Description
//-----------------------------------------------------------------------------
// GroundEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AvatarShadows
{
    /// <summary>
    /// Provides a basic wrapper on top of an Effect to expose our parameters like
    /// the built in effects.
    /// </summary>
    public class GroundEffect : IEffectMatrices
    {
        // Our parameters
        EffectParameter world;
        EffectParameter view;
        EffectParameter projection;
        EffectParameter texture;
        EffectParameter shadow;

        /// <summary>
        /// Gets the underlying Effect.
        /// </summary>
        public Effect BaseEffect { get; private set; }

        /// <summary>
        /// Gets or sets the World matrix parameter.
        /// </summary>
        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the View matrix parameter.
        /// </summary>
        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set { view.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the Projection matrix parameter.
        /// </summary>
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the diffuse texture parameter.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture.GetValueTexture2D(); }
            set { texture.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the shadow texture parameter.
        /// </summary>
        public Texture2D Shadow
        {
            get { return shadow.GetValueTexture2D(); }
            set { shadow.SetValue(value); }
        }
        
        /// <summary>
        /// Initializes a new GroundEffect on top of the given Effect.
        /// </summary>
        public GroundEffect(Effect effect)
        {
            // Store the effect
            BaseEffect = effect;

            // Get the parameters from the effect
            world = effect.Parameters["World"];
            view = effect.Parameters["View"];
            projection = effect.Parameters["Projection"];
            texture = effect.Parameters["Texture"];
            shadow = effect.Parameters["ShadowTexture"];
        }
    }
}
