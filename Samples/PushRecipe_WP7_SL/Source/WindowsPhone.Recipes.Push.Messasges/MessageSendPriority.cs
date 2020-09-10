using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Represents the priorities of which the Push Notification Service sends the message.
    /// </summary>
    public enum MessageSendPriority
    {
        /// <value>The message should be delivered by the Push Notification Service immediately.</value>
        High = 0,

        /// <value>The message should be delivered by the Push Notification Service within 450 seconds.</value>
        Normal = 1,

        /// <value>The message should be delivered by the Push Notification Service within 900 seconds.</value>
        Low = 2
    }
}
