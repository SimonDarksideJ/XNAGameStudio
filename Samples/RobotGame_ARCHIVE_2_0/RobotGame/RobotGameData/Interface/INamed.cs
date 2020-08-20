#region File Description
//-----------------------------------------------------------------------------
// INamed.cs
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

namespace RobotGameData.GameInterface
{
    #region Interface

    /// <summary>
    /// This is an interface of inherited class which needs a name.
    /// </summary>
    public interface INamed
    {
        string Name { get; set; }
    }

    #endregion
}
