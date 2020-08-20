// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Serialization;

namespace Microsoft.Phone.Applications.UnitConverter.Helpers
{

    /// <summary>
    /// Generic file operation helpers
    /// </summary>
    public static class FileOps
    {

        /// <summary>
        /// Loads a file from content file in the xap.
        /// </summary>
        /// <typeparam name="T">Type of data to deserialize</typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Default or deserialized object</returns>
        public static T LoadFromFileContent<T>(string fileName)
        {
            T loadedFile = default(T);

            try
            {
                StreamResourceInfo sr =
                    Application.GetResourceStream(new Uri(fileName, UriKind.Relative));
                if (sr != null)
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                    loadedFile = (T)mySerializer.Deserialize(sr.Stream);
                }
            }
            catch (Exception e)
            {
                ApplicationState.ErrorLog.Add(new ErrorLog("LoadFromFileContent", e.Message));
            }
            return loadedFile;
        }
    }
}
