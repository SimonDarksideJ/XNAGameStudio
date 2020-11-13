#region File Description
//-----------------------------------------------------------------------------
// PatternGroupData.cs
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
    /// This class manages the data of multiple pattern objects.
    /// In Layout, pattern groups correspond to these pattern objects.
    /// Sequence objects refer to these pattern groups and display them.
    ///
    /// 複数のパターンオブジェクトデータをまとめて保持するクラスです。
    /// Layoutではパターングループが相当します。
    /// シーケンスオブジェクトは、このパターングループを
    /// 参照・表示します。
    /// </summary>
    public class PatternGroupData
    {
        private List<PatternObjectData> patternObjectList = 
            new List<PatternObjectData>();

        #region Properties
        /// <summary>
        /// Obtains and sets the list of pattern data.
        /// This list can be also used to display a certain pattern.
        /// For details of pattern display, refer to the PatternObjectData.Draw function.
        ///
        /// パターンデータのリストを設定取得します。
        /// このリストの内容から、任意のパターンを表示することも可能です。
        /// 表示の詳細は、PatternObjectData.Draw関数を参照ください。
        /// </summary>
        public List<PatternObjectData> PatternObjectList
        {
            get
            {
                if (null == patternObjectList)
                    patternObjectList = new List<PatternObjectData>();

                return patternObjectList;
            }
        }
        #endregion
    }

}
