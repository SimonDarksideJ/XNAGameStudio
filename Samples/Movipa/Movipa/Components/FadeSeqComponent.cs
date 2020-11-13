#region File Description
//-----------------------------------------------------------------------------
// FadeSeqComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SceneDataLibrary;
#endregion

namespace Movipa.Components
{
    #region Public Types
    /// <summary>
    /// Fade status
    /// 
    /// フェード状態
    /// </summary>
    public enum FadeMode
    {
        /// <summary>
        /// Fade-in
        /// 
        /// フェードイン
        /// </summary>
        FadeIn,

        /// <summary>
        /// Fade-out
        /// 
        /// フェードアウト
        /// </summary>
        FadeOut,

        /// <summary>
        /// Stop fade
        /// 
        /// フェード停止
        /// </summary>
        None
    }

    /// <summary>
    /// Fade type
    /// 
    /// フェード種類
    /// </summary>
    public enum FadeType
    {
        /// <summary>
        /// Normal fade
        /// 
        /// 通常フェード
        /// </summary>
        Normal,
        
        /// <summary>
        /// Rectangle fade
        /// 
        /// 矩形フェード
        /// </summary>
        RotateBox,
        
        /// <summary>
        /// Gonzales fade
        /// 
        /// ゴンザレスフェード
        /// </summary>
        Gonzales,
    }
    #endregion

    /// <summary>
    /// The component that draws the fade.
    /// The fade animation involves loading the Layout 
    /// sequence and managing and drawing the in and out separately.
    /// To add more fade types, first add animations to the sequence
    /// used for fade processing, then add the Fadetype item, then
    /// load the corresponding animation. 
    /// 
    /// フェードの描画をするコンポーネントです。
    /// フェードのアニメーションにはLayoutのシーケンスを読み込み、
    /// インとアウトを別に管理して描画しています。
    /// フェードの種類を増やしたい場合は、事前にフェードに使用する
    /// シーケンスにアニメーションを追加し、FadeTypeの項目を追加し、
    /// 対応するアニメーションを読み込みます。
    /// </summary>
    public class FadeSeqComponent : SceneComponent
    {
        #region Private Types
        /// <summary>
        /// Fade-in sequence name
        /// 
        /// フェードインのシーケンス名
        /// </summary>
        private const string SeqFadeInName = "FadeIn";

        /// <summary>
        /// Fade-out sequence name
        /// 
        /// フェードアウトのシーケンス名
        /// </summary>
        private const string SeqFadeOutName = "FadeOut";
        #endregion

        #region Fields
        private Dictionary<FadeType, SceneData> sceneList;
        private Dictionary<FadeType, Dictionary<FadeMode, SequencePlayData>> seqList;
        private SequencePlayData curSeqData = null;
        private FadeMode fadeMode = FadeMode.None;
        private float count = 0.0f;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains or sets the fade status.
        /// 
        /// フェード状態を取得または設定します。
        /// </summary>
        public FadeMode FadeMode
        {
            get { return fadeMode; }
            set { fadeMode = value; }
        }


        /// <summary>
        /// Obtains the fade count.
        /// 
        /// フェードカウントを取得します。
        /// </summary>
        public float Count
        {
            get { return count; }
        }


        /// <summary>
        /// Obtains the playback status.
        /// 
        /// 再生状態を取得します。
        /// </summary>
        public bool IsPlay
        {
            get { return curSeqData.IsPlay; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        /// 
        /// インスタンスを初期化します。
        /// </summary>
        public FadeSeqComponent(Game game)
            : base(game)
        {
        }


        /// <summary>
        /// Executes content load processing.
        /// 
        /// コンテントの読み込み処理を実行します。
        /// </summary>
        protected override void LoadContent()
        {
            sceneList = new Dictionary<FadeType, SceneData>();
            seqList = new Dictionary<FadeType, Dictionary<FadeMode, SequencePlayData>>();

            // Loads the sequence data.
            // 
            // シーケンスデータを読み込みます。
            addFadeScene(FadeType.Normal, "Layout/Fade/Normal_Scene");
            addFadeScene(FadeType.RotateBox, "Layout/Fade/RotateBox_Scene");
            addFadeScene(FadeType.Gonzales, "Layout/Fade/Gonzales_Scene");

            base.LoadContent();
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Updates the sequence. 
        /// 
        /// シーケンスの更新処理を行います。
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (curSeqData != null)
            {
                curSeqData.Update(gameTime.ElapsedGameTime);
                count += 1.0f;
            }

            base.Update(gameTime);
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the sequence.
        /// 
        /// シーケンスの描画処理を行います。
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (curSeqData != null)
            {
                Batch.Begin();
                curSeqData.Draw(Batch, null);
                Batch.End();
            }

            base.Draw(gameTime);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Starts the fade processing.
        /// 
        /// フェード処理を開始します。
        /// </summary>
        /// <param name="type">Fade Type</param>
        ///
        /// <param name="type">フェードの種類</param>

        /// <param name="mode">Fade Status</param>
        /// 
        /// <param name="mode">フェードの状態</param>
        public void Start(FadeType type, FadeMode mode)
        {
            // No processing is performed when Stop status is specified.
            // 
            // 状態が停止で指定された場合は処理を行いません。
            if (mode == FadeMode.None)
            {
                return;
            }

            // Sets the current fade status.
            // 
            // 現在のフェード状態を設定します。
            FadeMode = mode;

            // Initializes the count. 
            // 
            // カウントを初期化します。
            count = 0.0f;

            // Replaces the sequence with the specified one.
            // 
            // シーケンスを指定のものに差し替えます。
            curSeqData = seqList[type][mode];
            curSeqData.SequenceData.SequenceGroupList[0].Replay();

            // Updates the first frame once.
            // 
            // 最初のフレームに一度更新します。
            curSeqData.Update(new TimeSpan());
        }


        /// <summary>
        /// Adds the specified sequence to the array.
        /// 
        /// 指定されたシーケンスを配列に追加します。
        /// </summary>
        /// <param name="type">Fade Type</param>
        ///  
        /// <param name="type">フェードタイプ</param>
        /// <param name="asset">Sequence Asset Name</param>
        ///  
        /// <param name="asset">シーケンスのアセット名</param>
        private void addFadeScene(FadeType type, string asset)
        {
            // Loads the scene data.
            // 
            // シーンデータを読み込みます。
            SceneData scene = Content.Load<SceneData>(asset);
            sceneList.Add(type, scene);

            // Loads the sequence from the scene data.
            // 
            // シーンデータからシーケンスを読み込みます。
            Dictionary<FadeMode, SequencePlayData> fadeList = 
                new Dictionary<FadeMode, SequencePlayData>();
            fadeList.Add(FadeMode.FadeIn, scene.CreatePlaySeqData(SeqFadeInName));
            fadeList.Add(FadeMode.FadeOut, scene.CreatePlaySeqData(SeqFadeOutName));
            seqList.Add(type, fadeList);
        }
        #endregion
    }
}


