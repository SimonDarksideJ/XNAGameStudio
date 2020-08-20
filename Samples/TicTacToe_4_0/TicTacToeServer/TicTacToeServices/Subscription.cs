#region File Description
//-----------------------------------------------------------------------------
// Subscription.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#endregion

namespace TicTacToeServices
{
    /// <summary>
    /// Represents user subscription.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets the user's unique ID.
        /// </summary>
        public Guid SessionID { get; private set; }

        /// <summary>
        /// Gets the user's notification channel uri.
        /// </summary>
        public Uri ChannelUri { get; private set; }

        /// <summary>
        /// Initialize a new instance of this type.
        /// </summary>
        public Subscription(Guid sessionID, Uri channelUri)
        {
            SessionID = sessionID;
            ChannelUri = channelUri;
        }

    }
}
