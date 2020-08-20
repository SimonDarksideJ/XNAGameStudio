using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Input;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Services;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Represents the Ask user to Pin the application push notification pattern.
    /// </summary>
    /// <remarks>
    /// Only users can pin a Windows Phone application to the Start screen, therefore if a given
    /// application wants to use the tile functionality and the user didn’t pinned
    /// the app’s tile, we want to notify the user she is missing additional functionality.
    /// This pattern is implemented in both client and server-side.
    /// </remarks>
    [Export(typeof(PushPatternViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class AskToPinPushPatternViewModel : PushPatternViewModel
    {
        #region Fields

        /// <value>A dictionary for tracking tile messages.</value>
        private readonly Dictionary<string, TilePushNotificationMessage> _messages = new Dictionary<string, TilePushNotificationMessage>();

        /// <value>Synchronizes access to the messages collection.</value>
        private readonly object MessageSync = new object();

        #endregion        

        #region Ctor

        /// <summary>
        /// Initialize new instance of this type with defaults.
        /// </summary>
        public AskToPinPushPatternViewModel()
        {
            InitializeDefaults();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Send a tile notification to all relevant subscribers.
        /// </summary>
        protected override void OnSend()
        {            
            // Asynchronously try to send this message to all relevant subscribers.
            foreach (var subscriber in PushService.Subscribers)
            {
                // Create a tile message to send an update.
                var tileMsg = GetOrCreateMessage(subscriber.UserName);

                tileMsg.SendAsync(
                    subscriber.ChannelUri,
                    result =>
                    {
                        Log(result);
                        OnMessageSent(subscriber.UserName, result);
                    },
                    Log);
            }
        }        

        /// <summary>
        /// Once an application is activated again (the client side phone application
        /// has subscription logic on startup), try to update the tile again.
        /// In case that the application is not pinned, send raw notification message
        /// to the client, asking to pin the application. This raw notification message
        /// has to be well-known and handled by the client side phone application.
        /// In our case the raw message is AskToPin.
        /// </summary>
        protected override void OnSubscribed(SubscriptionEventArgs args)
        {
            // Asynchronously try to send Tile message to the relevant subscriber
            // with data already sent before so the tile won't change.
            var tileMsg = GetOrCreateMessage(args.Subscription.UserName, false);

            tileMsg.SendAsync(
                args.Subscription.ChannelUri,
                result =>
                {
                    Log(result);
                    OnMessageSent(args.Subscription.UserName, result);
                },
                Log);
        }

        #endregion

        #region Privates

        /// <summary>
        /// Once tile update sent, check if handled by the phone.
        /// In case that the application is not pinned, ask to pin.
        /// </summary>
        private void OnMessageSent(string userName, MessageSendResult result)
        {
            if (!CheckIfPinned(result))
            {
                AskUserToPin(result.ChannelUri);
            }
        }

        /// <summary>
        /// Just in case that the application is running, send a raw message, asking
        /// the user to pin the application. This raw message has to be handled in client side.
        /// </summary>
        private void AskUserToPin(Uri uri)
        {
            new RawPushNotificationMessage(MessageSendPriority.High)
            {
                RawData = Encoding.ASCII.GetBytes(RawMessage)

            }.SendAsync(uri, Log, Log);
        }

        private bool CheckIfPinned(MessageSendResult result)
        {
            // We known if the application is pinned by checking the following send result flags:
            return result.DeviceConnectionStatus == DeviceConnectionStatus.Connected &&
                   result.SubscriptionStatus == SubscriptionStatus.Active &&
                   result.NotificationStatus == NotificationStatus.Received;
        }

        private TilePushNotificationMessage GetOrCreateMessage(string userName, bool overrideExisting = true)
        {
            lock (MessageSync)
            {
                TilePushNotificationMessage message;
                if (!_messages.TryGetValue(userName, out message) || overrideExisting)
                {
                    message = new TilePushNotificationMessage(MessageSendPriority.High)
                    {
                        BackgroundImageUri = BackgroundImageUri,
                        Count = Count,
                        Title = Title
                    };

                    _messages[userName] = message;
                }

                return message;
            }
        }

        private void InitializeDefaults()
        {
            DisplayName = "Ask to Pin";
            Description = "Only users can pin a Windows Phone application to the Start screen, therefore if a given application wants to use the tile functionality and the user didn’t pinned the app’s tile, we want to notify the user she is missing additional functionality. This pattern is implemented in both client and server-side.";
            BackgroundImageUri = TileImages.Length > 2 ? TileImages[2] : TileImages.FirstOrDefault();
            Count = 1;
            Title = "Game Update";
            RawMessage = "AskToPin";
        }

        #endregion
    }
}
