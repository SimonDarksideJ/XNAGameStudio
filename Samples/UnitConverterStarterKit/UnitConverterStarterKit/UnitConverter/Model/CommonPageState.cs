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
namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// Base class for page state settings for the main page or conversions page
    /// </summary>
    public class CommonPageState
    {
        /// <summary>
        /// Curerrent unit category, such as length, temp etc
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Name for the source unit
        /// </summary>
        public string SourceUnitName { get; set; }

        /// <summary>
        /// Name for the target name
        /// </summary>
        public string TargetUnitName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CommonPageState()
        {
        }

        /// <summary>
        /// Updates the name objects.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="sourceUnitName">Name of the source unit.</param>
        /// <param name="targetUnitName">Name of the target unit.</param>
        public void UpdateNames(
            string category,
            string sourceUnitName,
            string targetUnitName)
        {
            this.Category = category;
            this.SourceUnitName = sourceUnitName;
            this.TargetUnitName = targetUnitName;
        }

    }
}
