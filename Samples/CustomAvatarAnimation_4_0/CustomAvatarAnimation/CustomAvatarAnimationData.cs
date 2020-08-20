#region File Description
//-----------------------------------------------------------------------------
// CustomAvatarAnimationData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace CustomAvatarAnimation
{
    /// <summary>
    /// The type contains the animation data for a single animation
    /// </summary>
    public class CustomAvatarAnimationData
    {
        /// <summary>
        /// The name of the animation clip
        /// </summary>
        [ContentSerializer]
        public string Name { get; private set; }

        /// <summary>
        /// The total length of the animation.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Length { get; private set; }

        /// <summary>
        /// A combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<AvatarKeyFrame> Keyframes { get; private set; }

        /// <summary>
        /// A combined list containing all the keyframes for expressions,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<AvatarExpressionKeyFrame> ExpressionKeyframes { get; private set; }

        #region Initialization

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private CustomAvatarAnimationData() { }


        /// <summary>
        /// Constructs a new CustomAvatarAnimationData object.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <param name="length">The length of the animation.</param>
        /// <param name="keyframes">The keyframes in the animation.</param>
        public CustomAvatarAnimationData(string name, TimeSpan length,
                                         List<AvatarKeyFrame> keyframes,
                                         List<AvatarExpressionKeyFrame> expressionKeyframes)
        {
            // safety-check the parameters
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (length <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("length",
                                          "The length of the animation cannot be zero.");
            }
            if ((keyframes == null) || (keyframes.Count <= 0))
            {
                throw new ArgumentNullException("keyframes");
            }

            // assign the parameters
            Name = name;
            Length = length;
            Keyframes = keyframes;
            ExpressionKeyframes = expressionKeyframes;
        }

        #endregion
    }
}
