using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;

using Microsoft.Phone.Controls;
using WindowsPhone.Recipes.Push.Client.Views;
using WindowsPhone.Recipes.Push.Client.Controls;

namespace WindowsPhone.Recipes.Push.Client
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region Fields

        private readonly IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;
        private const string ChannelName = "OneTimePatternChannel";
        private const string ServiceName = "WindowsPhone.Recipes.Push.Server.PushService";

        private static readonly Uri[] AllowedDomains =
        {
            new Uri(App.ServerAddress)
        };

        private readonly ViewTransitions<ViewState> _viewTransitions;

        #endregion

        #region Properties

        private UIElement ActiveView
        {
            get { return activeView.Child; }
            set { activeView.Child = value; }
        }

        #endregion

        #region Ctor

        public MainPage()
        {            
            InitializeComponent();

            var viewState = CheckIfFirstTimeLoaded() ? ViewState.FirstInitial : ViewState.Initial;
            _viewTransitions = new ViewTransitions<ViewState>(viewState);
            InitializeViewTransitions();
        }        

        #endregion

        #region Overrides

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var pushContext = new PushContext(ChannelName, ServiceName, AllowedDomains, Dispatcher);

            _viewTransitions.Transition();

            base.OnNavigatedTo(e);
        }

        #endregion

        #region Event Handlers

        private void userLoginView_Login(object sender, LoginEventArgs e)
        {
            if (e.Exception != null)
            {
                e.Exception.Show("Login");
                return;
            }

            var userLoginView = sender as UserLoginView;
            userLoginView.Login -= userLoginView_Login;

            _viewTransitions.Transition();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _viewTransitions.Transition();
        }

        #endregion

        #region Privates

        private bool CheckIfFirstTimeLoaded()
        {
            object unused;
            if (!Settings.TryGetValue("MainPage.Loaded", out unused))
            {
                Settings["MainPage.Loaded"] = null;
                return true;
            }

            return false;
        }

        private void DisplayUserLoginView()
        {
            PageTitle.Text = "registration";
            button.Visibility = Visibility.Collapsed;
            var userLoginView = new UserLoginView
            {
                UserName = "tomer.shamam"
            };

            userLoginView.Login += userLoginView_Login;
            ActiveView = userLoginView;
        }

        private void DisplayPushSettingsView()
        {
            PageTitle.Text = "push settings";
            button.Content = "OK";
            button.Visibility = Visibility.Visible;
            ActiveView = new PushSettingsView();
        }

        private void DisplayInboxView()
        {
            PageTitle.Text = "server status";
            button.Content = "Settings";
            button.Visibility = Visibility.Visible;
            ActiveView = new InboxView();
        }

        private void InitializeViewTransitions()
        {
            _viewTransitions.AddTransition(ViewState.FirstInitial, ViewState.FirstSettings, DisplayPushSettingsView);
            _viewTransitions.AddTransition(ViewState.Initial, ViewState.Login, DisplayUserLoginView);
            _viewTransitions.AddTransition(ViewState.FirstSettings, ViewState.Login, DisplayUserLoginView);
            _viewTransitions.AddTransition(ViewState.Login, ViewState.Inbox, DisplayInboxView);
            _viewTransitions.AddTransition(ViewState.Settings, ViewState.Inbox, DisplayInboxView);
            _viewTransitions.AddTransition(ViewState.Inbox, ViewState.Settings, DisplayPushSettingsView);
        }

        #endregion

        #region ViewState

        [Flags]
        private enum ViewState
        {
            FirstTime = 1,
            Initial = 2,
            Settings = 4,
            Login = 8,
            Inbox = 16,
            FirstInitial = FirstTime | Initial,
            FirstSettings = FirstTime | Settings
        }

        #endregion
    }
}