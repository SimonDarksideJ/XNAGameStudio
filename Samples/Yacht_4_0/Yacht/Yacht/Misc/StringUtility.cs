#region File Description


//-----------------------------------------------------------------------------
// StringUtility.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using System.Text;
using System.Text.RegularExpressions; 
#endregion

namespace Yacht
{
    /// <summary>
    /// A class that contains auxiliary methods for strings.
    /// </summary>
    static class StringUtility
    {
        /// <summary>
        /// Check if a name is valid as a player or game name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if the name is valid and false otherwise.</returns>
        public static bool IsNameValid(string name)
        {
            Regex re = new Regex("^[-'a-zA-Z0-9]*$");
            return name != "" && name != String.Empty && re.IsMatch(name) && name.Length < 10;
        }
    }
}
