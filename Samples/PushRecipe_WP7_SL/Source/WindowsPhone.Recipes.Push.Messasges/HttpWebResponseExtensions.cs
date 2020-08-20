using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Extends the <see cref="HttpWebResponse"/> type with methods for translating push notification specific status codes strings to strong typed enumeration.
    /// </summary>
    internal static class HttpWebResponseExtensions
    {
        /// <summary>
        /// Gets the Notification Status code as <see cref="NotificationStatus"/> enumeration.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate enumeration value.</returns>
        public static NotificationStatus GetNotificationStatus(this HttpWebResponse response)
        {
            return response.GetStatus(
                NotificationStatus.NotApplicable,
                PushNotificationMessage.Headers.NotificationStatus);
        }

        /// <summary>
        /// Gets the Device Connection Status code as <see cref="NotificationStatus"/> enumeration.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate enumeration value.</returns>
        public static DeviceConnectionStatus GetDeviceConnectionStatus(this HttpWebResponse response)
        {
            return response.GetStatus(
                DeviceConnectionStatus.NotApplicable,
                PushNotificationMessage.Headers.DeviceConnectionStatus);
        }

        /// <summary>
        /// Gets the Subscription Status code as <see cref="NotificationStatus"/> enumeration.
        /// </summary>
        /// <param name="response">The http web response instance.</param>
        /// <returns>Correlate enumeration value.</returns>
        public static SubscriptionStatus GetSubscriptionStatus(this HttpWebResponse response)
        {
            return response.GetStatus(
                SubscriptionStatus.NotApplicable,
                PushNotificationMessage.Headers.SubscriptionStatus);
        }

        private static T GetStatus<T>(this HttpWebResponse response, T def, string header) where T : struct
        {
            string statusString = response.Headers[header];
            T status = def;
            Enum.TryParse<T>(statusString, out status);
            return status;
        }
    }
}
