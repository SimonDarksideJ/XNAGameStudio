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


namespace Microsoft.Phone.Applications.UnitConverter.Resources
{
    /// <summary>
    /// Resource class interface
    /// </summary>
    public class Resources
    {
        /// <summary>
        /// Backing store for string resources
        /// </summary>
        private static readonly Strings strings = new Strings();

        /// <summary>
        /// Access all string resources
        /// </summary>
        public Strings Strings
        {
            get { return strings; }
        }

        /// <summary>
        /// Return current culture
        /// </summary>
        public System.Globalization.CultureInfo CurrentCulture
        {
            get { return ApplicationState.CurrentCulture; }
        }
    }
}
