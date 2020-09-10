using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Push notification channel subscription status.
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <value>The request is not applicable.</value>
        NotApplicable,

        /// <value>The subscription is active.</value>
        Active,

        /// <value>The subscription has expired.</value>
        Expired
    }
}
