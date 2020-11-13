#region File Description
//-----------------------------------------------------------------------------
// ParticleInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace MovipaLibrary
{
    /// <summary>
    /// This class manages animation information of particles.
    /// There are the following parameters:
    /// asset name of the texture used in particles, the number of particles 
    /// to be generated, and particle movement information.
    ///
    /// パーティクルのアニメーション情報を持ちます。
    /// パーティクルで使用するテクスチャのアセット名と、生成数と
    /// 移動に関するパラメータがあります。
    /// </summary>
    public class ParticleInfo : AnimationInfo
    {
        #region Fields
        private string particleTexture;
        private float particleSize;
        private UInt32 particleMax;
        private UInt32 particleGenerateCount;
        private float particleJumpPower;
        private float particleMoveSpeed;
        private float particleBoundRate;
        private float particleGravity;
        private Vector3 cameraUpVector;
        private Vector3 cameraPosition;
        private Vector3 cameraLookAt;
        #endregion

        #region Property
        /// <summary>
        /// Obtains or sets the asset name of the particle texture.
        ///
        /// パーティクルテクスチャのアセット名を取得または設定します。
        /// </summary>
        public string ParticleTexture
        {
            get { return particleTexture; }
            set { particleTexture = value; }
        }

        
        /// <summary>
        /// Obtains or sets the particle size.
        ///
        /// パーティクルのサイズを取得または設定します。
        /// </summary>
        public float ParticleSize
        {
            get { return particleSize; }
            set { particleSize = value; }
        }


        /// <summary>
        /// Obtains or sets the maximum number of total particles to be generated. 
        ///
        /// パーティクルの上限を取得または設定します。
        /// </summary>
        public UInt32 ParticleMax
        {
            get { return particleMax; }
            set { particleMax = value; }
        }


        /// <summary>
        /// Obtains or sets the number of particles to be generated at one time.
        ///
        /// パーティクルの一度に生成する数を取得または設定します。
        /// </summary>
        public UInt32 ParticleGenerateCount
        {
            get { return particleGenerateCount; }
            set { particleGenerateCount = value; }
        }


        /// <summary>
        /// Obtains or sets the jump power of the particle.
        ///
        /// パーティクルが跳ねる力を取得または設定します。
        /// </summary>
        public float ParticleJumpPower
        {
            get { return particleJumpPower; }
            set { particleJumpPower = value; }
        }


        /// <summary>
        /// Obtains or sets the movement speed of the particle.
        ///
        /// パーティクルの移動速度を取得または設定します。
        /// </summary>
        public float ParticleMoveSpeed
        {
            get { return particleMoveSpeed; }
            set { particleMoveSpeed = value; }
        }


        /// <summary>
        /// Obtains or sets the bound rate of the particle.
        ///
        /// パーティクルの跳ね返りの力を取得または設定します。
        /// </summary>
        public float ParticleBoundRate
        {
            get { return particleBoundRate; }
            set { particleBoundRate = value; }
        }


        /// <summary>
        /// Obtains or sets the gravity of the particle.
        ///
        /// パーティクルの落ちる強さを取得または設定します。
        /// </summary>
        public float ParticleGravity
        {
            get { return particleGravity; }
            set { particleGravity = value; }
        }


        /// <summary>
        /// Obtains or sets the coordinate system of the camera.
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
