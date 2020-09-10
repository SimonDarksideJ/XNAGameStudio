using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Notification;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.ComponentModel;

namespace WindowsPhone.Recipes.Push.Client
{
    public sealed class PushContext : INotifyPropertyChanged
    {
        #region Fields

        private readonly IsolatedStorageSettings Settings = IsolatedStorageSettings.ApplicationSettings;
        private static PushContext _current;
        private bool _isConnected; 

        #endregion

        #region Properties
        private Dispatcher Dispatcher { get; set; }

        public string ChannelName { get; private set; }
        public string ServiceName { get; private set; }
        public IList<Uri> AllowedDomains { get; private set; }
        public HttpNotificationChannel NotificationChannel { get; private set; }

        public static PushContext Current
        {
            get { return _current; }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    NotifyPropertyChanged("IsConnected");
                }
            }
        }

        public bool IsPushEnabled
        {
            get { return GetOrCreate<bool>("PushContext.IsPushEnabled", false); }
            set
            {
                SetOrCreate("PushContext.IsPushEnabled", value);
                UpdateNotificationBindings();
                NotifyPropertyChanged("IsPushEnabled");
            }
        }

        public bool IsTileEnabled
        {
            get { return GetOrCreate<bool>("PushContext.IsTileEnabled", true); }
            set
            {
                SetOrCreate("PushContext.IsTileEnabled", value);
                UpdateNotificationBindings();
                NotifyPropertyChanged("IsTileEnabled");
            }
        }

        public bool IsToastEnabled
        {
            get { return GetOrCreate<bool>("PushContext.IsToastEnabled", true); }
            set
            {
                SetOrCreate("PushContext.IsToastEnabled", value);
                UpdateNotificationBindings();
                NotifyPropertyChanged("IsToastEnabled");
            }
        }

        public bool IsRawEnabled
        {
            get { return GetOrCreate<bool>("PushContext.IsRawEnabled", true); }
            set
            {
                SetOrCreate("PushContext.IsRawEnabled", value);
                NotifyPropertyChanged("IsRawEnabled");
            }
        } 
        #endregion

        #region Events
        public event EventHandler<PushContextErrorEventArgs> Error;
        public event EventHandler<PushContextEventArgs> ChannelPrepared;
        public event EventHandler<HttpNotificationEventArgs> RawNotification;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region Ctor

        public PushContext(string channelName, string serviceName, IList<Uri> allowedDomains, Dispatcher dispatcher)
        {
            if (_current != null)
            {
                throw new InvalidOperationException("There should be no more than one push context.");
            }

            ChannelName = channelName;
            ServiceName = serviceName;
            AllowedDomains = allowedDomains;
            Dispatcher = dispatcher;

            _current = this;
        }

        #endregion

        #region Public Methods
        public void Connect(Action<HttpNotificationChannel> prepared)
        {
            if (IsConnected)
            {
                prepared(NotificationChannel);
                return;
            }

            try
            {
                // First, try to pick up an existing channel.
                NotificationChannel = HttpNotificationChannel.Find(ChannelName);

                if (NotificationChannel == null)
                {
                    // Create new channel and subscribe events.
                    CreateChannel(prepared);
                }
                else
                {
                    // Channel exists, no need to create a new one.
                    SubscribeToNotificationEvents();
                    PrepareChannel(prepared);
                }

                IsConnected = true;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                if (NotificationChannel != null)
                {
                    UnbindFromTileNotifications();
                    UnbindFromToastNotifications();
                    NotificationChannel.Close();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                NotificationChannel = null;
                IsConnected = false;
            }
        } 
        #endregion

        #region Privates
        /// <summary>
        /// Create channel, subscribe to channel events and open the channel.
        /// </summary>
        private void CreateChannel(Action<HttpNotificationChannel> prepared)
        {
            // Create a new channel.
            NotificationChannel = new HttpNotificationChannel(ChannelName, ServiceName);

            // Register to UriUpdated event. This occurs when channel successfully opens.
            NotificationChannel.ChannelUriUpdated += (s, e) => Dispatcher.BeginInvoke(() => PrepareChannel(prepared));

            SubscribeToNotificationEvents();

            // Trying to Open the channel.
            NotificationChannel.Open();
        }

        private void SubscribeToNotificationEvents()
        {            
            // Register to raw notifications.
            NotificationChannel.HttpNotificationReceived += (s, e) =>
            {
                if (IsPushEnabled & IsRawEnabled)
                {
                    Dispatcher.BeginInvoke(() => OnRawNotification(e));
                }
            };
        }

        private void OnRawNotification(HttpNotificationEventArgs e)
        {
            if (RawNotification != null)
            {
                RawNotification(this, e);
            }
        }

        private void PrepareChannel(Action<HttpNotificationChannel> prepared)
        {
            try
            {
                // OnChannelPrepared(new PushContextEventArgs(NotificationChannel));
                prepared(NotificationChannel);
                UpdateNotificationBindings();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnError(Exception exception)
        {
            if (Error != null)
            {
                Error(this, new PushContextErrorEventArgs(exception));
            }
        }

        private void OnChannelPrepared(PushContextEventArgs args)
        {
            if (ChannelPrepared != null)
            {
                ChannelPrepared(this, args);
            }
        }

        private void BindToTileNotifications()
        {
            try
            {
                if (NotificationChannel != null && !NotificationChannel.IsShellTileBound)
                {
                    var listOfAllowedDomains = new Collection<Uri>(AllowedDomains);
                    NotificationChannel.BindToShellTile(listOfAllowedDomains);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void BindToToastNotifications()
        {
            try
            {
                if (NotificationChannel != null && !NotificationChannel.IsShellToastBound)
                {
                    NotificationChannel.BindToShellToast();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void UnbindFromTileNotifications()
        {
            try
            {
                if (NotificationChannel.IsShellTileBound)
                {
                    NotificationChannel.UnbindToShellTile();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void UnbindFromToastNotifications()
        {
            try
            {
                if (NotificationChannel.IsShellToastBound)
                {
                    NotificationChannel.UnbindToShellToast();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void UpdateNotificationBindings()
        {
            if (IsPushEnabled && IsTileEnabled)
            {
                BindToTileNotifications();
            }
            else
            {
                UnbindFromTileNotifications();
            }

            if (IsPushEnabled && IsToastEnabled)
            {
                BindToToastNotifications();
            }
            else
            {
                UnbindFromToastNotifications();
            }
        }

        private T GetOrCreate<T>(string key, T defaultValue = default(T))
        {
            T value;
            if (Settings.TryGetValue(key, out value))
            {
                return value;
            }

            return defaultValue;
        }

        private void SetOrCreate<T>(string key, T value)
        {
            Settings[key] = value;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        } 
        #endregion        
    }
}
