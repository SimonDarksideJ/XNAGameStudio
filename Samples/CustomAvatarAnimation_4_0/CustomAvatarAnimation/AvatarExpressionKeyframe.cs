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
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace CustomAvatarAnimation
{
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public struct AvatarExpressionKeyFrame
    {
        /// <summary>
        /// The time offset from the start of the animation to this keyframe.
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// The AvatarExpression to use at this keyframe
        /// </summary>
        public AvatarExpression Expression;

        #region Initialization

        /// <summary>
        /// Constructs a new AvatarExpressionKeyFrame object.
        /// </summary>
        public AvatarExpressionKeyFrame(TimeSpan time, AvatarExpression expression)
        {
            Time = time;
            Expression = expression;
        }

        #endregion
    }
}
