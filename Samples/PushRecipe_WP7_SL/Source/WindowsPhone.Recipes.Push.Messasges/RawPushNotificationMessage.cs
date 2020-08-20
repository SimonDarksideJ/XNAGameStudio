using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using WindowsPhone.Recipes.Push.Messasges.Properties;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Represents a raw push notification message.
    /// </summary>
    /// <remarks>
    /// If you do not wish to update the tile or send a toast notification, you can instead
    /// send raw information to your application using a raw notification. If your application
    /// is not currently running, the raw notification is discarded on the Microsoft Push
    /// Notification Service and is not delivered to the device.
    /// 
    /// This class members are thread safe.
    /// </remarks>
    public sealed class RawPushNotificationMessage : PushNotificationMessage
    {
        #region Constants

        /// <value>Calculated raw message headers size.</value>
        /// <remarks>This should ne updated if changing the protocol.</remarks>
        private const int RawMessageHeadersSize = 116;

        /// <value>Raw push notification message maximum payload size.</value>
        public const int MaxPayloadSize = MaxMessageSize - RawMessageHeadersSize;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the message raw data bytes.
        /// </summary>
        public byte[] RawData
        {
            get
            {
                return Payload;
            }

            set
            {
                Payload = value;
            }
        }

        /// <summary>
        /// Raw push notification message class id.
        /// </summary>
        protected override int NotificationClassId
        {
            get { return 3; }
        }
        
        #endregion        

        #region Ctor
        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        /// <param name="sendPriority">The send priority of this message in the MPNS.</param>
        public RawPushNotificationMessage(MessageSendPriority sendPriority = MessageSendPriority.Normal)
            : base(sendPriority)
        {

        } 
        #endregion

        #region Overrides

        protected override void VerifyPayloadSize(byte[] payload)
        {
            if (payload.Length > MaxPayloadSize)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.PayloadSizeIsTooBig, MaxPayloadSize));
            }
        }
        
        #endregion
    }
}
