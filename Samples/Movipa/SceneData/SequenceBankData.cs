#region File Description
//-----------------------------------------------------------------------------
// SequenceBankData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SceneDataLibrary
{
    /// <summary>
    /// This class manages multiple sequence groups.
    /// In Layout, sequence banks correspond to these sequence groups.
    /// The display priority can be specified by the sequence bank property of Layout.
    ///
    /// 複数のシーケンスグループを保持するクラスです。
    /// Layoutでは、シーケンスバンクが相当します。
    /// 表示プライオリティーは、Layoutのシーケンスバンクプロパティーで
    /// 設定できます。
    /// </summary>
    public class SequenceBankData
    {
        #region Fields

        private int zPosition;//Display priority //表示プライオリティー

        //Sequence group data 
        //シーケンスグループデータ
        private List<SequenceGroupData> sequenceGroupList = 
            new List<SequenceGroupData>();

        #endregion

        #region Properties
        /// <summary>
        /// Obtains and sets the display order.
        ///
        /// 表示順位を設定取得します。
        /// </summary>
        public int ZPos
        {
            get
            {
                return zPosition;
            }
            set
            {
                zPosition = value;
            }
        }


        /// <summary>
        /// Obtains the list of sequence groups.
        ///
        /// シーケンスグループののリストを設定取得します。
        /// </summary>
        public List<SequenceGroupData> SequenceGroupList
        {
            get { return sequenceGroupList; }
        }

        /// <summary>
        /// Obtains whether the held sequences are being played or not.
        /// If they are not being played, returns false.
        ///
        /// 保持しているシーケンス群が再生中かどうかを取得します。
        /// 停止している場合はfalseです。
        /// </summary>
        public bool IsPlay
        {
            get
            {
                bool result = false;
                foreach (SequenceGroupData group in SequenceGroupList)
                {
                    if (!group.IsStop)
                    {
                        result = true;
                        break;
                    }
                }

                return result;
            }
        }
        #endregion

        /// <summary>
        /// Sets the held sequence time forward.
        ///
        /// 保持しているシーケンスの時間を進めます。
        /// </summary>
        /// <param name="playFrames">
        /// Frame of each current sequence
        /// 
        /// 現在の各シーケンスのフレーム
        /// </param>
        /// <param name="elapsedGameTime">
        /// Time to be forwarded
        /// 
        /// 進める時間
        /// </param>
        /// <param name="bReverse">
        /// Specifies true in case of reverse play
        ///
        /// 逆再生の場合true
        /// </param>
        public void Update(float[] playFrames, TimeSpan elapsedGameTime, bool bReverse)
        {
            int nIndex = 0;

            foreach(SequenceGroupData group in SequenceGroupList)
            {
                playFrames[nIndex] = group.Update(   playFrames[nIndex],
                                                        elapsedGameTime, 
                                                        bReverse);

                nIndex++;
            }
        }

        /// <summary>
        /// Draws the held sequence group.
        /// Conversion settings can be applied to the entire sequence by specifying 
        /// values to baseDrawData.
        /// 
        /// 保持しているシーケンスグループを描画します。
        /// baseDrawDataに値を設定することで、シーケンス全体に変換を適用することが
        /// 出来ます。
        /// </summary>
        /// <param name="sb">
        /// SpriteBatch
        /// 
        /// スプライトバッチ
        /// </param>
        /// <param name="baseDrawData">
        /// Conversion information that affects the entire drawing target
        ///
        /// 描画対象全体影響する変換用情報
        /// </param>
        public void Draw(SpriteBatch sb, DrawData baseDrawData)
        {
            foreach (SequenceGroupData group in SequenceGroupList)
            {
                group.Draw(sb, baseDrawData);
            }
        }

        /// <summary>
        /// Obtains the conversion information for the patterns belonging 
        /// to the held sequence.  Any display operation related to animations 
        /// can be performed by using this data.
        ///
        /// 保持しているシーケンスに所属するパターンの変換情報を取得します。
        /// このデータを用いてアニメーションに追随した任意の表示を行うことができます。
        /// </summary>
        /// <param name="sequenceGroupId">
        /// Sequence group ID
        /// 
        /// シーケンスグループのID
        /// </param>
        /// <param name="sequenceObjectId">
        /// Sequence object ID
        /// 
        /// シーケンスオブジェクトのID
        /// </param>
        /// <param name="patternObjectId">
        /// Pattern object ID
        /// 
        /// パターンオブジェクトのID
        /// </param>
        /// <returns></returns>
        public DrawData GetDrawPatternObjectDrawData( int sequenceGroupId,
                                                int sequenceObjectId,
                                                int patternObjectId)
        {
            return SequenceGroupList[sequenceGroupId].
                    SequenceObjectList[sequenceObjectId].
                    PatternObjectList[patternObjectId].InterpolationDrawData;
        }

        /// <summary>
        /// Obtains the conversion information for the patterns belonging 
        /// to the sequence object that is being drawn in the held sequence.
        /// Any display operation related to animations can be performed
        /// by using this data.
        ///
        /// 保持しているシーケンスの現在描画中のシーケンスオブジェクトに所属する
        /// パターンの変換情報を取得します。
        /// このデータを用いてアニメーションに追随した任意の表示を行うことができます。
        /// </summary>
        /// <param name="sequenceGroupId">
        /// Sequence group ID
        /// 
        /// シーケンスグループのID
        /// </param>
        /// <param name="patternObjectId">
        /// Pattern object ID
        /// 
        /// パターンオブジェクトのID
        /// </param>
        /// <returns></returns>
        public DrawData GetDrawPatternObjectDrawData(int sequenceGroupId,
                                                    int patternObjectId)
        {
            return SequenceGroupList[sequenceGroupId].CurrentObjectList.
                PatternObjectList[patternObjectId].InterpolationDrawData;
        }



    }
}
