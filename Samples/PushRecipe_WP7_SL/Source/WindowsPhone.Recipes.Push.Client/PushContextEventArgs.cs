using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Notification;

namespace WindowsPhone.Recipes.Push.Client
{
    public class PushContextEventArgs : EventArgs
    {
        public HttpNotificationChannel NotificationChannel { get; private set; }

        internal PushContextEventArgs(HttpNotificationChannel channel)
        {
            NotificationChannel = channel;
        }
    }
}
