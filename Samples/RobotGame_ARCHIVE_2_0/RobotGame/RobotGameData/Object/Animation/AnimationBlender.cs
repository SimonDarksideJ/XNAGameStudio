#region File Description
//-----------------------------------------------------------------------------
// AnimationBlender.cs
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
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// this class blends two animation key frames.
    /// However, even when the animation blend is not carried out, 
    /// there must be AnimationBinder with one or more key frame in order 
    /// to play animation.
    /// Each bone of character corresponds to an AnimationBlender and 
    /// may have one or two AnimationBinder.
    /// </summary>
    public class AnimationBlender : INamed
    {
        #region Fields

        /// <summary>
        /// animation binder 1
        /// </summary>
        AnimationBinder firstBinder = null;

        /// <summary>
        /// animation binder 2
        /// </summary>
        AnimationBinder secondBinder = null;

        string name = String.Empty;
        int bindCount = 0;
        float blendTime = 0.0f;
        float elapsedTime = 0.0f;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public float BlendTime
        {
            get { return blendTime; }
        }

        public AnimationBinder AnimationBinder
        {
            get { return firstBinder; }
        }

        public int BindCount
        {
            get { return bindCount; }
        }

        public bool IsEmpty
        {
            get { return (BindCount == 0); }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnimationBlender()
        {
            this.firstBinder = new AnimationBinder();
            this.secondBinder = new AnimationBinder();
            this.bindCount = 0;
        }

        /// <summary>
        /// inserts the new KeyFrameSequence, which is specified in the Binder. 
        /// If there are two Binder’s, the first Binder will be gone and 
        /// the second Binder will be moved as the first Binder,
        /// and the new KeyFrameSequence will be inserted into the second Binder.
        /// When the every Binder is empty, it will then insert to the first Binder
        /// </summary>
        /// <param name="keyFrame">Animation key frame sequence</param>
        /// <param name="startTime">Start time of the animaton</param>
        /// <param name="blendTime">Blend time of two the animaton</param>
        /// <param name="timeScaleFactor">Time scale of the animaton</param>
        /// <param name="mode">Play mode of the animaton</param>
        public void AddKeyFrameSequence(KeyFrameSequence keyFrame, 
                                        float startTime, 
                                        float blendTime, 
                                        float timeScaleFactor, 
                                        AnimPlayMode mode)
        {
            this.blendTime = blendTime;
            this.elapsedTime = startTime;

            if (this.BlendTime == 0.0f)
            {
                ClearAllBinder();

                firstBinder.BindKeyFrameSequence(keyFrame, startTime, 
                                                 timeScaleFactor, mode);
            }
            else
            {
                // If binding above two binders, push out one binder
                if (this.bindCount == 2)
                    ShiftBinder();

                if (this.bindCount == 0)
                {
                    firstBinder.BindKeyFrameSequence(keyFrame, startTime, 
                                                     timeScaleFactor, mode);
                }
                else if (this.bindCount == 1)
                {
                    secondBinder.BindKeyFrameSequence(keyFrame, startTime, 
                                                      timeScaleFactor, mode);
                }
            }

            this.bindCount++;
        }

        /// <summary>
        /// calculates the key frame of the animation of the specified time.
        /// If there are two bound animations, it will calculate by blending.
        /// </summary>
        /// <param name="time">animation time</param>
        /// <returns>Matrix of the animation key frame</returns>
        public Matrix GetKeyFrameMatrix(float time)
        {
            Matrix keyFrameMatrix = Matrix.Identity;

            if (firstBinder == null)
                return keyFrameMatrix;

            this.elapsedTime += time;

            //  We have to blend animations, if it has multiple binders
            if (bindCount > 1)
            {
                float t = this.elapsedTime / this.blendTime;

                // Blending finished
                if (t > 1.0f)
                {
                    keyFrameMatrix = secondBinder.GetKeyFrameMatrix(time);

                    ShiftBinder();
                }
                // Calculate blending matrix
                else
                {
                    KeyFrame[] sourceKeyFrame = new KeyFrame[2];
                    KeyFrame targetKeyFrame = new KeyFrame();

                    //  Calculate two keyFrame
                    sourceKeyFrame[0] = firstBinder.GetKeyFrame(time);
                    sourceKeyFrame[1] = secondBinder.GetKeyFrame(time);

                    // Calculate blending key frame
                    {
                        //  Interpolate translation using two key frame
                        if (sourceKeyFrame[0].HasTranslation && 
                            sourceKeyFrame[1].HasTranslation)
                        {
                            targetKeyFrame.Translation = Vector3.Lerp(
                                                         sourceKeyFrame[0].Translation, 
                                                         sourceKeyFrame[1].Translation,
                                                         t);

                            targetKeyFrame.HasTranslation = true;
                        }
                        else if (sourceKeyFrame[0].HasTranslation)
                        {
                            targetKeyFrame.Translation = sourceKeyFrame[0].Translation;
                            targetKeyFrame.HasTranslation = true;
                        }
                        else if (sourceKeyFrame[1].HasTranslation)
                        {
                            targetKeyFrame.Translation = sourceKeyFrame[1].Translation;
                            targetKeyFrame.HasTranslation = true;
                        }

                        //  Interpolate scale using two key frame
                        if (sourceKeyFrame[0].HasScale && sourceKeyFrame[1].HasScale)
                        {
                            targetKeyFrame.Scale = Vector3.Lerp(
                                                         sourceKeyFrame[0].Scale,
                                                         sourceKeyFrame[1].Scale,
                                                         t);

                            targetKeyFrame.HasScale = true;
                        }
                        else if (sourceKeyFrame[0].HasScale)
                        {
                            targetKeyFrame.Scale = sourceKeyFrame[0].Scale;
                            targetKeyFrame.HasScale = true;
                        }
                        else if (sourceKeyFrame[1].HasScale)
                        {
                            targetKeyFrame.Scale = sourceKeyFrame[1].Scale;
                            targetKeyFrame.HasScale = true;
                        }

                        //  Interpolate rotation using two key frame
                        if (sourceKeyFrame[0].HasRotation && 
                            sourceKeyFrame[1].HasRotation)
                        {
                            targetKeyFrame.Rotation = Quaternion.Slerp(
                                                         sourceKeyFrame[0].Rotation, 
                                                         sourceKeyFrame[1].Rotation, 
                                                         t);

                            targetKeyFrame.HasRotation = true;
                        }
                        else if (sourceKeyFrame[0].HasRotation)
                        {
                            targetKeyFrame.Rotation = sourceKeyFrame[0].Rotation;
                            targetKeyFrame.HasRotation = true;
                        }
                        else if (sourceKeyFrame[1].HasRotation)
                        {
                            targetKeyFrame.Rotation = sourceKeyFrame[1].Rotation;
                            targetKeyFrame.HasRotation = true;
                        }
                    }

                    //  Final, creates a frame matrix using blending key frame
                    {
                        //  Has Rotation ?
                        if (targetKeyFrame.HasRotation)
                        {
                            // Calculates rotation matrix using the quaternion
                            keyFrameMatrix = Matrix.CreateFromQuaternion(
                                                        targetKeyFrame.Rotation);
                        }

                        //  Has translation ?
                        if (targetKeyFrame.HasTranslation)
                        {
                            // Calculates position using keyframe
                            keyFrameMatrix.Translation = targetKeyFrame.Translation;
                        }

                        //  Has scale ?
                        if (targetKeyFrame.HasScale)
                        {
                            // Calculates scale value using keyframe
                            keyFrameMatrix = keyFrameMatrix * 
                                             Matrix.CreateScale(targetKeyFrame.Scale);
                        }
                    }

                    return keyFrameMatrix;
                }
            }
            else
            {
                // No blending, or finished blending
                keyFrameMatrix = firstBinder.GetKeyFrameMatrix(time);
            }

            return keyFrameMatrix;
        }

        /// <summary>
        /// moves the position of the Binder.
        /// If there are two binders already, 
        /// the first Binder will be gone and the second Binder will be moved
        /// to the first Binder.
        /// </summary>
        private void ShiftBinder()
        {
            this.firstBinder.Initialize();
            this.secondBinder.CopyTo(firstBinder);
            this.secondBinder.Initialize();

            this.bindCount--;
        }

        private void ClearAllBinder()
        {
            this.firstBinder.Initialize();
            this.secondBinder.Initialize();
            this.bindCount = 0;
        }
    }
}
