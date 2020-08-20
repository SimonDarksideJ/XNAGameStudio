using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System;

using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Push server services.
    /// </summary>
    [ServiceContract]
    public interface IPushService
    {
        /// <summary>
        /// Register user name with a push channel uri.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="channelUri">Push notification channel uri.</param>
        [OperationContract]
        void Register(string userName, Uri channelUri);

        /// <summary>
        /// Get current server information/status.
        /// </summary>
        /// <returns>Current server status.</returns>
        [OperationContract]
        ServerInfo GetServerInfo();

        /// <summary>
        /// Send a tile update with given parameter.
        /// </summary>
        /// <returns>User parameter to send with the tile update request.</returns>
        [OperationContract]
        void UpdateTile(Uri channelUri, string parameter);
    }
}
