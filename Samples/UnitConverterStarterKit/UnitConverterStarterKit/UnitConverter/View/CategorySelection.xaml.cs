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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Applications.UnitConverter.Helpers;
using Microsoft.Phone.Applications.UnitConverter.Model;
using Microsoft.Phone.Applications.UnitConverter.ViewModel;
using Microsoft.Phone.Controls;

namespace Microsoft.Phone.Applications.UnitConverter.View
{
    /// <summary>
    /// Category selection code behind
    /// </summary>
    public partial class CategorySelection : PageCommon
    {
        /// <summary>
        /// View model object
        /// </summary>
        internal CategorySelectionViewModel viewModel = new CategorySelectionViewModel();


        /// <summary>
        /// Background thread object
        /// </summary>
        private BackgroundWorker worker = new BackgroundWorker();

        /// <summary>
        /// Multiple thread access lock
        /// </summary>
        private object threadLock = new object();

        /// <summary>
        /// Set to true when the pivot control is being updated. 
        /// </summary>
        private bool pivotLayoutUpdate = false;

        /// <summary>
        /// Hold the requested pivot item index to be set to work around a pivot 
        /// control  issue when setting a non default pivot item index
        /// </summary>
        private int requestedPivotDataSelectionIndex = -1;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public CategorySelection()
        {
            this.DataContext = this.viewModel;
            this.worker.DoWork += new DoWorkEventHandler(DisplayPivot);
            InitializeComponent();
        }

        /// <summary>
        /// Called when user presses back key on the page.
        /// Close the context menu if it is open else go back
        /// </summary>
        /// <param name="e">Event Data</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            contextMenuPanel.OnBackKeyPress(e);
        }

        /// <summary>
        /// Pivot control has been loaded
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.pivot.Loaded -= new RoutedEventHandler(OnPivotLoaded);
                    // We don't want multiple calls to the Selection Changed event, so we enable it now
                    this.pivot.SelectionChanged += new SelectionChangedEventHandler(OnPivotSelectionChanged);
                    // Set Pivot item index now to avoid known issue in pivot control
                    this.viewModel.SelectedDataIndex = this.requestedPivotDataSelectionIndex;
                    CategorySelectionViewModel.RestoreUserSelections();
                }
                );
        }
       

        /// <summary>
        /// Event that indicates we have moved to a new pivot page
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e"> instance containing the event data.</param>
        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem pivotItem = e.AddedItems[0] as PivotItem;
            this.viewModel.ProcessPivotSelectionChange(pivotItem, this.favoritesListView,
                this.favoritesNoItems);

            /* If this is the first time this function is called, the pivot is hidden so 
             * that we will be able to navigate to the desired pivot
             * item before showing the pivot control.
             * However, the header animation does not always complete before the pivot is shown.
             * To compensate for this, if we are not going to the default pivot page, we will 
             * enable a layout event handler on the pivot that will be called as the header 
             * animation progresses. A Background thread will monitor the updates, and on the 
             * last update, will wait some time, and then display the pivot
             */
            if (this.pivot.Opacity != 1)
            {
                if (this.requestedPivotDataSelectionIndex != 1)
                {
                    Dispatcher.BeginInvoke(() =>
                        {
                            this.pivot.LayoutUpdated += new EventHandler(pivot_LayoutUpdated);
                            this.worker.RunWorkerAsync(null);
                        });
                }
                else
                {
                    this.pivot.Opacity = 1;
                }
            }
          }

        /// <summary>
        /// Event Handler called when the pivot control is being updated.
        /// This will be called during the Header manipulation events. A flag
        /// is set to indicate that the pivot control is still updating.
        /// </summary>
        /// <param name="sender">Standard pattern</param>
        /// <param name="e">Standard pattern</param>
        void pivot_LayoutUpdated(object sender, EventArgs e)
        {
            lock (this.threadLock)
            {
                this.pivotLayoutUpdate = true;
            }
        }

        /// <summary>
        /// Background thread process to check for when we should display the pivot.
        /// This is to work around a Pivot Control known issue where we can't set the PivotIndex
        /// to any value or we get an argument null exception.
        /// If we are navigating to a pivot item that is not the default, there can 
        /// be a delay in the header display animation. To make sure that the 
        /// animation has completed, we will check the state of the pivotLayoutUpdate flag.
        /// If it has been set, we clear the flag and wait to see if there will be 
        /// any further pivot updates. If not, we then will show the pivot control
        /// </summary>
        /// <param name="sender">worker thread object</param>
        /// <param name="e">Standard Pattern</param>
        private void DisplayPivot(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(125);
                lock (this.threadLock)
                {
                    if (this.pivotLayoutUpdate)
                    {
                        this.pivotLayoutUpdate = false;
                        continue;
                    }
                }
                Dispatcher.BeginInvoke(() =>
                    {
                        this.pivot.LayoutUpdated -= new EventHandler(pivot_LayoutUpdated);
                        this.pivot.Opacity = 1;
                    });
                break;
            }
        }

        
        /// <summary>
        /// Called when we are leaving the page.
        /// We clear the constructorCalled flag so that we will be able to 
        /// determine if we were tomb stoned or not if we receive an activation event
        /// </summary>
        /// <param name="e">Event Data</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.viewModel.SyncStateToAppState(this.viewModel.SelectedDataIndex);
            ApplicationState.AddAppObjects(true);
        }
        
        /// <summary>
        /// Called when we have a page to navigated event
        /// This event is raised whenever we visit the page. This can occur for the 
        /// following situations
        /// 1) Application Activation where we are tomb stoned, or we are still in memory
        ///    If we have been tomb stoned, we need to read objects from the application
        ///    service, and then perform any initialization on the restored objects.
        ///    If we received and activation event, but we were not tomb stoned, then 
        ///    we actually don't need to do anything except clear the main application 
        ///    state flag and exit. This is because all objects are still in memory.
        /// 2) We have come from the main page of the application.
        /// To work around a known issue in the Pivot control related to applying templates to 
        /// non default pivot items we can't set the Pivot Index in this function. 
        /// We therefore save the desired pivot index and we will set this later
        /// </summary>
        /// <param name="e">Event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApplicationState.ApplicationStartup == AppOpenState.None &&
                this.pivot.Items.Count <= 1)
            {
                // We have come from the main page of the application
                this.viewModel.AllowNavigation = false;
                this.PostNavigationToInitialize();
                CategorySelectionViewModel.SyncStateToMainPageState();
                this.requestedPivotDataSelectionIndex = 
                    CategorySelectionViewModel.GetCategoryPivotIndex(pivot);
            }
            else
            {
                // Activated event case
                if (!this.IsPageActivated)
                {
                    // We are returning to the application, but we were not tomb stoned.
                    ApplicationState.ApplicationStartup = AppOpenState.None;
                    return;
                }
                this.viewModel.AllowNavigation = false;
                ApplicationState.RetrieveAppObjects(true);
                this.PostNavigationToInitialize();
                this.requestedPivotDataSelectionIndex = 
                    ApplicationState.CategoryPageInformation.PivotSelectedIndex;
                if (this.requestedPivotDataSelectionIndex == 0)
                {
                    /* If the index is 0, we don't receive an event for the pivot item
                     * changing which will cause the pivot to not render correctly.
                     * We can set the index to 1 ( normal default) without causing any
                     * problem for the pivot control. We will then set the index to 
                     * 0 later and we should get the event to fire
                     */
                    this.viewModel.SelectedDataIndex = 1;
                }
            }

            // Reenabled buttons 
            this.viewModel.AllowNavigation = true;
            pivot.Opacity = 0;
            ApplicationState.ApplicationStartup = AppOpenState.None;
        }
        
        /// <summary>
        /// Post Navigation event initialization for the page.
        /// This should be called if we are constructing the page.
        /// </summary>
        private void PostNavigationToInitialize()
        {

            CategorySelectionViewModel.InitializeCategoryBinding(this);
            // Show the pivot control, since we hid it until we go the correct page
            pivot.Opacity = 1;
            this.viewModel.UpdateFavoriteList(this.favoritesListView, this.favoritesNoItems);
        }
        
        /// <summary>
        /// User wants to apply settings and have the main page of the application apply
        /// the changes
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void OnDoneClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationState.CategoryPageInformation.Apply = true;
            this.viewModel.AllowNavigation = !this.ReverseNavigate();
        }
        
        /// <summary>
        /// The user has canceled the settings. We should ignore the settings in the main page
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationState.CategoryPageInformation.Apply = false;
            this.viewModel.AllowNavigation = !this.ReverseNavigate();
        }
        
        /// <summary>
        /// Handle the from list view selection event for the convert from unit list box
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        public void OnFromSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.AddedItems.Count <= 0)
                {
                    return;
                }
                ApplicationState.CategoryPageInformation.SourceUnitName =
                    (e.AddedItems[0] as UnitInformation).NameLocalized;
            }
        }

        /// <summary>
        /// Handle the to list view selection event from the convert to unit list box
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The instance containing the event data.</param>
        public void OnToSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.AddedItems.Count <= 0)
                {
                    return;
                }
                ApplicationState.CategoryPageInformation.TargetUnitName =
                    (e.AddedItems[0] as UnitInformation).NameLocalized;
            }
        }

        /// <summary>
        /// Called when a favorites selection changes
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        private void OnFavoritesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (contextMenuPanel.IsOpen == false)
            {
                this.viewModel.ProcessFavoriteSelectionChanged(e);
            }
            else
            {
                /* There is a small race condition between the selected event and when 
                 * the context menu gets events and takes control.
                 * A problem can occur that if an item in the listbox is not selected,
                 * on the mouse up event, the item can get selected even if the context
                 * menu is up. This is not the desired behavior.
                 * This can also cause a current selected item to switch incorrectly.
                 * To work around this problem, we force the list box selection to either 
                 * be no selection if we did not have an item currently selected, or back
                 * to the correct item if one was already selected.
                 */
                ListBox lb = sender as ListBox;
                if (lb != null)
                {
                    if (this.viewModel.lastSelectedFavorite == null)
                    {
                        lb.SelectedIndex = -1;
                    }
                    else
                    {
                        lb.SelectedItem = this.viewModel.lastSelectedFavorite;
                    }
                }
            }
        }

        /// <summary>
        /// Handler for user taps on delete context menu
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Event data.</param>
        private void ContextMenuDelete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var contextFavoriteItem = favoritesListView.Items[contextMenuPanel.LastSelectedIndex];
            if (contextFavoriteItem != null)
            {
                this.viewModel.lastSelectedFavorite = contextFavoriteItem as FavoriteData;
                this.viewModel.DeleteSelectedFavorite();
                this.viewModel.UpdateFavoriteList(this.favoritesListView, this.favoritesNoItems);
            }
        }

    }
}
