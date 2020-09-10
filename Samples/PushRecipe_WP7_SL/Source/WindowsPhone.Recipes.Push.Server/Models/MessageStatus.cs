using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.Models
{
    /// <summary>
    /// Represents a push notification message send response status.
    /// </summary>
    public class MessageStatus
    {
        private static readonly Dictionary<Type, string> MessageTypes = new Dictionary<Type, string>
        {
            {typeof(TilePushNotificationMessage), "Tile"},
            {typeof(ToastPushNotificationMessage), "Toast"},
            {typeof(RawPushNotificationMessage), "Raw"}
        };

        /// <summary>
        /// Gets the push notification pattern type.
        /// </summary>
        public string Pattern { get; private set; }
        
        /// <summary>
        /// Gets the response time stamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public string MessageType { get; private set; }

        /// <summary>
        /// Gets the message ID.
        /// </summary>
        public Guid MessageId { get; private set; }

        /// <summary>
        /// Gets the notification channel URI.
        /// </summary>
        public Uri ChannelUri { get; private set; }

        /// <summary>
        /// Gets the response status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the notification status.
        /// </summary>
        public NotificationStatus NotificationStatus { get; private set; }

        /// <summary>
        /// Gets the device connection status.
        /// </summary>
        public DeviceConnectionStatus DeviceConnectionStatus { get; private set; }

        /// <summary>
        /// Gets the subscription status.
        /// </summary>
        public SubscriptionStatus SubscriptionStatus { get; private set; }

        /// <summary>
        /// Initialize a new instance of this type.
        /// </summary>
        public MessageStatus(string pattern, MessageSendResult result)
        {
            Pattern = pattern;
            Timestamp = result.Timestamp;
            MessageType = MessageTypes[result.AssociatedMessage.GetType()];
            MessageId = result.AssociatedMessage.Id;
            ChannelUri = result.ChannelUri;
            StatusCode = result.StatusCode;
            NotificationStatus = result.NotificationStatus;
            DeviceConnectionStatus = result.DeviceConnectionStatus;
            SubscriptionStatus = result.SubscriptionStatus;
        }

        /// <summary>
        /// Initialize a new instance of this type.
        /// </summary>
        public MessageStatus(PushPatternType pattern, MessageSendException exception)
        {

        }
    }
}
