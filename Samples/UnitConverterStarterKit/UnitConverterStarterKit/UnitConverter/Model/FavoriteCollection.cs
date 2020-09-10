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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Microsoft.Phone.Applications.UnitConverter.Helpers;

namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// Hold a set of information for unit conversion settings that the user can 
    /// retrieve similar to a web browser favorite list.
    /// This collection will be data bound to the UI control.
    /// </summary>
    public class FavoriteCollection : ObservableCollection<FavoriteData>
    {
        /// <summary>
        /// Object used to prevent multiple save/load operations
        /// </summary>
        private static object saveLock = new object();

        /// <summary>
        /// Add a new item to the collection, only if unique
        /// </summary>
        /// <param name="data">Object to add</param>
        /// <returns>true if the object already exists</returns>
        internal bool AddNewItem(FavoriteData data)
        {
            bool favoriteExists = Contains(data);
            if (!favoriteExists)
            {
                data.LabelName = string.Format(CultureInfo.CurrentCulture,
                       UnitConverter.Resources.Strings.FavoriteItemLabel,
                       data.SourceUnitName, data.TargetUnitName);
                Add(data);
            }
            return favoriteExists;
        }


        /// <summary>
        /// Saves file on a thread pool thread
        /// </summary>
        /// <param name="data">Data to save</param>
        /// <param name="completed">Delegate to call on completed</param>
        /// <param name="handleException">Exception handler delegate</param>
        internal static void BeginSaveToFile(
            FavoriteCollection data,
            Action completed,
            Action<Exception> handleException)
        {
            IsolatedStorage<FavoriteCollection> f = new IsolatedStorage<FavoriteCollection>();
            f.BeginSave(FavoriteData.FavoriteFileName, data, completed, handleException);
        }

        /// <summary>
        /// Load a file on a thread pool thread
        /// </summary>
        /// <param name="completed">Delegate to call on completed</param>
        /// <param name="handleException">Exception handler delegate</param>
        public static void BeginLoad(Action completed, Action<Exception> handleException)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    lock (saveLock)
                    {
                        ApplicationState.Favorites = LoadFromFile() ?? new FavoriteCollection();
                    }

                    if (completed != null)
                    {
                        completed();
                    }
                }
                catch (Exception e)
                {
                    if (handleException != null)
                    {
                        handleException(e);
                    }
                }
            });
        }


        /// <summary>
        /// Load the saved favorites from a file in isolated storage.
        /// Perform culture conversion if needed
        /// </summary>
        /// <returns>Deserialized object</returns>
        internal static FavoriteCollection LoadFromFile()
        {
            IsolatedStorage<FavoriteCollection> f = new IsolatedStorage<FavoriteCollection>();
            FavoriteCollection loadedFile = f.LoadFromFile(FavoriteData.FavoriteFileName);
            if (loadedFile == null)
            {
                loadedFile = new FavoriteCollection();
            }
            /* In the case where the favorites were stored in one language, but we have 
             * switched to another language, check to see if the first item category matches
             * the current locale. If it does, then break out. Othewise, convert all of the
             * unit information into the current locale
             */
            foreach (FavoriteData d in loadedFile)
            {
                if (string.Compare(d.Category,
                    Resources.Strings.ResourceManager.GetString(d.CategoryResource),
                    StringComparison.OrdinalIgnoreCase) == 0)
                {
                    break;
                }
                d.Category = Resources.Strings.ResourceManager.GetString(d.CategoryResource);
                d.SourceUnitName = Resources.Strings.ResourceManager.GetString(d.SourceUnitNameResource);
                d.TargetUnitName = Resources.Strings.ResourceManager.GetString(d.TargetUnitNameResource);
                d.LabelName = string.Format(CultureInfo.CurrentCulture,
                      UnitConverter.Resources.Strings.FavoriteItemLabel,
                      d.SourceUnitName, d.TargetUnitName);
            }
            return loadedFile;
        }

        /// <summary>
        /// Save object to a file
        /// </summary>
        /// <param name="data">Object to save</param>
        internal static void SaveToFile(FavoriteCollection data)
        {
            IsolatedStorage<FavoriteCollection> f = new IsolatedStorage<FavoriteCollection>();
            f.SaveToFile(FavoriteData.FavoriteFileName, data);
        }
    }
}
