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

using WindowsPhone.Recipes.Push.Client.Services;
using System.Threading;

namespace WindowsPhone.Recipes.Push.Client.Views
{
    public partial class UserLoginView : UserControl
    {
        #region Fields

        private readonly IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;

        #endregion

        #region Properties

        public string UserName { get; set; }
        
        #endregion

        #region Events

        public event EventHandler<LoginEventArgs> Login;
        
        #endregion

        public UserLoginView()
        {
            DataContext = this;

            InitializeComponent();

            Loaded += UserLoginView_Loaded;
        }

        private void UserLoginView_Loaded(object sender, RoutedEventArgs e)
        {
            string userName;
            if (!Settings.TryGetValue("LoginPage.UserName", out userName))
            {                
                login.Visibility = Visibility.Visible;
            }
            else
            {
                UserName = userName;
                InternalLogin();
            }
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs args)
        {
            Settings["LoginPage.UserName"] = UserName;
            login.Visibility = Visibility.Collapsed;
            InternalLogin();            
        }

        private void OnLogin(LoginEventArgs args)
        {
            if (Login != null)
            {
                Login(this, args);
            }
        }

        private void InternalLogin()
        {
            login.Visibility = Visibility.Collapsed;
            progress.Visibility = Visibility.Visible;

            var pushContext = PushContext.Current;
            pushContext.Connect(c => RegisterClient(c.ChannelUri));
        }

        private void RegisterClient(Uri channelUri)
        {
            // Register the URI with 3rd party web service.
            try
            {
                var pushService = new PushServiceClient();
                pushService.RegisterCompleted += (s, e) =>
                {
                    pushService.CloseAsync();

                    Completed(e.Error);
                };

                pushService.RegisterAsync(UserName, channelUri);
            }
            catch (Exception ex)
            {
                Completed(ex);
            }
        }

        private void Completed(Exception ex)
        {
            login.Visibility = Visibility.Visible;
            progress.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(() => OnLogin(new LoginEventArgs(ex)));
        }
    }
}
