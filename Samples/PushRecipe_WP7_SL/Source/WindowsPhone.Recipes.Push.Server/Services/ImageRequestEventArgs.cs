using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Image request event arguments.
    /// </summary>
    internal class ImageRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Tile image maximum size in bytes.
        /// </summary>
        public const int MaxTileImageSize = 80 * 1024; // The max tile image size is 80k.

        public string Parameter { get; private set; }
        public Stream ImageStream { get; private set; }

        /// <summary>
        /// Initializes a new instance with a memory stream maximum size of <see cref="ImageRequestEventArgs.MaxTileImageSize"/>.
        /// </summary>
        public ImageRequestEventArgs(string parameter)
        {
            Parameter = parameter;
            ImageStream = new MemoryStream(MaxTileImageSize);
        }
    }
}
