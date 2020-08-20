using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Subscription event arguments.
    /// </summary>
    internal class SubscriptionEventArgs : EventArgs
    {
        public Subscription Subscription { get; private set; }

        public SubscriptionEventArgs(Subscription subscription)
        {
            Subscription = subscription;
        }
    }
}
