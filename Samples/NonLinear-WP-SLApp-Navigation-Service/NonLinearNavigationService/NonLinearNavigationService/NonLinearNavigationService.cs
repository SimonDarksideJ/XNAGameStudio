// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;
using System.Windows;
using System.Diagnostics;

namespace WindowsPhoneRecipes
{
    /// <summary>
    /// 
    /// </summary>
	public class NonLinearNavigationService
	{
		#region Static instance management

		/// <summary>
		/// The single reference to the navigation provider
		/// </summary>
		public static NonLinearNavigationService Instance { get; private set; }

        /// <summary>
        /// Static initializer   
        /// </summary>
		static NonLinearNavigationService()
		{
            Instance = new NonLinearNavigationService { };
		}

        /// <summary>
        /// Off-limits private constructor since this is a singleton...
        /// </summary>
        private NonLinearNavigationService() { }
 
		#endregion

		#region Fields

        /// <summary>
        /// Frame control at the visual root of the SL application. 
        /// Provides easy access to the root frame of the Phone Application.
        /// The frame is used for all navigation operations. 
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        private PhoneApplicationFrame _AppRootFrame;

		/// <summary>
		/// Constant value for history serialization in application state
		/// </summary>
		private const string _HistoryStateKey = "NavigationProviderHistoryKey";

		/// <summary>
        /// List of visited pages in the application. persisted to application state automatically
		/// </summary>
        /// <TODO>Change history to stack?</TODO>
        private List<Uri> _History;

        /// <summary>
        /// saves the last uri on which we canceled the navigation to prevent from
        /// endless loop if user cancel navigation in one of the pages in the loop
        /// </summary>
        private Uri _LastCanceleddUri = null;

		/// <summary>
        /// True when recursively navigating back (navigating back in a loop towards the beginning of the loop)
        /// 
        /// </summary>
		//private bool _IsRecursiveBackNavigation;// { get; private set; }
        public bool IsRecursiveBackNavigation { get; private set; }

		/// <summary>
		/// URI of page currently being navigated back to
        /// While in recursively back navigation, this URI is the begining of the loop
		/// </summary>
		private Uri _LoopStartTargetPageUri;// { get; set; }

        private double OpacityBeforeRecursiveNav;//

        /// <summary>
        /// navigation helper to save curr page and target page
        /// </summary>
        private NavigationHelper _NavHelper;

		#endregion

        #region Init 

        /// <summary>
        /// Call to init this service
        /// 
        /// Note - service will be init only when the mail SL application root is ready
        /// following WP init pattern
        /// </summary>
        public void Initialize(PhoneApplicationFrame root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root is null", "root frame cant be null");
            }

            lock (Instance) 
            {
                //check if already initialized 
                if (_AppRootFrame == null)
                {
                    _AppRootFrame = root;

                    // Defers initialization until application initializes.
                    // This ensures that RootVisual is set.
                    _AppRootFrame.Navigated += new NavigatedEventHandler(root_Navigated);
                }
                //
                //else
                //    throw new InvalidOperationException("object already initialized");
            }
        }

        /// <summary>
        /// Binds to all navigation events 
        /// 
        /// This is the "real" CTOR of this class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void root_Navigated(object sender, NavigationEventArgs e)
        {
            //Debug.WriteLine("in NonLinear root_Navigated = " + _AppRootFrame.Source.ToString()); 
            
            _AppRootFrame.Navigated -= root_Navigated;

            _History = LoadServiceHistory();

            // set the current to the first page on which the navigation is used and created... 
            // unless returning from tombstone where the uri already exists...
            if (!_History.Contains(e.Uri))
            {
                _History.Add(_AppRootFrame.Source);
            }

            // bind to application's navigation event 
            _AppRootFrame.Navigated += new NavigatedEventHandler(NonLinearNavigationService_Navigated);
            _AppRootFrame.Navigating += new NavigatingCancelEventHandler(NonLinearNavigationService_Navigating);
            _AppRootFrame.FragmentNavigation += new FragmentNavigationEventHandler(NonLinearNavigationService_FragmentNavigation);
            _AppRootFrame.NavigationFailed += new NavigationFailedEventHandler(NonLinearNavigationService_NavigationFailed);
            _AppRootFrame.NavigationStopped += new NavigationStoppedEventHandler(NonLinearNavigationService_NavigationStopped);

            // set our history reference to the global property bag - state for tombstoning 
            PhoneApplicationService.Current.State[_HistoryStateKey] = _History;
        }
#endregion

        #region Navigation Logic

        /// <summary>
        /// When a navigation request is initiated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonLinearNavigationService_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            //Debug.WriteLine("in NonLinearNavigationService_Navigating - navigating to " + e.Uri);
            //Debug.WriteLine("--------------------");

            _NavHelper = new NavigationHelper()
            {
                CurrentUri = _AppRootFrame.CurrentSource,
                NavMode = e.NavigationMode,
                TargetUri = e.Uri
            };

            if (e.NavigationMode == NavigationMode.New)
            {
                if (_AppRootFrame.CurrentSource != e.Uri &&
                    _History.Contains(e.Uri))
                {
                    // this is our target uri - this is the begining of the loop
                    _LoopStartTargetPageUri = e.Uri;
                    // flag to be used by the PhoneNavigationPage.OnNavigatedTo
                    IsRecursiveBackNavigation = true;

                    // Save opacity before recursive back navigation
                    OpacityBeforeRecursiveNav = _AppRootFrame.Opacity;
                    // make the rootframe and all it children (the pages) transperent 
                    // so no flick will be noticable during recursive nav back
                    _AppRootFrame.Opacity = 0;

                    // Cancel the user gen navigation to override with our
                    // recursive back navigation
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// This is corresponding to PhoneApplicationPage.NavigatedTo()
        /// entering a page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonLinearNavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            //Debug.WriteLine("in NonLinearNavigationService_Navigating - Navigated to " + e.Uri);
            //Debug.WriteLine("--------------------");

            switch (_NavHelper.NavMode)
            {
                case NavigationMode.Back:
                    // remove previous page from stack
                    _History.Remove(_NavHelper.CurrentUri);
                    
                    // if in recursive back mode automatic nav back
                    if (IsRecursiveBackNavigation == true)
                    {
                        if (_LoopStartTargetPageUri != e.Uri)
                        {
                            _AppRootFrame.GoBack();
                        }
                        else
                        {
                            EndRecursiveBackNavigation();
                            _LastCanceleddUri = null;
                        }
                    }

                    break;

                case NavigationMode.New:
                    if (!_NavHelper.TargetUri.ToString().Equals("app://external/") &&
                                      !_History.Contains(_NavHelper.TargetUri))
                    {
                        _History.Add(_NavHelper.TargetUri);
                    }
                   
                    break;

                   
                case NavigationMode.Forward:
                    break;
                case NavigationMode.Refresh:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// handle when navigation is canceled (e.Cancel=true) as we do 
        /// when recursivally navigating back. 
        ///
        /// Allow only one cancelation during recursive back, or else never stop looping back...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonLinearNavigationService_NavigationStopped(object sender, NavigationEventArgs e)
        {
            //Debug.WriteLine("in Nav Stop");

            if (IsRecursiveBackNavigation == true)
            {
                // first and only navigation cancel is performed by 
                // the NonLinearNavigatonServces 
                if (_LastCanceleddUri == null)
                {
                    _LastCanceleddUri = _AppRootFrame.CurrentSource;
                    _AppRootFrame.GoBack();
                }
                // the "user" canceled the navigation either in the back or navigating to method in 
                // one of the application's pages. We need to respect that
                else
                {
                    EndRecursiveBackNavigation();
                    _LastCanceleddUri = null;
                }
            }
        }

        /// <summary>
        /// Fragmented Navigation is not supported in WP 7 (build 7004)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonLinearNavigationService_FragmentNavigation(object sender, FragmentNavigationEventArgs e)
        {
            Debug.WriteLine("NonLinearNavigationService_FragmentNavigation");
        }

        /// <summary>
        /// this is bad (really)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonLinearNavigationService_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine("In Nav Faild");
        }

        #endregion

        #region Internal/Private Methods

        /// <summary>
        /// Indicates that a backward navigation cycle is complete, typically
        /// because the page has been navigated to.
        /// </summary>
        private void EndRecursiveBackNavigation()
        {
            IsRecursiveBackNavigation = false;
            _LoopStartTargetPageUri = null;

            _AppRootFrame.Opacity = OpacityBeforeRecursiveNav;
        }

         /// <summary>
        /// Loads the current provider with previously stored history values,
        /// or initializes a new history list.
        /// 
        /// When returning from Tombstone (Activated event) need to reload the previous
        /// page history list.
        /// </summary>
        private List<Uri> LoadServiceHistory()
        {
            if (PhoneApplicationService.Current.State.ContainsKey(_HistoryStateKey))
            {
                return (List<Uri>)PhoneApplicationService.Current.State[_HistoryStateKey];
            }
            else
            {
                var h = new List<Uri>();
                PhoneApplicationService.Current.State.Add(_HistoryStateKey, h);
                return h;
            }
        }
        #endregion
    }

    internal struct NavigationHelper
    {
        public Uri TargetUri { get; set; }
        public Uri CurrentUri { get; set; }
        public NavigationMode NavMode { get; set; }

    }


}