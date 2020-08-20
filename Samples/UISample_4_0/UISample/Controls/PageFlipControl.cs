//-----------------------------------------------------------------------------
// PageFlipControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;


namespace UserInterfaceSample.Controls
{
    /// <summary>
    /// This control aligns its child controls horizontally, and allows the user to flick
    /// through them.
    /// </summary>
    class PageFlipControl : PanelControl
    {
        #region Fields

        // PageFlipTracker handles the logic of scrolling / tracking etc.
        private PageFlipTracker tracker = new PageFlipTracker();
        #endregion

        #region Method overrides
        protected override void OnChildAdded(int index, Control child)
        {
            tracker.PageWidthList.Insert(index, (int)child.Size.X);
        }

        protected override void OnChildRemoved(int index, Control child)
        {
            tracker.PageWidthList.RemoveAt(index);
        }

        public override void Update(GameTime gametime)
        {
            tracker.Update();
            base.Update(gametime);
        }

        public override void HandleInput(InputState input)
        {
            tracker.HandleInput(input);

            if(ChildCount > 0)
            {
                // Only the child that currently has focus gets input
                int current = tracker.CurrentPage;
                this[current].HandleInput(input);
            }
        }

        public override void Draw(DrawContext context)
        {
            int childCount = ChildCount;
            if (childCount < 2)
            {
                // Default rendering behavior if we don't have enough
                // children to flip through.
                base.Draw(context);
                return;
            }
            Vector2 origin = context.DrawOffset;
            int iCurrent = tracker.CurrentPage;

            float horizontalOffset = tracker.CurrentPageOffset;
            context.DrawOffset = origin + new Vector2 { X = horizontalOffset };
            this[iCurrent].Draw(context);

            if (horizontalOffset > 0)
            {
                // The screen has been dragged to the right, so the edge of another
                // page is visible to the left.
                int iLeft = (iCurrent + childCount - 1) % childCount;
                context.DrawOffset.X = origin.X + horizontalOffset - tracker.EffectivePageWidth(iLeft);
                this[iLeft].Draw(context);
            }

            if (horizontalOffset + this[iCurrent].Size.X < context.Device.Viewport.Width)
            {
                // The edge of another page is visible to the right.
                // Note that if we have two pages, it's possible that a page will be
                // drawn twice, with parts of it visible on each edge of the screen.
                int iRight = (iCurrent + 1) % childCount;
                context.DrawOffset.X = origin.X + horizontalOffset + tracker.EffectivePageWidth(iCurrent);
                this[iRight].Draw(context);
            }
        }
        #endregion
    }
}
