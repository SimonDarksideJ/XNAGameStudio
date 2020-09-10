//-----------------------------------------------------------------------------
// PushNotificationSender.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Text;
using System.Net;
using System.IO;

namespace PushNotificationSender
{
    /// <summary>
    /// A utility class for sending the three different types of push notifications.
    /// </summary>
    public class PushNotificationSender
    {
        public enum NotificationType
        {
            Tile = 1,
            Toast = 2,
            Raw = 3
        }

        public delegate void SendCompletedEventHandler(PushNotificationCallbackArgs args);
        public event SendCompletedEventHandler NotificationSendCompleted;

        public const string MESSAGE_ID_HEADER = "X-MessageID";
        public const string NOTIFICATION_CLASS_HEADER = "X-NotificationClass";
        public const string NOTIFICATION_STATUS_HEADER = "X-NotificationStatus";
        public const string DEVICE_CONNECTION_STATUS_HEADER = "X-DeviceConnectionStatus";
        public const string SUBSCRIPTION_STATUS_HEADER = "X-SubscriptionStatus";
        public const string WINDOWSPHONE_TARGET_HEADER = "X-WindowsPhone-Target";
        public const int MAX_PAYLOAD_LENGTH = 1024;


        /// <summary>
        /// Sends a raw notification, which is just a byte payload.
        /// </summary>
        public void SendRawNotification(Uri deviceUri, byte[] payload)
        {
            SendNotificationByType(deviceUri, payload, NotificationType.Raw);
        }


        /// <summary>
        /// Sends a tile notification, which is a title, a count, and an image URI.
        /// </summary>
        public void SendTileNotification(Uri deviceUri, string title, int count, string backgroundImage)
        {
            // Malformed push notifications cause exceptions to be thrown on the receiving end
            // so make sure we have valid data to start with.
            if (string.IsNullOrEmpty(title))
            {
                throw new InvalidOperationException("Tile notifications require title text");
            }

            // Set up the XML
            string msg = 
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                    "<wp:Tile>";
                            if (!string.IsNullOrEmpty(backgroundImage))
                            {
                                msg += "<wp:BackgroundImage>" + backgroundImage + "</wp:BackgroundImage>";
                            }
                            msg +=
                        "<wp:Count>" + count.ToString() + "</wp:Count>" +
                        "<wp:Title>" + title + "</wp:Title>" +
                    "</wp:Tile>" +
                "</wp:Notification>";

            byte[] payload = new UTF8Encoding().GetBytes(msg);

            SendNotificationByType(deviceUri, payload, NotificationType.Tile);
        }


        /// <summary>
        /// Sends a toast notification, which is two lines of text.
        /// </summary>
        public void SendToastNotification(Uri deviceUri, string text1, string text2)
        {
            // Malformed push notifications cause exceptions to be thrown on the receiving end
            // so make sure we have valid data to start with.
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
            {
                throw new InvalidOperationException("toast notifications must have at least 1 valid string");
            }

            // Set up the XML            
            string msg =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                  "<wp:Toast>" +
                     "<wp:Text1>" + text1 + "</wp:Text1>" +
                     "<wp:Text2>" + text2 + "</wp:Text2>" +
                  "</wp:Toast>" +
                "</wp:Notification>";

            byte[] payload = new UTF8Encoding().GetBytes(msg);

            SendNotificationByType(deviceUri, payload, NotificationType.Toast);
        }


        /// <summary>
        /// helper function to set up the request headers based on type and send the notification payload.
        /// </summary>
        private void SendNotificationByType(Uri channelUri, byte[] payload, NotificationType notificationType)
        {
            // Check the length of the payload and reject it if too long.
            if (payload.Length > MAX_PAYLOAD_LENGTH)
                throw new ArgumentOutOfRangeException("Payload is too long. Maximum payload size shouldn't exceed " + MAX_PAYLOAD_LENGTH.ToString() + " bytes");

            try
            {
                // Create and initialize the request object.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(channelUri);
                request.Method = WebRequestMethods.Http.Post;
                request.ContentLength = payload.Length;
                request.Headers[MESSAGE_ID_HEADER] = Guid.NewGuid().ToString();

                // Each type of push notification uses a different code in its X-NotificationClass
                // header to specify its delivery priority.  The three priorities are:

                //     Realtime.  The notification is delivered as soon as possible.
                //     Priority.  The notification is delivered within 450 seconds.
                //     Regular.   The notification is delivered within 900 seconds.
                
                //	      Realtime    Priority    Regular
                // Raw    3-10        13-20       23-31
                // Tile   1           11          21
                // Toast  2           12          22

                switch (notificationType)
                {
                    case NotificationType.Tile:
                        // the notification type for a tile notification is "token".
                        request.Headers[WINDOWSPHONE_TARGET_HEADER] = "token";
                        request.ContentType = "text/xml";
                        // Request real-time delivery for tile notifications.
                        request.Headers[NOTIFICATION_CLASS_HEADER] = "1";
                        break;

                    case NotificationType.Toast:
                        request.Headers[WINDOWSPHONE_TARGET_HEADER] = "toast";
                        request.ContentType = "text/xml";
                        // Request real-time delivery for toast notifications.
                        request.Headers[NOTIFICATION_CLASS_HEADER] = "2";
                        break;

                    case NotificationType.Raw:
                        // Request real-time delivery for raw notifications.
                        request.Headers[NOTIFICATION_CLASS_HEADER] = "3";
                        break;

                    default:
                        throw new ArgumentException("Unknown notification type", "notificationType");

                }

                Stream requestStream = request.GetRequestStream();

                requestStream.Write(payload, 0, payload.Length);
                requestStream.Close();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (NotificationSendCompleted != null && response != null)
                {
                    PushNotificationCallbackArgs args = new PushNotificationCallbackArgs(notificationType, (HttpWebResponse)response);
                    NotificationSendCompleted(args);
                }
            }
            catch (WebException ex)
            {
                // Notify the caller on exception as well.
                if (NotificationSendCompleted != null)
                {
                    if (null != ex.Response)
                    {
                        PushNotificationCallbackArgs args = new PushNotificationCallbackArgs(notificationType, (HttpWebResponse)ex.Response);
                        NotificationSendCompleted(args);
                    }
                }
            }
        }
    }
}
