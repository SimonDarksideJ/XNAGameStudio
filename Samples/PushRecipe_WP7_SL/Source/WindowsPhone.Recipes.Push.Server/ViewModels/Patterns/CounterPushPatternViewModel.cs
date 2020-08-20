using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Xml.Linq;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Services;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Represents the Counter push notification pattern.
    /// </summary>
    /// <remarks>
    /// Send a tile push notification message with a counter value.
    /// Each time a push notification message is sent the counter value
    /// increases by one, unless user is running the application and notifies
    /// the server.
    /// </remarks>
    [Export(typeof(PushPatternViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class CounterPushPatternViewModel : PushPatternViewModel
    {
        #region Fields

        /// <value>A dictionary for tracking tile message count while phone-application is not running.</value>
        private readonly Dictionary<string, int> _messageCounter = new Dictionary<string, int>();

        /// <value>Synchronizes access to the message counter collection.</value>
        private readonly object MessageCounterSync = new object();

        #endregion        

        #region Ctor

        /// <summary>
        /// Initialize new instance of this type with defaults.
        /// </summary>
        public CounterPushPatternViewModel()
        {
            InitializeDefaults();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Send raw message to all subscribers. In case that the phone-application
        /// is not running, send tile update and increase tile counter.
        /// </summary>
        protected override void OnSend()
        {
            // Notify phone for having waiting messages.
            var rawMsg = new RawPushNotificationMessage(MessageSendPriority.High)
            {
                RawData = Encoding.ASCII.GetBytes(RawMessage)
            };

            foreach (var subscriber in PushService.Subscribers)
            {
                rawMsg.SendAsync(
                    subscriber.ChannelUri,
                    result =>
                    {
                        Log(result);
                        OnRawSent(subscriber.UserName, result);
                    },
                    Log);
            }
        }

        /// <summary>
        /// On subscription change, reset the subscriber tile counter if exist.
        /// </summary>
        protected override void OnSubscribed(SubscriptionEventArgs e)
        {
            // Create a tile message to reset tile count.
            var tileMsg = new TilePushNotificationMessage(MessageSendPriority.High)
            {
                Count = 0,
                BackgroundImageUri = BackgroundImageUri,
                Title = Title
            };

            tileMsg.SendAsync(e.Subscription.ChannelUri, Log, Log);

            ResetCounter(e.Subscription.UserName);
        }

        #endregion

        #region Privates

        private void OnRawSent(string userName, MessageSendResult result)
        {
            // In case that the device is disconnected, no need to send a tile message.
            if (result.DeviceConnectionStatus == DeviceConnectionStatus.TempDisconnected)
            {
                return;
            }

            // Checking these three flags we can know what's the state of both the device and apllication.
            bool isApplicationRunning =
                result.SubscriptionStatus == SubscriptionStatus.Active &&
                result.NotificationStatus == NotificationStatus.Received &&
                result.DeviceConnectionStatus == DeviceConnectionStatus.Connected;

            // In case that the application is not running, send a tile update with counter increase.
            if (!isApplicationRunning)
            {
                var tileMsg = new TilePushNotificationMessage(MessageSendPriority.High)
                {
                    Count = IncreaseCounter(userName),
                    BackgroundImageUri = BackgroundImageUri,
                    Title = Title
                };

                tileMsg.SendAsync(result.ChannelUri, Log, Log);
            }
        }

        private void ResetCounter(string userName)
        {
            lock (MessageCounterSync)
            {
                _messageCounter.Remove(userName);
            }
        }

        private int IncreaseCounter(string userName)
        {
            lock (MessageCounterSync)
            {
                int counter;
                if (_messageCounter.TryGetValue(userName, out counter))
                {
                    ++counter;
                }
                else
                {
                    counter = 1;
                }

                _messageCounter[userName] = counter;
                return counter;
            }
        }

        private void InitializeDefaults()
        {
            DisplayName = "Counter";
            Description = "Send push notification message of Tile type, with a counter value. Each time a push notification message is sent, the counter value increases by one, unless user is running the application and notifies the server.";
            BackgroundImageUri = TileImages.Length > 1 ? TileImages[1] : TileImages.FirstOrDefault();
            RawMessage = "Game Update";
            Count = 0;
            Title = "Updates";
        }

        #endregion
    }
}
