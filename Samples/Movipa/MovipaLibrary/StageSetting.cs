#region File Description
//-----------------------------------------------------------------------------
// StageSetting.cs
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

namespace MovipaLibrary
{
    /// <summary>
    /// This class manages setting information of stages.
    /// ContentTypeReader and ContentTypeWriter are provided in this class
    /// so that stage construction information can be specified by ContentPipeline.
    ///
    /// ステージの設定情報を管理します。
    /// このクラスは、ステージ構成をContentPipelineを通して設定出来るように
    /// ContentTypeReaderとContentTypeWriterを用意しています。
    /// </summary>
    public class StageSetting
    {
        #region Public Types
        /// <summary>
        /// Game mode
        ///
        /// ゲームモード
        /// </summary>
        public enum ModeList
        {
            Normal,
            Free,
        }

        /// <summary>
        /// Panel switch mode
        ///
        /// パネルの入れ替えるモード
        /// </summary>
        public enum StyleList
        {
            Change,
            Revolve,
            Slide,
        }

        /// <summary>
        /// Rotation
        ///
        /// 回転
        /// </summary>
        public enum RotateMode
        {
            On,
            Off,
        }
        #endregion

        #region Fields
        private ModeList mode;
        private StyleList style;
        private RotateMode rotate;
        private string movie;
        private Point divide;
        private TimeSpan timeLimit;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the game mode.
        ///
        /// ゲームモードを取得または設定します。
        /// </summary>
        public ModeList Mode
        {
            get { return mode; }
            set { mode = value; }
        }


        /// <summary>
        /// Obtains or sets the game style.
        ///
        /// ゲームスタイルを取得または設定します。
        /// </summary>
        public StyleList Style
        {
            get { return style; }
            set { style = value; }
        }


        /// <summary>
        /// Obtains or sets the rotation information (rotation is enabled or not).
        ///
        /// 回転の有無を取得または設定します。
        /// </summary>
        public RotateMode Rotate
        {
            get { return rotate; }
            set { rotate = value; }
        }


        /// <summary>
        /// Obtains or sets the asset name for the movie information.
        ///
        /// ムービー情報へのアセット名を取得または設定します。
        /// </summary>
        public string Movie
        {
            get { return movie; }
            set { movie = value; }
        }


        /// <summary>
        /// Obtains or sets the number of divisions.
        ///
        /// 分割数を取得または設定します。
        /// </summary>
        public Point Divide
        {
            get { return divide; }
            set { divide = value; }
        }


        /// <summary>
        /// Obtains or sets the time limit of the stage.
        ///
        /// ステージの制限時間を取得または設定します。
        /// </summary>
        [ContentSerializerIgnore]
        public TimeSpan TimeLimit
        {
            get { return timeLimit; }
        }

        /// <summary>
        /// Obtains or sets the time limit of the stage as a character string.
        ///
        /// ステージの制限時間を文字列型で取得または設定します。
        /// </summary>
        public string TimeLimitString
        {
            get
            {
                return timeLimit.ToString();
            }
            set
            {
                TimeSpan result = new TimeSpan();
                try
                {
                    result = TimeSpan.Parse(value);
                }
                finally
                {
                    timeLimit = result;
                }
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public StageSetting()
        {
            mode = ModeList.Normal;
            style = StyleList.Change;
            rotate = RotateMode.Off;
            movie = String.Empty;
            divide = new Point(3, 3);
            timeLimit = new TimeSpan();
        }
        #endregion
    }

}
