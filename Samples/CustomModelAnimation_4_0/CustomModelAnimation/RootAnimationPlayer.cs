#region File Description
//-----------------------------------------------------------------------------
// RootAnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace CustomModelAnimation
{
    /// <summary>
    /// The animation player contains a single transformation that is used to move/position/scale
    /// something.
    /// </summary>
    public class RootAnimationPlayer : ModelAnimationPlayerBase
    {
        Matrix currentTransform;        
        
        /// <summary>
        /// Initializes the transformation to the identity
        /// </summary>
        protected override void InitClip()
        {
            this.currentTransform = Matrix.Identity;
        }

        /// <summary>
        /// Sets the key frame by storing the current transform
        /// </summary>
        /// <param name="keyframe"></param>
        protected override void SetKeyframe(ModelKeyframe keyframe)
        {
            this.currentTransform = keyframe.Transform;
        }

        /// <summary>
        /// Gets the current transformation being applied
        /// </summary>
        /// <returns>Transformation matrix</returns>
        public Matrix GetCurrentTransform()
        {
            return this.currentTransform;
        }
    }
}
