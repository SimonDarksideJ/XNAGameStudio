#region File Description
//-----------------------------------------------------------------------------
// SaveFileLoader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;

using Movipa.Util;
#endregion

namespace Movipa.Components.Scene.Menu
{
    /// <summary>
    /// Performs an asynchronous search for the save
    /// file of the game. 
    /// This class is designed to execute the 
    /// Initialize method in a thread by inheriting 
    /// InitializeThread and invoking the associated 
    /// Run method. The save files are named SaveDataFile1.xml,
    /// SaveDataFile2.xml and SaveDataFile3.xml.
    /// 
    /// ゲームのセーブファイルを非同期で検索します。
    /// このクラスはInitializeThreadを継承し、InitializeThreadの
    /// Runメソッドを呼び出すことで、Initializeメソッドをスレッドで
    /// 実行するようになっています。
    /// セーブファイルの名称については、SaveDataFile1.xml、SaveDataFile2.xml、
    /// SaveDataFile3.xml、となっています。
    /// </summary>
    public class SaveFileLoader : InitializeThread
    {
        #region Fields
        private SaveData[] gameSettings = null;
        #endregion

        #region Properties
        /// <summary>
        /// Obtains settings information.
        /// 
        /// 設定情報を取得します。
        /// </summary>
        public SaveData[] GetGameSettings()
        {
            if (!Initialized)
                return null;

            return gameSettings;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the instance.
        ///
        /// インスタンスを初期化します。
        /// </summary>
        public SaveFileLoader(Game game, int cpu)
            : base(game, cpu)
        {
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Loads the save file.
        ///
        /// セーブファイルの読み込みを行います。
        /// </summary>
        protected override void Initialize()
        {
            // Sets the CPU core.
            // 
            // CPUコアの設定をします。
            SetCpuCore();

            // Sets the filename to be searched.
            // 
            // 検索するファイル名を設定します。
            string[] saveFilePathList = {
                GameData.Storage.GetStoragePath("SaveDataFile1.xml"),
                GameData.Storage.GetStoragePath("SaveDataFile2.xml"),
                GameData.Storage.GetStoragePath("SaveDataFile3.xml"),
            };

            // Loads the file and adds it to the list.
            //
            // ファイルを読み込み、リストに追加します。
            List<SaveData> saveList = new List<SaveData>();
            foreach (string saveFilePath in saveFilePathList)
            {
                saveList.Add(SettingsSerializer.LoadSaveData(saveFilePath));
            }

            // Converts the list to an array.
            // 
            // リストを配列に変換します。
            gameSettings = saveList.ToArray();

            base.Initialize();
        }
        #endregion
    }

}
