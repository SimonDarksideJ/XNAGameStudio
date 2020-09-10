using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Server info event arguments.
    /// </summary>
    internal class ServerInfoEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the server info.
        /// </summary>
        public ServerInfo ServerInfo { get; set; }
    }
}
