using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WindowsPhone.Recipes.Push.Server.Models
{
    /// <summary>
    /// Types of server push patterns.
    /// </summary>
    public enum PushPatternType
    {
        /// <value>One time server push pattern.</value>
        OneTime,

        /// <value>Login counter server push pattern.</value>
        LoginCounter,

        /// <value>Ask to pin server push pattern.</value>
        AskToPin,

        /// <value>Custom tile image server push pattern.</value>
        CustomTileImage,

        /// <value>Tile shedule server push pattern.</value>
        TileSchedule,
    }
}
