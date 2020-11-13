#region File Description
//-----------------------------------------------------------------------------
// RenderingInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class manages animation information of a rendering movie.
    /// This information includes the total number of frames and textures, 
    /// the size of the frame drawn in one texture, and the replay rate information. 
    /// With these pieces of information, animations can be drawn in sequence textures.
    ///
    /// レンダリングムービーのアニメーション情報を持ちます。
    /// 連番テクスチャで描画が出来るように、総フレーム数と、総テクスチャ数、
    /// 1枚のテクスチャに描かれているフレームのサイズと、再生レートの情報があります。
    /// </summary>
    public class RenderingInfo : AnimationInfo
    {
        #region Fields
        private string format;
        private uint totalTexture;
        private uint totalFrame;
        private Point imageSize;
        private uint frameRate;
        #endregion

        #region Property
        /// <summary>
        /// Obtains or sets the asset name format.
        ///
        /// アセット名のフォーマットを取得または設定します。
        /// </summary>
        public string Format
        {
            get { return format; }
            set { format = value; }
        }


        /// <summary>
        /// Obtains or sets the total number of textures.
        ///
        /// 総テクスチャ数を取得または設定します。
        /// </summary>
        public UInt32 TotalTexture
        {
            get { return totalTexture; }
            set { totalTexture = value; }
        }


        /// <summary>
        /// Obtains or sets the total number of frames.
        ///
        /// 総フレーム数を取得または設定します。
        /// </summary>
        public UInt32 TotalFrame
        {
            get { return totalFrame; }
            set { totalFrame = value; }
        }


        /// <summary>
        /// Obtains or sets the image size of the frame.
        ///
        /// フレームの画像サイズを取得または設定します。
        /// </summary>
        public Point ImageSize
        {
            get { return imageSize; }
            set { imageSize = value; }
        }


        /// <summary>
        /// Obtains or sets the frame rate.
        ///
        /// フレームレートを取得または設定します。
        /// </summary>
        public uint FrameRate
        {
            get { return frameRate; }
            set { frameRate = value; }
        }

        #endregion
    }
}
