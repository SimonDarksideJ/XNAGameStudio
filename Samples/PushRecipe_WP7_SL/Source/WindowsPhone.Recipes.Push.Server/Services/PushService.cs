using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ComponentModel.Composition;

using WindowsPhone.Recipes.Push.Server.Models;
using System.Xml.Linq;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Current push server services implementation.
    /// </summary>
    [Export, PartCreationPolicy(CreationPolicy.Shared), ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class PushService : IPushService
    {        
        #region Fields

        private ServiceHost _serviceHost;

        /// <value>Dictionary contains users subscription objects.</value>
        private readonly Dictionary<string, Subscription> _subscribers = new Dictionary<string, Subscription>();

        /// <value>Sync access to the subscribers dictionary.</value>
        private readonly object SubscribersSync = new object();        

        #endregion

        #region Events

        /// <summary>
        /// Raise when user subscribed.
        /// </summary>
        public event EventHandler<SubscriptionEventArgs> Subscribed;

        /// <summary>
        /// Raise when current server status is requested.
        /// </summary>
        public event EventHandler<ServerInfoEventArgs> GetInfo;

        /// <summary>
        /// Raise when user requests a tile update.
        /// </summary>
        public event EventHandler<TileUpdateRequestEventArgs> TileUpdateRequest;

        #endregion

        #region Properties

        /// <summary>
        /// Get subscription list.
        /// </summary>
        public IEnumerable<Subscription> Subscribers
        {
            get
            {
                lock (_subscribers)
                {
                    var subscribers = new Subscription[_subscribers.Count];
                    _subscribers.Values.CopyTo(subscribers, 0);
                    return subscribers;
                }
            }
        }

        /// <summary>
        /// Get subscription count.
        /// </summary>
        public int SubscribersCount
        {
            get
            {
                lock (_subscribers)
                {
                    return _subscribers.Count;
                }
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Register user with notification channel uri.
        /// </summary>
        /// <param name="userName">The user name to register.</param>
        /// <param name="channelUri">The notification channel uri.</param>
        public void Register(string userName, Uri channelUri)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Invalid user name", "userName");
            }

            if (channelUri == null)
            {
                throw new ArgumentNullException("channelUri");
            }

            var subscription = new Subscription(userName, channelUri);

            lock (SubscribersSync)
            {
                // Add or update existing.
                _subscribers[userName] = subscription;
            }

            OnSubscribed(new SubscriptionEventArgs(subscription));
        }        

        /// <summary>
        /// Gets current server info.
        /// </summary>
        /// <returns>An instance info object contains server status.</returns>
        public ServerInfo GetServerInfo()
        {
            var args = new ServerInfoEventArgs();
            OnGetInfo(args);
            return args.ServerInfo;
        }

        /// <summary>
        /// Send a tile update with given parameter.
        /// </summary>        
        /// <param name="channelUri">An instance info object contains server status.</param>
        /// <param name="parameter">User parameter to send with the tile update request.</param>
        public void UpdateTile(Uri channelUri, string parameter)
        {
            OnTileUpdateRequest(new TileUpdateRequestEventArgs(channelUri, parameter));
        }

        internal Subscription TryGetSubscription(string userName)
        {
            lock (SubscribersSync)
            {
                Subscription subscription;
                if (!_subscribers.TryGetValue(userName, out subscription))
                {
                    subscription = null;
                }

                return subscription;
            }
        }
        
        #endregion

        #region Privates Logic

        public void Host()
        {
            _serviceHost = new ServiceHost(this);
            _serviceHost.Open();
        }

        private void OnSubscribed(SubscriptionEventArgs args)
        {
            if (Subscribed != null)
            {
                Subscribed(this, args);
            }
        }

        private void OnGetInfo(ServerInfoEventArgs args)
        {
            if (GetInfo != null)
            {
                GetInfo(this, args);
            }
        }

        private void OnTileUpdateRequest(TileUpdateRequestEventArgs args)
        {
            if (TileUpdateRequest != null)
            {
                TileUpdateRequest(this, args);
            }
        }

        #endregion        
    }
}
