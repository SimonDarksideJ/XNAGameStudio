#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelAnimationInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class manages animation information of a skin model.
    /// Camera position and direction as well as model data are managed
    /// in this class.  SkinnedModelInfo is defined in a list format 
    /// so that multiple models can be managed.
    ///
    /// スキンモデルのアニメーション情報を持ちます。
    /// モデルデータの他に、カメラの位置や方向の情報を持ちます。
    /// 複数のモデルを管理できるようにSkinnedModelInfoはリスト形式で定義されています。
    /// </summary>
    public class SkinnedModelAnimationInfo : AnimationInfo
    {
        #region Fields
        private List<SkinnedModelInfo> skinnedModelInfoCollection = 
            new List<SkinnedModelInfo>();
        private Vector3 cameraUpVector;
        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the list of the skin model information.
        ///
        /// スキンモデル情報のリストを取得または設定します。
        /// </summary>
        public List<SkinnedModelInfo> SkinnedModelInfoCollection
        {
            get { return skinnedModelInfoCollection; }
        }


        /// <summary>
        /// Obtains or sets the camera coordinate system.
        ///
        /// カメラの座標系を取得または設定します。
        /// </summary>
        public Vector3 CameraUpVector
        {
            get { return cameraUpVector; }
            set { cameraUpVector = value; }
        }


        /// <summary>
        /// Obtains or sets the camera position.
        ///
        /// カメラの位置を取得または設定します。
        /// </summary>
        public Vector3 CameraPosition
        {
            get { return cameraPosition; }
            set { cameraPosition = value; }
        }


        /// <summary>
        /// Obtains or sets the camera viewpoint.
        ///
        /// カメラの視点を取得または設定します。
        /// </summary>
        public Vector3 CameraLookAt
        {
            get { return cameraLookAt; }
            set { cameraLookAt = value; }
        }
        #endregion
    }
}
