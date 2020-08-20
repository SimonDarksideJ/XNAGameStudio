//-----------------------------------------------------------------------------
// PanelControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;


namespace UserInterfaceSample.Controls
{
    public class PanelControl : Control
    {
        // Position child components in a column, with the given spacing between components
        public void LayoutColumn(float xMargin, float yMargin, float ySpacing)
        {
            float y = yMargin;

            for (int i = 0; i < ChildCount; i++)
            {
                Control child = this[i];
                child.Position = new Vector2 { X = xMargin, Y = y };
                y += child.Size.Y + ySpacing;
            }

            InvalidateAutoSize();
        }

        // Position child components in a row, with the given spacing between components
        public void LayoutRow(float xMargin, float yMargin, float xSpacing)
        {
            float x = xMargin;

            for (int i = 0; i < ChildCount; i++)
            {
                Control child = this[i];
                child.Position = new Vector2 { X = x, Y = yMargin };
                x += child.Size.X + xSpacing;
            }

            InvalidateAutoSize();
        }
    }
}
