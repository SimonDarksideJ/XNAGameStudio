#region File Description
//-----------------------------------------------------------------------------
// AnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace SkinnedModel
{
    /// <summary>
    /// An animation clip is the runtime equivalent of the
    /// Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// Constructs a new animation clip object.
        /// </summary>
        public AnimationClip(TimeSpan duration, IList<Keyframe> keyframes)
        {
            durationValue = duration;
            keyframesValue = keyframes;
        }


        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        public TimeSpan Duration
        {
            get { return durationValue; }
        }

        TimeSpan durationValue;


        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        public IList<Keyframe> Keyframes
        {
            get { return keyframesValue; }
        }

        IList<Keyframe> keyframesValue;
    }
}
