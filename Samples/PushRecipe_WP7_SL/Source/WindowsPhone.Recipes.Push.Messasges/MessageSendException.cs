using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using WindowsPhone.Recipes.Push.Messasges.Properties;

namespace WindowsPhone.Recipes.Push.Messasges
{
    /// <summary>
    /// Represents errors that occur during push notification message send operation.
    /// </summary>
    public class MessageSendException : Exception
    {
        /// <summary>
        /// Gets the message send result.
        /// </summary>
        public MessageSendResult Result { get; private set; }

        /// <summary>
        /// Initializes a new instance of this type.
        /// </summary>
        /// <param name="result">The send operation result.</param>
        /// <param name="innerException">An inner exception causes this error.</param>
        internal MessageSendException(MessageSendResult result, Exception innerException)
            : base(Resources.FailedToSendMessage, innerException)
        {
            Result = result;
        }
    }
}
