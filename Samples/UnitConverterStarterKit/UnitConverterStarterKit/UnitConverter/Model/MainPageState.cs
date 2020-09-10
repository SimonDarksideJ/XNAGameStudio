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
namespace Microsoft.Phone.Applications.UnitConverter.Model
{
    /// <summary>
    /// Main Page state information for application Deactivate
    /// </summary>
    public class MainPageState : CommonPageState
    {
        /// <summary>
        /// The value the user has entered for the source unit
        /// </summary>
        public string SourceUnitValue { get; set; }

        /// <summary>
        /// True if the upper unit is the source. False otherwise
        /// </summary>
        public bool IsUpperUnitSource { get; set; }


        /// <summary>
        /// Swap the source and target unit names to allow the user to 
        /// easily switch between which unit is the source
        /// </summary>
        /// <param name="isUpperUnitSource">current state of which unit is
        /// the source unit</param>
        public void SwapUnits(bool isUpperUnitSource)
        {
            this.IsUpperUnitSource = !isUpperUnitSource;
            string temp = this.SourceUnitName;
            this.SourceUnitName = this.TargetUnitName;
            this.TargetUnitName = temp;
        }
    }
}
