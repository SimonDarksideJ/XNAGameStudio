using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Input;

using WindowsPhone.Recipes.Push.Server.Services;
using System.Windows;
using WindowsPhone.Recipes.Push.Server.Models;
using WindowsPhone.Recipes.Push.Messasges;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Represents a base class for all push pattern view models.
    /// </summary>
    /// <remarks>
    /// A push pattern view model contains the logic and behavior of a server side push notification pattern.
    /// For demonstration purposes, and since we are using WPF as the 'face' of this server, each view model
    /// exposes properties and commands to be controlled and activated by this server UI.
    /// You can find more information about the MVVM pattern in <see cref="http://en.wikipedia.org/wiki/Model_View_ViewModel"/>.
    /// </remarks>
    internal abstract class PushPatternViewModel : ViewModelBase
    {
        #region Fields

        /// <value>Indicates if current pattern is active or not.</value>
        private bool _isActive;

        /// <value>Collection of tile image relative uri's available in the phone application.</value>
        private Uri[] _tileImages;

        /// <value>Selected tile background image uri.</value>
        private Uri _backgroundImageUri;

        /// <value>Tile message count.</value>
        private int _count;

        /// <value>Tile message title.</value>
        private string _title;

        /// <value>Toast message title.</value>
        private string _toastTitle;

        /// <value>Toast message sub title.</value>
        private string _toastSubTitle;

        /// <value>Raw text message.</value>
        private string _rawMessage;

        #endregion

        #region Properties

        [Import]
        protected PushService PushService { get; private set; }

        [Import]
        private IMessageSendResultLogger ResultLogger { get; set; }

        /// <summary>
        /// Gets current pattern display name.
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Gets current pattern description text.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Gets or sets value for indicating whether this pattern is active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive)
                    {
                        OnActivated();
                    }
                    else
                    {
                        OnDeactivated();
                    }

                    NotifyPropertyChanged("IsActive");
                }
            }
        }        

        /// <summary>
        /// Gets a collection of tile image uris available in the phone client application.
        /// </summary>
        public Uri[] TileImages
        {
            get
            {
                if (_tileImages == null)
                {
                    _tileImages = new Uri[]
                    {
                        ToUri("TileBackground1.jpg"),
                        ToUri("TileBackground2.jpg"),
                        ToUri("TileBackground3.jpg"),
                    };

                    BackgroundImageUri = _tileImages[0];
                }

                return _tileImages;
            }
        }

        /// <summary>
        /// Gets or sets the tile background uri.
        /// </summary>
        public Uri BackgroundImageUri
        {
            get { return _backgroundImageUri; }

            set
            {
                if (_backgroundImageUri != value)
                {
                    _backgroundImageUri = value;
                    NotifyPropertyChanged("BackgroundImageUri");
                }
            }
        }

        /// <summary>
        /// Gets or sets the tile count.
        /// </summary>
        public int Count
        {
            get { return _count; }

            set
            {
                if (_count != value)
                {
                    _count = value;
                    NotifyPropertyChanged("Count");
                }
            }
        }

        /// <summary>
        /// Gets or sets the tile tiltle.
        /// </summary>
        public string Title
        {
            get { return _title; }

            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        /// <summary>
        /// Gets or sets the toast title.
        /// </summary>
        public string ToastTitle
        {
            get { return _toastTitle; }

            set
            {
                if (_toastTitle != value)
                {
                    _toastTitle = value;
                    NotifyPropertyChanged("ToastTitle");
                }
            }
        }

        /// <summary>
        /// Gets or sets the toast sub-title.
        /// </summary>
        public string ToastSubTitle
        {
            get { return _toastSubTitle; }

            set
            {
                if (_toastSubTitle != value)
                {
                    _toastSubTitle = value;
                    NotifyPropertyChanged("ToastSubTitle");
                }
            }
        }

        /// <summary>
        /// Gets or sets the raw text message.
        /// </summary>
        public string RawMessage
        {
            get { return _rawMessage; }

            set
            {
                if (_rawMessage != value)
                {
                    _rawMessage = value;
                    NotifyPropertyChanged("RawMessage");
                }
            }
        }        

        #endregion        

        #region Commands

        /// <summary>
        /// Gets the command which executes the send operation.
        /// </summary>
        public ICommand SendCommand
        {
            get
            {
                return new RelayCommand(p =>
                    {
                        try
                        {
                            OnSend();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Send Error");
                        }
                    });
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Override this to send push message according to the pattern behavior.
        /// </summary>
        protected abstract void OnSend();

        /// <summary>
        /// Override this to add additional pattern-activation logic.
        /// </summary>
        protected virtual void OnActivated()
        {
            UpdateClient();

            PushService.Subscribed += PushService_Subscribed;
            PushService.GetInfo += PushService_GetInfo;
        }

        /// <summary>
        /// Override this to add additional pattern-deactivation logic.
        /// </summary>
        protected virtual void OnDeactivated()
        {
            PushService.Subscribed -= PushService_Subscribed;
            PushService.GetInfo -= PushService_GetInfo;
        }

        /// <summary>
        /// Override this to add logic when clients login.
        /// </summary>
        protected virtual void OnSubscribed(SubscriptionEventArgs args)
        {
        }

        /// <summary>
        /// Override this to add logic when clients request server's info.
        /// </summary>
        protected virtual void OnGetInfo(ServerInfoEventArgs args)
        {
            args.ServerInfo = new ServerInfo
            {
                PushPattern = DisplayName,
                Counter = Count
            };
        }

        /// <summary>
        /// Logs push message result.
        /// </summary>
        protected void Log(MessageSendResult result)
        {
            ResultLogger.Log(DisplayName, result);
        }

        /// <summary>
        /// Logs push message error.
        /// </summary>
        protected void Log(MessageSendException exception)
        {
            ResultLogger.Log(DisplayName, exception);
        }

        #endregion

        #region Event Handlers

        private void UpdateClient()
        {
            // Notify subscribers to get new info from the server.
            foreach (var subscriber in PushService.Subscribers)
            {
                new RawPushNotificationMessage(MessageSendPriority.High)
                {
                    RawData = Encoding.ASCII.GetBytes("Update Info")
                }.SendAsync(subscriber.ChannelUri);
            }
        }

        private void PushService_Subscribed(object sender, SubscriptionEventArgs args)
        {
            OnSubscribed(args);
        }

        private void PushService_GetInfo(object sender, ServerInfoEventArgs args)
        {
            OnGetInfo(args);
        }

        #endregion

        #region Helpers

        private static Uri ToUri(string imageName)
        {
            return new Uri(string.Format("/Resources/TileImages/{0}", imageName), UriKind.Relative);
        }

        #endregion
    }
}
