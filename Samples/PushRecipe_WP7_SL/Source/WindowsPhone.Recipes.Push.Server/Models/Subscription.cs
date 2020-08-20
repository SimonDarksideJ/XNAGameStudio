using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace WindowsPhone.Recipes.Push.Server.Models
{
    /// <summary>
    /// Represents user subscription.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the notification channel uri.
        /// </summary>
        public Uri ChannelUri { get; private set; }

        /// <summary>
        /// Initialize a new instance of this type.
        /// </summary>
        public Subscription(string userName, Uri channelUri)
        {
            UserName = userName;
            ChannelUri = channelUri;
        }

        /// <summary>
        /// Initialize a new instance of this type.
        /// </summary>
        public Subscription(string userName, string channelUri)
            : this (userName, new Uri(channelUri, UriKind.Absolute))
        {
        }        
    }
}
