#region File Description
//-----------------------------------------------------------------------------
// KeyFrameSequence.cs
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
    #region enum

    public enum AnimPlayMode
    {
        /// <summary>
        /// plays once.
        /// </summary>
        Once = 0,

        /// <summary>
        /// keeps repeating.
        /// </summary>
        Repeat,
    }

    #endregion

    #region key frame data

    /// <summary>
    /// key data of animation for one frame
    /// </summary>
    public struct KeyFrame
    {
        public bool HasTranslation;
        public bool HasRotation;
        public bool HasScale;

        public Vector3 Translation;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    #endregion

    /// <summary>
    /// this is the basic unit of animation which has single bone key frame.
    /// It calculates by interpolating two key frames.
    /// </summary>
    [Serializable]
    public class KeyFrameSequence
    {
        #region Fields

        public String BoneName = String.Empty; 

        /// <summary>
        /// Key frame count
        /// </summary>
        public int KeyCount = 0;

        /// <summary>
        /// Animation playing time
        /// </summary>
        public float Duration = 0.0f;

        /// <summary>
        /// Gap of time for the each frame.
        /// </summary>
        public float KeyInterval = 0.0f;

        public bool HasTranslation = false;
        public bool HasRotation = false;
        public bool HasScale = false;
        public bool HasTime = false;

        /// <summary>
        /// If this flag is set to true, the first key frame’s translation value will be
        /// fixed during all key frame of the animation.
        /// </summary>
        public bool FixedTranslation = false;

        /// <summary>
        /// If this flag is set to true, the first key frame’s rotaion value will be 
        /// fixed during all key frame of the animation.
        /// </summary>
        public bool FixedRotation = false;

        /// <summary>
        /// If this flag is set to true, the first key frame’s scale value will be 
        /// fixed during all key frame of the animation.
        /// </summary>
        public bool FixedScale = false;

        /// <summary>
        /// The translation value in the list.
        /// </summary>
        public List<Vector3> Translation = null;

        /// <summary>
        /// The rotation value in the list.
        /// </summary>
        public List<Quaternion> Rotation = null;

        /// <summary>
        /// The scale value in the list.
        /// </summary>
        public List<Vector3> Scale = null;

        /// <summary>
        /// The time value in the list.
        /// </summary>
        public List<float> Time = null;

        #endregion

        /// <summary>
        /// Gets a key frame matrix by index
        /// </summary>
        /// <param name="keyIndex">key frame index</param>
        /// <returns></returns>
        public Matrix GetMatrix(int keyIndex)
        {
            Matrix mat = Matrix.Identity;

            if (HasRotation )    // Calculates rotation matrix using quaternion
            {
                if (FixedRotation )
                    mat = Matrix.CreateFromQuaternion(Rotation[0]);
                else
                    mat = Matrix.CreateFromQuaternion(Rotation[keyIndex]);
            }

            if (HasTranslation )      // Calculates position
            {
                if (FixedRotation )
                    mat.Translation = Translation[0];
                else
                    mat.Translation = Translation[keyIndex];
            }

            if (HasScale )            // Calculates scale
            {
                if (FixedRotation )
                    mat = mat * Matrix.CreateScale(Scale[0]);
                else
                    mat = mat * Matrix.CreateScale(Scale[keyIndex]);
            }

            return mat;
        }

        /// <summary>
        /// calculates by interpolating two key frames.
        /// </summary>
        /// <param name="keyIndex1">key frame index 1</param>
        /// <param name="keyIndex2">key frame index 2</param>
        /// <param name="t">interpolate time (0.0 to 1.0)</param>
        /// <returns>Interpolated key frame matrix</returns>
        public Matrix GetInterpolateMatrix(int keyIndex1, int keyIndex2, float t)
        {
            //  Calculate KeyFrame interpolated matrix
            Matrix mat = Matrix.Identity;

            //  Interpolate rotation value by Slerp
            if (HasRotation )
            {
                Quaternion q = Quaternion.Identity;

                if (FixedRotation )
                    q = Rotation[0];
                else
                    q = Quaternion.Slerp(Rotation[keyIndex1], Rotation[keyIndex2], t);

                // Apply interpolate rotation to matrix
                mat = Matrix.CreateFromQuaternion(q);
            }

            //  Interpolate translation value by Lerp
            if (HasTranslation )
            {
                // Apply interpolate translation to matrix
                if (FixedTranslation )
                    mat.Translation = Translation[0];
                else
                    mat.Translation = 
                        Vector3.Lerp(Translation[keyIndex1], Translation[keyIndex2], t);
            }

            //  Interpolate scale value by Lerp
            if (HasScale )
            {
                Vector3 v = Vector3.Zero;

                if (FixedScale )
                    v = Scale[0];
                else
                    v = Vector3.Lerp(Scale[keyIndex1], Scale[keyIndex2], t);

                // Apply interpolate scale to matrix
                mat = mat * Matrix.CreateScale(v);
            }

            return mat;
        }

        /// <summary>
        /// calculates by interpolating two key frames.
        /// </summary>
        /// <param name="keyIndex1">key frame index 1</param>
        /// <param name="keyIndex2">key frame index 2</param>
        /// <param name="t">interpolate time (0.0 to 1.0)</param>
        /// <returns>Interpolated key frame</returns>
        public KeyFrame GetInterpolateKeyFrame(int keyIndex1, int keyIndex2, float t)
        {
            //  Calculate KeyFrame interpolated matrix
            KeyFrame keyFrame;

            //  Interpolate rotation value by Slerp
            if (HasRotation )
            {
                if (FixedRotation )
                    keyFrame.Rotation = Rotation[0];
                else
                    keyFrame.Rotation = Quaternion.Slerp(
                                                 Rotation[keyIndex1],
                                                 Rotation[keyIndex2],
                                                 t);
            }
            else
            {
                keyFrame.Rotation = Quaternion.Identity;
            }

            keyFrame.HasRotation = HasRotation;

            //  Interpolate translation value by Lerp
            if (HasTranslation )
            {
                if (FixedTranslation )
                    keyFrame.Translation = Translation[0];
                else
                    keyFrame.Translation = Vector3.Lerp(
                                                 Translation[keyIndex1],
                                                 Translation[keyIndex2],
                                                 t);
            }
            else
            {
                keyFrame.Translation = Vector3.Zero;
            }

            keyFrame.HasTranslation = HasTranslation;

            //  Interpolate scale value by Lerp
            if (HasScale )
            {
                if (FixedScale )
                    keyFrame.Scale = Scale[0];
                else
                    keyFrame.Scale = Vector3.Lerp(Scale[keyIndex1], Scale[keyIndex2], t);
            }
            else
            {
                keyFrame.Scale = Vector3.One;
            }

            keyFrame.HasScale = HasScale;

            return keyFrame;
        }

        public Matrix GetInterpolateMatrix(float localTime, AnimPlayMode mode)
        {
            int index1 = 0;       // first key frame index
            int index2 = 0;       // second key frame index
            float interpolateTime = 0.0f;

            CalculateKeyFrameIndex(localTime, mode, out index1, out index2, 
                                   out interpolateTime);

            //  Calcurate interpolate key frame matrix between KeyFrame1 and KeyFrame2
            return GetInterpolateMatrix(index1, index2, interpolateTime);
        }

        public KeyFrame GetInterpolateKeyFrame(float localTime, AnimPlayMode mode)
        {
            int index1 = 0;       // first key frame index
            int index2 = 0;       // second key frame index
            float interpolateTime = 0.0f;

            CalculateKeyFrameIndex(localTime, mode, out index1, out index2, 
                                   out interpolateTime);

            //  Calcurate interpolate key frame matrix between KeyFrame1 and KeyFrame2
            return GetInterpolateKeyFrame(index1, index2, interpolateTime);
        }

        /// <summary>
        /// returns two key frame index, which is included in the specified time.
        /// </summary>
        public void CalculateKeyFrameIndex(float localTime, AnimPlayMode mode, 
                                           out int index1, out int index2, 
                                           out float interpolateTime)
        {
            index1 = 0;       // first key frame index
            index2 = 0;       // second key frame index
            interpolateTime = 0.0f;

            //  Calculate first key frame index
            if (HasTime )
                index1 = GetKeyFrameIndex(localTime);
            else
                index1 = (int)(localTime / KeyInterval);

            //  Calculate second key frame index by play mode
            switch (mode)
            {
                case AnimPlayMode.Once:     //  Just play once
                    {
                        //  if index1 is last index
                        index2 = (index1 >= KeyCount - 1 ? index1 : index1 + 1);
                    }
                    break;
                case AnimPlayMode.Repeat:   //  Play looping
                    {
                        //  if index1 is last index, index2 must be begin (looping)
                        index2 = (index1 >= KeyCount - 1 ? 0 : index1 + 1);
                    }
                    break;
                default:
                    throw new NotSupportedException("Not supported play mode");
            }

            if (index1 >= KeyCount - 1)
            {
                index1 = index2 = KeyCount - 1;

                interpolateTime = 1.0f;
            }
            else
            {
                if (HasTime )
                {
                    interpolateTime = (localTime - Time[index1]) / 
                                      (Time[index2] - Time[index1]);
                }
                else
                {
                    interpolateTime = HelperMath.CalculateModulo(localTime, KeyInterval) 
                                        / KeyInterval;
                }
            }
        }

        /// <summary>
        /// returns two key frame index, which is included in the specified time.
        /// </summary>
        public int GetKeyFrameIndex(float localTime)
        {
            //  Calculate index between two key frame on this time
            int startIndex, endIndex, middleIndex;

            startIndex = 0;
            endIndex = KeyCount - 1;

            if (localTime >= Time[endIndex])
            {
                return endIndex;
            }

            do
            {
                middleIndex = (startIndex + endIndex) / 2;

                if ((endIndex - startIndex) <= 1)
                {
                    break;
                }
                else if (Time[middleIndex] < localTime)
                {
                    startIndex = middleIndex;
                }
                else if (Time[middleIndex] > localTime)
                {
                    endIndex = middleIndex;
                }
                else
                {
                    startIndex = middleIndex;
                    break;
                }
            } while ((endIndex - startIndex) > 1);

            return startIndex;
        }
    }
}
