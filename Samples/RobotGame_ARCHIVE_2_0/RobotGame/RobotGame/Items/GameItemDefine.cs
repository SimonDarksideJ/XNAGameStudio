#region File Description
//-----------------------------------------------------------------------------
// GameItemDefine.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RobotGame
{
    #region Enum

    public enum ItemType
    {
        /// <summary>
        /// N/A
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// a HP recovery item.
        /// </summary>
        RemedyBox,

        /// <summary>
        /// an ammunition replenishment item.
        /// </summary>
        MagazineBox,
    }

    #endregion

    /// <summary>
    /// It has a number that shows the item’s capabilities and 
    /// is used as reference during game play. 
    /// The class reads from an item spec file(Content/Data/Items/<*>.spec) 
    /// and stores the values.
    /// The item spec file is in XML format and the values can be easily changed.
    /// </summary>
    [Serializable]
    public class ItemBoxSpec : GameDataSpec
    {
        public ItemType Type = ItemType.Unknown;

        public float ModelRadius = 0.0f;

        public int RecoveryLife = 0;
        public int RecoveryBullet = 0;

        public string ModelFilePath = String.Empty;
    }
}
