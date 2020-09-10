// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Phone.Applications.UnitConverter.Helpers
{
    /// <summary>
    /// Logging helper class to get time stamp information during the app startup process
    /// </summary>
    public class ErrorLog : INotifyPropertyChanged
    {

        #region Bindable Properties

        /// <summary>
        /// log event
        /// </summary>
        private string logEvent;


        /// <summary>
        /// Gets or sets the log event description
        /// </summary>
        public string LogEvent
        {
            get { return this.logEvent; }

            set
            {
                this.logEvent = value;
                this.OnPropertyChanged("LogEvent");
            }
        }

        /// <summary>
        /// Gets or sets the log event message
        /// </summary>
        private string message;


        /// <summary>
        /// Gets or sets the log event message
        /// </summary>
        public string Message
        {
            get { return this.message; }

            set
            {
                this.message = value;
                this.OnPropertyChanged("Message");
            }
        }


        #endregion Bindable properties

        #region INotifyPropertyChanged

        /// <summary>
        /// Standard pattern for data binding and notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify subscribers of a change in the property
        /// </summary>
        /// <param name="propertyName">Name of the property to signal there has been a changed</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, args);
            }
        }

        #endregion
     

        /// <summary>
        /// Default constructor
        /// </summary>
        public ErrorLog()
        {
        }


        /// <summary>
        /// Set the description and a null message
        /// </summary>
        /// <param name="logEvent">The log event to log</param>
        public ErrorLog(string logEvent)
        {
            this.LogEvent = logEvent;
            this.Message = "";
        }


        /// <summary>
        /// Set the description and error message
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="message">The time to log for the event.</param>
        public ErrorLog(string logEvent, string message)
        {
            this.LogEvent = logEvent;
            this.Message = message;
        }
    }
}
