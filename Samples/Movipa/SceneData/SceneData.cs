#region File Description
//-----------------------------------------------------------------------------
// SceneData.cs
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SceneDataLibrary
{
    /// <summary>
    /// This class manages scene data (pattern and sequence).
    /// In Layout, stage data corresponds to this scene data.
    ///
    /// シーンデータ（パターン、シーケンス）を保持します。
    /// Layoutではステージデータに相当します。
    /// </summary>
    public class SceneData
    {
        #region Fields
        //List of pattern groups
        //
        //パターングループのリスト
        private Dictionary<String, PatternGroupData> patternGroupDictionary = 
            new Dictionary<string,PatternGroupData>();
        //List of sequence banks
        //
        //シーケンスバンクのリスト
        private Dictionary<String, SequenceBankData> sequenceBankDictionary = 
            new Dictionary<string,SequenceBankData>();
        //List of sequence play data
        //
        //シーケンス再生データのリスト
        private List<SequencePlayData> sequencePlayList = new List<SequencePlayData>();
        #endregion

        #region Propaties

        /// <summary>
        /// Obtains the dictionary list for pattern groups.
        ///
        /// パターングループデータの辞書リストを設定取得します。
        /// </summary>
        public Dictionary<String, PatternGroupData> PatternGroupDictionary
        {
            get { return patternGroupDictionary; }
        }

        /// <summary>
        /// Obtains the dictionary list for sequence bank data.
        ///
        /// シーケンスバンクデータの辞書リストを設定取得します。
        /// </summary>
        public Dictionary<String, SequenceBankData> SequenceBankDictionary
        {
            get { return sequenceBankDictionary; }
        }
        #endregion

        /// <summary>
        /// Creates data to play the sequence.
        /// When specifying the target sequence, uses the sequence bank name.
        /// 
        /// シーケンスを再生するためのデータを作成します。
        /// 対象は、シーケンスバンク名で名前で指定します。
        /// </summary>
        /// <param name="name">
        /// Sequence name
        /// 
        /// シーケンスの名前
        /// </param>
        /// <returns></returns>
        public SequencePlayData CreatePlaySeqData(String name)
        {
            return new SequencePlayData(sequenceBankDictionary[name]);
        }

        /// <summary>
        /// Adds sequences to be played.
        /// When adding them to the list, the display priority specified in 
        /// the layout tool must be considered.
        /// 
        /// 再生するシーケンスを追加します。
        /// レイアウトツールで指定された表示優先順位を考慮して
        /// リストに追加します。
        /// </summary>
        /// <param name="data">
        /// Sequence data to be played
        /// 
        /// 再生するシーケンスのデータ
        /// </param>
        public void AddPlaySeqData(SequencePlayData data)
        {
            int nInsertIndex = 0;

            for(int i = sequencePlayList.Count - 1; i >= 0; i--)
            {
                int nZPos = sequencePlayList[i].SequenceData.ZPos;

                if (nZPos <= data.SequenceData.ZPos)
                {
                    nInsertIndex = i + 1;

                    break;
                }
            }

            sequencePlayList.Insert(nInsertIndex, data);
        }

        /// <summary>
        /// Adds the sequences specified by their names to the play list.
        /// Creation of sequence play data and addition of it to the list 
        /// can be performed at the same time.
        ///
        /// 名前で指定したシーケンスを再生リストに追加します。
        /// シーケンス再生データの作成と、リストへの追加を同時に行えます。
        /// </summary>
        /// <param name="name">
        /// Sequence name to be added
        ///
        /// 追加するシーケンス名
        /// </param>
        public void AddPlaySeqData(String name)
        {
            AddPlaySeqData(CreatePlaySeqData(name));
        }

        /// <summary>
        /// Sets the sequence time in the play list forward.
        /// Updates the scene data.
        ///
        /// 再生リストにあるシーケンスの時間を進めます。
        /// シーンデータの更新関数です。
        /// </summary>
        /// <param name="elapsedGameTime">
        /// Time to be forwarded
        /// 
        /// 進める時間
        /// </param>
        public void Update(TimeSpan elapsedGameTime)
        {
            foreach (SequencePlayData data in sequencePlayList)
                data.Update(elapsedGameTime);
        }

        /// <summary>
        /// Draws the sequence in the play list.
        ///
        /// 再生リストにあるシーケンスを描画します。
        /// </summary>
        /// <param name="sb">
        /// SpriteBatch
        /// 
        /// スプラインバッチ
        /// </param>
        /// <param name="baseDrawData">
        /// Conversion information that affects the entire drawing target
        /// 
        /// 描画対象すべてに影響する変換情報
        /// </param>
        public void Draw(SpriteBatch sb, DrawData baseDrawData)
        {
            foreach (SequencePlayData data in sequencePlayList)
                data.Draw(sb, baseDrawData);
        }

        /// <summary>
        /// Displays the pattern group specified by its name.
        ///
        /// 名前で指定したパターングループを表示します。
        /// </summary>
        /// <param name="sb">
        /// SpriteBatch
        /// 
        /// スプラインバッチ
        /// </param>
        /// <param name="name">
        /// Pattern group name
        /// 
        /// パターングループの名前
        /// </param>
        /// <param name="baseDrawData">
        /// Conversion information that affects the entire drawing target
        /// 
        /// 描画対象すべてに影響する変換情報
        /// </param>
        public void DrawPattern(SpriteBatch sb, String name, DrawData baseDrawData)
        {
            PatternGroupData group = PatternGroupDictionary[name];

            foreach (PatternObjectData pattern in group.PatternObjectList)
                pattern.Draw(sb, pattern.Data, baseDrawData);
        }

        /// <summary>
        /// Specifies a pattern group by its name and specifies a pattern object
        /// in this pattern group by its index to obtain the position information.
        ///
        /// パターングループを名前で指定し、
        /// その内部にあるパターンオブジェクトをインデックスで指定。
        /// 位置情報を取得します。
        /// </summary>
        /// <param name="name">
        /// Pattern group name
        /// 
        /// パターングループの名前
        /// </param>
        /// <param name="nObjectId">
        /// Pattern object ID
        /// 
        /// パターンオブジェクトのID
        /// </param>
        /// <returns></returns>
        public Point GetPatternPosition(String name, int nObjectId)
        {
            return PatternGroupDictionary[name].PatternObjectList[nObjectId].
                Data.Position;
        }
    }
}
