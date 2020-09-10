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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Applications.UnitConverter.Helpers;
using Microsoft.Phone.Applications.UnitConverter.Model;
using Microsoft.Phone.Applications.UnitConverter.Resources;
using Microsoft.Phone.Applications.UnitConverter.View;
using Microsoft.Phone.Controls;

namespace Microsoft.Phone.Applications.UnitConverter.ViewModel
{
    /// <summary>
    /// View model for the category selection page
    /// </summary>
    public class CategorySelectionViewModel : INotifyPropertyChanged
    {

        #region Bindable Propeties

        /// <summary>
        /// Backing store for Done Button Enabled state
        /// </summary>
        private bool isButtonDoneEnabled;

        /// <summary>
        /// Name of the category to display
        /// </summary>
        public bool IsButtonDoneEnabled
        {
            get { return this.isButtonDoneEnabled; }

            set
            {
                this.isButtonDoneEnabled = value;
                this.OnPropertyChanged("IsButtonDoneEnabled");
            }
        }

        /// <summary>
        /// Backing store for Done Button Enabled state
        /// </summary>
        private bool isButtonCancelEnabled;

        /// <summary>
        /// Name of the category to display
        /// </summary>
        public bool IsButtonCancelEnabled
        {
            get { return this.isButtonCancelEnabled; }

            set
            {
                this.isButtonCancelEnabled = value;
                this.OnPropertyChanged("IsButtonCancelEnabled");
            }
        }

        /// <summary>
        /// Backing store for the Pivot Item selection index
        /// </summary>
        private Int32 selectedDataIndex;

        /// <summary>
        /// Name of the category to display
        /// </summary>
        public Int32 SelectedDataIndex
        {
            get { return this.selectedDataIndex; }

            set
            {
                this.selectedDataIndex = value;
                this.OnPropertyChanged("SelectedDataIndex");
            }
        }


        #endregion Bindable Properties

        /// <summary>
        /// Controls  whether the page navigation buttons are enabled/disabled
        /// </summary>
        /// <value>True when we can allow page buttons to be enabled</value>
        internal bool AllowNavigation
        {
            set
            {
                this.IsButtonDoneEnabled = value;
                this.IsButtonCancelEnabled = value;
            }
        }
      

        /// <summary>
        /// The last selected favorite by the user.
        /// </summary>
        internal FavoriteData lastSelectedFavorite;


        #region INotifyPropertyChanged Members

        /// <summary>
        /// Standard pattern for data binding and notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Notify subscribers of a change in the property
        /// </summary>
        /// <param name="propertyName">Property Name being changed</param>
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
        public CategorySelectionViewModel()
        {
        }


        /// <summary>
        /// Setup the required data object to be able to data bind to the list views in the 
        /// pivot control
        /// </summary>
        /// <param name="view">View </param>
        internal static void InitializeCategoryBinding(CategorySelection view)
        {
            AddPivotItems(view);
        }


        /// <summary>
        /// Adds the pivot items for the unit categories.
        /// Create a new pivot item for each category, and a user control that will have
        /// the required two list boxes for each pivot item.
        /// </summary>
        /// <param name="view">The view.</param>
        private static void AddPivotItems(CategorySelection view)
        {
            foreach (CategoryInformation category in ApplicationState.SupportedConversions)
            {
                PivotItem p = new PivotItem();
                TwoListBoxes l = new TwoListBoxes();
                l.Name = "pivotItem" + category.CategoryLocalized;
                l.FromSelectionChanged += new SelectionChangedEventHandler(view.OnFromSelectionChanged);
                l.ToSelectionChanged += new SelectionChangedEventHandler(view.OnToSelectionChanged);
                p.Header = category.CategoryLocalized;
                p.Content = l;
                view.pivot.Items.Add(p);
                // Store the TwoListBoxes object in the category for later access
                category.PivotUnitSelect = l;
                category.PivotUnitSelect.toListView.ItemsSource = null;
                category.PivotUnitSelect.fromListView.ItemsSource = null;
            }
        }

        /// <summary>
        /// Gets the index of the category pivot that we should select
        /// </summary>
        /// <param name="pivot">The pivot.</param>
        /// <returns></returns>
        internal static int GetCategoryPivotIndex(Pivot pivot)
        {
            ItemCollection headerItems = pivot.Items;
            List<PivotItem> headerList = new List<PivotItem>();

            foreach (object h in headerItems)
            {
                headerList.Add((PivotItem)h);
            }

            PivotItem itemToSelect = headerList.SingleOrDefault(
                m => m.Header.ToString() == ApplicationState.MainPageInformation.Category);
            Int32 headerIndex = itemToSelect != null ? headerList.IndexOf(itemToSelect) : 1;
            return headerIndex;
        }

        /// <summary>
        /// Processes the pivot selection change event. We need to check if have 
        /// the list boxes in the new pivot page data bound. If not, we need to update
        /// </summary>
        /// <param name="pivotItem">The pivot item.</param>
        internal void ProcessPivotSelectionChange(
            PivotItem pivotItem ,
             ListBox favoritesListView ,
            TextBlock favoritesNoItems)
        {
            ApplicationState.CategoryPageInformation.Category = pivotItem.Header.ToString();
            ApplicationState.CategoryPageInformation.IsFavorite =
              string.Compare(ApplicationState.CategoryPageInformation.Category,
              Strings.PivotHeader_Favorites, StringComparison.OrdinalIgnoreCase) == 0;
            //Update the data binding if needed for this page
            if (!ApplicationState.CategoryPageInformation.IsFavorite)
            {
                ApplicationState.UnitCategoryAccess[pivotItem.Header.ToString()].
                    ConfigureItemsSourceBinding(true);
            }
            else
            {
                this.UpdateFavoriteList(favoritesListView, favoritesNoItems);
            }
        }


        /// <summary>
        /// Update the selections from the main application page current settings
        /// </summary>
        internal static void SyncStateToMainPageState()
        {
            if (ApplicationState.CategoryPageInformation == null)
            {
                ApplicationState.CategoryPageInformation = new CategoryPageState();
                ApplicationState.CategoryPageInformation.Apply = false;
            }
            MainPageState m = ApplicationState.MainPageInformation;
            ApplicationState.CategoryPageInformation.
                UpdateNames(m.Category, m.SourceUnitName, m.TargetUnitName);
        }


        /// <summary>
        /// Syncs the state of selections on this page  to the main application state 
        /// objetc. THis will be used for either Tombstone support, or returning to the main 
        /// application page
        /// </summary>
        /// <param name="pivotIndex">Index of the page in the pivot control</param>
        internal void SyncStateToAppState(Int32 pivotIndex)
        {
            CategoryPageState c = ApplicationState.CategoryPageInformation;
            ApplicationState.CategoryPageInformation.PivotSelectedIndex = pivotIndex;
            // We may have an invalid unit category if we chose a favorite and then deleted 
            // it, in which case the user has not made a selection. We will set the 
            // apply flag to false so that the main page will not try to apply the settings
            // from this page.
            if (!String.IsNullOrEmpty(c.Category))
            {
                // Favorite category is not in the list of supported categories, because
                // it is not in the XML file. 
                if (ApplicationState.UnitCategoryAccess.ContainsKey(c.Category))
                {
                    // Non favorite category selection
                    CategoryInformation category = ApplicationState.UnitCategoryAccess[c.Category];
                    // We only set the selection information back on the app for use on the main 
                    // page if it's valid. The user must have chosen an item for the to and from
                    // unit list on the same category.
                    // Our category is only updated once we've made a selection
                    // in both the to/from list boxes in order 
                    // for the main page to update the conversion information
                    if (!(category.IsUnit(c.SourceUnitName) &&  category.IsUnit(c.TargetUnitName)))
                    {
                        // The unit selections are not both valid. Don't apply any change
                        c.Apply = false;
                    }
                }
                else
                {
                    // Favorite category case
                    c.Apply = this.lastSelectedFavorite != null ? true : false;
                }
            }
            else
            {
                c.Apply = false;
            }
        }


        /// <summary>
        /// Restores the user selections. This can be used when we are navigating from 
        /// the main page, or are returning from a tombstone operation.
        /// </summary>
        internal static void RestoreUserSelections()
        {

            CategoryPageState p = ApplicationState.CategoryPageInformation;
            // If we are on the favorite tab, we handle the data binding differently
            if (p.IsFavorite)
            {
                return;
            }
            // In case the user does not make any changes or selections, we need to have the data
            // to send back to the main page, so we select the current conversion units
            CategoryInformation c = ApplicationState.UnitCategoryAccess[p.Category];
            try
            {

                if ((p.SourceUnitName != null) && (c.PivotUnitSelect.fromListView.ItemsSource != null))
                {
                    c.PivotUnitSelect.fromListView.SelectedItem = c.FindUnit(p.SourceUnitName);
                }
                else
                {
                    c.PivotUnitSelect.fromListView.SelectedIndex = -1;
                }

                if ((p.TargetUnitName != null) && (c.PivotUnitSelect.toListView.ItemsSource != null))
                {
                    c.PivotUnitSelect.toListView.SelectedItem = c.FindUnit(p.TargetUnitName);
                }
                else
                {
                    c.PivotUnitSelect.toListView.SelectedIndex = -1;
                }
            }
            catch (Exception e)
            {
                ApplicationState.ErrorLog.Add(new ErrorLog("RestoreUserSelections", e.Message));
                c.PivotUnitSelect.fromListView.SelectedIndex = -1;
                c.PivotUnitSelect.toListView.SelectedIndex = -1;
            }
        }
       
        /// <summary>
        /// Clear the list of favorites, remove the selected item from isolated storage,
        /// and remove installed event handlers
        /// </summary>
        internal void DeleteSelectedFavorite()
        {
            // Remove favorite and store.
            FavoriteCollection favCollection = ApplicationState.Favorites;
            favCollection.Remove(this.lastSelectedFavorite);
            FavoriteCollection.SaveToFile(favCollection);
            this.lastSelectedFavorite = null;
        }

        /// <summary>
        /// Update the favorite list, attaching all required event handlers.
        /// We will load favorites from isolated storage, on the state of the 
        /// input parameter
        /// </summary>
        /// <param name="favoritesListView">List view for the favorites</param>
        /// <param name="favoritesNoItems">Text block for the favorites</param>
        internal void UpdateFavoriteList(
            ListBox favoritesListView ,
            TextBlock favoritesNoItems)
        {
            FavoriteCollection favCollection = ApplicationState.Favorites == null ?
                favCollection = FavoriteCollection.LoadFromFile() : ApplicationState.Favorites;

            if (favCollection.Count > 0)
            {
                // show listbox, hide no favorites label
                favoritesListView.Visibility = Visibility.Visible;
                favoritesNoItems.Visibility = Visibility.Collapsed;
                favoritesListView.ItemsSource = favCollection;
            }
            else
            {
                // hide listbox, show no favorites label
                favoritesListView.Visibility = Visibility.Collapsed;
                favoritesNoItems.Visibility = Visibility.Visible;
            }

            // We need to instal these handlers in the order below to be sure they don't get 
            // eaten by the ListView control
            this.lastSelectedFavorite = null;
        }

        /// <summary>
        /// Processes the favorite selection changed.
        /// </summary>
        /// <param name="e">Event data.</param>
        internal void ProcessFavoriteSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                FavoriteData favorite = e.AddedItems[0] as FavoriteData;

                ApplicationState.CategoryPageInformation.UpdateNames(
                    favorite.Category, favorite.SourceUnitName, favorite.TargetUnitName);
                this.lastSelectedFavorite = favorite;
            }
        }
    }
}
