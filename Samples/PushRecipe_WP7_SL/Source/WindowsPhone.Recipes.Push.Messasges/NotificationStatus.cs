using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Microsoft Push Notification Service notification request status.
    /// </summary>
    public enum NotificationStatus
    {
        /// <value>The request is not applicable.</value>
        NotApplicable,

        /// <value>The notification request was accepted.</value>
        Received,

        /// <value>Queue overflow. The Push Notification Service should re-send the notification later.</value>
        QueueFull,

        /// <value>The push notification was suppressed by the Push Notification Service.</value>
        Suppressed,

        /// <value>The push notification was dropped by the Push Notification Service.</value>
        Dropped,
    }
}
