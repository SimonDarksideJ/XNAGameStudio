// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace Microsoft.Phone.Applications.Common.Controls
{
    /// <summary>
    /// Represents a pop-up menu that enables any control 
    /// to expose functionality that is specific to the context of the control.
    /// All menu items are expected to be frameworkelement (currently no support for container override)
    /// All menu items should have height specified. Otherwise context menu will assume a height value
    /// TODO: the context menu is not orientation aware. The reason is that the popup is not in visual tree
    /// </summary>
    [TemplatePart(Name = MenuContainer, Type = typeof(FrameworkElement))]
    [TemplateVisualState(Name = VisibleState, GroupName = CommonStates)]
    [TemplateVisualState(Name = InvisibleState, GroupName = CommonStates)]
    public class ContextMenu : ItemsControl
    {
        const string MenuContainer = "MenuContainer";
        const string CommonStates = "CommonStates";
        const string VisibleState = "Visible";
        const string InvisibleState = "Invisible";

        /// <summary>
        /// Default appbar height
        /// </summary>
        const long DefaultAppBarHeight = 32;

        /// <summary>
        /// The finger movement distance in pixel that will cancel the context menu
        /// </summary>
        const int ContextMenuCancelMovement = 5;

        #region Private members

        /// <summary>
        /// Stores a reference to the root.
        /// </summary>
        private PhoneApplicationPage parentPage;

        /// <summary>
        /// Stores a reference to the menu.
        /// </summary>
        private FrameworkElement menuContainer;

        /// <summary>
        /// Stores a reference to the root.
        /// </summary>
        private FrameworkElement rootVisual;

        /// <summary>
        /// Stores a reference to the object that owns the ContextMenu.
        /// </summary>
        private FrameworkElement owner;

        /// <summary>
        /// Stores a reference to the current Popup.
        /// </summary>
        private Popup popup;

        /// <summary>
        /// Stores a reference to the current overlay.
        /// </summary>
        private Panel overlay;

        /// <summary>
        /// Stores a reference to the tap position.
        /// </summary>
        private Point fingerPosition;

        /// <summary>
        /// Stores a reference to the current Popup alignment point.
        /// </summary>
        private double menuPosition;

        /// <summary>
        /// Height of the menu (calculated at runtime)
        /// </summary>
        private double menuHeight;

        /// <summary>
        /// Timer for tap and hold
        /// </summary>
        private DispatcherTimer tapAndHoldTimer;

        /// <summary>
        /// Stores the last selected index if Owner is a ListBox
        /// </summary>
        private int lastSelectedIndex;

        /// <summary>
        /// The FrameworkElement at touch point
        /// The owner element for non-ListBox 
        /// ListBoxItem if owner is ListBox
        /// </summary>        
        private FrameworkElement lastSelectedItem;

        /// <summary>
        /// Indicates template applied to the control
        /// </summary>
        private bool isTemplateApplied;

        /// <summary>
        /// Deferred actions till template applied 
        /// used when first time the menu is shown
        /// </summary>
        private Queue<Action> afterTemplateApplied;

        /// <summary>
        /// Indicates whether menu position changed due to screen size
        /// </summary>
        private bool menuPositionChanged;

        /// <summary>
        /// Indicates whether appbar visibilty is changed or not
        /// </summary>
        private bool appBarVisibilityChanged;

        /// <summary>
        /// Animation when menu is shown
        /// </summary>
        private Storyboard showMenuAnimation;

        #endregion

        #region Constructors/Events/Properties

        /// <summary>
        /// Initializes a new instance of the ContextMenu class.
        /// </summary>
        public ContextMenu()
        {
            DefaultStyleKey = typeof(ContextMenu);

            // Hook LayoutUpdated to get access to RootVisual.
            SizeChanged += OnSizeChanged;
            LayoutUpdated += OnLayoutUpdated;

            // Setup the timer for tap and hold
            tapAndHoldTimer = new DispatcherTimer { Interval = TapAndHoldDuration };
            tapAndHoldTimer.Tick += OnTapAndHoldTimerTick;

            // By default hide the appbar on show
            ShouldHideAppBarWhenVisible = true;

            // Intialize the last selected index 
            LastSelectedIndex = -1;

            // Initialize the queue
            afterTemplateApplied = new Queue<Action>();
        }

        /// <summary>
        /// This event is triggered right when the context begins to show up
        /// </summary>
        public event EventHandler Opening;

        /// <summary>
        /// This event is triggered right when the context begins to go away
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Gets or sets the owning object for the ContextMenu.
        /// </summary>
        public FrameworkElement Owner
        {
            get { return owner; }
            internal set
            {
                // Detach from previous owner
                DetachFromOwner();
                if (value != null)
                {
                    // Set the new owner and attach to it
                    owner = value;
                    AttachToOwner();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value whether to style menu items or not
        /// Only style TextBlock elements only
        /// </summary>
        public bool ShouldApplyDefaultItemStyle { get; set; }

        /// <summary>
        /// Gets or sets a value whether to hide the app bar 
        /// when context menu is visible, default is true
        /// if you choose not to the tapping on appbar should dismiss the context menu 
        /// but does not perform the appbar action until another tap on the app bar.
        /// </summary>
        public bool ShouldHideAppBarWhenVisible { get; set; }

        /// <summary>
        /// Gets whether appbar is visible
        /// Sets the appbar visibility
        /// </summary>
        internal bool IsAppBarVisible
        {
            get
            {
                if (ParentPage != null
                    && ParentPage.ApplicationBar != null)
                {
                    return ParentPage.ApplicationBar.IsVisible;
                }

                return true;
            }
            set
            {
                if (ParentPage != null
                    && ParentPage.ApplicationBar != null)
                {
                    if (value)
                    {
                        if (appBarVisibilityChanged)
                        {
                            ParentPage.ApplicationBar.IsVisible = true;
                            appBarVisibilityChanged = false;
                        }
                    }
                    else
                    {
                        if (ShouldHideAppBarWhenVisible
                            && ParentPage.ApplicationBar.IsVisible)
                        {
                            ParentPage.ApplicationBar.IsVisible = false;
                            appBarVisibilityChanged = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the parent page
        /// </summary>
        internal PhoneApplicationPage ParentPage
        {
            get
            {
                if (parentPage == null)
                {
                    parentPage = ((PhoneApplicationFrame)RootVisual).Content as PhoneApplicationPage;
                }

                return parentPage;
            }
        }

        /// <summary>
        /// Gets the root visual
        /// </summary>
        internal FrameworkElement RootVisual
        {
            get
            {
                if (rootVisual == null)
                {
                    rootVisual = Application.Current.RootVisual as FrameworkElement;
                }

                return rootVisual;
            }
        }

        /// <summary>
        /// Gets the client area height
        /// </summary>
        internal double PageHeight
        {
            get
            {
                double pageHeight = RootVisual.ActualHeight;
                if (!ShouldHideAppBarWhenVisible
                    && IsAppBarVisible)
                {
                    pageHeight -= DefaultAppBarHeight;
                }

                return pageHeight;
            }
        }

        /// <summary>
        /// Gets the popup
        /// </summary>
        internal Popup Popup
        {
            get
            {
                InitializePopup();
                return popup;
            }
        }

        /// <summary>
        /// Gets the last selected index if Owner is a ListBox
        /// Make sure you check for -1 before acting on it
        /// </summary>
        public int LastSelectedIndex
        {
            get { return lastSelectedIndex; }
            private set { lastSelectedIndex = value; }
        }

        /// <summary>
        /// Gets the FrameworkElement at touch point
        /// The owner element for non-ListBox 
        /// ListBoxItem if owner is ListBox
        /// Make sure you check for null before acting on it
        /// </summary>
        public FrameworkElement LastSelectedItem
        {
            get { return lastSelectedItem; }
            private set
            {
                if (lastSelectedItem != value)
                {
                    // Unhook event handlers on old item
                    if (lastSelectedItem != null)
                    {
                        FrameworkElement lastItemToUnhookInDispatchHandler = lastSelectedItem;

                        // Unhook the event handler                
                        Dispatcher.BeginInvoke(() =>
                        {
                            if (lastItemToUnhookInDispatchHandler != null)
                            {
                                lastItemToUnhookInDispatchHandler.ManipulationCompleted -= OnItemManipulationCompleted;
                            }
                        });
                    }

                    lastSelectedItem = value;

                    if (lastSelectedItem == null)
                    {
                        // Remove data context since no item is selected
                        DataContext = null;
                    }
                    else
                    {
                        // Hookup event handlers on new item

                        // Temporarily handle manipulationcompleted for the listboxitem
                        LastSelectedItem.AddHandler(ManipulationCompletedEvent,
                            new EventHandler<ManipulationCompletedEventArgs>(OnItemManipulationCompleted),
                            true);

                        // Set popup data context to data context from selected item so menu items can bind to 
                        DataContext = lastSelectedItem.DataContext;
                    }

                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether context menu is open or not
        /// </summary>
        public bool IsOpen
        {
            get { return popup != null && popup.IsOpen; }
        }

        /// <summary>
        /// Gets or sets the duartion of tap and hold.
        /// </summary>
        public TimeSpan TapAndHoldDuration
        {
            get { return (TimeSpan)GetValue(TapAndHoldDurationProperty); }
            set { SetValue(TapAndHoldDurationProperty, value); }
        }

        /// <summary>
        /// TapAndHoldDuration dependency property.
        /// </summary>
        public static readonly DependencyProperty TapAndHoldDurationProperty =
            DependencyProperty.Register(
                "TapAndHoldDuration",
                typeof(TimeSpan),
                typeof(ContextMenu),
                new PropertyMetadata(TimeSpan.FromMilliseconds(500)
                    , OnTapAndHoldDurationChanged));

        /// <summary>
        /// Handles changes to the ContextMenu DependencyProperty.
        /// </summary>
        /// <param name="o">DependencyObject that changed.</param>
        /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
        private static void OnTapAndHoldDurationChanged(DependencyObject o
            , DependencyPropertyChangedEventArgs e)
        {
            var menu = o as ContextMenu;
            menu.tapAndHoldTimer.Interval = (TimeSpan)e.NewValue;
        }

        #endregion

        #region Static members

        /// <summary>
        /// The context menu in foreground or last one visible
        /// </summary>
        private static ContextMenu current;

        /// <summary>
        /// Gets the current context menu in foreground
        /// </summary>
        public static ContextMenu Current
        {
            get
            {
                return current;
            }

            private set
            {
                if (current != null
                    && current != value)
                {
                    current.Close();
                }

                current = value;
            }
        }

        #endregion

        /// <summary>
        /// Explicitly closes the context menu
        /// </summary>
        public void Close()
        {
            // Stop the timer of already initialized
            if (tapAndHoldTimer != null)
            {
                tapAndHoldTimer.Stop();
            }

            // Hide the popup if visible
            Hide();
        }

        /// <summary>
        /// Back key handling
        /// NOTE: Context menu doesn't get notified when back key is pressed
        /// So the app developer need to call it explicity from the page back key handler
        /// </summary>
        /// <param name="e">Cancel event args</param>
        public void OnBackKeyPress(CancelEventArgs e)
        {
            if (IsOpen
                || tapAndHoldTimer.IsEnabled)
            {
                // Stops the timer if yet to open
                // If open the hides it
                Close();

                e.Cancel = true;
            }
        }

        /// <summary>
        /// Builds the visual tree for the control 
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            menuContainer = GetTemplateChild(MenuContainer) as FrameworkElement;
            if (menuContainer != null)
            {
                if (RootVisual != null)
                {
                    menuContainer.Width = RootVisual.ActualWidth;
                }

                showMenuAnimation = menuContainer.FindName("storyBoard") as Storyboard;
                if (showMenuAnimation != null)
                {
                    showMenuAnimation.Completed += StoryBoard_Completed;
                }

                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] is TextBlock && ShouldApplyDefaultItemStyle)
                    {
                        TextBlock t = Items[i] as TextBlock;
                        int pos = Items.IndexOf(t);
                        Items.Remove(t);
                        Border border = new Border();
                        border.BorderThickness = new Thickness(0);
                        border.Child = t;
                        border.Margin = t.Margin;
                        t.Margin = new Thickness(0);
                        Items.Insert(pos, border);
                    }
                }
            }

            ChangeVisualState(false);
            isTemplateApplied = true;

            while (afterTemplateApplied.Count > 0)
            {
                var action = afterTemplateApplied.Dequeue();
                action.Invoke();
            }
        }

        /// <summary>
        /// Override the base implementation to calculate initial menu height
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var feItem = item as FrameworkElement;
                    if (feItem != null)
                    {
                        if (double.IsNaN(feItem.Height)
                            || double.IsInfinity(feItem.Height))
                        {
                            feItem.Height = 64;
                        }

                        FontFamily fontFamily = Resources["PhoneFontFamilySemiLight"] as FontFamily;
                        Brush foregroundBrush = Resources["PhoneContrastForegroundBrush"] as Brush;
                        double fontSize = 0;
                        if (Resources["PhoneFontSizeLarge"] != null)
                        {
                            fontSize = (double)Resources["PhoneFontSizeLarge"];
                        }

                        // default styling
                        if (ShouldApplyDefaultItemStyle
                            && feItem is TextBlock)
                        {
                            TextBlock t = (TextBlock)feItem;
                            if (fontFamily != null)
                            {
                                t.FontFamily = fontFamily;
                            }

                            if (fontSize > 0)
                            {
                                t.FontSize = fontSize;
                            }

                            if (foregroundBrush != null)
                            {
                                t.Foreground = foregroundBrush;
                            }

                            t.VerticalAlignment = VerticalAlignment.Center;

                            double left = 23, top = 0, right = 23, bottom = 0;
                            if (e.NewStartingIndex == 0)
                            {
                                top = 29;
                            }

                            // last item
                            if (e.NewStartingIndex == Items.Count - 1)
                            {
                                bottom = 19;
                            }

                            t.Margin = new Thickness(left, top, right, bottom);
                        }
                    }
                }

                CalculateMenuHeight();
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the StoryBoard.Completed event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void StoryBoard_Completed(object sender, EventArgs e)
        {
            if (menuPositionChanged)
            {
                double actualMenuPosition = menuPosition;
                if (actualMenuPosition - menuHeight >= 0)
                {
                    actualMenuPosition -= menuHeight;
                }
                else
                {
                    actualMenuPosition = fingerPosition.Y;
                    if (actualMenuPosition + menuHeight > PageHeight)
                    {
                        actualMenuPosition = PageHeight - menuHeight;
                    }
                }

                Canvas.SetTop(this, actualMenuPosition);
            }
            if (IsOpen)
            {
                ChangeOwnerVisual(true);
            }
        }

        /// <summary>
        /// Handles the LayoutUpdated event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            // Remove the layout event handler asynchronously 
            Dispatcher.BeginInvoke(() => { LayoutUpdated -= OnLayoutUpdated; });

            InitializePopup();
        }

        /// <summary>
        /// Handles the size change events for the control.
        /// This will set the final height of the menu
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            menuHeight = e.NewSize.Height;
        }

        /// <summary>
        /// If the tap and hold timer ticks, that means the user pressed 
        /// and held long enough to bring up the menu
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnTapAndHoldTimerTick(object sender, EventArgs e)
        {
            tapAndHoldTimer.Stop();
            Show();
        }

        /// <summary>
        /// Handles the MouseButtonDown events for the overlay.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOverlayMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Hide();
        }

        /// <summary>
        /// Event fired when the owner control is unloaded
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOwnerUnloaded(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Event fired as the user first presses down. 
        /// Starts the press and hold timer
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOwnerManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            double animationOrigin = 0.5;
            menuPositionChanged = false;

            // Because the Loaded and Layout are not reliable. 
            // Those events are not garenteed to happen before the first manipulation
            InitializePopup();

            fingerPosition = e.ManipulationContainer.TransformToVisual(RootVisual).Transform(e.ManipulationOrigin);

            // Calculate animation origin in X coord
            animationOrigin = fingerPosition.X / RootVisual.ActualWidth;

            FrameworkElement newSelectedItem = null;
            int newSelectedIndex = -1;

            var listBox = Owner as ListBox;
            if (listBox == null)
            {
                // For non-listbox controls
                // Assign last selected item as the owner
                newSelectedItem = Owner;
            }
            else
            {
                // For listbox, use the VisualTreeHelper to look for the ListBoxItem 
                // that is at the point of the gesture
                var listBoxItems = VisualTreeHelper.FindElementsInHostCoordinates(
                    listBox.TransformToVisual(RootVisual).Transform(e.ManipulationOrigin)
                    , listBox).OfType<ListBoxItem>();

                if (listBoxItems.Count() > 0)
                {
                    newSelectedItem = listBoxItems.First();

                    // Save the index for the listbox
                    newSelectedIndex = listBox.ItemContainerGenerator
                                        .IndexFromContainer(newSelectedItem);
                }
            }

            // use Owner's data context as data context: allows binding context menu props to SelectedItem's context
            if (newSelectedItem != null)
            {
                DataContext = newSelectedItem.DataContext;
            }

            // Ability to disable the context menu
            if (!IsEnabled || Visibility == Visibility.Collapsed)
            {
                DataContext = null;
                return;
            }

            LastSelectedItem = newSelectedItem;
            LastSelectedIndex = newSelectedIndex;

            if (LastSelectedItem != null)
            {
                // Get the position on RootVisual coordinate
                var top = LastSelectedItem.TransformToVisual(RootVisual).Transform(new Point(0, 0)).Y;
                menuPosition = LastSelectedItem.ActualHeight + top;

                // Now make sure the menu fits on screen. 
                // If it is too low, put it above the finger instead of below it
                // Caution: menuheight is set thru SizeChanged event handler,
                // which occurs only after OnApplyTemplate or LayoutUpdated
                // so the first attempt to open the context menu might have value 0 
                if (menuPosition + menuHeight > PageHeight)
                {
                    menuPosition -= LastSelectedItem.ActualHeight;
                    menuPositionChanged = true;
                }

                ExecuteAftertemplateApplied(() =>
                {
                    CalculateMenuHeight();

                    Point renderTransformOrigin = menuContainer.RenderTransformOrigin;
                    renderTransformOrigin.X = animationOrigin;
                    renderTransformOrigin.Y = menuPositionChanged ? 1.0 : 0.0;
                    menuContainer.RenderTransformOrigin = renderTransformOrigin;

                    var heightAnim = menuContainer.FindName("heightAnim") as DiscreteObjectKeyFrame;
                    if (heightAnim != null)
                    {
                        heightAnim.Value = menuHeight;
                    }

                    var yScaleStart = menuContainer.FindName("YScaleStart") as EasingDoubleKeyFrame;
                    if (yScaleStart != null)
                    {
                        yScaleStart.Value = 1 / menuHeight;
                    }

                    var positionAnimY1 = menuContainer.FindName("PositionAnimY1") as DiscreteObjectKeyFrame;
                    var positionAnimY2 = menuContainer.FindName("PositionAnimY2") as DiscreteObjectKeyFrame;
                    if (positionAnimY1 != null)
                    {
                        positionAnimY1.Value = 0;
                    }
                    if (positionAnimY2 != null)
                    {
                        positionAnimY2.Value = 0;
                    }

                    if (menuPositionChanged)
                    {
                        if (positionAnimY1 != null)
                        {
                            positionAnimY1.Value = 0;
                        }
                        if (positionAnimY2 != null)
                        {
                            positionAnimY2.Value = -menuHeight;
                        }
                    }
                });

                // Start the timer - if only suitable visual element found
                tapAndHoldTimer.Start();
            }
        }

        /// <summary>
        /// Event fired when the user wiggles their finger after the initial press
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOwnerManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // If the context menu is open already, then complete this gesture
            // good to do for owner of type ListBox, Pivot, Panorama
            if (IsOpen == true)
            {
                e.Complete();
                e.Handled = true;
            }

            // If there is a non-negative drag or multi-finger, then cancel the timer
            if (Math.Abs(e.DeltaManipulation.Translation.X) != ContextMenuCancelMovement
                || Math.Abs(e.DeltaManipulation.Translation.Y) != ContextMenuCancelMovement
                || Math.Abs(e.DeltaManipulation.Scale.X) != 0
                || Math.Abs(e.DeltaManipulation.Scale.Y) != 0)
            {
                tapAndHoldTimer.Stop();
            }
        }

        /// <summary>
        /// Event fired when the user lifts their finger, 
        /// which will cancel the menu if they lifted really early
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOwnerManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // If the context menu is open already, then set the event to be handled
            // good to do for owner of type Button, CheckBox, RadioButton
            if (IsOpen == true)
            {
                e.Handled = true;
            }

            // Stop the timer if not already
            tapAndHoldTimer.Stop();
        }

        /// <summary>
        /// Event fired when the user lifts their finger from a listboxitem
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void OnItemManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            // If context menu is open then disable selection for the listbox
            if (IsOpen)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Raise the Opening event
        /// </summary>
        private void RaiseOpeningEvent()
        {
            var handler = Opening;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raise the Closing event
        /// </summary>
        private void RaiseClosingEvent()
        {
            var handler = Closing;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Calculate menu height at runtime
        /// </summary>
        private void CalculateMenuHeight()
        {
            double height = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                var feItem = Items[i] as FrameworkElement;
                if (feItem != null)
                {
                    if (double.IsNaN(feItem.Height)
                        || double.IsInfinity(feItem.Height))
                    {
                        feItem.Height = 64;
                    }

                    // default styling
                    if (ShouldApplyDefaultItemStyle
                        && feItem is TextBlock)
                    {
                        if (i == 0)
                        {
                            height += 29;
                        }

                        // last item
                        if (i == Items.Count - 1)
                        {
                            height += 19;
                        }
                    }
                    else
                    {
                        if (feItem.Margin.Top != 0)
                        {
                            height += feItem.Margin.Top;
                        }

                        if (feItem.Margin.Bottom != 0)
                        {
                            height += feItem.Margin.Bottom;
                        }
                    }

                    height += feItem.Height;
                }
            }

            menuHeight = height;
        }

        /// <summary>
        /// Initialize the popup control
        /// It is smart enough to handle multiple calls
        /// </summary>
        private void InitializePopup()
        {
            if (popup == null)
            {
                // Add overlay which is the size of RootVisual
                overlay = new Canvas
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Width = RootVisual.ActualWidth,
                    Height = RootVisual.ActualHeight
                };

                overlay.Children.Add(this);

                // Dismiss the context menu if clicked else where
                overlay.AddHandler(MouseLeftButtonUpEvent
                    , new MouseButtonEventHandler(OnOverlayMouseButtonUp)
                    , true);

                // Initialize popup to draw the context menu over all controls
                popup = new Popup { Child = overlay };

                // NOTE: Tried to add to visual tree, 
                // to make popup orientation aware, but did't work
                //afterTemplateApplied.Enqueue(() =>
                //                                 {
                //                                     var panel = FindPanel(Owner);
                //                                     if (panel != null)
                //                                     {
                //                                         panel.Children.Add(popup);
                //                                     }
                //                                 });
            }
        }

        /// <summary>
        /// Find the first Grid/StackPanel/Canvas above the element in the visual tree
        /// </summary>
        /// <param name="element">Visual element</param>
        /// <returns>Panel containing the element</returns>
        private Panel FindPanel(DependencyObject element)
        {
            if (element == null) return null;
            var panel = element as Panel;
            return panel ?? FindPanel(VisualTreeHelper.GetParent(element));
        }

        /// <summary>
        /// Changes the visual state of the control.
        /// </summary>
        /// <param name="show">Show or hide the menu</param>
        private void ChangeVisualState(bool show)
        {
            if (show)
            {
                VisualStateManager.GoToState(this, VisibleState, true);
            }
            else
            {
                VisualStateManager.GoToState(this, InvisibleState, true);
            }
        }

        /// <summary>
        /// Execute a action only after template is applied
        /// </summary>
        /// <param name="action">Custom action</param>
        private void ExecuteAftertemplateApplied(Action action)
        {
            if (isTemplateApplied)
            {
                // Perform the action immediately
                action.Invoke();
            }
            else
            {
                // Defer the action till template applied
                // If not deferred, animations will not happen
                // Animations are available only after OnApplyTemplate
                afterTemplateApplied.Enqueue(action);
            }
        }

        /// <summary>
        /// Shows the context menu.
        /// </summary>
        private void Show()
        {
            if (IsOpen == false)
            {
                // Apply tilt effect to context menu items - add Border as tiltable
                TiltEffect.TiltableItems.Add(typeof(Border));

                RaiseOpeningEvent();

                // Set this as the current context menu
                Current = this;

                // Update popup position
                if (overlay != null)
                {
                    TiltEffect.SetIsTiltEnabled(overlay, true);
                    Canvas.SetTop(this, menuPosition);
                }

                // Show the popup
                Popup.IsOpen = true;

                IsAppBarVisible = false;

                ExecuteAftertemplateApplied(() =>
                {
                    // Change visual state
                    ChangeVisualState(true);
                });
            }
        }

        /// <summary>
        /// Hides the context menu.
        /// </summary>
        private void Hide()
        {
            // If the animation is active, then skip to end
            // This will ensure the animation is not in some intermittent state when stopped abruptly
            if (showMenuAnimation != null
                && showMenuAnimation.GetCurrentState() == ClockState.Active)
            {
                showMenuAnimation.SkipToFill();
            }

            if (IsOpen == true)
            {
                // Remove Border from tiltable - to avoid unwanted effects to other visual
                TiltEffect.TiltableItems.Remove(typeof(Border));

                RaiseClosingEvent();

                // Hide the popup
                Popup.IsOpen = false;

                // If appbar visibilty was changed, then restore it
                IsAppBarVisible = true;

                ChangeOwnerVisual(false);
                ChangeVisualState(false);
            }

            LastSelectedItem = null;
            LastSelectedIndex = -1;
        }

        /// <summary>
        /// Attaches this context menu to a control. 
        /// </summary> 
        private void AttachToOwner()
        {
            var element = Owner;
            if (element == null) return;

            element.Unloaded += OnOwnerUnloaded;

            element.AddHandler(ManipulationStartedEvent,
                new EventHandler<ManipulationStartedEventArgs>(OnOwnerManipulationStarted),
                true);
            element.AddHandler(ManipulationDeltaEvent,
                new EventHandler<ManipulationDeltaEventArgs>(OnOwnerManipulationDelta),
                true);
            element.AddHandler(ManipulationCompletedEvent,
                new EventHandler<ManipulationCompletedEventArgs>(OnOwnerManipulationCompleted),
                true);
        }

        /// <summary>
        /// Detaches this context menu from the control.
        /// </summary>
        private void DetachFromOwner()
        {
            var element = Owner;
            if (element == null) return;

            // Close the menu if open
            Close();

            element.Unloaded -= OnOwnerUnloaded;

            element.ManipulationStarted -= OnOwnerManipulationStarted;
            element.ManipulationDelta -= OnOwnerManipulationDelta;
            element.ManipulationCompleted -= OnOwnerManipulationCompleted;
        }

        /// <summary>
        /// Hides all the non-selected items of the owner, when the context menu shows
        /// Restores the visibility of all the non-selected items of the owner, 
        /// when the context menu hides
        /// </summary>
        /// <param name="show">Menu shown or hidden</param>
        private void ChangeOwnerVisual(bool show)
        {
            var itemsControl = Owner as ItemsControl;
            if (itemsControl == null) return;
            foreach (var item in
                itemsControl.Items.Select(GetUnderlyingVisualElement)
                .Where(item => item != null && item != LastSelectedItem))
            {
                item.Opacity = show ? 0.2 : 1.0;
            }
        }

        /// <summary>
        /// Determines the visual element associated with the item
        /// </summary>
        /// <param name="item">Binding object</param>
        /// <returns>Visual element</returns>
        private UIElement GetUnderlyingVisualElement(object item)
        {
            var itemsControl = Owner as ItemsControl;
            if (itemsControl != null)
            {
                var obj = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
                return obj as UIElement;
            }

            return item as UIElement;
        }

        #endregion
    }
}
