#region File Description
//-----------------------------------------------------------------------------
// ModelAnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace CustomModelAnimation
{
    /// <summary>
    /// A model animation clip is the runtime equivalent of the
    /// Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single model animation.
    /// </summary>
    public class ModelAnimationClip
    {
        /// <summary>
        /// Gets the total length of the model animation clip
        /// </summary>
        [ContentSerializer]
        public TimeSpan Duration { get; private set; }
        
        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<ModelKeyframe> Keyframes { get; private set; }

        /// <summary>
        /// Constructs a new model animation clip object.
        /// </summary>
        public ModelAnimationClip(TimeSpan duration, List<ModelKeyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private ModelAnimationClip()
        {
        }
    }
}
