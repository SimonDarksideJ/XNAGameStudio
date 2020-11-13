#region File Description
//-----------------------------------------------------------------------------
// StageResult.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Movipa
{
    /// <summary>
    /// Contains the stage play results information.
    /// Retains the movement count together with score and clear time information.
    /// 
    /// ステージのプレイ結果の情報を持ちます。
    /// 移動回数とスコア、クリアタイムの情報を保持します。
    /// </summary>
    public class StageResult
    {
        #region Fields
        // Movement count
        // 
        // 移動回数
        private long moveCount;

        // Single completed score
        // 
        // シングル揃いスコア
        private long singleScore;

        // Double completed score
        // 
        // ダブル揃いスコア
        private long doubleScore;

        // Hint score
        // 
        // ヒントのスコア
        private long hintScore;
        
        // Clear time
        // 
        // クリア時間
        private TimeSpan clearTime;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the movement count.
        /// 
        /// 移動回数を取得または設定します。
        /// </summary>
        public long MoveCount
        {
            get { return moveCount; }
            set { moveCount = value; }
        }


        /// <summary>
        /// Obtains or sets the single score.
        /// 
        /// シングルスコアを取得または設定します。
        /// </summary>
        public long SingleScore
        {
            get { return singleScore; }
            set { singleScore = value; }
        }


        /// <summary>
        /// Obtains or sets the double score.
        /// 
        /// ダブルスコアを取得または設定します。
        /// </summary>
        public long DoubleScore
        {
            get { return doubleScore; }
            set { doubleScore = value; }
        }


        /// <summary>
        /// Obtains or sets the hint score.
        /// 
        /// ヒントスコアを取得または設定します。
        /// </summary>
        public long HintScore
        {
            get { return hintScore; }
            set { hintScore = value; }
        }


        /// <summary>
        /// Obtains or sets the clear time.
        /// 
        /// クリアタイムを取得または設定します。
        /// </summary>
        public TimeSpan ClearTime
        {
            get { return clearTime; }
            set { clearTime = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public StageResult()
        {
            moveCount = 0;
            singleScore = 0;
            doubleScore = 0;
            hintScore = 0;
            clearTime = TimeSpan.Zero;
        }
        #endregion
    }
}
