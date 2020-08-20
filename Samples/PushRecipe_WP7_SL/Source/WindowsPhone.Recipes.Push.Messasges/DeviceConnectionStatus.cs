using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Windows Phone Device connection status.
    /// </summary>
    public enum DeviceConnectionStatus
    {
        /// <value>The request is not applicable.</value>
        NotApplicable,

        /// <value>The device is connected.</value>
        Connected,

        /// <value>The device is temporarily disconnected.</value>
        TempDisconnected,

        /// <value>The device is in an inactive state.</value>
        Inactive
    }
}
