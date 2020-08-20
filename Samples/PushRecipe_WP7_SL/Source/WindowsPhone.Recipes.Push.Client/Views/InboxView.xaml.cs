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
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Phone.Shell;

using Microsoft.Phone.Notification;

using WindowsPhone.Recipes.Push.Client.Services;
using WindowsPhone.Recipes.Push.Client.Controls;

namespace WindowsPhone.Recipes.Push.Client.Views
{
    public partial class InboxView : UserControl
    {
        #region Fields

        /// <value>Url of the GetTileImage REST service.</value>
        private static readonly string GetTileImageService = App.ServerAddress + "/ImageService/GetTileImage?uri={0}";

        private readonly ObservableCollection<string> _rawMessages = new ObservableCollection<string>();
        private ShellTileSchedule _tileSchedule;

        #endregion

        #region Properties

        public ObservableCollection<string> RawMessages
        {
            get { return _rawMessages; }
        }

        public IEnumerable<string> ServerImages
        {
            get
            {
                return new string[]
                {
                    "number0.png",
                    "number1.png",
                    "number2.png",
                    "number3.png",
                    "number4.png",
                    "number5.png",
                    "number6.png",
                    "number7.png",
                    "number8.png",
                    "number9.png"
                };
            }
        }


        public string SelectedServerImage
        {
            get { return (string)GetValue(SelectedServerImageProperty); }
            set { SetValue(SelectedServerImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedServerImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedServerImageProperty =
            DependencyProperty.Register(
                "SelectedServerImage",
                typeof(string),
                typeof(InboxView),
                new PropertyMetadata("number0.png"));

        

        public IEnumerable<string> PushPatterns
        {
            get { return new string[] { "One Time", "Counter", "Ask to Pin", "Custom Tile", "Tile Schedule" }; }
        }

        public string PushPattern
        {
            get { return (string)GetValue(PushPatternProperty); }
            set { SetValue(PushPatternProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PushPattern.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushPatternProperty =
            DependencyProperty.Register(
                "PushPattern",
                typeof(string),
                typeof(InboxView),
                new PropertyMetadata(null, PushPatternChanged));

        private static void PushPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as InboxView;
            if ("Tile Schedule".CompareTo(e.NewValue) == 0)
            {
                VisualStateManager.GoToState(view, "ScheduleView", false);
            }
            else
            {
                VisualStateManager.GoToState(view, "NormalView", false);
            }
        }

        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Counter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CounterProperty =
            DependencyProperty.Register(
                "Counter",
                typeof(int),
                typeof(InboxView),
                new PropertyMetadata(0));

        public string TileScheduleParameter
        {
            get { return (string)GetValue(TileScheduleParameterProperty); }
            set { SetValue(TileScheduleParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TileScheduleParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TileScheduleParameterProperty =
            DependencyProperty.Register(
                "TileScheduleParameter",
                typeof(string),
                typeof(InboxView),
                new PropertyMetadata("number0.png"));

        #endregion

        public InboxView()
        {
            DataContext = this;
            InitializeComponent();

            Loaded += InboxView_Loaded;
            Unloaded += InboxView_Unloaded;
            UpdateServerInfo();
        }

        private void InboxView_Unloaded(object sender, RoutedEventArgs e)
        {
            UnregisterRawNotification();
        }

        private void InboxView_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterRawNotification();
        }

        private void UpdateServerInfo()
        {
            try
            {
                var pushService = new PushServiceClient();
                pushService.GetServerInfoCompleted += (s1, e1) =>
                {
                    try
                    {
                        pushService.CloseAsync();
                        if (e1.Result != null)
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                PushPattern = e1.Result.PushPattern;
                                Counter = e1.Result.Counter;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Show();
                    }
                };

                pushService.GetServerInfoAsync();
            }
            catch (Exception ex)
            {
                ex.Show();
            }
        }

        private void RegisterRawNotification()
        {
            var context = PushContext.Current;
            if (context != null)
            {
                context.RawNotification += context_RawNotification;
            }
        }

        private void UnregisterRawNotification()
        {
            var context = PushContext.Current;
            if (context != null)
            {
                context.RawNotification -= context_RawNotification;
            }
        }

        private void context_RawNotification(object sender, HttpNotificationEventArgs e)
        {
            try
            {
                using (var stream = new StreamReader(e.Notification.Body))
                {
                    var rawMessage = stream.ReadToEnd();
                    _rawMessages.Insert(0, rawMessage);
                    if ("AskToPin".CompareTo(rawMessage) == 0)
                    {
                        AskToPin();
                    }

                    UpdateServerInfo();
                }
            }
            catch (Exception ex)
            {
                ex.Show();
            }
        }

        private void AskToPin()
        {
            NotificationBox.Show("Important", "Please pin your application to Start Screen so this application can work properly.");
        }

        private void ButtonSchedule_Click(object sender, RoutedEventArgs e)
        {
            _tileSchedule = new ShellTileSchedule();
            _tileSchedule.Recurrence = UpdateRecurrence.Interval;
            _tileSchedule.StartTime = DateTime.Now;
            _tileSchedule.Interval = UpdateInterval.EveryHour;
            _tileSchedule.RemoteImageUri = new Uri(string.Format(GetTileImageService, TileScheduleParameter));
            _tileSchedule.Start();
        }

        private void ButtonTestNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pushService = new PushServiceClient();
                pushService.UpdateTileCompleted += (s1, e1) =>
                {
                    try
                    {
                        pushService.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        ex.Show();
                    }
                };

                pushService.UpdateTileAsync(PushContext.Current.NotificationChannel.ChannelUri, SelectedServerImage);
            }
            catch (Exception ex)
            {
                ex.Show();
            }
        }
    }
}
