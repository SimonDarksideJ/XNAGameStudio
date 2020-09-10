#region File Description
//-----------------------------------------------------------------------------
// AvatarKeyframe.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace CustomAvatarAnimation
{
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public struct AvatarKeyFrame
    {
        /// <summary>
        /// The index of the target bone that is animated by this keyframe.
        /// </summary>
        public int Bone;

        /// <summary>
        /// The time offset from the start of the animation to this keyframe.
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// The bone transform for this keyframe.
        /// </summary>
        public Matrix Transform;

        #region Initialization

        /// <summary>
        /// Constructs a new AvatarKeyFrame object.
        /// </summary>
        public AvatarKeyFrame(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }

        #endregion
    }
}
