#region File Description
//-----------------------------------------------------------------------------
// AnimationBinder.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using RobotGameData.Helper;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// This class calculates the animation key frame for the bones of character 
    /// with time flow. 
    /// Each character bone corresponds to each AnimtionBinder and each has 
    /// key frame for an animation.  
    /// Therefore, in order to do the animation blend, 
    /// two key frame values of AnimationBinder need to be combined.
    /// </summary>
    public class AnimationBinder
    {
        #region Fields

        AnimPlayMode playMode = AnimPlayMode.Repeat;
        float localTime = 0.0f;
        float timeScaleFactor = 1.0f;
        KeyFrameSequence keyFrameSequence = null;

        #endregion

        #region Properties

        public KeyFrameSequence KeyFrameSequence { get { return keyFrameSequence; } }
        public AnimPlayMode PlayMode { get { return playMode; } }
        public float LocalTime { get { return localTime; } }
        public float Duration { get { return KeyFrameSequence.Duration; } }
        public float ScaleFactor { get { return timeScaleFactor; } }
        public bool IsPlayDone { get { return (LocalTime >= Duration); } }

        #endregion

        /// <summary>
        /// sets the mode of the animation play.
        /// </summary>
        /// <param name="mode">animation mode</param>
        public void SetPlayMode(AnimPlayMode mode)
        {
            playMode = mode;
        }

        /// <summary>
        /// sets the animation’s local time.
        /// </summary>
        public void SetTime(float time)
        {
            localTime = time;
        }

        /// <summary>
        /// sets the time flow of animation.
        /// If the value is bigger than 1.0, the animate speed becomes faster.  
        /// If lower than 1.0, the animate speed becomes lower.
        /// </summary>
        /// <param name="scaleFactor">time scale factor</param>
        public void SetTimeScaleFactor(float scaleFactor)
        {
            timeScaleFactor = scaleFactor;
        }

        /// <summary>
        /// sets the animation key frame structure.
        /// </summary>
        public void SetKeyFrameSequence(KeyFrameSequence keyFrame)
        {
            keyFrameSequence = keyFrame;
        }

        /// <summary>
        /// binds the animation key frame.
        /// </summary>
        /// <param name="keyFrame">Animation key frame sequence</param>
        /// <param name="startTime">Start time of the animaton</param>
        /// <param name="timeScaleFactor">Time scale of the animaton</param>
        /// <param name="mode">Play mode of the animaton</param>
        public void BindKeyFrameSequence(KeyFrameSequence keyFrame, 
                                         float startTime, 
                                         float timeScaleFactor, 
                                         AnimPlayMode mode)
        {
            SetKeyFrameSequence(keyFrame);
            SetTime(startTime);
            SetTimeScaleFactor(timeScaleFactor);
            SetPlayMode(mode);
        }

        /// <summary>
        /// Initialize members
        /// </summary>
        public void Initialize()
        {
            SetKeyFrameSequence(null);
            SetTime(0.0f);
            SetTimeScaleFactor(1.0f);
            SetPlayMode(AnimPlayMode.Repeat);
        }

        /// <summary>
        /// Copies to target AnimationBinder
        /// </summary>
        /// <param name="target">target AnimationBinder</param>
        public void CopyTo(AnimationBinder target)
        {
            target.SetKeyFrameSequence(KeyFrameSequence);
            target.SetTime(LocalTime);
            target.SetTimeScaleFactor(ScaleFactor);
            target.SetPlayMode(PlayMode);
        }

        /// <summary>
        /// calculates the key frame of the animation of the specified time.
        /// </summary>
        /// <param name="time">animation time</param>
        /// <returns>animation key frame</returns>
        public KeyFrame GetKeyFrame(float time)
        {
            if (KeyFrameSequence == null)
            {
                throw new InvalidOperationException("Sequence is not set.");
            }

            // Don't do anything if the timeScaleFactor is 0. (default scale is 1.0)
            if (ScaleFactor != 0.0f)
            {
                //  Accumulate animation local time
                localTime += (float)(ScaleFactor * time);

                switch (PlayMode)
                {
                    case AnimPlayMode.Once:     //  Just play once
                        {
                            if (localTime > KeyFrameSequence.Duration)
                                localTime = KeyFrameSequence.Duration;
                        }
                        break;
                    case AnimPlayMode.Repeat:   //  Play looping
                        {
                            //  Calculate time remainder after local time looping
                            if (localTime > Duration)
                                localTime = HelperMath.CalculateModulo(localTime, 
                                                        KeyFrameSequence.Duration);
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported play mode");
                }

                return KeyFrameSequence.GetInterpolateKeyFrame(localTime, PlayMode);
            }

            return new KeyFrame();
        }

        /// <summary>
        /// calculates the key frame of the animation of the specified time.
        /// </summary>
        /// <param name="time">animation time</param>
        /// <returns>Matrix of the animation key frame</returns>
        public Matrix GetKeyFrameMatrix(float time)
        {
            if (KeyFrameSequence == null)
            {
                throw new InvalidOperationException("Sequence is not set.");
            }

            // Don't do anything if the timeScaleFactor is 0. (default scale is 1.0)
            if (ScaleFactor != 0.0f)
            {
                //  Accumulate animation local time
                localTime += (float)(ScaleFactor * time);

                switch (PlayMode)
                {
                    case AnimPlayMode.Once:     //  Just play once
                        {
                            if (localTime > KeyFrameSequence.Duration)
                                localTime = KeyFrameSequence.Duration;
                        }
                        break;
                    case AnimPlayMode.Repeat:   //  Play looping
                        {
                            //  Calculate time remainder after local time looping
                            if (localTime > Duration)
                                localTime = HelperMath.CalculateModulo(localTime, 
                                                        KeyFrameSequence.Duration);
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported play mode");
                }

                return KeyFrameSequence.GetInterpolateMatrix(localTime, PlayMode);
            }

            return Matrix.Identity;
        }
    }
}
