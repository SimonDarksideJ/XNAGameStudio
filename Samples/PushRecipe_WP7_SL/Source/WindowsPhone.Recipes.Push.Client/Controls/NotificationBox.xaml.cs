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
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;

namespace WindowsPhone.Recipes.Push.Client.Controls
{
    public partial class NotificationBox : UserControl
    {
        #region Fields

        private readonly IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;
        private static Popup _popup;

        #endregion

        public string Title { get; set; }
        public string Message { get; set; }
        
        public bool ShowAgain
        {
            get
            {
                bool showAgain;
                if (!Settings.TryGetValue("NotificationBox.ShowAgain", out showAgain))
                {
                    showAgain = true;
                    ShowAgain = showAgain;                    
                }

                return showAgain;
            }

            set
            {
                Settings["NotificationBox.ShowAgain"] = value;
            }
        }

        private NotificationBox()
        {
            DataContext = this;

            InitializeComponent();
        }

        public static void Show(string title, string message)
        {
            if (_popup != null)
            {
                return;
            }

            var root = Application.Current.RootVisual as PhoneApplicationFrame;            
            var notificationBox = new NotificationBox
            {
                Title = title,
                Message = message,
                Width = root.ActualWidth,
                MaxHeight = root.ActualHeight,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!notificationBox.ShowAgain)
                return;
            
            _popup = new Popup
            {
                Child = notificationBox,
                IsOpen = true,                
            };
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = false;
            _popup = null;
        }
    }
}
