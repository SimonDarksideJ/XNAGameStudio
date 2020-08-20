#region File Information
//-----------------------------------------------------------------------------
// AnimationStore.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// Stores a collection of animations.
    /// </summary>
    public class AnimationStore
    {
        #region Fields


        [ContentSerializer]
        public Dictionary<string, Animation> Animations { set; private get; }


        #endregion     

        #region Accessor


        /// <summary>
        /// Returns an animation from the store which has the specified alias.
        /// </summary>
        /// <param name="animationAlias">Alias of the desired animation.</param>
        /// <returns>An animation object matching the specified alias.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the store does not contain an animation with the 
        /// specified alias.</exception>
        public Animation this[string animationAlias]
        {
            get
            {
                return Animations[animationAlias];
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Initializes all contained animation by loading their associated sprite sheets. Call this method before
        /// attempting to use any of the animations in the store.
        /// </summary>
        /// <param name="contentManager">Content manager used to initialize the animations.</param>
        public void Initialize(ContentManager contentManager)
        {
            foreach (Animation animation in Animations.Values)
            {
                animation.LoadSheet(contentManager);
            }
        }


        #endregion
    }
}
