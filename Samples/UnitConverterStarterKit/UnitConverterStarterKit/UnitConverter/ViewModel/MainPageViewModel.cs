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
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using Microsoft.Phone.Applications.UnitConverter.Helpers;
using Microsoft.Phone.Applications.UnitConverter.Model;

namespace Microsoft.Phone.Applications.UnitConverter.ViewModel
{
    /// <summary>
    /// View model for the main application page
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// Image for conversion switch for light theme.
        /// </summary>
        private const string SwitchConversionImageDark = "/Images/switchinuc.png";

        /// <summary>
        /// Image for conversion switch for dark theme.
        /// </summary>
        private const string SwitchConversionImageLight = "/Images/switchinuc.light.png";


        #region Bindable Properties

        /// <summary>
        /// Backing store for the switch conversions source/target image
        /// </summary>
        private string conversionImageSource;

        /// <summary>
        /// Image for the switch source/target unit depending on the theme
        /// </summary>
        public string ConversionImageSource
        {
            get { return this.conversionImageSource; }

            set
            {
                this.conversionImageSource = value;
                this.OnPropertyChanged("ConversionImageSource");
            }
        }

        

        /// <summary>
        /// Backing store for Category
        /// </summary>
        private string category;

        /// <summary>
        /// Name of the category to display
        /// </summary>
        public string Category
        {
            get { return this.category; }

            set
            {
                this.category = value;
                this.OnPropertyChanged("Category");
                this.OnPropertyChanged("UppercasedCategory");
            }
        }


        /// <summary>
        /// Uppercased category
        /// </summary>
        public string UppercasedCategory
        {
            get { return category.ToUpper(CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// Backing store for upper unit Name
        /// </summary>
        private string upperUnitName;

        /// <summary>
        /// Upper Unit Name
        /// </summary>
        public string UpperUnitName
        {
            get { return this.upperUnitName; }

            set
            {
                this.upperUnitName = value;
                this.OnPropertyChanged("UpperUnitName");
            }
        }

        /// <summary>
        /// Backing store for upper unit value
        /// </summary>
        private string upperUnitValue;

        /// <summary>
        /// Upper unit value
        /// </summary>
        public string UpperUnitValue
        {
            get { return this.upperUnitValue; }

            set
            {
                this.upperUnitValue = value;
                this.OnPropertyChanged("UpperUnitValue");
            }
        }



        /// <summary>
        /// Backing store for lower unit Name
        /// </summary>
        private string lowerUnitName;

        /// <summary>
        /// Lower Unit Name
        /// </summary>
        public string LowerUnitName
        {
            get { return this.lowerUnitName; }

            set
            {
                this.lowerUnitName = value;
                this.OnPropertyChanged("LowerUnitName");
            }
        }


        /// <summary>
        /// Backing store for lower unit value
        /// </summary>
        private string lowerUnitValue;

        /// <summary>
        /// Lower unit value
        /// </summary>
        public string LowerUnitValue
        {
            get { return this.lowerUnitValue; }

            set
            {
                this.lowerUnitValue = value;
                this.OnPropertyChanged("LowerUnitValue");
            }
        }


        /// <summary>
        /// Backing store for summary conversion results
        /// </summary>
        private string summary;

        /// <summary>
        /// Summary of conversion 
        /// </summary>
        public string Summary
        {
            get { return this.summary; }

            set
            {
                this.summary = value;
                this.OnPropertyChanged("Summary");
            }
        }


        /// <summary>
        /// Backing store for Conversions Button  Enabled state
        /// </summary>
        private bool isButtonConversionsEnabled;

        /// <summary>
        /// Conversion Button enabled state
        /// </summary>
        public bool IsButtonConversionEnabled
        {
            get { return this.isButtonConversionsEnabled; }

            set
            {
                this.isButtonConversionsEnabled = value;
                this.OnPropertyChanged("IsButtonConversionEnabled");
            }
        }

        /// <summary>
        /// Backing store for Add To Favorite Button  Enabled state
        /// </summary>
        private bool isButtonAddToFavoritesEnabled;

        /// <summary>
        /// Add To Favorites button state
        /// </summary>
        public bool IsButtonAddToFavoritesEnabled
        {
            get { return this.isButtonAddToFavoritesEnabled; }

            set
            {
                this.isButtonAddToFavoritesEnabled = value;
                this.OnPropertyChanged("IsButtonAddToFavoritesEnabled");
            }
        }

        /// <summary>
        /// Set to true to allow the main page button handlers to be enabled
        /// </summary>
        internal bool AllowNavigation
        {
            set
            {
                this.IsButtonConversionEnabled = value;
                this.IsButtonAddToFavoritesEnabled = value;
            }
        }


        #endregion


       
        /// <summary>
        /// When no user text is entered, this is what we display in the 
        /// source unit value field
        /// </summary>
        internal const string DefaultSourceValueText = "0";

        /// <summary>
        /// 
        /// </summary>
        static private readonly string completeFormat = 
            string.Format(CultureInfo.InvariantCulture , "0.{0}", new string('#', 32));

        /// <summary>
        /// Limit the number of input digits allowed to not overflow the screen
        /// </summary>
        private const Int32 MaximumNumberOfInputDigits = 11;


        /// <summary>
        /// Object used to prevent multiple save/load operations
        /// </summary>
        private object threadLock = new object();

        /// <summary>
        /// Background thread object
        /// </summary>
        private BackgroundWorker worker = new BackgroundWorker();



        /// <summary>
        /// Current Conversion information
        /// </summary>
        internal CurrentConversion ConversionSettings { get; private set; }



        #region INotifyPropertyChanged Members

        /// <summary>
        /// Standard pattern for data binding and notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Notify subscribers of a change in the property
        /// </summary>
        /// <param name="propertyName">Name of the property to signal there has been a changed</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, args);
            }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        internal MainPageViewModel()
        {
            this.ConversionSettings = new CurrentConversion();
            this.ConversionImageSource = ApplicationState.IsDarkTheme ?
                SwitchConversionImageLight : SwitchConversionImageDark;
            this.worker.DoWork += new DoWorkEventHandler(DeferStartupWork);
            
        }


        /// <summary>
        /// Deferred startup work to improve page render performance.
        /// </summary>
        /// <param name="completed">Delegate to call on completed</param>
        internal void DeferStartup(Action completed)
        {
            this.worker.RunWorkerAsync(completed);
        }


        /// <summary>
        /// Background thread work for loading 
        /// The loading of the supported conversions from the Xap as well as the 
        /// favorites from isolated storage is done on a non UI thread to allow the 
        /// fastest possible page render
        /// </summary>
        /// <param name="sender">worker thread object</param>
        /// <param name="e">Action delegate for completion</param>
        private void DeferStartupWork(object sender, DoWorkEventArgs e)
        {
            Action completed = e.Argument as Action;
            lock (threadLock)
            {
                ApplicationState.AppLaunchInitialization();

                this.SetDefaultCategoryAndUnits();
                ApplicationState.Favorites =
                    FavoriteCollection.LoadFromFile() ?? new FavoriteCollection();
            }

            if (completed != null)
            {
                completed();
            }
        }

        /// <summary>
        /// Processes the navigated to event.
        /// </summary>
        /// <param name="isPageActivated">true if the page has been activated</param>
        /// <param name="notifyOfLoadCompleted">Action delegate to call when the background thread
        /// processing has completed</param>
        internal void ProcessNavigatedToEvent(
            bool isPageActivated,
            Action notifyOfLoadCompleted )
        {

            if (ApplicationState.ApplicationStartup == AppOpenState.Launching)
            {
                // Initial application startup.
                this.AllowNavigation = false;
                this.SetDefaults();
                // Initialization of the app deferred until the page has rendered. See
                // the MainPage_LayoutUpdated handler
                return;
            }
            if (ApplicationState.ApplicationStartup == AppOpenState.Activated &&
                 !isPageActivated)
            {
                // We are returning to the application, but we were not tomb stoned.
                ApplicationState.ApplicationStartup = AppOpenState.None;
                return;
            }
            this.AllowNavigation = false;
            if (ApplicationState.RetrieveAppObjects(false))
            {
                this.RefreshStateFromAppState();
                this.AllowNavigation = true;
            }
            else
            {
                // Slight possibility that we did not complete 1st time init because of 
                // of a app deactivate immediately after start. Protect against this and
                // perform application 1st time startup sequence.
                this.SetDefaults();
                this.DeferStartup(notifyOfLoadCompleted);
            }
            ApplicationState.ApplicationStartup = AppOpenState.None;
        }


        /// <summary>
        /// Sets the default category and units.
        /// This is hard coded to allow deferring of loading the supported units file
        /// because of trying to move any access to after the first page loads.
        /// The ability to have the user select the default unit through a settings 
        /// page would be a nice enhancement.
        /// </summary>
        private void SetDefaults()
        {
            if (this.ConversionSettings.IsUpperUnitSource)
            {
                this.ConversionSettings.SourceUnit = new UnitInformation("Units_Length_Inches",
                     0.08333333333333333333333333333333, 0, false);
                this.ConversionSettings.SourceUnit.NameLocalized = Resources.Strings.Units_Length_Inches;
                this.ConversionSettings.TargetUnit = new UnitInformation("Units_Length_Feet",
                    1, 0, false);
                this.ConversionSettings.TargetUnit.NameLocalized = Resources.Strings.Units_Length_Feet;

            }
            else
            {
                this.ConversionSettings.TargetUnit = new UnitInformation("Units_Length_Inches",
                     0.08333333333333333333333333333333, 0, false);
                this.ConversionSettings.TargetUnit.NameLocalized = Resources.Strings.Units_Length_Inches;
                this.ConversionSettings.SourceUnit = new UnitInformation("Units_Length_Feet",
                    1, 0, false);
                this.ConversionSettings.SourceUnit.NameLocalized = Resources.Strings.Units_Length_Feet;

            }
            this.Category = Resources.Strings.UnitGroup_Length;
            this.UpperUnitName = this.ConversionSettings.SourceUnit.NameLocalized;
            this.LowerUnitName = this.ConversionSettings.TargetUnit.NameLocalized;
            this.ConversionSettings.UserInput.Append(MainPageViewModel.DefaultSourceValueText);
            this.UpdateUnitDisplayStrings();
        }


        /// <summary>
        /// Sets the default category and units.
        /// </summary>
        internal void SetDefaultCategoryAndUnits()
        {
            string sourceUnitName = null;
            string targetUnitName = null;
            this.ConversionSettings.SetDefaultCategoryAndUnits(out sourceUnitName, out targetUnitName);
        }


        /// <summary>
        /// Gets the favorite collection.
        /// </summary>
        /// <returns>Favorite collection for test purposes</returns>
        internal static FavoriteCollection GetFavoriteCollection()
        {
            FavoriteCollection favCollection = ApplicationState.Favorites;
            return favCollection;
        }

        /// <summary>
        /// Add the current set of units to the Favorit list
        /// </summary>
        /// <param name="signalCompleted">Delegate to call when the thread pool
        /// operation has completed</param>
        /// <returns>True signals that the favorite is already added ( no need to save).
        /// False indicates that we are in process of saving</returns>
        internal bool AddToFavorite(Action signalCompleted)
        {

            FavoriteCollection favCollection = ApplicationState.Favorites;
            FavoriteData favorite = new FavoriteData(this.ConversionSettings, 0);
            bool favoriteExists = favCollection.AddNewItem(favorite);
            if (!favoriteExists)
            {
                // Thread pool queue operation
                FavoriteCollection.BeginSaveToFile(favCollection, signalCompleted, null);
            }
            return favoriteExists;
        }

        /// <summary>
        /// Refreshes the state of the state from the application state settings
        /// </summary>
        internal void RefreshStateFromAppState()
        {
            this.ConversionSettings.RefreshStateFromAppState(this , false);
        }

        /// <summary>
        /// Syncs the state of the state to app.
        /// </summary>
        internal void SyncStateToAppState()
        {
            this.ConversionSettings.SyncStateToAppState(this.Category, this.UpperUnitName, 
                this.LowerUnitName);
        }

        /// <summary>
        /// Helper function to assign the proper value to the upper/lower UI element
        /// depending on how the user has configured the source/target unit selection
        /// </summary>
        /// <param name="sourceUnit">true if the value to assign is for the source
        /// unit. False for the target unit</param>
        /// <param name="value">The value to assign</param>
        private void AssignUnitValue(bool sourceUnit, string value)
        {
            if (sourceUnit)
            {
                    this.UpperUnitValue = value;
            }
            else
            {
                    this.LowerUnitValue = value;
            }
          
        }

        /// <summary>
        /// Clears the user input selection
        /// </summary>
        internal void ClearKeyHandler()
        {
            this.ConversionSettings.UserInput.Remove(0, this.ConversionSettings.UserInput.Length);
            this.ConversionSettings.UserInput.Append(DefaultSourceValueText);
            this.UpdateUnitDisplayStrings();
        }

        /// <summary>
        /// Handles the keyboard +/- sign change 
        /// </summary>
        internal void SignKeyHandler()
        {
            StringBuilder b = this.ConversionSettings.UserInput;
            // We don't add the sign on a zero value (displays by default) unless there's a decimal
            if (b.Length <= 0 || (b.Length == 1 && b[0] == '0'))
            {
                return;
            }

            if (b.Length > 0)
            {
                if (b[0] == '-')
                {
                    b.Remove(0, 1);
                }
                else
                {
                    b.Insert(0, "-");
                }

                this.UpdateUnitDisplayStrings();
            }
        }

        /// <summary>
        /// Handle the back ( erase single character ) keyboard entry
        /// </summary>
        internal void BackKeyHandler()
        {
            StringBuilder b = this.ConversionSettings.UserInput;
            if (b.Length >= 1)
            {
                b.Remove(b.Length - 1, 1);

                if (b.Length <= 0)
                {
                    b.Append(DefaultSourceValueText);
                }
            }
            this.UpdateUnitDisplayStrings();
        }

        /// <summary>
        /// Handle numeric keyboard input
        /// </summary>
        /// <param name="button">The button.</param>
        internal void NumberKeyHandler(Button button)
        {
            StringBuilder b = this.ConversionSettings.UserInput;
            string buttonContent = button.Content.ToString();
            string input = b.ToString();

            if (input == "0")
            {
                // If our current input is already sitting at a zero value we don't 
                // need to add any more
                if (buttonContent == "0")
                {
                    return;
                }

                // We don't want to display a zero preceding the actual number, so we need
                // to remove it as a real number comes in (unless we're adding a decimal!)
                if (buttonContent != 
                    ApplicationState.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                {
                    b.Remove(0, 1);
                }
            }

            // If the input is a decimal separator, we need to make sure one isn't already 
            // entered into the input string (we can't have two in a number!) so just 
            // ignore this if that's the case
            if (buttonContent == 
                ApplicationState.CurrentCulture.NumberFormat.NumberDecimalSeparator)
            {
                if (input.Length <= 0)
                {
                    b.Append('0');
                }

                bool containsDecimal = input.Contains(
                    ApplicationState.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (containsDecimal)
                {
                    return;
                }
            }

            if (b.Length < MaximumNumberOfInputDigits)
            {
                b.Append(buttonContent);

                this.UpdateUnitDisplayStrings();
            }
        }


        /// <summary>
        /// Updates the target unit value, and summary display field
        /// </summary>
        internal void UpdateUnitDisplayStrings()
        {
            StringBuilder b = this.ConversionSettings.UserInput;

            string input = b.ToString();
            this.AssignUnitValue(true, input);
            double value;
            if (double.TryParse(input, NumberStyles.Any, ApplicationState.CurrentCulture, out value))
            {
                this.ConvertAndDisplayTargetUnit(value);
            }
            else
            {
                if (b.Length == 0)
                {
                    this.AssignUnitValue(true, DefaultSourceValueText);
                }
                this.AssignUnitValue(false, String.Empty);
            }

            this.UpdateSummaryFromTextBlocks();
        }




        /// <summary>
        /// Converts from the user input data to the target unit and applies appropriate
        /// formatting to the converted value
        /// </summary>
        /// <param name="sourceUnitValue">Value to be converted to the target unit</param>
        private void ConvertAndDisplayTargetUnit(double sourceUnitValue)
        {
            double convertedValue = 0.0;
           
                convertedValue = CategoryInformation.Convert(
                  sourceUnitValue, this.ConversionSettings.SourceUnit, this.ConversionSettings.TargetUnit);

            string potentialConvertedText = string.Empty;

            // Regardless of value, just try and fit the text on the screen, if it works we don't
            // need to use scientific notation otherwise we do
            potentialConvertedText = 
                convertedValue.ToString(completeFormat, ApplicationState.CurrentCulture);

            if (potentialConvertedText.Length > MaximumNumberOfInputDigits)
            {
                const double StandardDisplayLarge = 1.0e11;
                // two less available spaces for decimal and zero
                const double StandardDisplaySmall = 1.0e-9; 
                const string ScientificNotationFormat = "0.{0}e-0";
                const string RegularNotationFormat = "0.{0}";

                string notationFormat = RegularNotationFormat;

                if (Math.Abs(convertedValue) >= StandardDisplayLarge || 
                    Math.Abs(convertedValue) <= StandardDisplaySmall)
                {
                    notationFormat = ScientificNotationFormat;
                }

                int numDecimalPlaces = potentialConvertedText.Length;
                string decimalSpecifier = new string('#', numDecimalPlaces);

                for (; numDecimalPlaces > 0 && 
                    potentialConvertedText.Length > MaximumNumberOfInputDigits; --numDecimalPlaces)
                {
                    // Strip off one of the #s, we need to lose some precision to fit.
                    decimalSpecifier = decimalSpecifier.Remove(0, 1);
                    string format = string.Format(CultureInfo.CurrentCulture , notationFormat, decimalSpecifier);
                    potentialConvertedText = convertedValue.ToString(format, ApplicationState.CurrentCulture);
                }
            }
            this.AssignUnitValue(false, potentialConvertedText);
        }


        /// <summary>
        /// Updates the summary conversion text UI element with the current source/
        /// target unit information
        /// </summary>
        private void UpdateSummaryFromTextBlocks()
        {
            string sourceValue = (this.UpperUnitValue.Length > 0)
                ? this.UpperUnitValue : "0";

            string targetValue = (this.LowerUnitValue.Length > 0)
                ? this.LowerUnitValue : "0";
                this.Summary = string.Format(
                    CultureInfo.CurrentCulture , 
                    "{0} {1} = {2} {3}", sourceValue, this.UpperUnitName,
                    targetValue, this.LowerUnitName);
        }

        /// <summary>
        /// Toggle which unit type is the source.
        /// </summary>
        internal void SwitchSourceTargetUnit()
        {
            this.SyncStateToAppState();
            // Reset the user input value to the current target unit value.
            ApplicationState.MainPageInformation.SourceUnitValue = this.LowerUnitValue;
            // Ensure we don't have a blank source unit value
            if (String.IsNullOrEmpty(ApplicationState.MainPageInformation.SourceUnitValue))
            {
                ApplicationState.MainPageInformation.SourceUnitValue = DefaultSourceValueText;
            }
            // Toggle the source unit state
            ApplicationState.MainPageInformation.SwapUnits(this.ConversionSettings.IsUpperUnitSource);
            this.ConversionSettings.RefreshStateFromAppState(this , true);
        }
    }
}
