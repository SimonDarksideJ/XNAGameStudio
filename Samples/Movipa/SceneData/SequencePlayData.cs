#region File Description
//-----------------------------------------------------------------------------
// SequencePlayData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SceneDataLibrary
{
    /// <summary>
    /// This class manages the sequence play status.
    /// This status is managed for each sequence bank in Layout.
    /// This class contains display frame data assigned to each 
    /// sequence group in a sequence bank.
    /// This data value will be the same in all sequence groups 
    /// unless special operations are performed.
    ///
    /// シーケンスの再生状況を管理します。
    /// 管理するのはLayout上でのシーケンスバンク単位です。
    /// 保持するデータは、シーケンスバンク内の各シーケンスグループに割り当てられる
    /// 表示フレームです。特別操作しない限り、すべてのシーケンスグループで
    /// このデータは同じ値になります。
    /// </summary>
    public class SequencePlayData
    {
        #region Fields

        //Sequence bank to be displayed 
        //表示するシーケンスバンク
        private SequenceBankData sequenceData;

        //Reverse play flag 
        //逆再生フラグ
        private bool reverse;

        //Current frame of each sequence group contained 
        // in the sequence bank to be displayed
        //
        //表示するシーケンスバンクに含まれる各シーケンスグループの現在のフレーム
        private float[] playFrames;

        #endregion

        #region Properties

        /// <summary>
        /// Obtains the sequence bank to be displayed.
        ///
        /// 表示するシーケンスバンクを取得します。
        /// </summary>
        public SequenceBankData SequenceData
        {
            get
            {
                return sequenceData;
            }
        }

        /// <summary>
        /// If the sequence data is being played, returns true.
        ///
        /// 再生中の場合はtrue
        /// </summary>
        public bool IsPlay
        {
            get { return sequenceData.IsPlay; }
        }

        #endregion

        /// <summary>
        /// Sets the sequence time forward.
        ///
        /// シーケンスの時間を進めます。
        /// </summary>
        /// <param name="elapsedGameTime">
        /// Time to be forwarded
        /// 
        /// 進める時間
        /// </param>
        public void Update(TimeSpan elapsedGameTime)
        {
            sequenceData.Update(playFrames, elapsedGameTime, reverse);
        }

        /// <summary>
        /// Draws the sequence data.
        ///
        /// 描画します。
        /// </summary>
        /// <param name="sb">
        /// SpriteBatch
        /// 
        /// スプライトバッチ
        /// </param>
        /// <param name="data">
        /// Conversion information for drawing
        /// 
        /// 描画変換情報
        /// </param>
        public void Draw(SpriteBatch sb, DrawData data)
        {
            sequenceData.Draw(sb, data);
        }

        /// <summary>
        /// Constructor
        /// Sets the sequence bank to be played and resets the time and play direction.
        ///
        /// コンストラクタ
        /// 再生するシーケンスバンクを設定し、時刻と再生方向をリセットします。
        /// </summary>
        /// <param name="bank">
        /// Sets the sequence bank to be drawn
        /// 
        /// 描画するシーケンスバンクを設定します
        /// </param>
        public SequencePlayData(SequenceBankData bank)
        {
            sequenceData = bank;
            reverse = false;
            playFrames = new float[bank.SequenceGroupList.Count];
        }

        /// <summary>
        /// Resets the play time and plays the sequence data from the beginning.
        ///
        /// 再生時刻をリセットして最初から再生します。
        /// </summary>
        public void Replay()
        {
            foreach (SequenceGroupData group in sequenceData.SequenceGroupList)
            {
                group.Replay();
            }

            Update(new TimeSpan());
        }
    }
}
