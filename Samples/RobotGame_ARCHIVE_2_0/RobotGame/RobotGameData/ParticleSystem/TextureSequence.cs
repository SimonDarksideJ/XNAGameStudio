#region File Description
//-----------------------------------------------------------------------------
// TextureSequence.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Helper;
using RobotGameData.Resource;
#endregion

namespace RobotGameData.ParticleSystem
{
    /// <summary>
    /// a structure for the animation of consecutive textures.
    /// </summary>
    [Serializable]
    public class TextureSequence
    {
        #region Style

        [Flags]
        public enum Styles
        {
            None =             0x00000000,
            Repeat =             0x00000001,
            StaticTime =       0x00000002,
            Random =             0x00000004,
            FixedFrame =       0x00000008,
        }

        #endregion

        #region Fields

        // Persistent Members        
        public string TextureFileName = String.Empty;
        public bool IsUseStaticTime = true;
        public bool IsRepeat = false;
        public bool IsRandomMode = false;
        public bool IsFixedFrameMode = false;
        public float FrameWidth = 0.0f;     //  Width rate (maximum is 1.0)
        public float FrameHeight = 0.0f;    //  Height rate (maximum is 1.0)
        public float StaticInterval = 0.016666666666666666666666666666667f;    // 60 FPS
        public uint Count = 0;
        public uint StartIndex = 0;
        public List<float> TimeTable = null;

        // Volatile Members        
        uint style = 0;
        int lineCount = 0;
        float frameWidthPixel = 0.0f;
        float frameHeightPixel = 0.0f;
        float rInterval = 0.0f;
        float widthFactor = 0.0f;
        float heightFactor = 0.0f;
        float duration = 0.0f;
        Texture2D texture = null;        

        #endregion

        #region Properties

        public Texture2D Texture
        {
            get { return texture; }
        }

        public bool UseStaticTime
        {
            get { return IsUseStaticTime; }
            set
            {
                IsUseStaticTime = value;

                if (value)
                {
                    style |= (uint)Styles.StaticTime;

                    if (TimeTable == null)
                        TimeTable = new List<float>();
                }
                else
                {
                    style &= ~(uint)Styles.StaticTime;

                    if (TimeTable != null)
                    {
                        TimeTable.Clear();
                        TimeTable = null;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Loads a texture.
        /// </summary>
        /// <param name="resourcePath">texture file name</param>
        public void LoadTexture(string resourcePath)
        {
            string resourceFullPath = Path.Combine(resourcePath, TextureFileName);

            GameResourceTexture2D resource = 
                            FrameworkCore.ResourceManager.LoadTexture(resourceFullPath);

            //  Set to texture
            SetTexture( resource.Texture2D);
        }

        /// <summary>
        /// Set the texture.
        /// </summary>
        /// <param name="tex">texture</param>
        public void SetTexture(Texture2D tex)
        {
            texture = tex;

            CalcParams();
        }

        /// <summary>
        /// Initialize parameters.
        /// </summary>
        public void CalcParams()
        {
            if (texture == null) 
                return;

            if (texture.Width == 0 || texture.Height == 0)
                return;

            if (StaticInterval == 0.0f)
                return;

            duration = StaticInterval * Count;
            rInterval = 1.0f / StaticInterval;

            frameWidthPixel = (float)texture.Width * FrameWidth;
            frameHeightPixel = (float)texture.Height * FrameHeight;

            lineCount = (int)((float)texture.Width / frameWidthPixel);

            widthFactor = frameWidthPixel / (float)texture.Width;
            heightFactor = frameHeightPixel / (float)texture.Height;

            bool val = IsUseStaticTime;
            UseStaticTime = val;
        }

        /// <summary>
        /// gets texture coordinates by the time flow.
        /// </summary>
        /// <param name="time">defined time</param>
        /// <param name="uv1">texture coordinates 1</param>
        /// <param name="uv2">texture coordinates 2</param>
        public void GetUV(float time, out Vector2 uv1, out Vector2 uv2)
        {
            uint x = 0, y = 0, idx = 0;
            float u = 0.0f, v = 0.0f;

            if (IsRepeat)
                time = HelperMath.CalculateModulo(time, duration);

            if (IsRandomMode)
                idx = (uint)HelperMath.Randomi() % Count;
            else
            {
                if (IsUseStaticTime)
                {
                    //  Calculate index by static interval time
                    idx = (uint)(time * rInterval);

                    if (idx >= Count)
                        idx = Count - 1;
                }
                else
                {
                    uint start = 0;
                    uint end = Count - 1;

                    //  Calculate index by dynamic interval time
                    do
                    {
                        idx = (start + end) / 2;

                        if (TimeTable[(int)idx] > time)
                            end = idx - 1;
                        else
                            start = idx + 1;
                    } while (start < end);
                }
            }

            idx += StartIndex;

            y = idx / (uint)lineCount;
            v = y * heightFactor;

            x = idx % (uint)lineCount;
            u = x * widthFactor;

            uv1 = new Vector2(u, v);
            uv2 = new Vector2(u + widthFactor, v + heightFactor);
        }
    }
}
