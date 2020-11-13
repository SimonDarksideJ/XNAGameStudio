#region File Description
//-----------------------------------------------------------------------------
// MenuData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

using Movipa.Components.Animation;
using Movipa.Components.Scene.Puzzle;
using Movipa.Util;
using MovipaLibrary;
using SceneDataLibrary;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Stores common data used in menus.
    /// 
    /// メニューで使用する共通データを格納します。
    /// </summary>
    public class MenuData : IDisposable
    {
        #region Fields
        /// <summary>
        /// Cursor sphere rotation speed
        /// 
        /// カーソルの球体の回転速度
        /// </summary>
        public readonly float CursorSphereRotate;

        /// <summary>
        /// Cursor sphere default size
        /// 
        /// カーソルの球体のデフォルトサイズ
        /// </summary>
        public readonly float CursorSphereSize;

        /// <summary>
        /// Small cursor sphere 
        /// 
        /// カーソルの球体の小さいサイズ
        /// </summary>
        public readonly float CursorSphereMiniSize;

        /// <summary>
        /// Enlarge value when cursor sphere selected
        /// 
        /// カーソルの球体の選択時に拡大する値
        /// </summary>
        public readonly float CursorSphereZoomSpeed;

        /// <summary>
        /// Change value during cursor sphere fade
        ///
        /// カーソルの球体のフェード時に変化する値
        /// </summary>
        public readonly float CursorSphereFadeSpeed;

        /// <summary>
        /// Camera position viewing sphere
        ///
        /// 球体を見るカメラ位置
        /// </summary>
        public readonly Vector3 CameraPosition;
        
        /// <summary>
        /// Menu scene data
        ///
        /// メニューのシーンデータ
        /// </summary>
        public SceneData sceneData;

        /// <summary>
        /// Background and selected status sphere
        ///
        /// 背景と選択状態の球体
        /// </summary>
        public BasicModelData[][] Spheres;

        /// <summary>
        /// Movie animation
        /// 
        /// ムービーアニメーション
        /// </summary>
        public PuzzleAnimation movie;

        /// <summary>
        /// Movie loader
        ///
        /// ムービーローダ
        /// </summary>
        public MovieLoader movieLoader;

        /// <summary>
        /// Movie texture
        ///
        /// ムービーテクスチャ
        /// </summary>
        public Texture2D movieTexture;

        /// <summary>
        /// Split preview render target
        ///
        /// 分割プレビューのレンダーターゲット
        /// </summary>
        public RenderTarget2D DividePreview;

        /// <summary>
        /// Split texture
        /// 
        /// 分割テクスチャ
        /// </summary>
        public Texture2D divideTexture;

        /// <summary>
        /// Style animation render target
        ///
        /// スタイルアニメーションのレンダーターゲット
        /// </summary>
        public RenderTarget2D StyleAnimation;

        /// <summary>
        /// Style animation texture
        ///
        /// スタイルアニメーションのテクスチャ
        /// </summary>
        public Texture2D StyleAnimationTexture;

        /// <summary>
        /// Style animation texture
        ///
        /// スタイルアニメーションのテクスチャ
        /// </summary>
        public SequencePlayData SeqStyleAnimation;

        /// <summary>
        /// Stage settings information 
        ///
        /// ステージの設定情報
        /// </summary>
        public StageSetting StageSetting;

        /// <summary>
        /// Panel management class 
        ///
        /// パネルの管理クラス
        /// </summary>
        public PanelManager PanelManager;

        /// <summary>
        /// Primitive drawing class
        ///
        /// 基本描画クラス
        /// </summary>
        public PrimitiveDraw2D primitiveDraw;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public MenuData(Game game)
        {
            CursorSphereRotate = MathHelper.ToRadians(1.0f);
            CursorSphereSize = 0.4f;
            CursorSphereMiniSize = 0.18f;
            CursorSphereZoomSpeed = 0.01f;
            CursorSphereFadeSpeed = 0.1f;

            CameraPosition = new Vector3(0.0f, 0.0f, 200.0f);
            
            StageSetting = new StageSetting();
            PanelManager = new PanelManager(game);
            primitiveDraw = new PrimitiveDraw2D(game.GraphicsDevice);
        }
        #endregion

        #region IDisposable Members

        private bool disposed = false;
        public bool Disposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources.
        /// 
        /// 全てのリソースを開放します。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                if (primitiveDraw != null)
                {
                    primitiveDraw.Dispose();
                    primitiveDraw = null;
                }
                if (DividePreview != null && !DividePreview.IsDisposed)
                {
                    DividePreview.Dispose();
                    DividePreview = null;
                }
                if (StyleAnimation != null && !StyleAnimation.IsDisposed)
                {
                    StyleAnimation.Dispose();
                    StyleAnimation = null;
                }
                disposed = true;
            }
        }

        #endregion

    }
}
