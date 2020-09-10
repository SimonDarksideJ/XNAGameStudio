#region File Information
//-----------------------------------------------------------------------------
// ITextControl.cs
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

namespace DynamicMenu.Controls
{
    /// <summary>
    /// The interface for controls that show text
    /// </summary>
    public interface ITextControl
    {
        /// <summary>
        /// The text to display in this control
        /// </summary>
        string Text { get; set; }
    }
}
