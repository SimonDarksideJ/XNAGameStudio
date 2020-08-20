#region File Description
//-----------------------------------------------------------------------------
// UtilData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using RobotGameData;
#endregion

namespace RobotGameData.Helper
{
    /// <summary>
    /// Useful functions about file system.
    /// </summary>
    public static class HelperFile
    {
        /// <summary>
        /// It saves the specified object’s data into a XML file.
        /// </summary>
        public static void SaveData(string fileName, object data)
        {
            string path = fileName;
            Stream stream = File.Create(path);

            // Convert the object to XML data and put it in the stream
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(stream, data);

            // Close the file
            stream.Close();
        }

        /// <summary>
        /// It reads from a XML file into the specified type class.
        /// </summary>
        public static object LoadData(string fileName, Type type)
        {
            string path = fileName;
            Stream stream = File.OpenRead(Path.Combine("Content", path));

            XmlSerializer serializer = new XmlSerializer(type);

            return serializer.Deserialize(stream);
        }
    }
}
