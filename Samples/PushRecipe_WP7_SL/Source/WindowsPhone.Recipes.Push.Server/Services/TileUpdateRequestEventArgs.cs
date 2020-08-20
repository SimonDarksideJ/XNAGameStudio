using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Tile update request event arguments.
    /// </summary>
    internal class TileUpdateRequestEventArgs : EventArgs
    {
        public Uri ChannelUri { get; private set; }

        public string Parameter { get; private set; }

        public TileUpdateRequestEventArgs(Uri channelUri, string parameter)
        {
            this.ChannelUri = channelUri;
            this.Parameter = parameter;
        }
    }
}
