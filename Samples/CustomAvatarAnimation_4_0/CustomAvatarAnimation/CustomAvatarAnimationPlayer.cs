#region File Description
//-----------------------------------------------------------------------------
// CustomAvatarAnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace CustomAvatarAnimation
{
    /// <summary>
    /// This type implements an animation at runtime, including the 
    /// current state and updating that state for time.
    /// </summary>
    public class CustomAvatarAnimationPlayer : CustomAvatarAnimationData, IAvatarAnimation
    {
        #region Current Animation State

        /// <summary>
        /// The current keyframe in the animation.
        /// </summary>
        private int currentKeyframe = 0;

        /// <summary>
        /// The current expression keyframe in the animation
        /// </summary>
        private int currentExpressionKeyframe = 0;

        /// <summary>
        /// The current temporal position in the animation.
        /// </summary>
        private TimeSpan currentPosition = TimeSpan.Zero;

        /// <summary>
        /// The current temporal position in the animation.
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                currentPosition = value;

                // Set the current keyframe to 0 since we don't know where we are
                // in the list of keyframes. The next update will set the correct 
                // keyframe.
                currentKeyframe = 0;
                currentExpressionKeyframe = 0;

                // update the animation for the new position, 
                // elapsing zero additional time
                Update(TimeSpan.Zero, false);
            }
        }

        /// <summary>
        /// The current position of the bones as the current time in the animation.
        /// </summary>
        Matrix[] avatarBoneTransforms = new Matrix[AvatarRenderer.BoneCount];

        /// <summary>
        /// The current position of the bones as the current time in the animation.
        /// </summary>
        public ReadOnlyCollection<Matrix> BoneTransforms
        {
            get { return boneTransforms; }
        }
        private ReadOnlyCollection<Matrix> boneTransforms;

        /// <summary>
        /// Returns the avatars current expression
        /// </summary>
        public AvatarExpression Expression
        {
            get
            {
                return avatarExpression;
            }
        }
        AvatarExpression avatarExpression = new AvatarExpression();

        #endregion


        #region Initialization

        /// <summary>
        /// Constructs a new CustomAvatarAnimationPlayer object.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <param name="length">The length of the animation.</param>
        /// <param name="keyframes">The keyframes in the animation.</param>
        public CustomAvatarAnimationPlayer(string name, TimeSpan length,
                                           List<AvatarKeyFrame> keyframes,
                                           List<AvatarExpressionKeyFrame> expressionKeyframes) :
            base(name, length, keyframes, expressionKeyframes)
        {
            // Reset the current bone transforms
            for (int i = 0; i < AvatarRenderer.BoneCount; i++)
            {
                avatarBoneTransforms[i] = Matrix.Identity;
            }

            boneTransforms = new ReadOnlyCollection<Matrix>(avatarBoneTransforms);

            // Update the current bone transforms to the first position in the animation
            Update(TimeSpan.Zero, false);
        }

        #endregion


        #region Updating

        /// <summary>
        /// Updates the current position of the animation.
        /// </summary>
        /// <param name="timeSpan">The elapsed time since the last update.</param>
        /// <param name="loop">If true, the animation will loop.</param>
        public void Update(TimeSpan timeSpan, bool loop)
        {
            // Add the elapsed time to the current time.
            currentPosition += timeSpan;

            // Check current time against the length
            if (currentPosition > Length)
            {
                if (loop)
                {
                    // Find the right time in the new loop iteration
                    while (currentPosition > Length)
                    {
                        currentPosition -= Length;
                    }
                    // Set the keyframe to 0.
                    currentKeyframe = 0;
                    currentExpressionKeyframe = 0;
                }
                else
                {
                    // If the animation is not looping, 
                    // then set the time to the end of the animation.
                    currentPosition = Length;
                }
            }
            // Check to see if we are less than zero
            else if (currentPosition < TimeSpan.Zero)
            {
                if (loop)
                {
                    // If the animation is looping, 
                    // then find the right time in the new loop iteration
                    while (currentPosition < TimeSpan.Zero)
                    {
                        currentPosition += Length;
                    }
                    // Set the keyframe to the last keyframe
                    currentKeyframe = Keyframes.Count - 1;
                    currentExpressionKeyframe = ExpressionKeyframes.Count - 1;
                }
                else
                {
                    // If the animation is not looping, 
                    // then set the time to the beginning of the animation.
                    currentPosition = TimeSpan.Zero;
                }
            }

            // Update the bone transforms based on the current time.
            UpdateBoneTransforms(timeSpan >= TimeSpan.Zero);

            // Update the expression
            UpdateAvatarExpression(timeSpan >= TimeSpan.Zero);
        }


        /// <summary>
        /// Updates the transforms with the correct keyframes based on the current time.
        /// </summary>
        /// <param name="playingForward">
        /// If true, the animation is playing forward; otherwise, it is playing backwards
        /// </param>
        private void UpdateBoneTransforms(bool playingForward)
        {
            if (playingForward)
            {
                while (currentKeyframe < Keyframes.Count)
                {
                    // Get the current keyframe
                    AvatarKeyFrame keyframe = Keyframes[currentKeyframe];

                    // Stop when we've read up to the current time.
                    if (keyframe.Time >= currentPosition)
                        break;

                    // Apply the current keyframe's transform to the bone array.
                    avatarBoneTransforms[keyframe.Bone] = keyframe.Transform;

                    // Move the current keyframe forward.
                    currentKeyframe++;
                }
            }
            else
            {
                while (currentKeyframe >= 0)
                {
                    // Get the current keyframe
                    AvatarKeyFrame keyframe = Keyframes[currentKeyframe];

                    // Stop when we've read back to the current time.
                    if (keyframe.Time <= currentPosition)
                        break;

                    // Apply the current keyframe's transform to the bone array.
                    avatarBoneTransforms[keyframe.Bone] = keyframe.Transform;

                    // Move the current keyframe backwards.
                    currentKeyframe--;
                }
            }
        }


        /// <summary>
        /// Updates the expression with the correct keyframes based on the current time.
        /// </summary>
        /// <param name="playingForward">
        /// If true, the animation is playing forward; otherwise, it is playing backwards
        /// </param>
        private void UpdateAvatarExpression(bool playingForward)
        {
            // Check to see if we have an expression animation
            if (ExpressionKeyframes == null || ExpressionKeyframes.Count == 0)
                return;

            if (playingForward)
            {
                while (currentExpressionKeyframe < ExpressionKeyframes.Count)
                {
                    // Get the current keyframe
                    AvatarExpressionKeyFrame keyframe = ExpressionKeyframes[currentExpressionKeyframe];

                    // Stop when we've read up to the current time.
                    if (keyframe.Time >= currentPosition)
                        break;

                    // Set the current expression
                    avatarExpression = keyframe.Expression;

                    // Move the current keyframe forward.
                    currentExpressionKeyframe++;
                }
            }
            else
            {
                while (currentExpressionKeyframe >= 0)
                {
                    // Get the current keyframe
                    AvatarExpressionKeyFrame keyframe = ExpressionKeyframes[currentExpressionKeyframe];

                    // Stop when we've read back to the current time.
                    if (keyframe.Time <= currentPosition)
                        break;

                    // Set the current expression
                    avatarExpression = keyframe.Expression;

                    // Move the current keyframe backwards.
                    currentExpressionKeyframe--;
                }
            }
        }

        #endregion
    }
}
