#region File Description
//-----------------------------------------------------------------------------
// SettingsSerializer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
#endregion

namespace Movipa.Util
{
    /// <summary>
    /// Reads and writes parameters to XML.
    /// Values defined in this class can be used by specifying an
    /// optional type in the template. However, supported types are limited to 
    /// those which can be serialized to XML.
    /// 
    /// XMLへパラメータを書き込んだり、読み込んだりします。
    /// テンプレートに任意の型を指定する事によって、クラスで設定されている値も
    /// 扱うことが可能です。ただし、サポートしている型はXMLへのシリアライズが
    /// サポートされている物に限ります。
    /// </summary>
    public static class SettingsSerializer
    {
        #region Serialize Method
        /// <summary>
        /// Serializes and saves to the file.
        /// 
        /// シリアライズしてファイルに保存します。
        /// </summary>
        /// <param name="filename">File name</param>
        ///  
        /// <param name="filename">ファイル名</param>
        public static void SaveSaveData(string filename, SaveData saveData)
        {
            // Opens the file or creates a new one.
            // 
            // ファイルを開く、または新規作成をするします。
            using (FileStream stream = File.Open(filename, FileMode.Create))
            {
                // Serializes and writes the content.
                // 
                // 内容をシリアライズして書き込みます。
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                serializer.Serialize(stream, saveData);
                stream.Flush();
            }
        }


        /// <summary>
        /// Loads the serialized information.
        /// Returns NULL if the file cannot be found. 
        /// 
        /// シリアライズされた情報を読み込みます。
        /// ファイルが見つからなかった場合はNULLが戻ります。
        /// </summary>
        /// <param name="filename">File name</param>
        ///  
        /// <param name="filename">ファイル名</param>
        public static SaveData LoadSaveData(string filename)
        {
            SaveData settings = default(SaveData);

            // Checks for file; if file cannot be found, returns the default value.
            // 
            // ファイルの有無をチェックし、見つからなければ初期値を返します。
            if (File.Exists(filename) == false)
            {
                return settings;
            }

            // Opens the file as a stream.
            // 
            // ファイルをストリームで開きます。
            using (FileStream stream = File.Open(filename, FileMode.OpenOrCreate))
            {
                StreamReader streamReader = new StreamReader(stream);

                // Reads the file content to the end.
                // 
                // ファイルの内容を最後まで読み込みます。
                string xml = streamReader.ReadToEnd();

                // Restores the values.
                // 
                // 値を復元します。
                settings = DeserializeSaveData(xml);
            }

            return settings;
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Performs deserialization from XML. 
        /// 
        /// XMLからデシリアライズを実行します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">XML text string</param>
        ///  
        /// <param name="xml">XMLの文字列</param>

        /// <returns>Restored setting values</returns>
        ///  
        /// <returns>復元された設定情報の値</returns>

        private static SaveData DeserializeSaveData(string xml)
        {
            SaveData settings = default(SaveData);

            // Creates stream from text.
            // 
            // テキストからストリームを作成します。
            using (StringReader stream = new StringReader(xml))
            {
                // Restores the values.
                // 
                // 値を復元します。
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                settings = (SaveData)serializer.Deserialize(stream);
            }

            return settings;
        }
        #endregion
    }
}