using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Models;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Implement this interface to log push notification messages send result and error.
    /// </summary>
    internal interface IMessageSendResultLogger
    {
        /// <summary>
        /// Implement this method by logging push notification messages send result.
        /// </summary>
        /// <param name="patternName">The server push pattern used while reporting this message send result.</param>
        /// <param name="result">The message send result to log.</param>
        void Log(string patternName, MessageSendResult result);

        /// <summary>
        /// Implement this method by logging push notification messages error result.
        /// </summary>
        /// <param name="patternName">The server push pattern used while reporting this message send error.</param>
        /// <param name="result">The message send result to log.</param>
        void Log(string patternName, MessageSendException exception);
    }
}
