#region File Description
//-----------------------------------------------------------------------------
// SequenceObjectData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
#endregion

namespace SceneDataLibrary
{
    /// <summary>
    /// Sequence object data is stored in a sequence group and refers
    /// to a pattern group.  A sequence group displays pattern groups 
    /// referred to by sequence objects in sequence.
    /// In this case, if interpolation is needed, necessary calculations 
    /// are performed in advance.  Otherwise, pictures will be only 
    /// sequentially switched like a simple animation.
    /// In Layout, sequence objects correspond to this sequence object data.
    ///
    /// シーケンスグループ内に保持され、パターングループを参照します。
    /// シーケンスグループは順次シーケンスオブジェクトが参照している
    /// パターングループを表示していきます。
    /// その際、補間が必要であればあらかじめ計算が行われます。
    /// 補完しない場合は、パタパタアニメのように
    /// 絵が順次切り替わるだけです。
    /// Layoutではシーケンスオブジェクトが相当します。
    /// </summary>
    public class SequenceObjectData
    {
        #region Fields

        ///Frame to be displayed 
        ///表示するフレーム
        private int frame = 0;

        //Pattern group name to be displayed 
        //表示するパターングループの名前
        private String patternGroupName = null;

        //Pattern group to be displayed 
        //表示するパターングループ
        private PatternGroupData patternGroup = null;

        #endregion

        #region Properties
        /// <summary>
        /// Obtains and sets the frame to be displayed.
        ///
        /// 表示するフレームを設定取得します。
        /// </summary>
        public int Frame
        {
            get
            {
                return frame;
            }
            set
            {
                frame = value;
            }
        }

        /// <summary>
        /// Obtains and sets the name of the pattern group to be displayed.
        ///
        /// 表示するパターングループ名を設定取得します。
        /// </summary>
        public String PatternGroupName
        {
            get
            {
                return patternGroupName;
            }
            set
            {
                patternGroupName = value;
            }
        }

        /// <summary>
        /// Obtains the pattern object list in the pattern group to be displayed.
        ///
        /// 表示するパターングループ内のパターンオブジェクトリストを取得します。
        /// </summary>
        [ContentSerializerIgnore()]
        public List<PatternObjectData> PatternObjectList
        {
            get
            {
                return patternGroup.PatternObjectList;
            }
        }
        #endregion


        /// <summary>
        /// Performs initialization.
        /// Obtains the pattern group by using the specified name.
        ///
        /// 初期化します。
        /// 設定されている名前から、パターングループを取得します。
        /// </summary>
        /// <param name="list">
        /// Collection of pattern groups
        /// 
        /// パターングループのコレクション
        /// </param>
        public void Init(Dictionary<String, PatternGroupData> list)
        {
            if (list.ContainsKey(PatternGroupName))
                patternGroup = list[PatternGroupName];
        }
    }
}
