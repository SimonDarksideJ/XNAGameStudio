#region File Information
//-----------------------------------------------------------------------------
// IControl.cs
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
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace DynamicMenu.Controls
{
    /// <summary>
    /// The interface for all controls
    /// </summary>
    public interface IControl
    {
        // See Control.cs for documentation on these properties

        int Width { get; set; }
        int Height { get; set; }
        int Top { get; set; }
        int Left { get; set; }
        int Bottom { get; }
        int Right { get; }
        string Name { get; set; }
        Color Hue { get; set; }

        IControl Parent { get; set; }

        Point GetAbsoluteTopLeft();

        void Update(GameTime gameTime, List<GestureSample> gestures);
    }
}
