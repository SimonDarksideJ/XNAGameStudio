using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Push notification message send operation result.
    /// </summary>
    public class MessageSendResult
    {
        #region Properties

        /// <summary>
        /// Gets the original exception or null.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the response time offset.
        /// </summary>
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>
        /// Gets the associated message.
        /// </summary>
        public PushNotificationMessage AssociatedMessage { get; private set; }

        /// <summary>
        /// Gets the channel URI.
        /// </summary>
        public Uri ChannelUri { get; private set; }

        /// <summary>
        /// Gets the web request status.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the push notification status.
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

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        internal MessageSendResult(PushNotificationMessage associatedMessage, Uri channelUri, WebResponse response)
        {
            Timestamp = DateTimeOffset.Now;
            AssociatedMessage = associatedMessage;
            ChannelUri = channelUri;

            InitializeStatusCodes(response as HttpWebResponse);
        }        

        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        internal MessageSendResult(PushNotificationMessage associatedMessage, Uri channelUri, WebException exception)
            : this(associatedMessage, channelUri, response: exception.Response)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        internal MessageSendResult(PushNotificationMessage associatedMessage, Uri channelUri, Exception exception)
            : this(associatedMessage, channelUri, response: null)
        {
            Exception = exception;
        }

        #endregion

        #region Privates

        private void InitializeStatusCodes(HttpWebResponse response)
        {
            if (response == null)
            {
                StatusCode = HttpStatusCode.InternalServerError;
                NotificationStatus = NotificationStatus.NotApplicable;
                DeviceConnectionStatus = DeviceConnectionStatus.NotApplicable;
                SubscriptionStatus = SubscriptionStatus.NotApplicable;
            }
            else
            {
                StatusCode = response.StatusCode;
                NotificationStatus = response.GetNotificationStatus();
                DeviceConnectionStatus = response.GetDeviceConnectionStatus();
                SubscriptionStatus = response.GetSubscriptionStatus();
            }
        }
        
        #endregion
    }    
}
