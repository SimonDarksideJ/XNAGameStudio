#region File Description
//-----------------------------------------------------------------------------
// Sounds.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace Movipa.Util
{
    /// <summary>
    /// Class that collects sound cue name constants.
    /// Used for the SoundComponent PlayBackgroundMusic or PlaySoundEffect.
    /// 
    /// サウンドのキュー名の定数をまとめるクラスです。
    /// SoundComponentのPlayBackgroundMusicまたはPlaySoundEffectに使用します。
    /// </summary>
    public static class Sounds
    {
        /// <summary>
        /// Title BackgroundMusic
        /// 
        /// タイトルBackgroundMusic
        /// </summary>
        public const string TitleBackgroundMusic = "TitleBackgroundMusic";

        /// <summary>
        /// Menu BackgroundMusic
        /// 
        /// メニューBackgroundMusic
        /// </summary>
        public const string SelectBackgroundMusic = "SelectBackgroundMusic";

        /// <summary>
        /// Main game BackgroundMusic
        /// 
        /// メインゲームBackgroundMusic
        /// </summary>
        public const string GameBackgroundMusic = "GameBackgroundMusic";

        /// <summary>
        /// Stage completion BackgroundMusic
        /// 
        /// ステージクリアBackgroundMusic
        /// </summary>
        public const string GameClearBackgroundMusic = "GameClearBackgroundMusic";

        /// <summary>
        /// Game over BackgroundMusic
        /// 
        /// ゲームオーバーBackgroundMusic
        /// </summary>
        public const string GameOverBackgroundMusic = "GameOverBackgroundMusic";

        /// <summary>
        /// OK
        /// 
        /// 決定
        /// </summary>
        public const string SoundEffectOkay = "SoundEffectOkay";

        /// <summary>
        /// Cancel
        /// 
        /// キャンセル
        /// </summary>
        public const string SoundEffectCancel = "SoundEffectCancel";

        /// <summary>
        /// Cursor 1
        /// 
        /// カーソル1
        /// </summary>
        public const string SoundEffectCursor1 = "SoundEffectCursor1";

        /// <summary>
        /// Cursor 2
        /// 
        /// カーソル2
        /// </summary>
        public const string SoundEffectCursor2 = "SoundEffectCursor2";

        /// <summary>
        /// All panels completed
        /// 
        /// 全てパネル完成
        /// </summary>
        public const string SoundEffectClear = "SoundEffectClear";

        /// <summary>
        /// Result score addition
        /// 
        /// リザルトのスコア加算
        /// </summary>
        public const string ResultScore = "ResultScore";
    }
}