#region File Description
//-----------------------------------------------------------------------------
// DrawData.cs
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
    /// This class manages conversion information for drawing.
    /// This class is used to indicate changes of position, rotation, scale, center, 
    /// and brightness both for pattern objects and pattern group sequences, 
    /// which are drawing units of Layout.
    /// This class also manages default conversion information for these elements.
    ///
    /// 描画用の変換情報を保持します。
    /// Layoutの描画単位である、パターンオブジェクト、パターングループ
    /// シーケンス、それぞれに対して、位置・回転・スケール・中心・輝度の変更を
    /// 指示する際に使用されます。
    /// 上記要素のデフォルトの変換情報の保持もこのクラスを使用します。
    /// </summary>
    public class DrawData
    {
        #region Fields

        private Point position = new Point(); // Distance 
        private Color color = Color.White; // Color 
        private Vector2 scale = new Vector2(1.0f, 1.0f); // Enlargement scale
        private Point center = new Point(); // Center of rotation enlargement
        private float rotateZ = 0.0f; // Rotation value

        #endregion

        #region Properties
        /// <summary>
        /// Obtains and sets the display position.
        ///
        /// 表示位置を取得設定します。
        /// </summary>
        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Obtains and sets the display color.
        ///
        /// 表示色を取得設定します。
        /// </summary>
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        /// <summary>
        /// Obtains and sets the enlargement scale.
        ///
        /// 拡大率の取得設定を行います。
        /// </summary>
        public Vector2 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        /// <summary>
        /// Obtains and sets the center position of rotation enlargement.
        ///
        /// 回転拡大の中心位置の取得設定を行います。
        /// </summary>
        public Point Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
            }
        }

        /// <summary>
        /// Obtains and sets the rotation value.
        ///
        /// 回転量の取得設定を行います。
        /// </summary>
        public float RotateZ
        {
            get
            {
                return rotateZ;
            }
            set
            {
                rotateZ = value;
            }
        }
        #endregion

        /// <summary>
        /// Converts the held data to character strings.
        ///
        /// 保持しているデータを文字列に変換します。
        /// </summary>
        /// <returns>
        /// Converted character string
        /// 
        /// 変換された文字列
        /// </returns>
        public override string ToString()
        {
            string value = base.ToString() + "\n";
            value += string.Format("Point  : {0}\n", position);
            value += string.Format("Scale  : {0}\n", scale);
            value += string.Format("Center : {0}\n", center);
            value += string.Format("Rotate : {0}\n", rotateZ);
            value += string.Format("Color  : {0}\n", color);

            return value;
        }

    }
}
