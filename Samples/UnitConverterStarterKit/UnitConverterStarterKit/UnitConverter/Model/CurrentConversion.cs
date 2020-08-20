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
using System.Text;
using Microsoft.Phone.Applications.UnitConverter.ViewModel;
using Microsoft.Phone.Applications.UnitConverter.Helpers;

namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// Represents the current state of the conversion on the main application page
    /// </summary>
    public class CurrentConversion
    {

        /// <summary>
        /// String that holds the user's input as he types in the source unit
        /// values. Conversion are done for each character entered if possible.
        /// </summary>
        internal StringBuilder UserInput { get; set; }

        /// <summary>
        /// Current selected unit category, such as length, time, temp etc
        /// </summary>
        internal CategoryInformation CurrentCategory { get; private set; }


        /// <summary>
        /// Source unit type. Unit to convert from
        /// </summary>
        internal UnitInformation SourceUnit { get; set; }


        /// <summary>
        /// Targeet unit type. Unit to convert to
        /// </summary>
        internal UnitInformation TargetUnit { get; set; }


        /// <summary>
        /// True if the upper fields on the display are the source unit
        /// </summary>
        internal bool IsUpperUnitSource { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentConversion"/> class.
        /// </summary>
        public CurrentConversion()
        {
            this.UserInput = new StringBuilder();
            this.IsUpperUnitSource = true;
        }

        /// <summary>
        /// Initialize the category and source/target units
        /// </summary>
        /// <param name="category">unit category</param>
        /// <param name="sourceUnit">Source Unit </param>
        /// <param name="targetUnit">Target unit</param>
        public CurrentConversion(
            CategoryInformation category , 
            UnitInformation sourceUnit ,
            UnitInformation targetUnit )
        {
            this.UserInput = new StringBuilder();
            this.IsUpperUnitSource = true;
            this.CurrentCategory = category;
            this.SourceUnit = sourceUnit;
            this.TargetUnit = targetUnit;
        }

        /// <summary>
        /// Sets the default category and units.
        /// Used when the application first starts
        /// </summary>
        /// <param name="sourceUnitName">Name of the source unit.</param>
        /// <param name="targetUnitName">Name of the target unit.</param>
        internal void SetDefaultCategoryAndUnits(
            out string sourceUnitName,
            out string targetUnitName)
        {
            string categorySelection = ApplicationState.SupportedConversions[0].CategoryLocalized;
            this.CurrentCategory = ApplicationState.UnitCategoryAccess[categorySelection];
            MainPageState m = ApplicationState.MainPageInformation;
            m.IsUpperUnitSource = this.IsUpperUnitSource;
            if (this.IsUpperUnitSource)
            {
                sourceUnitName = this.CurrentCategory.Units[0].NameLocalized;
                targetUnitName = this.CurrentCategory.Units[1].NameLocalized;
            }
            else
            {
                sourceUnitName = this.CurrentCategory.Units[1].NameLocalized;
                targetUnitName = this.CurrentCategory.Units[0].NameLocalized;
            }

            this.SourceUnit = this.CurrentCategory.UnitAccess[sourceUnitName];
            this.TargetUnit = this.CurrentCategory.UnitAccess[targetUnitName];
        }
       

        /// <summary>
        /// Refreshes the state of the page from the application state or the category
        /// page state. This will not be called for initial application launch on 
        /// navigating to the page, but for either a activation, or returning from the 
        /// category page.
        /// 
        /// </summary>
        /// <param name="v">The view model object</param>
        /// <param name="forceUseOfAppState">When true, force use of the 
        /// application state instead of possibly using the category page settings</param>
        internal void RefreshStateFromAppState(
            MainPageViewModel v ,
            bool forceUseOfAppState)
        {
            MainPageState m = ApplicationState.MainPageInformation;
            try
            {
                if (!forceUseOfAppState && ApplicationState.CategoryPageInformation != null)
                {
                    // If we were on the category page, then this object should be 
                    // non null.
                    CategoryPageState c = ApplicationState.CategoryPageInformation;
                    if (c.Apply)
                    {
                        // Only update from the category page if appropriate
                        v.Category = c.Category;
                        this.IsUpperUnitSource = m.IsUpperUnitSource;
                        v.UpperUnitName =  c.SourceUnitName;
                        v.LowerUnitName =  c.TargetUnitName;
                        this.UserInput = new StringBuilder();
                        if (!String.IsNullOrEmpty(m.SourceUnitValue))
                        {
                            this.UserInput.Append(m.SourceUnitValue);
                        }
                        this.CurrentCategory = ApplicationState.UnitCategoryAccess[v.Category];
                        this.SourceUnit = this.CurrentCategory.UnitAccess[v.UpperUnitName];
                        this.TargetUnit = this.CurrentCategory.UnitAccess[v.LowerUnitName];
                        v.UpdateUnitDisplayStrings();
                        return;
                    }
                }
                // Restore the state from the application main page state. App tomb  
                // stoned from this page, and now we need to restore the state
                v.Category = m.Category;
                this.IsUpperUnitSource = m.IsUpperUnitSource;
                v.UpperUnitName =  m.SourceUnitName;
                v.LowerUnitName =  m.TargetUnitName;
                this.UserInput = new StringBuilder();
                if (!String.IsNullOrEmpty(m.SourceUnitValue))
                {
                    this.UserInput.Append(m.SourceUnitValue);
                }
                this.CurrentCategory = ApplicationState.UnitCategoryAccess[v.Category];
                this.SourceUnit = this.CurrentCategory.UnitAccess[v.UpperUnitName];
                this.TargetUnit = this.CurrentCategory.UnitAccess[v.LowerUnitName];
                v.UpdateUnitDisplayStrings();
            }
            catch (Exception e)
            {
                ApplicationState.ErrorLog.Add(new ErrorLog("RefreshStateFromAppState", e.Message));
            }
        }

        /// <summary>
        /// Syncs the state of this page to the central application state object.
        /// </summary>
        /// <param name="category">The current category.</param>
        /// <param name="upperUnitName">Name of the upper unit.</param>
        /// <param name="lowerUnitName">Name of the lower unit.</param>
        internal void SyncStateToAppState(
            string category,
            string upperUnitName,
            string lowerUnitName)
        {
            if (ApplicationState.MainPageInformation == null)
            {
                ApplicationState.MainPageInformation = new MainPageState();
            }
            MainPageState m = ApplicationState.MainPageInformation;
            m.IsUpperUnitSource = this.IsUpperUnitSource;
            m.SourceUnitValue = this.UserInput.ToString();
            m.UpdateNames(category,upperUnitName , lowerUnitName);
        }
    }
}
