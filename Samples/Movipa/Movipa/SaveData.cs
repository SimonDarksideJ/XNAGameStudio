#region File Description
//-----------------------------------------------------------------------------
// SaveData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
#endregion

namespace Movipa
{
    /// <summary>
    /// Manages the values used by Save Data.
    /// Serialized and deserialized in XML, except for the TimeSpan type,
    /// which is serialized and deserialized via text strings by returning the value
    /// in TimeSpan.Parse.
    /// 
    /// セーブデータで使用する値を管理します。
    /// XMLでシリアライズ、デシリアライズを行いますが、TimeSpan型のみ文字列を介して
    /// シリアライズ、デシリアライズ時は文字列をTimeSpan.Parseで値を戻しています。
    /// </summary>
    public class SaveData
    {
        #region Fields
        // Number of stages
        // 
        // ステージ数
        private int stage;

        // Play count
        // 
        // プレイ回数
        private int playCount;

        // Score
        //
        //スコア
        private long score;

        // Best score
        //
        // ベストスコア
        private long bestScore;

        // Best clear time
        //
        // ベストクリアタイム
        private TimeSpan bestTime;

        // Total play time
        // 
        // 総プレイ時間
        private TimeSpan totalPlayTime;

        // File name
        // The original file name must be used when overwriting the save. 
        //
        // ファイル名
        // セーブを上書きする時に元のファイル名を持つ必要があります。
        private string fileName;
        #endregion

        #region Property
        /// <summary>
        /// Obtains or sets the number of stages.
        /// 
        /// ステージ数を取得または設定します。
        /// </summary>
        public int Stage
        {
            get { return stage; }
            set { stage = value; }
        }

        /// <summary>
        /// Obtains or sets the play count.
        /// 
        /// プレイ回数を取得または設定します。
        /// </summary>
        public int PlayCount
        {
            get { return playCount; }
            set { playCount = value; }
        }

        /// <summary>
        /// Obtains or sets the score.
        ///
        /// スコアを取得または設定します。
        /// </summary>
        public long Score
        {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// Obtains or sets the best score.
        /// 
        /// ベストスコアを取得または設定します。
        /// </summary>
        public long BestScore
        {
            get { return bestScore; }
            set { bestScore = value; }
        }

        /// <summary>
        /// Obtains or sets the best time. 
        /// 
        /// ベストタイムを取得または設定します。
        /// </summary>
        [XmlIgnoreAttribute()]
        public TimeSpan BestTime
        {
            get { return bestTime; }
            set { bestTime = value; }
        }

        /// <summary>
        /// Obtains or sets the best time as a text string. 
        /// 
        /// ベストタイムを文字列で取得または設定します。
        /// </summary>
        public string BestTimeString
        {
            get
            {
                return bestTime.ToString();
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
                    bestTime = result;
                }
            }
        }


        /// <summary>
        /// Obtains or sets the total play time.
        /// 
        /// 総プレイタイムを取得または設定します。
        /// </summary>
        [XmlIgnoreAttribute()]
        public TimeSpan TotalPlayTime
        {
            get { return totalPlayTime; }
            set { totalPlayTime = value; }
        }


        /// <summary>
        /// Obtains or sets the total play time as a text string.
        /// 
        /// 総プレイタイムを文字列型で取得または設定します。
        /// </summary>
        public string TotalPlayTimeString
        {
            get
            {
                return totalPlayTime.ToString();
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
                    totalPlayTime = result;
                }
            }
        }


        /// <summary>
        /// Obtains or sets the file name.
        /// 
        /// ファイル名を取得または設定します。
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public SaveData()
        {
            stage = 0;
            playCount = 0;
            score = 0;
            bestScore = 0;
            bestTime = new TimeSpan();
            totalPlayTime = new TimeSpan();
            fileName = String.Empty;
        }
        #endregion
    }
}
