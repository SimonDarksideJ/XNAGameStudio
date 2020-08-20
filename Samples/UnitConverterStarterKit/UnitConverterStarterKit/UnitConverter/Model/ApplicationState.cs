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

using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Microsoft.Phone.Applications.UnitConverter.Helpers;
using Microsoft.Phone.Applications.UnitConverter.Model;
using Microsoft.Phone.Shell;


namespace Microsoft.Phone.Applications.UnitConverter
{
    /// <summary>
    /// Application state information that is used to pass information between pages,
    /// and support application activiation and deactivation
    /// </summary>
    internal class ApplicationState
    {
        /// <summary>
        /// Page URL for the categories page
        /// </summary>
        internal const string CategoriesFileName = @"Resources/SupportedUnits.xml";

        /// <summary>
        /// Main Page URL
        /// </summary>
        internal const string MainPageName = @"/View/MainPage.xaml";


        /// <summary>
        /// Category Page URL
        /// </summary>
        internal const string CategoryPageName = @"/View/CategorySelection.xaml";

        /// <summary>
        /// Test page URL
        /// </summary>
        internal const string TestPageName = @"/View/TestInterface.xaml";

        /// <summary>
        /// Key for the Main Page state storage
        /// </summary>
        internal const string MainPageState = "MainPageState";

        /// <summary>
        /// Key for the Category page state storage
        /// </summary>
        internal const string CategoryPageState = "CategoryPageState";

        /// <summary>
        /// Key for the supported conversions storage
        /// </summary>
        internal const string SupportedConversionsState = "SupportedConversions";

        /// <summary>
        /// Key for the favorages storage
        /// </summary>
        internal const string FavoritesState = "Favorites";

        /// <summary>
        /// Max size of the startup log to keep from growing as the app is used
        /// </summary>
        internal const int StartupLogSize = 50;

        /// <summary>
        /// Key to access the phone's background color
        /// </summary>
        private const string PhoneBackground = "PhoneBackgroundColor";

        /// <summary>
        /// Current culture backing store
        /// </summary>
        private static CultureInfo _currentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets the current culture.
        /// </summary>
        internal static CultureInfo CurrentCulture { get { return _currentCulture; } }

        /// <summary>
        /// Gets or sets the application startup state.
        /// This is a hint for the page loading to use to optimize the work done.
        /// </summary>
        internal static AppOpenState ApplicationStartup { get; set; }

        /// <summary>
        /// Gets or sets the supported conversions.
        /// This will be read from a content file on app launch
        /// </summary>
        internal static CategoryInformation[] SupportedConversions { get; set; }

        /// <summary>
        /// Category page state information.
        /// </summary>
        internal static CategoryPageState CategoryPageInformation { get; set; }

        /// <summary>
        ///Main page state information.
        /// </summary>
        internal static MainPageState MainPageInformation { get; set; }

        /// <summary>
        /// Favorite list to be shared between application pages
        /// </summary>
        internal static FavoriteCollection Favorites { get; set; }

        /// <summary>
        /// Direct access to unit categories
        /// </summary>
        [XmlIgnore]
        internal static Dictionary<string, CategoryInformation> 
            UnitCategoryAccess { get; private set; }

        /// <summary>
        /// Log to hold exception or error information
        /// </summary>
        internal static ErrorLogCollection ErrorLog = new ErrorLogCollection();


        /// <summary>
        /// Gets the current theme. True for a dark theme, false otherwise
        /// </summary>
        public static bool IsDarkTheme
        {
            get
            {
                return ((Color)Application.Current.Resources[PhoneBackground]).Equals(Colors.Black);
            }
        }
      

        /// <summary>
        /// Initial application initialization. Logic that is deferred related to 
        /// loading the supported conversions from a file in the xap to speed up
        /// 1st page render
        /// </summary>
        internal static void AppLaunchInitialization()
        {
            if (ApplicationState.MainPageInformation == null)
            {
                ApplicationState.MainPageInformation = new MainPageState();
            }
            SupportedConversions =
               FileOps.LoadFromFileContent<CategoryInformation[]>(ApplicationState.CategoriesFileName);
            UnitCategoryAccess = new Dictionary<string, CategoryInformation>();
            foreach (CategoryInformation c in SupportedConversions)
            {
                c.CategoryLocalized = Resources.Strings.ResourceManager.GetString(c.Category);
                UnitCategoryAccess.Add(c.CategoryLocalized, c);
                c.ReadLocalizedNames();
                c.InitializeDictionary();
            }
        }

        /// <summary>
        /// Apps the activated initialization.
        /// </summary>
        internal static void AppActivatedInitialization()
        {
            UnitCategoryAccess = new Dictionary<string, CategoryInformation>();
            foreach (CategoryInformation c in SupportedConversions)
            {
                UnitCategoryAccess.Add(c.CategoryLocalized, c);
                c.InitializeDictionary();
            }
        }


        


        /// <summary>
        /// Adds the app state objects to the system object store
        /// </summary>
        /// <param name="categoryPage">If true, save the category page state</param>
        internal static void AddAppObjects(bool categoryPage)
        {
            AddObject(MainPageState, MainPageInformation);
            AddObject(SupportedConversionsState, SupportedConversions);
            if (categoryPage)
            {
                AddObject(CategoryPageState, CategoryPageInformation);
            }
            AddObject(FavoritesState, Favorites);
        }

       
        /// <summary>
        /// Retrieves the app state objects from the system object store
        /// </summary>
        /// <param name="categoryPage">If true, retrieve the category page state</param>
        /// <returns>True if all objects are non null, false otherwise</returns>
        internal static bool RetrieveAppObjects(bool categoryPage)
        {
            bool allObjectsNonNull = false;
            MainPageInformation =  RetrieveObject<MainPageState>(MainPageState);
            SupportedConversions = RetrieveObject<CategoryInformation[]>(SupportedConversionsState);
            Favorites = RetrieveObject<FavoriteCollection>(FavoritesState);
            if (SupportedConversions != null &&
                 MainPageInformation != null &&
                 Favorites != null)
            {
                allObjectsNonNull = true;
                AppActivatedInitialization();
                if (categoryPage)
                {
                    CategoryPageInformation = RetrieveObject<CategoryPageState>(CategoryPageState);
                    allObjectsNonNull = CategoryPageInformation != null ? true : false;
                }
            }
            return allObjectsNonNull;
        }


        /// <summary>
        /// Retrieves the specified object from the system object store
        /// </summary>
        /// <typeparam name="T">Data to retrieve</typeparam>
        /// <param name="key">The object key</param>
        /// <returns>object default, or deserialized object</returns>
        private static T RetrieveObject<T>(string key)
        {
            T data = default(T);
            if (PhoneApplicationService.Current.State.ContainsKey(key))
            {
                data = (T)PhoneApplicationService.Current.State[key];
            }
            return data;
        }

        /// <summary>
        /// Adds the specified data object to the system object store
        /// </summary>
        /// <param name="key">The object key.</param>
        /// <param name="data">The data to store.</param>
        private static void AddObject(string key, object data)
        {
            if (PhoneApplicationService.Current.State.ContainsKey(key))
            {
                PhoneApplicationService.Current.State.Remove(key);
            }
            PhoneApplicationService.Current.State.Add(key, data);
        }


    }
}
