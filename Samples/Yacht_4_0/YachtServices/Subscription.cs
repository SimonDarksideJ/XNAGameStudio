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
using System.Text;


#endregion

namespace YachtServices
{
    /// <summary>
    /// Represents user subscription information which represents a registration with the game server.
    /// </summary>
    public class Subscription
    {
        #region Properties

        
        /// <summary>
        /// The client's session identifier.
        /// </summary>
        public int SessionID { get; private set; }

        /// <summary>
        /// The notification channel Uri to use for contacting the client.
        /// </summary>
        public Uri ChannelUri { get; set; }

        /// <summary>
        /// Unique identifier for the game which the client joined.
        /// </summary>
        public Guid GameID { get; set; }

        /// <summary>
        /// The client's display name.
        /// </summary>
        public string Name { get; set; }


        #endregion

        #region Initialize
        

        /// <summary>
        /// Initialize a new instance of this type.
        /// </summary>
        public Subscription(int sessionID, Uri channelUri, string name)
        {
            SessionID = sessionID;
            ChannelUri = channelUri;
            Name = name;
        }


        #endregion
    }
}
