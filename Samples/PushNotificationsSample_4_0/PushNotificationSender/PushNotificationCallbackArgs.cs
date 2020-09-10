//-----------------------------------------------------------------------------
// PushNotificationCallbackArgs.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Net;

namespace PushNotificationSender
{
    /// <summary>
    /// A wrapper class for the status of a sent push notification.
    /// </summary>
    public class PushNotificationCallbackArgs
    {
        public PushNotificationCallbackArgs(PushNotificationSender.NotificationType notificationType, HttpWebResponse response)
        {
            this.Timestamp = DateTimeOffset.Now;
            this.NotificationType = notificationType;

            if (null != response)
            {
                this.MessageId = response.Headers[PushNotificationSender.MESSAGE_ID_HEADER];
                this.ChannelUri = response.ResponseUri.ToString();
                this.StatusCode = response.StatusCode;
                this.NotificationStatus = response.Headers[PushNotificationSender.NOTIFICATION_STATUS_HEADER];
                this.DeviceConnectionStatus = response.Headers[PushNotificationSender.DEVICE_CONNECTION_STATUS_HEADER];
                this.SubscriptionStatus = response.Headers[PushNotificationSender.SUBSCRIPTION_STATUS_HEADER];
            }
        }

        public DateTimeOffset Timestamp { get; private set; }
        public string MessageId { get; private set; }
        public string ChannelUri { get; private set; }
        public PushNotificationSender.NotificationType NotificationType { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string NotificationStatus { get; private set; }
        public string DeviceConnectionStatus { get; private set; }
        public string SubscriptionStatus { get; private set; }
    }
}
